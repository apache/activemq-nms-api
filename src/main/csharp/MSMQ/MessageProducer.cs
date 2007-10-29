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
using System.Messaging;
using Apache.NMS;

namespace Apache.MSMQ
{
    /// <summary>
    /// An object capable of sending messages to some destination
    /// </summary>
    public class MessageProducer : IMessageProducer
    {
		
        private readonly Session session;
        private Destination destination;

        //private long messageCounter;
        private bool persistent;
        private TimeSpan timeToLive;
        private byte priority;
        private bool disableMessageID;
        private bool disableMessageTimestamp;
        
        private MessageQueue messageQueue;
        private IMessageConverter messageConverter;

        public MessageProducer(Session session, Destination destination)
        {
            this.session = session;
            this.destination = destination;
            MessageConverter = session.MessageConverter;
            if (destination != null)
            {
                messageQueue = openMessageQueue(destination);
            }
        }

        private MessageQueue openMessageQueue(Destination dest)
        {
            MessageQueue rc=null;
            try
            {
                if (!MessageQueue.Exists(dest.Path))
                {
                    // create the new message queue and make it transactional
                    rc = MessageQueue.Create(dest.Path, session.Transacted);
                    this.destination.Path = rc.Path;
                } else
                {
                    rc = new MessageQueue(dest.Path);
                    this.destination.Path = rc.Path;
                    if( !rc.CanWrite )
                    {
                        throw new NMSSecurityException("Do not have write access to: " + dest);
                    }
                }
            }
            catch( Exception e )
            {
                if( rc!=null )
                {
                    rc.Dispose();
                }
                throw new NMSException(e.Message+": "+dest, e);
            }
            return rc;
        }

        public void Send(IMessage message)
        {
            Send(Destination, message);
        }

		public void Send(IMessage message, bool persistent, byte priority, TimeSpan timeToLive)
		{
			Send(Destination, message, persistent, priority, timeToLive);
		}

        public void Send(IDestination destination, IMessage message)
        {
			Send(destination, message, Persistent, Priority, TimeToLive);
        }
		
        public void Send(IDestination destination, IMessage imessage, bool persistent, byte priority, TimeSpan timeToLive)
        {
            BaseMessage message = (BaseMessage) imessage;
            MessageQueue mq=null;
            MessageQueue responseQueue = null;
            MessageQueueTransaction transaction = null;
            try
            {
                // Locate the MSMQ Queue we will be sending to
                if (messageQueue != null)
                {
                    if( destination.Equals(this.destination) )
                    {
                        mq = messageQueue;
                    }
                    else
                    {
                        throw new NMSException("This producer can only be used to send to: " + destination);
                    }
                }
                else
                {
                    mq = openMessageQueue((Destination) destination);
                }
				
                // Convert the Mesasge into a MSMQ message
                message.NMSPersistent = persistent;
                message.NMSTimeToLive = timeToLive;
                message.NMSPriority = priority;
                
                // message.NMSTimestamp = new DateTime().Date.;
                Message msg = messageConverter.ToMsmqMessage(message);
                // TODO: message.NMSMessageId =
                // Now Send the message
                if( mq.Transactional )
                {
                    if (session.Transacted)
                    {
                        mq.Send(msg, session.MessageQueueTransaction);
                        
                    } else
                    {
                        // Start our own mini transaction here to send the message.
                        transaction = new MessageQueueTransaction();
                        transaction.Begin();
                        mq.Send(msg, transaction);
                        transaction.Commit();
                    }
                } else
                {
                    if( session.Transacted )
                    {
                        // We may want to raise an exception here since app requested
                        // a transeced NMS session, but is using a non transacted message queue
                        // For now silently ignore it.
                    }
                    mq.Send(msg);
                }
                
            } finally
            {
                // Cleanup
                if(transaction!=null)
                {
                    transaction.Dispose();
                }
                if (responseQueue != null)
                {
                    responseQueue.Dispose();
                }
                if( mq!=null && mq!=messageQueue )
                {
                    mq.Dispose();
                }
            }
        }
		
		
		
        public void Dispose()
        {
            if( messageQueue!=null )
            {
                messageQueue.Dispose();
                messageQueue = null;
            }
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

        public bool Persistent
        {
            get { return persistent; }
            set { persistent = value; }
        }

        public TimeSpan TimeToLive
        {
            get { return timeToLive; }
            set { timeToLive = value; }
        }

        byte IMessageProducer.Priority
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

		public IDestination Destination
		{
            get { return destination; }
            set { destination = (Destination) value; }
		}
		
        public byte Priority
        {
            get { return priority; }
            set { priority = value; }
        }

        public bool DisableMessageID
        {
            get { return disableMessageID; }
            set { disableMessageID = value; }
        }

        public bool DisableMessageTimestamp
        {
            get { return disableMessageTimestamp; }
            set { disableMessageTimestamp = value; }
        }

        public IMessageConverter MessageConverter
        {
            get { return messageConverter; }
            set { messageConverter = value; }
        }

    }
}
