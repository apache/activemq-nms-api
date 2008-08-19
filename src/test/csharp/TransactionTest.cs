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
using System.Collections;
using Apache.NMS.Util;
using NUnit.Framework;

namespace Apache.NMS.Test
{
	[TestFixture]
	abstract public class TransactionTest : NMSTestSupport
	{
		protected static string DESTINATION_NAME = "TransactionTestDestination";
		protected static string TEST_CLIENT_ID = "TransactionTestClientId";
		protected static string TEST_CLIENT_ID2 = "TransactionTestClientId2";

		[Test]
		public void TestSendRollback()
		{
			using(IConnection connection = CreateConnection(TEST_CLIENT_ID))
			{
				connection.Start();
				using(ISession session = connection.CreateSession(AcknowledgementMode.Transactional))
				{
					IDestination destination = SessionUtil.GetDestination(session, DESTINATION_NAME);
					using(IMessageConsumer consumer = session.CreateConsumer(destination))
					using(IMessageProducer producer = session.CreateProducer(destination))
					{
						producer.Persistent = false;
						producer.RequestTimeout = receiveTimeout;
						ITextMessage firstMsgSend = session.CreateTextMessage("First Message");
						producer.Send(firstMsgSend);
						session.Commit();

						ITextMessage rollbackMsg = session.CreateTextMessage("I'm going to get rolled back.");
						producer.Send(rollbackMsg);
						session.Rollback();

						ITextMessage secondMsgSend = session.CreateTextMessage("Second Message");
						producer.Send(secondMsgSend);
						session.Commit();

						// Receive the messages

						IMessage message = consumer.Receive(receiveTimeout);
						AssertTextMessageEqual(firstMsgSend, message, "First message does not match.");

						message = consumer.Receive(receiveTimeout);
						AssertTextMessageEqual(secondMsgSend, message, "Second message does not match.");

						// validates that the rollback was not consumed
						session.Commit();
					}
				}
			}
		}

		[Test]
		public void TestSendSessionClose()
		{
			ITextMessage firstMsgSend;
			ITextMessage secondMsgSend;

			using(IConnection connection1 = CreateConnection(TEST_CLIENT_ID))
			{
				connection1.Start();
				using(ISession session1 = connection1.CreateSession(AcknowledgementMode.Transactional))
				{
					IDestination destination1 = SessionUtil.GetDestination(session1, DESTINATION_NAME);
					using(IMessageConsumer consumer = session1.CreateConsumer(destination1))
					{
						// First connection session that sends one message, and the
						// second message is implicitly rolled back as the session is
						// disposed before Commit() can be called.
						using(IConnection connection2 = CreateConnection(TEST_CLIENT_ID2))
						{
							connection2.Start();
							using(ISession session2 = connection2.CreateSession(AcknowledgementMode.Transactional))
							{
								IDestination destination2 = SessionUtil.GetDestination(session2, DESTINATION_NAME);
								using(IMessageProducer producer = session2.CreateProducer(destination2))
								{
									producer.Persistent = false;
									producer.RequestTimeout = receiveTimeout;
									firstMsgSend = session2.CreateTextMessage("First Message");
									producer.Send(firstMsgSend);
									session2.Commit();

									ITextMessage rollbackMsg = session2.CreateTextMessage("I'm going to get rolled back.");
									producer.Send(rollbackMsg);
								}
							}
						}

						// Second connection session that will send one message.
						using(IConnection connection2 = CreateConnection(TEST_CLIENT_ID2))
						{
							connection2.Start();
							using(ISession session2 = connection2.CreateSession(AcknowledgementMode.Transactional))
							{
								IDestination destination2 = SessionUtil.GetDestination(session2, DESTINATION_NAME);
								using(IMessageProducer producer = session2.CreateProducer(destination2))
								{
									producer.Persistent = false;
									producer.RequestTimeout = receiveTimeout;
									secondMsgSend = session2.CreateTextMessage("Second Message");
									producer.Send(secondMsgSend);
									session2.Commit();
								}
							}
						}

						// Check the consumer to verify which messages were actually received.
						IMessage message = consumer.Receive(receiveTimeout);
						AssertTextMessageEqual(firstMsgSend, message, "First message does not match.");

						message = consumer.Receive(receiveTimeout);
						AssertTextMessageEqual(secondMsgSend, message, "Second message does not match.");

						// validates that the rollback was not consumed
						session1.Commit();
					}
				}
			}
		}

		[Test]
		public void TestReceiveRollback()
		{
			using(IConnection connection = CreateConnection(TEST_CLIENT_ID))
			{
				connection.Start();
				using(ISession session = connection.CreateSession(AcknowledgementMode.Transactional))
				{
					IDestination destination = SessionUtil.GetDestination(session, DESTINATION_NAME);
					using(IMessageConsumer consumer = session.CreateConsumer(destination))
					using(IMessageProducer producer = session.CreateProducer(destination))
					{
						producer.Persistent = false;
						producer.RequestTimeout = receiveTimeout;
						// Send both messages
						ITextMessage firstMsgSend = session.CreateTextMessage("First Message");
						producer.Send(firstMsgSend);
						ITextMessage secondMsgSend = session.CreateTextMessage("Second Message");
						producer.Send(secondMsgSend);
						session.Commit();

						// Receive the messages

						IMessage message = consumer.Receive(receiveTimeout);
						AssertTextMessageEqual(firstMsgSend, message, "First message does not match.");
						session.Commit();

						message = consumer.Receive(receiveTimeout);
						AssertTextMessageEqual(secondMsgSend, message, "Second message does not match.");

						// Rollback so we can get that last message again.
						session.Rollback();
						IMessage rollbackMsg = consumer.Receive(receiveTimeout);
						AssertTextMessageEqual(secondMsgSend, rollbackMsg, "Rollback message does not match.");
						session.Commit();
					}
				}
			}
		}


		[Test]
		public void TestReceiveTwoThenRollback()
		{
			using(IConnection connection = CreateConnection(TEST_CLIENT_ID))
			{
				connection.Start();
				using(ISession session = connection.CreateSession(AcknowledgementMode.Transactional))
				{
					IDestination destination = SessionUtil.GetDestination(session, DESTINATION_NAME);
					using(IMessageConsumer consumer = session.CreateConsumer(destination))
					using(IMessageProducer producer = session.CreateProducer(destination))
					{
						producer.Persistent = false;
						producer.RequestTimeout = receiveTimeout;
						// Send both messages
						ITextMessage firstMsgSend = session.CreateTextMessage("First Message");
						producer.Send(firstMsgSend);
						ITextMessage secondMsgSend = session.CreateTextMessage("Second Message");
						producer.Send(secondMsgSend);
						session.Commit();

						// Receive the messages

						IMessage message = consumer.Receive(receiveTimeout);
						AssertTextMessageEqual(firstMsgSend, message, "First message does not match.");
						message = consumer.Receive(receiveTimeout);
						AssertTextMessageEqual(secondMsgSend, message, "Second message does not match.");

						// Rollback so we can get that last two messages again.
						session.Rollback();
						IMessage rollbackMsg = consumer.Receive(receiveTimeout);
						AssertTextMessageEqual(firstMsgSend, rollbackMsg, "First rollback message does not match.");
						rollbackMsg = consumer.Receive(receiveTimeout);
						AssertTextMessageEqual(secondMsgSend, rollbackMsg, "Second rollback message does not match.");
			
						Assert.IsNull(consumer.ReceiveNoWait());
						session.Commit();
					}
				}
			}
		}

		/// <summary>
		/// Assert that two messages are ITextMessages and their text bodies are equal.
		/// </summary>
		/// <param name="expected"></param>
		/// <param name="actual"></param>
		/// <param name="message"></param>
		protected void AssertTextMessageEqual(IMessage expected, IMessage actual, String message)
		{
			ITextMessage expectedTextMsg = expected as ITextMessage;
			Assert.IsNotNull(expectedTextMsg, "'expected' message not a text message");
			ITextMessage actualTextMsg = actual as ITextMessage;
			Assert.IsNotNull(actualTextMsg, "'actual' message not a text message");
			Assert.AreEqual(expectedTextMsg.Text, actualTextMsg.Text, message);
		}
	}
}


