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
using NUnit.Framework;

namespace Apache.NMS.Test
{
	[TestFixture]
	public abstract class DurableTest : NMSTestSupport
	{
		private static string TOPIC = "TestTopicDurableConsumer";
		private static string SEND_CLIENT_ID = "SendDurableTestClientId";
		private static string TEST_CLIENT_ID = "DurableTestClientId";
		private static string CONSUMER_ID = "DurableTestConsumerId";
		private static string DURABLE_SELECTOR = "2 > 1";

		protected void RegisterDurableConsumer()
		{
			using(IConnection connection = CreateConnection(TEST_CLIENT_ID))
			{
				connection.Start();
				using(ISession session = connection.CreateSession(AcknowledgementMode.DupsOkAcknowledge))
				{
					ITopic topic = session.GetTopic(TOPIC);
					using(IMessageConsumer consumer = session.CreateDurableConsumer(topic, CONSUMER_ID, DURABLE_SELECTOR, false))
					{
					}
				}
			}
		}

		protected void UnregisterDurableConsumer()
		{
			using(IConnection connection = CreateConnection(TEST_CLIENT_ID))
			{
				connection.Start();
				using(ISession session = connection.CreateSession(AcknowledgementMode.DupsOkAcknowledge))
				{
					session.DeleteDurableConsumer(CONSUMER_ID);
				}
			}
		}

		protected void SendPersistentMessage()
		{
			using(IConnection connection = CreateConnection(SEND_CLIENT_ID))
			{
				connection.Start();
				using(ISession session = connection.CreateSession(AcknowledgementMode.DupsOkAcknowledge))
				{
					ITopic topic = session.GetTopic(TOPIC);
					ITextMessage message = session.CreateTextMessage("Persistent Hello");
					using(IMessageProducer producer = session.CreateProducer())
					{
						producer.Send(topic, message, true, message.NMSPriority, receiveTimeout);
					}
				}
			}
		}

		[Test]
		public void TestDurableConsumer()
		{
			RegisterDurableConsumer();
			SendPersistentMessage();

			try
			{
				using(IConnection connection = CreateConnection(TEST_CLIENT_ID))
				{
					connection.Start();
					using(ISession session = connection.CreateSession(AcknowledgementMode.DupsOkAcknowledge))
					{
						ITopic topic = session.GetTopic(TOPIC);
						using(IMessageConsumer consumer = session.CreateDurableConsumer(topic, CONSUMER_ID, DURABLE_SELECTOR, false))
						{
							IMessage msg = consumer.Receive(receiveTimeout);
							Assert.IsNotNull(msg, "Did not receive first durable message.");
							SendPersistentMessage();

							msg = consumer.Receive(receiveTimeout);
							Assert.IsNotNull(msg, "Did not receive second durable message.");
						}
					}
				}
			}
			finally
			{
				UnregisterDurableConsumer();
			}
		}

		[Test]
		public void TestDurableConsumerTransactional()
		{
			RegisterDurableConsumer();
			try
			{
				RunTestDurableConsumerTransactional();
				RunTestDurableConsumerTransactional();
			}
			finally
			{
				UnregisterDurableConsumer();
			}
		}

		protected void RunTestDurableConsumerTransactional()
		{
			SendPersistentMessage();

			using(IConnection connection = CreateConnection(TEST_CLIENT_ID))
			{
				connection.Start();
				using(ISession session = connection.CreateSession(AcknowledgementMode.Transactional))
				{
					ITopic topic = session.GetTopic(TOPIC);
					using(IMessageConsumer consumer = session.CreateDurableConsumer(topic, CONSUMER_ID, DURABLE_SELECTOR, false))
					{
						IMessage msg = consumer.Receive(receiveTimeout);
						Assert.IsNotNull(msg, "Did not receive first durable transactional message.");
						SendPersistentMessage();

						msg = consumer.Receive(receiveTimeout);
						Assert.IsNotNull(msg, "Did not receive second durable transactional message.");
						session.Commit();
					}
				}
			}
		}
	}
}
