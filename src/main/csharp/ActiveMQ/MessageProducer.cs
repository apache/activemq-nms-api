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
        
        private bool persistent = NMSConstants.defaultPersistence;
        private TimeSpan timeToLive;
		private bool specifiedTimeToLive;
        private byte priority = NMSConstants.defaultPriority;
        private bool disableMessageID = false;
        private bool disableMessageTimestamp = false;
        
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
			Send(destination, message, Persistent, Priority, TimeToLive, specifiedTimeToLive);
		}
		
        public void Send(IMessage message, bool persistent, byte priority, TimeSpan timeToLive)
        {
			Send(info.Destination, message, persistent, priority, timeToLive);
		}
		
        public void Send(IDestination destination, IMessage message, bool persistent, byte priority, TimeSpan timeToLive)
        {
			Send(destination, message, persistent, priority, timeToLive, true);
		}
		
        public void Send(IDestination destination, IMessage message, bool persistent, byte priority, TimeSpan timeToLive, bool specifiedTimeToLive)
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

			if (!disableMessageTimestamp && specifiedTimeToLive)
			{
				Console.WriteLine(">>> sending message with Timestamp: " + activeMessage.Timestamp + " and timeToLive:  " + timeToLive);
				activeMessage.Timestamp = ActiveMQ.Util.DateUtils.ToJavaTime(DateTime.UtcNow);
			}
			
			if (specifiedTimeToLive)
			{
				activeMessage.Expiration = ActiveMQ.Util.DateUtils.ToJavaTime(timeToLive);
			}
				
            activeMessage.ProducerId = info.ProducerId;
            activeMessage.Destination = ActiveMQDestination.Transform(destination);
            
            if (session.Transacted)
            {
                session.DoStartTransaction();
                activeMessage.TransactionId = session.TransactionContext.TransactionId;
            }
			
			activeMessage.Persistent = persistent;
			activeMessage.Priority = priority;
			activeMessage.Destination = ActiveMQDestination.Transform(destination);
		    
            session.DoSend(activeMessage);
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

        public byte Priority
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
        
		public IMessage CreateMessage()
		{
			return session.CreateMessage();
		}
		
		public ITextMessage CreateTextMessage()
		{
			return session.CreateTextMessage();
		}
		
		public ITextMessage CreateTextMessage(String text)
		{
			return session.CreateTextMessage(text);
		}
		
		public IMapMessage CreateMapMessage()
		{
			return session.CreateMapMessage();
		}
		
		public IObjectMessage CreateObjectMessage(Object body)
		{
			return session.CreateObjectMessage(body);
		}
		
		public IBytesMessage CreateBytesMessage()
		{
			return session.CreateBytesMessage();
		}
		
		public IBytesMessage CreateBytesMessage(byte[] body)
		{
			return session.CreateBytesMessage(body);
		}
    }
}
