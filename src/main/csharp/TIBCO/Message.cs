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

namespace Apache.TibcoEMS
{
	class Message : Apache.NMS.IMessage
	{
		public TIBCO.EMS.Message tibcoMessage;

		public Message(TIBCO.EMS.Message message)
		{
			this.tibcoMessage = message;
		}

#region IMessage Members

		/// <summary>
		/// If using client acknowledgement mode on the session then this method will acknowledge that the
		/// message has been processed correctly.
		/// </summary>
		public void Acknowledge()
		{
			this.tibcoMessage.Acknowledge();
		}

		/// <summary>
		/// Provides access to the message properties (headers)
		/// </summary>
		public Apache.NMS.IPrimitiveMap Properties
		{
			get { return TibcoUtil.ToMessageProperties(this.tibcoMessage); }
		}

		/// <summary>
		/// The correlation ID used to correlate messages from conversations or long running business processes
		/// </summary>
		public string NMSCorrelationID
		{
			get { return this.tibcoMessage.CorrelationID; }
			set { this.tibcoMessage.CorrelationID = value; }
		}

		/// <summary>
		/// The destination of the message
		/// </summary>
		public Apache.NMS.IDestination NMSDestination
		{
			get { return TibcoUtil.ToNMSDestination(this.tibcoMessage.Destination); }
		}

		protected TimeSpan timeToLive;

		/// <summary>
		/// The amount of time that this message is valid for.  null If this
		/// message does not expire.
		/// </summary>
		public TimeSpan NMSTimeToLive
		{
			get { return this.timeToLive; }
			set { this.timeToLive = value; }
		}
#if false
		{
			get
			{
				if(this.tibcoMessage.Expiration > 0)
				{
					return Apache.ActiveMQ.Util.DateUtils.ToDateTime(this.tibcoMessage.Expiration)
					       - DateTime.Now;
				}
				else
				{
					return new TimeSpan();
				}
			}

			set
			{
				this.tibcoMessage.Expiration =
						Apache.ActiveMQ.Util.DateUtils.ToJavaTime(DateTime.Now + value);
			}
		}
#endif

		/// <summary>
		/// The message ID which is set by the provider
		/// </summary>
		public string NMSMessageId
		{
			get { return this.tibcoMessage.MessageID; }
		}

		/// <summary>
		/// Whether or not this message is persistent
		/// </summary>
		public bool NMSPersistent
		{
			get { return TibcoUtil.ToPersistent(this.tibcoMessage.MsgDeliveryMode); }
			set { this.tibcoMessage.MsgDeliveryMode = TibcoUtil.ToMessageDeliveryMode(value); }
		}

		/// <summary>
		/// The Priority on this message
		/// </summary>
		public byte NMSPriority
		{
			get { return (byte) this.tibcoMessage.Priority; }
			set { this.tibcoMessage.Priority = value; }
		}

		/// <summary>
		/// Returns true if this message has been redelivered to this or another consumer before being acknowledged successfully.
		/// </summary>
		public bool NMSRedelivered
		{
			get { return this.tibcoMessage.Redelivered; }
		}

		/// <summary>
		/// The destination that the consumer of this message should send replies to
		/// </summary>
		public Apache.NMS.IDestination NMSReplyTo
		{
			get { return TibcoUtil.ToNMSDestination(this.tibcoMessage.ReplyTo); }
			set { this.tibcoMessage.ReplyTo = ((Apache.TibcoEMS.Destination) value).tibcoDestination; }
		}

		/// <summary>
		/// The timestamp of when the message was pubished in UTC time.  If the publisher disables setting 
		/// the timestamp on the message, the time will be set to the start of the UNIX epoch (1970-01-01 00:00:00).
		/// </summary>
		public DateTime NMSTimestamp
		{
			get { return TibcoUtil.ToDateTime(this.tibcoMessage.Timestamp); }
		}

		/// <summary>
		/// The type name of this message
		/// </summary>
		public string NMSType
		{
			get { return this.tibcoMessage.MsgType; }
			set { this.tibcoMessage.MsgType = value; }
		}

		#endregion
	}
}
