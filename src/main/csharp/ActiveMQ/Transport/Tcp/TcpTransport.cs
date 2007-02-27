/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using ActiveMQ;
using ActiveMQ.Commands;
using ActiveMQ.OpenWire;
using ActiveMQ.Transport;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ActiveMQ.Transport.Tcp
{
	
    /// <summary>
    /// An implementation of ITransport that uses sockets to communicate with the broker
    /// </summary>
    public class TcpTransport : ITransport
    {
        private Socket socket;
        private IWireFormat wireformat = new OpenWireFormat();
        private BinaryReader socketReader;
        private BinaryWriter socketWriter;
        private Thread readThread;
        private bool started;
        private Util.AtomicBoolean closed = new Util.AtomicBoolean(false);
        
        private CommandHandler commandHandler;
        private ExceptionHandler exceptionHandler;
        
        public TcpTransport(Socket socket, IWireFormat wireformat)
        {
            this.socket = socket;
			this.wireformat = wireformat;
        }
        
        /// <summary>
        /// Method Start
        /// </summary>
        public void Start()
        {
            if (!started)
            {
                if( commandHandler == null )
                    throw new InvalidOperationException ("command cannot be null when Start is called.");
                if( exceptionHandler == null )
                    throw new InvalidOperationException ("exception cannot be null when Start is called.");

                started = true;
                
                // As reported in AMQ-988 it appears that NetworkStream is not thread safe
                // so lets use an instance for each of the 2 streams
                socketWriter = new OpenWireBinaryWriter(new NetworkStream(socket));
                socketReader = new OpenWireBinaryReader(new NetworkStream(socket));
                
                // now lets create the background read thread
                readThread = new Thread(new ThreadStart(ReadLoop));
                readThread.Start();
            }
        }
        
        public void Oneway(Command command)
        {
            Wireformat.Marshal(command, socketWriter);
            socketWriter.Flush();
        }
        
        public FutureResponse AsyncRequest(Command command)
        {
            throw new NotImplementedException("Use a ResponseCorrelator if you want to issue AsyncRequest calls");
        }
        
        public Response Request(Command command)
        {
            throw new NotImplementedException("Use a ResponseCorrelator if you want to issue Request calls");
        }

        public void Close()
        {
            if (closed.CompareAndSet(false, true))
            {
                socket.Close();
                if (System.Threading.Thread.CurrentThread != readThread)
                    readThread.Join();
                socketWriter.Close();
                socketReader.Close();
            }
        }

        public void Dispose()
        {
            Close();
        }
        
        public void ReadLoop()
        {
            // This is the thread function for the reader thread. This runs continuously
            // performing a blokcing read on the socket and dispatching all commands
            // received.
            //
            // Exception Handling
            // ------------------
            // If an Exception occurs during the reading/marshalling, then the connection
            // is effectively broken because position cannot be re-established to the next
            // message.  This is reported to the app via the exceptionHandler and the socket
            // is closed to prevent further communication attempts.
            //
            // An exception in the command handler may not be fatal to the transport, so
            // these are simply reported to the exceptionHandler.
            //
            while (!closed.Value)
            {
                Command command = null;
                try
                {
                    command = (Command) Wireformat.Unmarshal(socketReader);
                }
                catch(Exception ex)
                {
                    if( !closed.Value )
                    {
                        this.exceptionHandler(this, ex);
                        // Close the socket as there's little that can be done with this transport now.
                        Close();
                        break;
                    }
                }

                try
                {
					if (command != null)
					{
						this.commandHandler(this, command);
					}
                }
                catch ( Exception e)
                {
                    this.exceptionHandler(this, e);
                }
            }
        }
                
        // Implementation methods
                
        public CommandHandler Command {
            get { return commandHandler; }
            set { this.commandHandler = value; }
        }

        public  ExceptionHandler Exception {
            get { return exceptionHandler; }
            set { this.exceptionHandler = value; }
        }

        public IWireFormat Wireformat
        {
            get { return wireformat; }
            set { wireformat = value; }
        }
    }
}



