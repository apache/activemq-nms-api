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
using ActiveMQ.Commands;
using NMS;
using System;
using System.Collections;
using System.Threading;

namespace ActiveMQ
{
	
	/// <summary>
	/// Handles the multi-threaded dispatching between the transport and the consumers
	/// </summary>
	public class Dispatcher
    {
        Queue queue = new Queue();
        Object semaphore = new Object();
        ArrayList messagesToRedeliver = new ArrayList();
        readonly AutoResetEvent resetEvent = new AutoResetEvent(false);
		
        /// <summary>
        /// Whem we start a transaction we must redeliver any rolled back messages
        /// </summary>
        public void RedeliverRolledBackMessages()
		{
            lock (semaphore)
            {
                Queue replacement = new Queue(queue.Count + messagesToRedeliver.Count);
                foreach (ActiveMQMessage element in messagesToRedeliver)
				{
                    replacement.Enqueue(element);
                }
                messagesToRedeliver.Clear();
                
                while (queue.Count > 0)
                {
                    ActiveMQMessage element = (ActiveMQMessage) queue.Dequeue();
                    replacement.Enqueue(element);
                }
                queue = replacement;
                if (queue.Count > 0)
                    resetEvent.Set();
            }
        }
        
        /// <summary>
        /// Redeliver the given message, putting it at the head of the queue
        /// </summary>
        public void Redeliver(ActiveMQMessage message)
        {
            lock (semaphore)
			{
				messagesToRedeliver.Add(message);
            }
        }
        
        /// <summary>
        /// Method Enqueue
        /// </summary>
        public void Enqueue(ActiveMQMessage message)
        {
            lock (semaphore)
            {
                queue.Enqueue(message);
                resetEvent.Set();
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
                if (queue.Count > 0)
                {
                    rc = (IMessage) queue.Dequeue();
                    if (queue.Count > 0)
                    {
                        resetEvent.Set();
                    }
                } 
            }
            return rc;
        }

        /// <summary>
        /// Method Dequeue
        /// </summary>
        public IMessage Dequeue(TimeSpan timeout)
        {
            IMessage rc = DequeueNoWait();
            while (rc == null)
            {
                if( !resetEvent.WaitOne((int)timeout.TotalMilliseconds, false) )
                {
                    break;
                }
                rc = DequeueNoWait();
            }
            return rc;
        }
        
        /// <summary>
        /// Method Dequeue
        /// </summary>
        public IMessage Dequeue()
        {
            IMessage rc = DequeueNoWait();
            while (rc == null)
            {
                if (!resetEvent.WaitOne(-1, false))
                {
                    break;
                }
                rc = DequeueNoWait();
            }
            return rc;
        }
        
    }
}

