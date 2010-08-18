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

namespace Apache.NMS.Test
{
	[TestFixture]
	public class MessageSelectorTest : NMSTestSupport
	{
		protected const string QUEUE_DESTINATION_NAME = "queue://MessageSelectorQueue";
		protected const string TOPIC_DESTINATION_NAME = "topic://MessageSelectorTopic";
		protected const string TEST_CLIENT_ID = "MessageSelectorClientId";
		protected const string TEST_CLIENT_ID2 = "MessageSelectorClientId2";
		protected const string TEST_CLIENT_ID3 = "MessageSelectorClientId3";

		private int receivedNonIgnoredMsgCount = 0;
		private int receivedIgnoredMsgCount = 0;

		[Test]
		public void FilterIgnoredMessagesTest(
			[Values(QUEUE_DESTINATION_NAME, TOPIC_DESTINATION_NAME)]
			string destinationName)
		{
			TimeSpan ttl = TimeSpan.FromMinutes(30);
			const int MaxNumRequests = 100000;

			using(IConnection connection1 = CreateConnection(TEST_CLIENT_ID))
			using(IConnection connection2 = CreateConnection(TEST_CLIENT_ID2))
			using(IConnection connection3 = CreateConnection(TEST_CLIENT_ID3))
			{
				connection1.Start();
				connection2.Start();
				connection3.Start();
				using(ISession session1 = connection1.CreateSession(AcknowledgementMode.AutoAcknowledge))
				using(ISession session2 = connection2.CreateSession(AcknowledgementMode.AutoAcknowledge))
				using(ISession session3 = connection3.CreateSession(AcknowledgementMode.AutoAcknowledge))
				{
					IDestination destination1 = CreateDestination(session1, destinationName);
					IDestination destination2 = SessionUtil.GetDestination(session2, destinationName);
					IDestination destination3 = SessionUtil.GetDestination(session3, destinationName); //jdg + "?consumer.prefetchSize=10000");

					using(IMessageProducer producer = session1.CreateProducer(destination1))
					using(IMessageConsumer consumer1 = session2.CreateConsumer(destination2, "JMSType NOT LIKE '%IGNORE'"))
					{
						int numNonIgnoredMsgsSent = 0;
						int numIgnoredMsgsSent = 0;

						producer.DeliveryMode = MsgDeliveryMode.NonPersistent;

						receivedNonIgnoredMsgCount = 0;
						receivedIgnoredMsgCount = 0;
						consumer1.Listener += new MessageListener(OnNonIgnoredMessage);
						IMessageConsumer consumer2 = null;

						for(int index = 1; index <= MaxNumRequests; index++)
						{
							IMessage request = session1.CreateTextMessage(String.Format("Hello World! [{0} of {1}]", index, MaxNumRequests));

							request.NMSTimeToLive = ttl;
							if(0 == (index % 2))
							{
								request.NMSType = "ACTIVE";
								numNonIgnoredMsgsSent++;
							}
							else
							{
								request.NMSType = "ACTIVE.IGNORE";
								numIgnoredMsgsSent++;
							}

							producer.Send(request);

							if(20000 == index)
							{
								// Start the second consumer
								if(destination3.IsTopic)
								{
									// Reset the ignored message sent count, since all previous messages
									// will not have been consumed on a topic.
									numIgnoredMsgsSent = 0;
								}

								consumer2 = session3.CreateConsumer(destination3, "JMSType LIKE '%IGNORE'");
								consumer2.Listener += new MessageListener(OnIgnoredMessage);
							}
						}

						int waitCount = 0;
						int lastReceivedINongnoredMsgCount = receivedNonIgnoredMsgCount;
						int lastReceivedIgnoredMsgCount = receivedIgnoredMsgCount;

						while(receivedNonIgnoredMsgCount < numNonIgnoredMsgsSent
								|| receivedIgnoredMsgCount < numIgnoredMsgsSent)
						{
							if(lastReceivedINongnoredMsgCount != receivedNonIgnoredMsgCount
								|| lastReceivedIgnoredMsgCount != receivedIgnoredMsgCount)
							{
								// Reset the wait count.
								waitCount = 0;
							}
							else
							{
								waitCount++;
							}

							lastReceivedINongnoredMsgCount = receivedNonIgnoredMsgCount;
							lastReceivedIgnoredMsgCount = receivedIgnoredMsgCount;

							Assert.IsTrue(waitCount <= 30, String.Format("Timeout waiting for all messages to be delivered. Only {0} of {1} non-ignored messages delivered.  Only {2} of {3} ignored messages delivered.",
								receivedNonIgnoredMsgCount, numNonIgnoredMsgsSent, receivedIgnoredMsgCount, numIgnoredMsgsSent));
							//Console.WriteLine("Waiting ({0}) to receive all non-ignored messages...", waitCount);
							Thread.Sleep(1000);
						}

						consumer2.Dispose();
					}
				}
			}
		}

		protected void OnNonIgnoredMessage(IMessage message)
		{
			receivedNonIgnoredMsgCount++;
			Assert.AreEqual(message.NMSType, "ACTIVE");
		}

		protected void OnIgnoredMessage(IMessage message)
		{
			receivedIgnoredMsgCount++;
			Assert.AreEqual(message.NMSType, "ACTIVE.IGNORE");
			// Simulate a slow consumer
			//Thread.Sleep(10);
		}
	}
}
