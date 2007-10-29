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
using Apache.ActiveMQ;
using Apache.ActiveMQ.Util;
using Apache.NMS;
using System;
using System.Messaging;
using System.Threading;

namespace Apache.MSMQ
{
    /// <summary>
    /// An object capable of receiving messages from some destination
    /// </summary>
    public class MessageConsumer : IMessageConsumer
    {
		protected TimeSpan zeroTimeout = new TimeSpan(0);
		
        private readonly Session session;
        private readonly AcknowledgementMode acknowledgementMode;
		private MessageQueue messageQueue;
		private event MessageListener listener;
		private AtomicBoolean asyncDelivery = new AtomicBoolean(false);
		
        public MessageConsumer(Session session, AcknowledgementMode acknowledgementMode, MessageQueue messageQueue)
        {
            this.session = session;
            this.acknowledgementMode = acknowledgementMode;
			this.messageQueue = messageQueue;
        }
        
        public event MessageListener Listener
        {
			add {
				listener += value;
				StartAsyncDelivery();
			}
			remove {
				listener -= value;
			}
        }
		
        public IMessage Receive()
        {
			Message message = messageQueue.Receive();
			return ToNmsMessage(message);
        }

        public IMessage Receive(TimeSpan timeout)
        {
			Message message = messageQueue.Receive(timeout);
			return ToNmsMessage(message);
        }

        public IMessage ReceiveNoWait()
        {
			Message message = messageQueue.Receive(zeroTimeout);
			return ToNmsMessage(message);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
			StopmAsyncDelivery();
            Dispose();
        }
		
		public void StopmAsyncDelivery()
		{
			asyncDelivery.Value = true;
		}

		protected virtual void StartAsyncDelivery()
		{
			if (asyncDelivery.CompareAndSet(false, true)) {
				Thread thread = new Thread(DispatchLoop);
				thread.Start();
			}
		}
		
		protected virtual void DispatchLoop()
		{
			Tracer.Info("Starting dispatcher thread consumer: " + this);
			while (asyncDelivery.Value)
			{
				IMessage message = Receive();
				if (message != null)
				{
					try
					{
						listener(message);
					}
					catch (Exception e)
					{
						HandleAsyncException(e);
					}
				}
			}
			Tracer.Info("Stopping dispatcher thread consumer: " + this);
		}

		protected virtual void HandleAsyncException(Exception e)
		{
			session.Connection.HandleException(e);
		}
		
		protected virtual IMessage ToNmsMessage(Message message)
		{
			if (message == null)
			{
				return null;
			}
			return session.MessageConverter.ToNmsMessage(message);
		}
    }
}
