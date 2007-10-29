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
	class MessageConsumer : Apache.NMS.IMessageConsumer
	{
		private readonly Apache.TibcoEMS.Dispatcher dispatcher = new Apache.TibcoEMS.Dispatcher();
		protected readonly Apache.TibcoEMS.Session nmsSession;
		public TIBCO.EMS.MessageConsumer tibcoMessageConsumer;
		private bool closed = false;
		private bool disposed = false;

		public MessageConsumer(Apache.TibcoEMS.Session session, TIBCO.EMS.MessageConsumer consumer)
		{
			this.nmsSession = session;
			this.tibcoMessageConsumer = consumer;
			this.tibcoMessageConsumer.MessageHandler += this.HandleTibcoMsg;
		}

		~MessageConsumer()
		{
			Dispose(false);
		}

		#region IMessageConsumer Members

		/// <summary>
		/// Waits until a message is available and returns it
		/// </summary>
		public Apache.NMS.IMessage Receive()
		{
			return this.dispatcher.Dequeue();
		}

		/// <summary>
		/// If a message is available within the timeout duration it is returned otherwise this method returns null
		/// </summary>
		public Apache.NMS.IMessage Receive(TimeSpan timeout)
		{
			return this.dispatcher.Dequeue(timeout);
		}

		/// <summary>
		/// If a message is available immediately it is returned otherwise this method returns null
		/// </summary>
		public Apache.NMS.IMessage ReceiveNoWait()
		{
			return this.dispatcher.DequeueNoWait();
		}

		/// <summary>
		/// An asynchronous listener which can be used to consume messages asynchronously
		/// </summary>
		public event Apache.NMS.MessageListener Listener;

		/// <summary>
		/// Closes the message consumer. 
		/// </summary>
		/// <remarks>
		/// Clients should close message consumers them when they are not needed.
		/// This call blocks until a receive or message listener in progress has completed.
		/// A blocked message consumer receive call returns null when this message consumer is closed.
		/// </remarks>
		public void Close()
		{
			lock(this)
			{
				if(closed)
				{
					return;
				}
			}

			// wake up any pending dequeue() call on the dispatcher
			this.dispatcher.Close();

			lock(this)
			{
				if(!this.nmsSession.tibcoSession.IsClosed)
				{
					this.tibcoMessageConsumer.MessageHandler -= this.HandleTibcoMsg;
					this.tibcoMessageConsumer.Close();
				}

				closed = true;
			}
		}

		#endregion

		#region IDisposable Members

		///<summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
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

		private void HandleTibcoMsg(object sender, TIBCO.EMS.EMSMessageEventArgs arg)
		{
			Apache.NMS.IMessage message = TibcoUtil.ToNMSMessage(arg.Message);

			if(null != message)
			{
				if(Listener != null)
				{
					try
					{
						Listener(message);
					}
					catch(Exception ex)
					{
						Apache.NMS.Tracer.Debug("Error handling message: " + ex.Message);
					}
				}
				else
				{
					this.dispatcher.Enqueue(message);
				}
			}
		}
	}
}
