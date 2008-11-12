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
		protected static string TEST_CLIENT_ID = "ConsumerTestClientId";
		protected static string TOPIC = "TestTopicConsumerTest";
		protected static string CONSUMER_ID = "ConsumerTestConsumerId";

		[RowTest]
		[Row(true)]
		[Row(false)]
		public void TestDurableConsumerSelectorChange(bool persistent)
		{
			try
			{
				using(IConnection connection = CreateConnection(TEST_CLIENT_ID))
				{
					connection.Start();
					using(ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
					{
						ITopic topic = SessionUtil.GetTopic(session, TOPIC);
						IMessageProducer producer = session.CreateProducer(topic);
						IMessageConsumer consumer = session.CreateDurableConsumer(topic, CONSUMER_ID, "color='red'", false);

						producer.Persistent = persistent;

						// Send the messages
						ITextMessage sendMessage = session.CreateTextMessage("1st");
						sendMessage.Properties["color"] = "red";
						producer.Send(sendMessage);

						ITextMessage receiveMsg = consumer.Receive(receiveTimeout) as ITextMessage;
						Assert.IsNotNull(receiveMsg, "Failed to retrieve 1st durable message.");
						Assert.AreEqual("1st", receiveMsg.Text);
						Assert.AreEqual(persistent, receiveMsg.NMSPersistent, "NMSPersistent does not match");

						// Change the subscription.
						consumer.Dispose();
						consumer = session.CreateDurableConsumer(topic, CONSUMER_ID, "color='blue'", false);

						sendMessage = session.CreateTextMessage("2nd");
						sendMessage.Properties["color"] = "red";
						producer.Send(sendMessage);
						sendMessage = session.CreateTextMessage("3rd");
						sendMessage.Properties["color"] = "blue";
						producer.Send(sendMessage);

						// Selector should skip the 2nd message.
						receiveMsg = consumer.Receive(receiveTimeout) as ITextMessage;
						Assert.IsNotNull(receiveMsg, "Failed to retrieve durable message.");
						Assert.AreEqual("3rd", receiveMsg.Text, "Retrieved the wrong durable message.");
						Assert.AreEqual(persistent, receiveMsg.NMSPersistent, "NMSPersistent does not match");

						// Make sure there are no pending messages.
						Assert.IsNull(consumer.ReceiveNoWait(), "Wrong number of messages in durable subscription.");
					}
				}
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message);
			}
			finally
			{
				UnregisterDurableConsumer(TEST_CLIENT_ID, CONSUMER_ID);
			}
		}

		[Test]
		public void TestNoTimeoutConsumer()
		{
			// Launch a thread to perform IMessageConsumer.Receive().
			// If it doesn't fail in less than three seconds, no exception was thrown.
			Thread receiveThread = new Thread(new ThreadStart(TimeoutConsumerThreadProc));
			using(IConnection connection = CreateConnection(TEST_CLIENT_ID))
			{
				connection.Start();
				using(ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
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
	}
}
