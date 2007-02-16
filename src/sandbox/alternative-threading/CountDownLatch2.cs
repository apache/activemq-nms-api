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
using System.Threading;

namespace ActiveMQ.Util
{
	/// <summary>
	/// An alternative implementation using the EventSemaphore class
	/// <summary>
    class CountDownLatch2
    {
		readonly EventSemaphore mutex = new EventSemaphore();
        int remaining;
        
        public CountDownLatch2(int i)
        {
            remaining=i;
        }

        public void countDown()
        {
            lock(mutex)
            {
                if( remaining > 0 ) 
				{
                    remaining--;
                    if( remaining <= 0 )
                    {
	                    mutex.PulseAll();
					}
                }
            }	
        }

        public int Remaining
        {
            get { 
                lock(mutex)
                {
                    return remaining;
                }            
            }
        }
        
		/// <summary>
		/// Waits forever for the latch to be completed
		/// <summary>
        public bool await()
        {
            lock (mutex)
            {
				TimeSpan elapsed = new TimeSpan(0, 0, 5);
                while (remaining > 0)
                {
                    mutex.Wait(elapsed);
                }
            }
			return true;
        }
        
		/// <summary>
		/// Waits the specified amount of time for the latch
		/// returning true if the latch was acquired
		/// <summary>
        public bool await(TimeSpan timeout)
        {
			DateTime end = DateTime.Now.Add(timeout);
            lock (mutex)
            {
                while (remaining > 0)
                {
					TimeSpan elapsed = end.Subtract(DateTime.Now);
					if (elapsed.Milliseconds < 0) {
						break;
					}
					Console.WriteLine("About to wait on semaphore for: " + elapsed);
					
                    mutex.Wait(elapsed);
                }
	            return remaining > 0;
            }
        }

        
		/// <summary>
		/// Waits the specified amount of time for the latch
		/// returning true if the latch was acquired
		/// <summary>
        public bool await(int millis)
        {
			long ticks = millis * (1000000 / 100); // 1,000,000 nanos in a millisecond
			TimeSpan span = new TimeSpan(ticks);
			return await(span);
		}
    }
}
