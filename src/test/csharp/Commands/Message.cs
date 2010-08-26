/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections;

using Apache.NMS.Util;

namespace Apache.NMS.Commands
{
    public class Message : IMessage, ICloneable
    {
        private IDestination destination;
        private string transactionId;
        private string messageId;
        private string groupID;
		private int groupSequence;
        private string correlationId;
        private bool persistent;
        private long expiration;
        private byte priority;
        private IDestination replyTo;
        private long timestamp;
        private string type;
        private bool redelivered;
        private byte[] content;
        private bool readOnlyMsgProperties;
        private bool readOnlyMsgBody;

        private MessagePropertyIntercepter propertyHelper;
        private PrimitiveMap properties;

        ///
        /// <summery>
        ///  Clone this object and return a new instance that the caller now owns.
        /// </summery>
        ///
        public virtual Object Clone()
        {
            // Since we are the lowest level base class, do a
            // shallow copy which will include the derived classes.
            // From here we would do deep cloning of other objects
            // if we had any.
            Message o = (Message) this.MemberwiseClone();

            if(this.messageId != null)
            {
                o.NMSMessageId = (string) this.messageId.Clone();
            }
			
			return o;
		}		

        ///
        /// <summery>
        ///  Returns a string containing the information for this DataStructure
        ///  such as its type and value of its elements.
        /// </summery>
        ///
        public override string ToString()
        {
            return GetType().Name + "[" +
                "Destination=" + destination + ", " +
                "TransactionId=" + transactionId + ", " +
                "MessageId=" + messageId + ", " +
                "GroupID=" + groupID + ", " +
                "GroupSequence=" + groupSequence + ", " +
                "CorrelationId=" + correlationId + ", " +
                "Expiration=" + expiration + ", " +
                "Priority=" + priority + ", " +
                "ReplyTo=" + replyTo + ", " +
                "Timestamp=" + timestamp + ", " +
                "Type=" + type + ", " +
                "Redelivered=" + redelivered +
                "]";
        }

        public void Acknowledge()
        {
        }
		
        public virtual void ClearBody()
        {
			this.content = null;
        }

        public virtual void ClearProperties()
        {
            this.properties.Clear();
        }
		
        protected void FailIfReadOnlyBody()
        {
            if(ReadOnlyBody == true)
            {
                throw new MessageNotWriteableException("Message is in Read-Only mode.");
            }
        }

        protected void FailIfWriteOnlyBody()
        {
            if(ReadOnlyBody == false)
            {
                throw new MessageNotReadableException("Message is in Write-Only mode.");
            }
        }
		
        #region Properties
		
		public string TransactionId
		{
			get { return this.transactionId; }
			set { this.transactionId = value; }
		}

        public byte[] Content
        {
            get { return content; }
            set { this.content = value; }
        }
		
        public virtual bool ReadOnlyProperties
        {
            get { return this.readOnlyMsgProperties; }
            set { this.readOnlyMsgProperties = value; }
        }

        public virtual bool ReadOnlyBody
        {
            get { return this.readOnlyMsgBody; }
            set { this.readOnlyMsgBody = value; }
        }
		
        public IPrimitiveMap Properties
        {
            get
            {
                if(null == properties)
                {
                    properties = new PrimitiveMap();
                    propertyHelper = new MessagePropertyIntercepter(this, properties, this.ReadOnlyProperties);
                    propertyHelper.AllowByteArrays = false;
                }

                return propertyHelper;
            }
        }

        /// <summary>
        /// The correlation ID used to correlate messages with conversations or long running business processes
        /// </summary>
        public string NMSCorrelationID
        {
            get { return correlationId; }
            set { correlationId = value; }
        }

        /// <summary>
        /// The destination of the message
        /// </summary>
        public IDestination NMSDestination
        {
            get { return destination; }
            set { this.destination = Destination.Transform(value); }
        }

        private TimeSpan timeToLive = TimeSpan.FromMilliseconds(0);
        /// <summary>
        /// The time in milliseconds that this message should expire in
        /// </summary>
        public TimeSpan NMSTimeToLive
        {
            get { return timeToLive; }

            set
            {
                timeToLive = value;
                if(timeToLive.TotalMilliseconds > 0)
                {
                    long timeStamp = timestamp;

                    if(timeStamp == 0)
                    {
                        timeStamp = DateUtils.ToJavaTimeUtc(DateTime.UtcNow);
                    }

                    expiration = timeStamp + (long) timeToLive.TotalMilliseconds;
                }
                else
                {
                    expiration = 0;
                }
            }
        }

        /// <summary>
        /// The timestamp the broker added to the message
        /// </summary>
        public DateTime NMSTimestamp
        {
            get { return DateUtils.ToDateTime(timestamp); }
            set
            {
                timestamp = DateUtils.ToJavaTimeUtc(value);
                if(timeToLive.TotalMilliseconds > 0)
                {
                    expiration = timestamp + (long) timeToLive.TotalMilliseconds;
                }
            }
        }

        /// <summary>
        /// The message ID which is set by the provider
        /// </summary>
        public string NMSMessageId
        {
            get { return this.messageId; }
            set { this.messageId = value; }
        }

        /// <summary>
        /// Whether or not this message is persistent
        /// </summary>
        public MsgDeliveryMode NMSDeliveryMode
        {
            get { return (persistent ? MsgDeliveryMode.Persistent : MsgDeliveryMode.NonPersistent); }
            set { persistent = (MsgDeliveryMode.Persistent == value); }
        }

        /// <summary>
        /// The Priority on this message
        /// </summary>
        public MsgPriority NMSPriority
        {
            get { return (MsgPriority) priority; }
            set { priority = (byte) value; }
        }

        /// <summary>
        /// Returns true if this message has been redelivered to this or another consumer before being acknowledged successfully.
        /// </summary>
        public bool NMSRedelivered
        {
            get { return this.redelivered; }
            set { this.redelivered = value; }
        }

        /// <summary>
        /// The destination that the consumer of this message should send replies to
        /// </summary>
        public IDestination NMSReplyTo
        {
            get { return replyTo; }
            set { replyTo = Destination.Transform(value); }
        }

        /// <summary>
        /// The type name of this message
        /// </summary>
        public string NMSType
        {
            get { return type; }
            set { type = value; }
        }

        #endregion

        #region NMS Extension headers

        /// <summary>
        /// Returns the number of times this message has been redelivered to other consumers without being acknowledged successfully.
        /// </summary>
        public int NMSXDeliveryCount
        {
            get { return 0; }
        }

        /// <summary>
        /// The Message Group ID used to group messages together to the same consumer for the same group ID value
        /// </summary>
        public string NMSXGroupID
        {
            get { return groupID; }
            set { groupID = value; }
        }
        /// <summary>
        /// The Message Group Sequence counter to indicate the position in a group
        /// </summary>
        public int NMSXGroupSeq
        {
            get { return groupSequence; }
            set { groupSequence = value; }
        }

        /// <summary>
        /// Returns the ID of the producers transaction
        /// </summary>
        public string NMSXProducerTXID
        {
            get
            {
                if(null != transactionId)
                {
                    return transactionId;
                }

                return String.Empty;
            }
        }

        #endregion

    };
}

