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
using Apache.NMS.Util;
using NUnit.Framework;
using NUnit.Framework.Extensions;
using System.Threading;

namespace Apache.NMS.Test
{
	[TestFixture]
	[Explicit]
	public class MessageSelectorTest : NMSTestSupport
	{
		protected const string QUEUE_DESTINATION_NAME = "queue://MessageSelectorQueue";
		protected const string TOPIC_DESTINATION_NAME = "topic://MessageSelectorTopic";
		protected const string TEST_CLIENT_ID = "MessageSelectorClientId";
		protected const string TEST_CLIENT_ID2 = "MessageSelectorClientId2";

		private int receivedNonIgnoredMsgCount = 0;

#if !NET_1_1
		[RowTest]
		[Row(true, QUEUE_DESTINATION_NAME)]
		[Row(false, QUEUE_DESTINATION_NAME)]
		[Row(true, TOPIC_DESTINATION_NAME)]
		[Row(false, TOPIC_DESTINATION_NAME)]
#endif
		public void FilterIgnoredMessagesTest(bool persistent, string destinationName)
		{
			using(IConnection connection1 = CreateConnection(TEST_CLIENT_ID))
			using(IConnection connection2 = CreateConnection(TEST_CLIENT_ID2))
			{
				connection1.Start();
				connection2.Start();
				using(ISession session1 = connection1.CreateSession(AcknowledgementMode.AutoAcknowledge))
				using(ISession session2 = connection2.CreateSession(AcknowledgementMode.AutoAcknowledge))
				{
					IDestination destination1 = CreateDestination(session1, destinationName);
					IDestination destination2 = CreateDestination(session2, destinationName);

					using(IMessageProducer producer = session1.CreateProducer(destination1))
					using(IMessageConsumer consumer = session2.CreateConsumer(destination2, "JMSType NOT LIKE '%IGNORE'"))
					{
						const int MaxNumRequests = 100000;
						int numNonIgnoredMsgsSent = 0;

						producer.Persistent = persistent;
						// producer.RequestTimeout = receiveTimeout;

						receivedNonIgnoredMsgCount = 0;
						consumer.Listener += new MessageListener(OnNonIgnoredMessage);

						for(int index = 1; index <= MaxNumRequests; index++)
						{
							IMessage request = session1.CreateTextMessage(String.Format("Hello World! [{0} of {1}]", index, MaxNumRequests));

							//request.NMSTimeToLive = TimeSpan.FromSeconds(10);
							if(0 == (index % 2))
							{
								request.NMSType = "ACTIVE";
								numNonIgnoredMsgsSent++;
							}
							else
							{
								request.NMSType = "ACTIVE.IGNORE";
							}

							producer.Send(request);
						}

						while(receivedNonIgnoredMsgCount < numNonIgnoredMsgsSent)
						{
							Console.WriteLine("Waiting to receive all non-ignored messages...");
							Thread.Sleep(1000);
						}
					}
				}
			}
		}

		protected void OnNonIgnoredMessage(IMessage message)
		{
			receivedNonIgnoredMsgCount++;
			Assert.AreEqual(message.NMSType, "ACTIVE");
		}
	}
}
