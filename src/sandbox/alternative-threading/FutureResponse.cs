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
using System;
using System.Threading;
using ActiveMQ.Util;

namespace ActiveMQ.Transport
{
	
	/// <summary>
	/// Handles asynchronous responses
	/// </summary>
	public class FutureResponse 
    {
	    
        private static int maxWait = -1;

        private readonly CountDownLatch latch = new CountDownLatch(1);
        private Response response;
        
/*        public WaitHandle AsyncWaitHandle
        {
            get { return latch.AsyncWaitHandle; }
        }        
*/        
        public Response Response
        {
            // Blocks the caller until a value has been set
            get {
                lock (latch)
                {
	                while (response == null)
	                {
	                    try
						{
							if (maxWait > 0) 
							{
		                        latch.await(maxWait);
							}
							else 
							{
		                        latch.await();
							}
	                    }
	                    catch (Exception e)
						{
	                        Tracer.Error("Caught while waiting on monitor: " + e);
	                    }
	                }
                    return response;
                }
            }
            
            set {
                lock (latch)
                {
                    response = value;
                }
                latch.countDown();
            }
        }
    }
}

