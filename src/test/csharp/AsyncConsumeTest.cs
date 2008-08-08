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

using System.Threading;
using Apache.NMS.Util;
using NUnit.Framework;

namespace Apache.NMS.Test
{
	[TestFixture]
	public abstract class AsyncConsumeTest : NMSTestSupport
	{
		protected static string DESTINATION_NAME = "AsyncConsumeDestination";
		protected static string TEST_CLIENT_ID = "AsyncConsumeClientId";
		protected object semaphore = new object();
		protected bool received;
		protected IMessage receivedMsg;

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
			received = false;
			receivedMsg = null;
		}

		[Test]
		public void TestAsynchronousConsume()
		{
			doTestAsynchronousConsume(false);
		}

		[Test]
		public void TestAsynchronousConsumePersistent()
		{
			doTestAsynchronousConsume(true);
		}

		protected void doTestAsynchronousConsume(bool persistent)
		{
			using(IConnection connection = CreateConnection(TEST_CLIENT_ID))
			{
				connection.Start();
				using(ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
				{
					IDestination destination = SessionUtil.GetDestination(session, DESTINATION_NAME);
					using(IMessageConsumer consumer = session.CreateConsumer(destination, receiveTimeout))
					using(IMessageProducer producer = session.CreateProducer(destination, receiveTimeout))
					{
						producer.Persistent = persistent;
						producer.RequestTimeout = receiveTimeout;
						consumer.Listener += new MessageListener(OnMessage);

						IMessage request = session.CreateMessage();
						request.NMSCorrelationID = "AsyncConsume";
						request.NMSType = "Test";
						producer.Send(request);

						WaitForMessageToArrive();
						Assert.AreEqual(request.NMSCorrelationID, receivedMsg.NMSCorrelationID, "Invalid correlation ID.");
					}
				}
			}
		}

		[Test]
		public void TestCreateConsumerAfterSend()
		{
			doTestCreateConsumerAfterSend(false);
		}

		[Test]
		public void TestCreateConsumerAfterSendPersistent()
		{
			doTestCreateConsumerAfterSend(true);
		}

		protected void doTestCreateConsumerAfterSend(bool persistent)
		{
			using(IConnection connection = CreateConnection(TEST_CLIENT_ID))
			{
				connection.Start();
				using(ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
				{
					IDestination destination = SessionUtil.GetDestination(session, DESTINATION_NAME);
					using(IMessageProducer producer = session.CreateProducer(destination, receiveTimeout))
					{
						producer.Persistent = false;
						producer.RequestTimeout = receiveTimeout;

						IMessage request = session.CreateMessage();
						request.NMSCorrelationID = "AsyncConsumeAfterSend";
						request.NMSType = "Test";
						producer.Send(request);

						using(IMessageConsumer consumer = session.CreateConsumer(destination, receiveTimeout))
						{
							consumer.Listener += new MessageListener(OnMessage);
							WaitForMessageToArrive();
							Assert.AreEqual(request.NMSCorrelationID, receivedMsg.NMSCorrelationID, "Invalid correlation ID.");
						}
					}
				}
			}
		}

		[Test]
		public void TestCreateConsumerBeforeSendAddListenerAfterSend()
		{
			doTestCreateConsumerBeforeSendAddListenerAfterSend(false);
		}

		[Test]
		public void TestCreateConsumerBeforeSendAddListenerAfterSendPersistent()
		{
			doTestCreateConsumerBeforeSendAddListenerAfterSend(true);
		}

		public void doTestCreateConsumerBeforeSendAddListenerAfterSend(bool persistent)
		{
			using(IConnection connection = CreateConnection(TEST_CLIENT_ID))
			{
				connection.Start();
				using(ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
				{
					IDestination destination = SessionUtil.GetDestination(session, DESTINATION_NAME);
					using(IMessageConsumer consumer = session.CreateConsumer(destination, receiveTimeout))
					using(IMessageProducer producer = session.CreateProducer(destination, receiveTimeout))
					{
						producer.Persistent = persistent;
						producer.RequestTimeout = receiveTimeout;

						IMessage request = session.CreateMessage();
						request.NMSCorrelationID = "AsyncConsumeAfterSendLateListener";
						request.NMSType = "Test";
						producer.Send(request);

						// now lets add the listener
						consumer.Listener += new MessageListener(OnMessage);
						WaitForMessageToArrive();
						Assert.AreEqual(request.NMSCorrelationID, receivedMsg.NMSCorrelationID, "Invalid correlation ID.");
					}
				}
			}
		}

		[Test]
		public void TestAsynchronousTextMessageConsume()
		{
			doTestAsynchronousTextMessageConsume(false);
		}

		[Test]
		public void TestAsynchronousTextMessageConsumePersistent()
		{
			doTestAsynchronousTextMessageConsume(true);
		}

		public void doTestAsynchronousTextMessageConsume(bool persistent)
		{
			using(IConnection connection = CreateConnection(TEST_CLIENT_ID))
			{
				connection.Start();
				using(ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
				{
					IDestination destination = SessionUtil.GetDestination(session, DESTINATION_NAME);
					using(IMessageConsumer consumer = session.CreateConsumer(destination, receiveTimeout))
					{
						consumer.Listener += new MessageListener(OnMessage);
						using(IMessageProducer producer = session.CreateProducer(destination, receiveTimeout))
						{
							producer.Persistent = persistent;
							producer.RequestTimeout = receiveTimeout;

							ITextMessage request = session.CreateTextMessage("Hello, World!");
							request.NMSCorrelationID = "AsyncConsumeTextMessage";
							request.Properties["NMSXGroupID"] = "cheese";
							request.Properties["myHeader"] = "James";

							producer.Send(request);

							WaitForMessageToArrive();
							Assert.AreEqual(request.NMSCorrelationID, receivedMsg.NMSCorrelationID, "Invalid correlation ID.");
							Assert.AreEqual(request.Properties["NMSXGroupID"], receivedMsg.Properties["NMSXGroupID"], "Invalid NMSXGroupID.");
							Assert.AreEqual(request.Properties["myHeader"], receivedMsg.Properties["myHeader"], "Invalid myHeader.");
							Assert.AreEqual(request.Text, ((ITextMessage) receivedMsg).Text, "Invalid text body.");
						}
					}
				}
			}
		}

		protected void OnMessage(IMessage message)
		{
			lock(semaphore)
			{
				receivedMsg = message;
				received = true;
				Monitor.PulseAll(semaphore);
			}
		}

		protected void WaitForMessageToArrive()
		{
			lock(semaphore)
			{
				if(!received)
				{
					Monitor.Wait(semaphore, receiveTimeout);
				}
			}

			Assert.IsTrue(received, "Should have received a message by now!");
		}
	}
}
