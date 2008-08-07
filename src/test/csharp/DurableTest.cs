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
using Apache.NMS.Util;

namespace Apache.NMS.Test
{
	[TestFixture]
	public abstract class DurableTest : NMSTestSupport
	{
		protected static string TOPIC = "TestTopicDurableConsumer";
		protected static string SEND_CLIENT_ID = "SendDurableTestClientId";
		protected static string TEST_CLIENT_ID = "DurableTestClientId";
		protected static string CONSUMER_ID = "DurableTestConsumerId";
		protected static string DURABLE_SELECTOR = "2 > 1";

		protected void SendPersistentMessage()
		{
			using(IConnection connection = CreateConnection(SEND_CLIENT_ID))
			{
				connection.Start();
				using(ISession session = connection.CreateSession(AcknowledgementMode.DupsOkAcknowledge))
				{
					ITopic topic = SessionUtil.GetTopic(session, TOPIC);
					using(IMessageProducer producer = session.CreateProducer(topic, receiveTimeout))
					{
						ITextMessage message = session.CreateTextMessage("Persistent Hello");

						producer.Persistent = true;
						producer.RequestTimeout = receiveTimeout;
						producer.Send(message);
					}
				}
			}
		}

		[Test]
		public void TestDurableConsumer()
		{
			try
			{
				RegisterDurableConsumer(TEST_CLIENT_ID, TOPIC, CONSUMER_ID, DURABLE_SELECTOR, false);
				SendPersistentMessage();

				using(IConnection connection = CreateConnection(TEST_CLIENT_ID))
				{
					connection.Start();
					using(ISession session = connection.CreateSession(AcknowledgementMode.DupsOkAcknowledge))
					{
						ITopic topic = SessionUtil.GetTopic(session, TOPIC);
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
				UnregisterDurableConsumer(TEST_CLIENT_ID, CONSUMER_ID);
			}
		}

		[Test]
		public void TestDurableConsumerTransactional()
		{
			try
			{
				RegisterDurableConsumer(TEST_CLIENT_ID, TOPIC, CONSUMER_ID, DURABLE_SELECTOR, false);
				RunTestDurableConsumerTransactional();
				RunTestDurableConsumerTransactional();
			}
			finally
			{
				UnregisterDurableConsumer(TEST_CLIENT_ID, CONSUMER_ID);
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
					ITopic topic = SessionUtil.GetTopic(session, TOPIC);
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
