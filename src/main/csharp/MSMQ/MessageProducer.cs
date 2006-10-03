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
using NMS;

namespace MSMQ
{
    /// <summary>
    /// An object capable of sending messages to some destination
    /// </summary>
    public class MessageProducer : IMessageProducer
    {
        private readonly Session session;
        private Destination destination;

        private long messageCounter;
        private bool persistent;
        private long timeToLive;
        private int priority;
        private bool disableMessageID;
        private bool disableMessageTimestamp;
        
        private MessageQueue messageQueue;

        public MessageProducer(Session session, Destination destination)
        {
            this.session = session;
            this.destination = destination;
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
            Send(destination, message);
        }

        public void Send(IDestination dest, IMessage imessage)
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
                    if( dest.Equals(destination) )
                    {
                        mq = messageQueue;
                    } 
                    else
                    {
                        throw new NMSException("This producer can only be used to send to: "+destination);
                    }
                }
                else
                {
                    mq = openMessageQueue((Destination)dest);
                }

                // Convert the Mesasge into a MSMQ message
                Message msg = new Message();                
                if ( message.NMSReplyTo!=null )
                {
                    responseQueue = new MessageQueue(((Destination)message.NMSReplyTo).Path);
                }
                if ( timeToLive!=null )
                {
                    msg.TimeToBeReceived = TimeSpan.FromMilliseconds(timeToLive);
                }
                if ( message.NMSCorrelationID!=null )
                {
                    msg.CorrelationId = message.NMSCorrelationID;
                }
                msg.Recoverable = persistent;
                msg.Priority = MessagePriority.Normal;
                msg.ResponseQueue = responseQueue;

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

        public bool Persistent
        {
            get { return persistent; }
            set { persistent = value; }
        }

        public long TimeToLive
        {
            get { return timeToLive; }
            set { timeToLive = value; }
        }

        public int Priority
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
    }
}