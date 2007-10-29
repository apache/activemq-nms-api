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
using Apache.ActiveMQ.Commands;
using Apache.ActiveMQ.Util;
using Apache.NMS;
using System;

namespace Apache.ActiveMQ
{
	/// <summary>
	/// An object capable of sending messages to some destination
	/// </summary>
	public class MessageProducer : IMessageProducer
	{
		private readonly Session session;
		private readonly ProducerInfo info;
		private long messageCounter = 0;

		private bool msgPersistent = NMSConstants.defaultPersistence;
		private TimeSpan msgTimeToLive;
		private readonly bool defaultSpecifiedTimeToLive = false;
		private byte msgPriority = NMSConstants.defaultPriority;
		private bool disableMessageID = false;
		private bool disableMessageTimestamp = false;
		protected bool disposed = false;

		public MessageProducer(Session session, ProducerInfo info)
		{
			this.session = session;
			this.info = info;
		}

		~MessageProducer()
		{
			Dispose(false);
		}

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
				session.Connection.DisposeOf(info.ProducerId);
			}
			catch
			{
				// Ignore network errors.
			}

			disposed = true;
		}

		public void Send(IMessage message)
		{
			Send(info.Destination, message);
		}

		public void Send(IDestination destination, IMessage message)
		{
			Send(destination, message, Persistent, Priority, TimeToLive, defaultSpecifiedTimeToLive);
		}

		public void Send(IMessage message, bool persistent, byte priority, TimeSpan timeToLive)
		{
			Send(info.Destination, message, persistent, priority, timeToLive);
		}

		public void Send(IDestination destination, IMessage message, bool persistent, byte priority, TimeSpan timeToLive)
		{
			Send(destination, message, persistent, priority, timeToLive, true);
		}

		protected void Send(IDestination destination, IMessage message, bool persistent, byte priority, TimeSpan timeToLive, bool specifiedTimeToLive)
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

			activeMessage.ProducerId = info.ProducerId;
			activeMessage.FromDestination = destination;
			activeMessage.NMSPersistent = persistent;
			activeMessage.NMSPriority = priority;

			if (session.Transacted)
			{
				session.DoStartTransaction();
				activeMessage.TransactionId = session.TransactionContext.TransactionId;
			}

			if (specifiedTimeToLive)
			{
				activeMessage.NMSTimeToLive = timeToLive;
			}

			if (!disableMessageTimestamp)
			{
				activeMessage.NMSTimestamp = DateTime.UtcNow;
			}

			session.DoSend(activeMessage);
		}

		public bool Persistent
		{
			get { return msgPersistent; }
			set { this.msgPersistent = value; }
		}

		public TimeSpan TimeToLive
		{
			get { return msgTimeToLive; }
			set { this.msgTimeToLive = value; }
		}

		public byte Priority
		{
			get { return msgPriority; }
			set { this.msgPriority = value; }
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

		public ITextMessage CreateTextMessage(string text)
		{
			return session.CreateTextMessage(text);
		}

		public IMapMessage CreateMapMessage()
		{
			return session.CreateMapMessage();
		}

		public IObjectMessage CreateObjectMessage(object body)
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
