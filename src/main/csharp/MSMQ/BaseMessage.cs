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
	public delegate void AcknowledgeHandler(BaseMessage baseMessage);

    public class BaseMessage : IMessage
    {
        private PrimitiveMap propertiesMap = new PrimitiveMap();
        private IDestination destination;
        private string correlationId;
        private TimeSpan timeToLive;
        private string messageId;
        private bool persistent;
        private byte priority;
        private Destination replyTo;
        private string type;
        private event AcknowledgeHandler Acknowledger;
        private byte[] content;
        private DateTime timestamp = new DateTime();

        public byte[] Content
        {
            get { return content; }
            set { content = value; }
        }

        public void Acknowledge()
        {
            if (Acknowledger == null)
			{
                throw new NMSException("No Acknowledger has been associated with this message: " + this);
			}
            else
			{
                Acknowledger(this);
            }
        }
        
        
        // Properties
        
        public IPrimitiveMap Properties
        {
            get {
				return propertiesMap;
            }
        }
        
        
        // IMessage interface
        
        // NMS headers
        
        /// <summary>
        /// The correlation ID used to correlate messages with conversations or long running business processes
        /// </summary>
        public string NMSCorrelationID
        {
            get {
                return correlationId;
            }
            set {
                correlationId = value;
            }
        }
        
        /// <summary>
        /// The destination of the message
        /// </summary>
        public IDestination NMSDestination
        {
            get {
                return destination;
            }
            set {
                destination = value;
            }
        }
        
        /// <summary>
        /// The time in milliseconds that this message should expire in
        /// </summary>
        public TimeSpan NMSTimeToLive
        {
            get {
				return timeToLive;
            }
            set {
				timeToLive = value;
            }
        }
        
        /// <summary>
        /// The message ID which is set by the provider
        /// </summary>
        public string NMSMessageId
        {
            get {
                return messageId;
            }
			set {
				messageId = value;
			}
        }
        
        /// <summary>
        /// Whether or not this message is persistent
        /// </summary>
        public bool NMSPersistent
        {
            get {
                return persistent;
            }
            set {
                persistent = value;
            }
        }
        
        /// <summary>
        /// The Priority on this message
        /// </summary>
        public byte NMSPriority
        {
            get {
                return priority;
            }
            set {
                priority = value;
            }
        }
        
        /// <summary>
        /// Returns true if this message has been redelivered to this or another consumer before being acknowledged successfully.
        /// </summary>
        public bool NMSRedelivered
        {
            get {
                return false;
            }
        }
        
        
        /// <summary>
        /// The destination that the consumer of this message should send replies to
        /// </summary>
        public IDestination NMSReplyTo
        {
            get {
                return replyTo;
            }
            set {
                replyTo = (Destination) value;
            }
        }
        
        
        /// <summary>
        /// The timestamp the broker added to the message
        /// </summary>
        public DateTime NMSTimestamp
        {
            get {
                return timestamp;
            }
        }
        
        /// <summary>
        /// The type name of this message
        /// </summary>
        public string NMSType
        {
            get {
                return type;
            }
            set {
                type = value;
            }
        }
        

        public object GetObjectProperty(string name)
        {
            return null;
        }
        
        public void SetObjectProperty(string name, object value)
        {
        }
                
        
    }
}

