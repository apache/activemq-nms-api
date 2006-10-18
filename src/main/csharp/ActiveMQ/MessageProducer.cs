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
using ActiveMQ.Commands;
using NMS;
using System;

namespace ActiveMQ
{
    /// <summary>
    /// An object capable of sending messages to some destination
    /// </summary>
    public class MessageProducer : IMessageProducer
    {
        
        private Session session;
        private ProducerInfo info;
        private long messageCounter;
        
        bool persistent;
        TimeSpan timeToLive;
        int priority;
        bool disableMessageID;
        bool disableMessageTimestamp;
        
        public MessageProducer(Session session, ProducerInfo info)
        {
            this.session = session;
            this.info = info;
        }
        
        public void Send(IMessage message)
        {
            Send(info.Destination, message);
        }
        
        public void Send(IDestination destination, IMessage message)
        {
			ActiveMQMessage activeMessage = (ActiveMQMessage)message;

			if (!disableMessageID)
			{
				MessageId id = new MessageId();
				id.ProducerId = info.ProducerId;
				lock (this)
				{
					id.ProducerSequenceId = ++messageCounter;
				}
				activeMessage.MessageId = id;
			}

			if (!disableMessageTimestamp)
			{
				activeMessage.Timestamp = ActiveMQ.Util.DateUtils.ToJavaTime(DateTime.UtcNow);
			}

            activeMessage.ProducerId = info.ProducerId;
            activeMessage.Destination = ActiveMQDestination.Transform(destination);
            
            if (session.Transacted)
            {
                session.DoStartTransaction();
                activeMessage.TransactionId = session.TransactionContext.TransactionId;
            }
            
            session.DoSend(destination, message);
        }
        
        public void Dispose()
        {
            session.Connection.DisposeOf(info.ProducerId);
        }
        
        public bool Persistent
        {
            get { return persistent; }
            set { this.persistent = value; }
        }

        public TimeSpan TimeToLive
        {
            get { return timeToLive; }
            set { this.timeToLive = value; }
        }
        public int Priority
        {
            get { return priority; }
            set { this.priority = value; }
        }
        
        public bool DisableMessageID
        {
            get { return disableMessageID; }
            set { this.disableMessageID = value; }
        }
        
        public bool DisableMessageTimestamp
        {
            get { return disableMessageTimestamp; }
            set { this.disableMessageTimestamp = value; }
        }
        
    }
}
