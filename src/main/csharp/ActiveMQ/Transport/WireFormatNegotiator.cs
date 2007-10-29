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
using System.IO;
using System.Threading;
using Apache.ActiveMQ.Commands;
using Apache.ActiveMQ.OpenWire;
using Apache.ActiveMQ.Transport;
using System;
using Apache.ActiveMQ.Util;

namespace Apache.ActiveMQ.Transport
{
	
    /// <summary>
    /// A Transport which negotiates the wire format
    /// </summary>
    public class WireFormatNegotiator : TransportFilter
    {
        private OpenWireFormat wireFormat;
        private int negotiateTimeout=15000;
    
        private AtomicBoolean firstStart=new AtomicBoolean(true);
        private CountDownLatch readyCountDownLatch = new CountDownLatch(1);
        private CountDownLatch wireInfoSentDownLatch = new CountDownLatch(1);

        public WireFormatNegotiator(ITransport next, OpenWireFormat wireFormat)
            : base(next)
        {
            this.wireFormat = wireFormat;
        }
        
        public override void Start() {
            base.Start();
            if (firstStart.CompareAndSet(true, false))
            {
                try
                {
                    next.Oneway(wireFormat.PreferedWireFormatInfo);
                }
                finally
                {
                    wireInfoSentDownLatch.countDown();
                }
            }
        }
        
        public override void Dispose() {
        	base.Dispose();
            readyCountDownLatch.countDown();
        }

        public override void Oneway(Command command)
        {
            if (!readyCountDownLatch.await(negotiateTimeout))
                throw new IOException("Wire format negociation timeout: peer did not send his wire format.");
            next.Oneway(command);
        }

        protected override void OnCommand(ITransport sender, Command command)
        {
            if ( command.GetDataStructureType() == WireFormatInfo.ID_WireFormatInfo )
            {
                WireFormatInfo info = (WireFormatInfo)command;
                try
                {
                    if (!info.Valid)
                    {
                        throw new IOException("Remote wire format magic is invalid");
                    }
                    wireInfoSentDownLatch.await(negotiateTimeout);
                    wireFormat.renegotiateWireFormat(info);
                }
                catch (Exception e)
                {
                    OnException(this, e);
                } 
                finally
                {
                    readyCountDownLatch.countDown();
                }
            }
            this.commandHandler(sender, command);
        }

        protected override void OnException(ITransport sender, Exception command)
        {
            readyCountDownLatch.countDown();
            this.exceptionHandler(sender, command);
        }
    }
}

