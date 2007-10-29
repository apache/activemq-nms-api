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
using Apache.NMS;
using System;

namespace Apache.MSMQ
{
    /// <summary>
    /// Represents a NMS connection MSMQ.  Since the underlying MSMQ APIs are actually
    /// connectionless, NMS connection in the MSMQ case are not expensive operations.
    /// </summary>
    ///
    public class Connection : IConnection
    {
        
        private AcknowledgementMode acknowledgementMode = AcknowledgementMode.AutoAcknowledge;
        private IMessageConverter messageConverter = new DefaultMessageConverter();

        private bool connected;
        private bool closed;
        private string clientId;

        /// <summary>
        /// Starts message delivery for this connection.
        /// </summary>
        public void Start()
        {
            CheckConnected();
        }
                
        /// <summary>
        /// Stop message delivery for this connection.
        /// </summary>
        public void Stop()
        {
            CheckConnected();
        }
        
        /// <summary>
        /// Creates a new session to work on this connection
        /// </summary>
        public ISession CreateSession()
        {
            return CreateSession(acknowledgementMode);
        }
        
        /// <summary>
        /// Creates a new session to work on this connection
        /// </summary>
        public ISession CreateSession(AcknowledgementMode mode)
        {
            CheckConnected();
            return new Session(this, mode);
        }
        
        public void Dispose()
        {
            closed = true;
        }
        
        public AcknowledgementMode AcknowledgementMode
        {
            get { return acknowledgementMode; }
            set { acknowledgementMode = value; }
        }

        public IMessageConverter MessageConverter
        {
            get { return messageConverter; }
            set { messageConverter = value; }
        }

        public string ClientId
        {
            get { return clientId; }
            set {
                if (connected)
                {
                    throw new NMSException("You cannot change the ClientId once the Connection is connected");
                }
                clientId = value;
            }
        }

        public event ExceptionListener ExceptionListener;

        protected void CheckConnected()
        {
            if (closed)
            {
                throw new NMSException("Connection Closed");
            }
            if (!connected)
            {
                connected = true;
                // now lets send the connection and see if we get an ack/nak
                // TODO: establish a connection
            }
        }

        public void Close()
        {
            Dispose();
        }

		public void HandleException(Exception e)
		{
			if (ExceptionListener != null)
			{
				ExceptionListener(e);
			}
			else
			{
				Tracer.Error(e);
			}
			
		}
        	        
    }
}
