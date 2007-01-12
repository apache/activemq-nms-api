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
using System;
using NMS;
using System.Threading;

namespace MSMQ
{
    /// <summary>
    /// An object capable of receiving messages from some destination
    /// </summary>
    public class MessageConsumer : IMessageConsumer
    {
        private readonly Session session;
        private readonly AcknowledgementMode acknowledgementMode;

        public MessageConsumer(Session session, AcknowledgementMode acknowledgementMode)
        {
            this.session = session;
            this.acknowledgementMode = acknowledgementMode;            
        }

        public IMessage Receive()
        {
            throw new NotImplementedException();
        }

        public IMessage Receive(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public IMessage ReceiveNoWait()
        {
            throw new NotImplementedException();
        }

        public event MessageListener Listener;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            Dispose();
        }

    }
}
