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

namespace Apache.ActiveMQ.Util
{
    class CountDownLatch
    {
        readonly ManualResetEvent mutex = new ManualResetEvent(false);
        int remaining;
        
        public CountDownLatch(int i)
        {
            remaining=i;
        }

        public void countDown()
        {
            lock (mutex)
            {
                if( remaining > 0 )
                {
                    remaining--;
                    if( remaining == 0 )
                    {
                        mutex.Set();
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
        
        public bool await(int timeout)
        {
            return mutex.WaitOne(timeout, false);
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return mutex; }
        }
        
    }
}
