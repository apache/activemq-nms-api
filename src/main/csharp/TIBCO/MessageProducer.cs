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
	class MessageProducer : Apache.NMS.IMessageProducer
	{
		protected readonly Apache.TibcoEMS.Session nmsSession;
		public TIBCO.EMS.MessageProducer tibcoMessageProducer;
		private bool closed = false;
		private bool disposed = false;

		public MessageProducer(Apache.TibcoEMS.Session session, TIBCO.EMS.MessageProducer producer)
		{
			this.nmsSession = session;
			this.tibcoMessageProducer = producer;
		}

		~MessageProducer()
		{
			Dispose(false);
		}

		#region IMessageProducer Members

		/// <summary>
		/// Sends the message to the default destination for this producer
		/// </summary>
		public void Send(Apache.NMS.IMessage message)
		{
			Apache.TibcoEMS.Message msg = (Apache.TibcoEMS.Message) message;
			long timeToLive = (long) message.NMSTimeToLive.TotalMilliseconds;

			if(0 == timeToLive)
			{
				timeToLive = this.tibcoMessageProducer.TimeToLive;
			}

			this.tibcoMessageProducer.Send(
						msg.tibcoMessage,
						this.tibcoMessageProducer.MsgDeliveryMode,
						this.tibcoMessageProducer.Priority,
						timeToLive);
		}

		/// <summary>
		/// Sends the message to the default destination with the explicit QoS configuration
		/// </summary>
		public void Send(Apache.NMS.IMessage message, bool persistent, byte priority, TimeSpan timeToLive)
		{
			Apache.TibcoEMS.Message msg = (Apache.TibcoEMS.Message) message;

			this.tibcoMessageProducer.Send(
						msg.tibcoMessage,
						TibcoUtil.ToMessageDeliveryMode(persistent),
						priority,
						(long) timeToLive.TotalMilliseconds);
		}

		/// <summary>
		/// Sends the message to the given destination
		/// </summary>
		public void Send(Apache.NMS.IDestination destination, Apache.NMS.IMessage message)
		{
			Apache.TibcoEMS.Destination dest = (Apache.TibcoEMS.Destination) destination;
			Apache.TibcoEMS.Message msg = (Apache.TibcoEMS.Message) message;
			long timeToLive = (long) message.NMSTimeToLive.TotalMilliseconds;

			if(0 == timeToLive)
			{
				timeToLive = this.tibcoMessageProducer.TimeToLive;
			}

			this.tibcoMessageProducer.Send(
						dest.tibcoDestination,
						msg.tibcoMessage,
						this.tibcoMessageProducer.MsgDeliveryMode,
						this.tibcoMessageProducer.Priority,
						timeToLive);
		}

		/// <summary>
		/// Sends the message to the given destination with the explicit QoS configuration
		/// </summary>
		public void Send(Apache.NMS.IDestination destination, Apache.NMS.IMessage message,
						bool persistent, byte priority, TimeSpan timeToLive)
		{
			Apache.TibcoEMS.Destination dest = (Apache.TibcoEMS.Destination) destination;
			Apache.TibcoEMS.Message msg = (Apache.TibcoEMS.Message) message;

			this.tibcoMessageProducer.Send(
						dest.tibcoDestination,
						msg.tibcoMessage,
						TibcoUtil.ToMessageDeliveryMode(persistent),
						priority,
						(long) timeToLive.TotalMilliseconds);
		}

		public bool Persistent
		{
			get { return TibcoUtil.ToPersistent(this.tibcoMessageProducer.MsgDeliveryMode); }
			set { this.tibcoMessageProducer.MsgDeliveryMode = TibcoUtil.ToMessageDeliveryMode(value); }
		}

		public TimeSpan TimeToLive
		{
			get { return TimeSpan.FromMilliseconds(this.tibcoMessageProducer.TimeToLive); }
			set { this.tibcoMessageProducer.TimeToLive = (long) value.TotalMilliseconds; }
		}

		public byte Priority
		{
			get { return (byte) this.tibcoMessageProducer.Priority; }
			set { this.tibcoMessageProducer.Priority = value; }
		}

		public bool DisableMessageID
		{
			get { return this.tibcoMessageProducer.DisableMessageID; }
			set { this.tibcoMessageProducer.DisableMessageID = value; }
		}

		public bool DisableMessageTimestamp
		{
			get { return this.tibcoMessageProducer.DisableMessageTimestamp; }
			set { this.tibcoMessageProducer.DisableMessageTimestamp = value; }
		}

		/// <summary>
		/// Creates a new message with an empty body
		/// </summary>
		public Apache.NMS.IMessage CreateMessage()
		{
			return this.nmsSession.CreateMessage();
		}

		/// <summary>
		/// Creates a new text message with an empty body
		/// </summary>
		public Apache.NMS.ITextMessage CreateTextMessage()
		{
			return this.nmsSession.CreateTextMessage();
		}

		/// <summary>
		/// Creates a new text message with the given body
		/// </summary>
		public Apache.NMS.ITextMessage CreateTextMessage(string text)
		{
			return this.nmsSession.CreateTextMessage(text);
		}

		/// <summary>
		/// Creates a new Map message which contains primitive key and value pairs
		/// </summary>
		public Apache.NMS.IMapMessage CreateMapMessage()
		{
			return this.nmsSession.CreateMapMessage();
		}

		/// <summary>
		/// Creates a new Object message containing the given .NET object as the body
		/// </summary>
		public Apache.NMS.IObjectMessage CreateObjectMessage(object body)
		{
			return this.nmsSession.CreateObjectMessage(body);
		}

		/// <summary>
		/// Creates a new binary message
		/// </summary>
		public Apache.NMS.IBytesMessage CreateBytesMessage()
		{
			return this.nmsSession.CreateBytesMessage();
		}

		/// <summary>
		/// Creates a new binary message with the given body
		/// </summary>
		public Apache.NMS.IBytesMessage CreateBytesMessage(byte[] body)
		{
			return this.nmsSession.CreateBytesMessage(body);
		}

		#endregion

		#region IDisposable Members

		public void Close()
		{
			lock(this)
			{
				if(closed)
				{
					return;
				}

				if(!this.nmsSession.tibcoSession.IsClosed)
				{
					this.tibcoMessageProducer.Close();
				}

				closed = true;
			}
		}
		///<summary>
		///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		///</summary>
		///<filterpriority>2</filterpriority>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing)
		{
			if(disposed)
			{
				return;
			}

			if(disposing)
			{
				// Dispose managed code here.
			}

			try
			{
				Close();
			}
			catch
			{
				// Ignore errors.
			}

			disposed = true;
		}

		#endregion
	}
}
