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
using Apache.NMS.Util;
using NUnit.Framework;
using NUnit.Framework.Extensions;

namespace Apache.NMS.Test
{
	[TestFixture]
	public class ConsumerTest : NMSTestSupport
	{
		protected static string TEST_CLIENT_ID = "TestConsumerClientId";

// The .NET CF does not have the ability to interrupt threads, so this test is impossible.
#if !NETCF
		[RowTest]
		[Row(AcknowledgementMode.AutoAcknowledge)]
		[Row(AcknowledgementMode.ClientAcknowledge)]
		[Row(AcknowledgementMode.DupsOkAcknowledge)]
		[Row(AcknowledgementMode.Transactional)]
		public void TestNoTimeoutConsumer(AcknowledgementMode ackMode)
		{
			// Launch a thread to perform IMessageConsumer.Receive().
			// If it doesn't fail in less than three seconds, no exception was thrown.
			Thread receiveThread = new Thread(new ThreadStart(TimeoutConsumerThreadProc));
			using(IConnection connection = CreateConnection(TEST_CLIENT_ID))
			{
				connection.Start();
				using(ISession session = connection.CreateSession(ackMode))
				{
					ITemporaryQueue queue = session.CreateTemporaryQueue();
					using(this.timeoutConsumer = session.CreateConsumer(queue))
					{
						receiveThread.Start();
						if(receiveThread.Join(3000))
						{
							Assert.Fail("IMessageConsumer.Receive() returned without blocking.  Test failed.");
						}
						else
						{
							// Kill the thread - otherwise it'll sit in Receive() until a message arrives.
							receiveThread.Interrupt();
						}
					}
				}
			}
		}

		protected IMessageConsumer timeoutConsumer;
		
		protected void TimeoutConsumerThreadProc()
		{
			try
			{
				timeoutConsumer.Receive();
			}
			catch(ArgumentOutOfRangeException e)
			{
				// The test failed.  We will know because the timeout will expire inside TestNoTimeoutConsumer().
				Assert.Fail("Test failed with exception: " + e.Message);
			}
			catch(ThreadInterruptedException)
			{
				// The test succeeded!  We were still blocked when we were interrupted.
			}
			catch(Exception e)
			{
				// Some other exception occurred.
				Assert.Fail("Test failed with exception: " + e.Message);
			}
		}

		[RowTest]
		[Row(AcknowledgementMode.AutoAcknowledge)]
		[Row(AcknowledgementMode.ClientAcknowledge)]
		[Row(AcknowledgementMode.DupsOkAcknowledge)]
		[Row(AcknowledgementMode.Transactional)]
		public void TestSyncReceiveConsumerClose(AcknowledgementMode ackMode)
		{
			// Launch a thread to perform IMessageConsumer.Receive().
			// If it doesn't fail in less than three seconds, no exception was thrown.
			Thread receiveThread = new Thread(new ThreadStart(TimeoutConsumerThreadProc));
			using (IConnection connection = CreateConnection(TEST_CLIENT_ID))
			{
				connection.Start();
				using (ISession session = connection.CreateSession(ackMode))
				{
					ITemporaryQueue queue = session.CreateTemporaryQueue();
					using (this.timeoutConsumer = session.CreateConsumer(queue))
					{
						receiveThread.Start();
						if (receiveThread.Join(3000))
						{
							Assert.Fail("IMessageConsumer.Receive() returned without blocking.  Test failed.");
						}
						else
						{
							// Kill the thread - otherwise it'll sit in Receive() until a message arrives.
							this.timeoutConsumer.Close();
							receiveThread.Join(10000);
							if (receiveThread.IsAlive)
							{
								// Kill the thread - otherwise it'll sit in Receive() until a message arrives.
								receiveThread.Interrupt();
								Assert.Fail("IMessageConsumer.Receive() thread is still alive, close should have killed it.");
							}
						}
					}
				}
			}
		}
#endif

	}
}
