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
	class MessageConsumer : Apache.NMS.IMessageConsumer, TIBCO.EMS.IMessageListener
	{
		public TIBCO.EMS.MessageConsumer tibcoMessageConsumer;

		public MessageConsumer(TIBCO.EMS.MessageConsumer consumer)
		{
			this.tibcoMessageConsumer = consumer;
		}

		#region IMessageConsumer Members

		/// <summary>
		/// Waits until a message is available and returns it
		/// </summary>
		public Apache.NMS.IMessage Receive()
		{
			return TibcoUtil.ToNMSMessage(this.tibcoMessageConsumer.Receive());
		}

		/// <summary>
		/// If a message is available within the timeout duration it is returned otherwise this method returns null
		/// </summary>
		public Apache.NMS.IMessage Receive(TimeSpan timeout)
		{
			return TibcoUtil.ToNMSMessage(this.tibcoMessageConsumer.Receive((long) timeout.TotalMilliseconds));
		}

		/// <summary>
		/// If a message is available immediately it is returned otherwise this method returns null
		/// </summary>
		public Apache.NMS.IMessage ReceiveNoWait()
		{
			return TibcoUtil.ToNMSMessage(this.tibcoMessageConsumer.ReceiveNoWait());
		}

		/// <summary>
		/// An asynchronous listener which can be used to consume messages asynchronously
		/// </summary>
		private event Apache.NMS.MessageListener listener;
		public event Apache.NMS.MessageListener Listener
		{
			add
			{
				listener += value;
				this.tibcoMessageConsumer.MessageListener = this;
			}

			remove
			{
				listener -= value;
				if(null == listener)
				{
					this.tibcoMessageConsumer.MessageListener = null;
				}
			}
		}

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
			this.tibcoMessageConsumer.Close();
		}

		#endregion

		#region IDisposable Members

		///<summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		///</summary>
		///<filterpriority>2</filterpriority>
		public void Dispose()
		{
			Close();
		}

		#endregion

		#region IMessageListener Members

		public void OnMessage(TIBCO.EMS.Message message)
		{
			HandleMessage(TibcoUtil.ToNMSMessage(message));
		}

		#endregion

		public void HandleMessage(Apache.NMS.IMessage message)
		{
			if(null != message)
			{
				if(listener != null)
				{
					listener(message);
				}
				else
				{
					Apache.ActiveMQ.Tracer.Info("Unhandled message");
				}
			}
		}
	}
}
