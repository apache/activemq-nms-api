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
using System.Collections;
using System.Threading;

namespace Apache.ActiveMQ
{
	/// <summary>
	/// Handles the multi-threaded dispatching between the transport and the consumers
	/// </summary>
	public class Dispatcher
    {
        Queue queue = new Queue();
		readonly Object semaphore = new Object();
		readonly ArrayList messagesToRedeliver = new ArrayList();
        
        // TODO can't use EventWaitHandle on MONO 1.0
        AutoResetEvent messageReceivedEventHandle = new AutoResetEvent(false);
        bool m_bAsyncDelivery = false;
        bool m_bClosed = false;

		public void SetAsyncDelivery(AutoResetEvent eventHandle)
		{
			lock (semaphore)
			{
				messageReceivedEventHandle = eventHandle;
				m_bAsyncDelivery = true;
				if (queue.Count > 0)
				{
					messageReceivedEventHandle.Set();
				}
			}
		}

        /// <summary>
        /// Whem we start a transaction we must redeliver any rolled back messages
        /// </summary>
        public void RedeliverRolledBackMessages()
		{
            lock (semaphore)
            {
                Queue replacement = new Queue(queue.Count + messagesToRedeliver.Count);
                foreach (IMessage element in messagesToRedeliver)
                {
                    replacement.Enqueue(element);
                }
                messagesToRedeliver.Clear();
                
                while (queue.Count > 0)
                {
                    IMessage element = (IMessage) queue.Dequeue();
                    replacement.Enqueue(element);
                }

				queue = replacement;
                if (queue.Count > 0)
                {
                	messageReceivedEventHandle.Set();
                }
            }
        }
        
        /// <summary>
        /// Redeliver the given message, putting it at the head of the queue
        /// </summary>
        public void Redeliver(IMessage message)
        {
            lock (semaphore)
			{
				messagesToRedeliver.Add(message);
            }
        }
        
        /// <summary>
        /// Method Enqueue
        /// </summary>
        public void Enqueue(IMessage message)
        {
            lock (semaphore)
            {
                queue.Enqueue(message);
                messageReceivedEventHandle.Set();
            }
        }
        
        /// <summary>
        /// Method DequeueNoWait
        /// </summary>
        public IMessage DequeueNoWait()
        {
            IMessage rc = null;
            lock (semaphore)
            {
                if (!m_bClosed && queue.Count > 0)
                {
                    rc = (IMessage) queue.Dequeue();
                } 
            }
            return rc;
        }

        /// <summary>
        /// Method Dequeue
        /// </summary>
        public IMessage Dequeue(TimeSpan timeout)
        {
            IMessage rc;
			bool bClosed = false;
			lock (semaphore)
			{
				bClosed = m_bClosed;
				rc = DequeueNoWait();
			}

            while (!bClosed && rc == null)
            {
                if( !messageReceivedEventHandle.WaitOne((int) timeout.TotalMilliseconds, false))
                {
                    break;
                }

				lock (semaphore)
				{
					rc = DequeueNoWait();
					bClosed = m_bClosed;
				}
            }
            return rc;
        }
        
        /// <summary>
        /// Method Dequeue
        /// </summary>
        public IMessage Dequeue()
        {
			TimeSpan indefiniteWait = TimeSpan.FromMilliseconds(Timeout.Infinite);
			return Dequeue(indefiniteWait);
        }

		public void Close()
		{
			lock (semaphore)
			{
				m_bClosed = true;
				queue.Clear();
				if(m_bAsyncDelivery)
				{
					messageReceivedEventHandle.Set();
				}
			}
		}
	}
}
