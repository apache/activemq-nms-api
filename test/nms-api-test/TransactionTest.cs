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

namespace Apache.NMS.Test
{
    [TestFixture]
    public class TransactionTest : NMSTestSupport
    {
        protected static string DESTINATION_NAME = "queue://TEST.TransactionTestDestination";

        [Test]
        public void TestSendRollback(
            [Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
            MsgDeliveryMode deliveryMode)
        {
            using (IConnection connection = CreateConnection(GetTestClientId()))
            {
                connection.Start();
                using (ISession session = connection.CreateSession(AcknowledgementMode.Transactional))
                {
                    IDestination destination = CreateDestination(session, DESTINATION_NAME);
                    using (IMessageConsumer consumer = session.CreateConsumer(destination))
                    using (IMessageProducer producer = session.CreateProducer(destination))
                    {
                        producer.DeliveryMode = deliveryMode;
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
        public void TestSendSessionClose(
            [Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
            MsgDeliveryMode deliveryMode)
        {
            ITextMessage firstMsgSend;
            ITextMessage secondMsgSend;

            using (IConnection connection1 = CreateConnection(GetTestClientId()))
            {
                connection1.Start();
                using (ISession session1 = connection1.CreateSession(AcknowledgementMode.Transactional))
                {
                    IDestination destination1 = CreateDestination(session1, DESTINATION_NAME);
                    using (IMessageConsumer consumer = session1.CreateConsumer(destination1))
                    {
                        // First connection session that sends one message, and the
                        // second message is implicitly rolled back as the session is
                        // disposed before Commit() can be called.
                        using (IConnection connection2 = CreateConnection(GetTestClientId()))
                        {
                            connection2.Start();
                            using (ISession session2 = connection2.CreateSession(AcknowledgementMode.Transactional))
                            {
                                IDestination destination2 = CreateDestination(session2, DESTINATION_NAME);
                                using (IMessageProducer producer = session2.CreateProducer(destination2))
                                {
                                    producer.DeliveryMode = deliveryMode;
                                    firstMsgSend = session2.CreateTextMessage("First Message");
                                    producer.Send(firstMsgSend);
                                    session2.Commit();

                                    ITextMessage rollbackMsg =
                                        session2.CreateTextMessage("I'm going to get rolled back.");
                                    producer.Send(rollbackMsg);
                                }
                            }
                        }

                        // Second connection session that will send one message.
                        using (IConnection connection2 = CreateConnection(GetTestClientId()))
                        {
                            connection2.Start();
                            using (ISession session2 = connection2.CreateSession(AcknowledgementMode.Transactional))
                            {
                                IDestination destination2 = CreateDestination(session2, DESTINATION_NAME);
                                using (IMessageProducer producer = session2.CreateProducer(destination2))
                                {
                                    producer.DeliveryMode = deliveryMode;
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
        public void TestReceiveRollback(
            [Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
            MsgDeliveryMode deliveryMode)
        {
            using (IConnection connection = CreateConnection(GetTestClientId()))
            {
                connection.Start();
                using (ISession session = connection.CreateSession(AcknowledgementMode.Transactional))
                {
                    IDestination destination = CreateDestination(session, DESTINATION_NAME);
                    using (IMessageConsumer consumer = session.CreateConsumer(destination))
                    using (IMessageProducer producer = session.CreateProducer(destination))
                    {
                        producer.DeliveryMode = deliveryMode;
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
        public void TestReceiveTwoThenRollback(
            [Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
            MsgDeliveryMode deliveryMode)
        {
            using (IConnection connection = CreateConnection(GetTestClientId()))
            {
                connection.Start();
                using (ISession session = connection.CreateSession(AcknowledgementMode.Transactional))
                {
                    IDestination destination = CreateDestination(session, DESTINATION_NAME);
                    using (IMessageConsumer consumer = session.CreateConsumer(destination))
                    using (IMessageProducer producer = session.CreateProducer(destination))
                    {
                        producer.DeliveryMode = deliveryMode;
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

        [Test]
        public void TestSendCommitNonTransaction(
            [Values(AcknowledgementMode.AutoAcknowledge, AcknowledgementMode.ClientAcknowledge)]
            AcknowledgementMode ackMode,
            [Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
            MsgDeliveryMode deliveryMode)
        {
            using (IConnection connection = CreateConnection(GetTestClientId()))
            {
                connection.Start();
                using (ISession session = connection.CreateSession(ackMode))
                {
                    IDestination destination = CreateDestination(session, DESTINATION_NAME);
                    using (IMessageConsumer consumer = session.CreateConsumer(destination))
                    using (IMessageProducer producer = session.CreateProducer(destination))
                    {
                        producer.DeliveryMode = deliveryMode;
                        ITextMessage firstMsgSend = session.CreateTextMessage("SendCommitNonTransaction Message");
                        producer.Send(firstMsgSend);
                        try
                        {
                            session.Commit();
                            Assert.Fail("Should have thrown an InvalidOperationException.");
                        }
                        catch (InvalidOperationException)
                        {
                        }
                    }
                }
            }
        }

        [Test]
        public void TestReceiveCommitNonTransaction(
            [Values(AcknowledgementMode.AutoAcknowledge, AcknowledgementMode.ClientAcknowledge)]
            AcknowledgementMode ackMode,
            [Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
            MsgDeliveryMode deliveryMode)
        {
            using (IConnection connection = CreateConnection(GetTestClientId()))
            {
                connection.Start();
                using (ISession session = connection.CreateSession(ackMode))
                {
                    IDestination destination = CreateDestination(session, DESTINATION_NAME);
                    using (IMessageConsumer consumer = session.CreateConsumer(destination))
                    using (IMessageProducer producer = session.CreateProducer(destination))
                    {
                        producer.DeliveryMode = deliveryMode;
                        ITextMessage firstMsgSend = session.CreateTextMessage("ReceiveCommitNonTransaction Message");
                        producer.Send(firstMsgSend);

                        // Receive the messages

                        IMessage message = consumer.Receive(receiveTimeout);
                        AssertTextMessageEqual(firstMsgSend, message, "First message does not match.");
                        if (AcknowledgementMode.ClientAcknowledge == ackMode)
                        {
                            message.Acknowledge();
                        }

                        try
                        {
                            session.Commit();
                            Assert.Fail("Should have thrown an InvalidOperationException.");
                        }
                        catch (InvalidOperationException)
                        {
                        }
                    }
                }
            }
        }

        [Test]
        public void TestReceiveRollbackNonTransaction(
            [Values(AcknowledgementMode.AutoAcknowledge, AcknowledgementMode.ClientAcknowledge)]
            AcknowledgementMode ackMode,
            [Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
            MsgDeliveryMode deliveryMode)
        {
            using (IConnection connection = CreateConnection(GetTestClientId()))
            {
                connection.Start();
                using (ISession session = connection.CreateSession(ackMode))
                {
                    IDestination destination = CreateDestination(session, DESTINATION_NAME);
                    using (IMessageConsumer consumer = session.CreateConsumer(destination))
                    using (IMessageProducer producer = session.CreateProducer(destination))
                    {
                        producer.DeliveryMode = deliveryMode;
                        ITextMessage firstMsgSend = session.CreateTextMessage("ReceiveCommitNonTransaction Message");
                        producer.Send(firstMsgSend);

                        // Receive the messages

                        IMessage message = consumer.Receive(receiveTimeout);
                        AssertTextMessageEqual(firstMsgSend, message, "First message does not match.");
                        if (AcknowledgementMode.ClientAcknowledge == ackMode)
                        {
                            message.Acknowledge();
                        }

                        try
                        {
                            session.Rollback();
                            Assert.Fail("Should have thrown an InvalidOperationException.");
                        }
                        catch (InvalidOperationException)
                        {
                        }
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

        [Test]
        public void TestRedispatchOfRolledbackTx(
            [Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
            MsgDeliveryMode deliveryMode)
        {
            using (IConnection connection = CreateConnection(GetTestClientId()))
            {
                connection.Start();
                ISession session = connection.CreateSession(AcknowledgementMode.Transactional);
                IDestination destination = CreateDestination(session, DestinationType.Queue);

                SendMessages(connection, destination, deliveryMode, 2);

                IMessageConsumer consumer = session.CreateConsumer(destination);
                Assert.IsNotNull(consumer.Receive(TimeSpan.FromMilliseconds(1500)));
                Assert.IsNotNull(consumer.Receive(TimeSpan.FromMilliseconds(1500)));

                // install another consumer while message dispatch is unacked/uncommitted
                ISession redispatchSession = connection.CreateSession(AcknowledgementMode.Transactional);
                IMessageConsumer redispatchConsumer = redispatchSession.CreateConsumer(destination);

                session.Rollback();
                session.Close();

                IMessage msg = redispatchConsumer.Receive(TimeSpan.FromMilliseconds(1500));
                Assert.IsNotNull(msg);
                Assert.IsTrue(msg.NMSRedelivered);
                Assert.AreEqual(2, msg.Properties.GetLong("NMSXDeliveryCount"));
                msg = redispatchConsumer.Receive(TimeSpan.FromMilliseconds(1500));
                Assert.IsNotNull(msg);
                Assert.IsTrue(msg.NMSRedelivered);
                Assert.AreEqual(2, msg.Properties.GetLong("NMSXDeliveryCount"));
                redispatchSession.Commit();

                Assert.IsNull(redispatchConsumer.Receive(TimeSpan.FromMilliseconds(500)));
                redispatchSession.Close();
            }
        }

        [Test]
        public void TestRedispatchOfUncommittedTx(
            [Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
            MsgDeliveryMode deliveryMode)
        {
            using (IConnection connection = CreateConnection(GetTestClientId()))
            {
                connection.Start();
                ISession session = connection.CreateSession(AcknowledgementMode.Transactional);
                IDestination destination = CreateDestination(session, DestinationType.Queue);

                SendMessages(connection, destination, deliveryMode, 2);

                IMessageConsumer consumer = session.CreateConsumer(destination);
                Assert.IsNotNull(consumer.Receive(TimeSpan.FromMilliseconds(2000)));
                Assert.IsNotNull(consumer.Receive(TimeSpan.FromMilliseconds(2000)));

                // install another consumer while message dispatch is unacked/uncommitted
                ISession redispatchSession = connection.CreateSession(AcknowledgementMode.Transactional);
                IMessageConsumer redispatchConsumer = redispatchSession.CreateConsumer(destination);

                // no commit so will auto rollback and get re-dispatched to redisptachConsumer
                session.Close();

                IMessage msg = redispatchConsumer.Receive(TimeSpan.FromMilliseconds(2000));
                Assert.IsNotNull(msg);
                Assert.IsTrue(msg.NMSRedelivered);
                Assert.AreEqual(2, msg.Properties.GetLong("NMSXDeliveryCount"));

                msg = redispatchConsumer.Receive(TimeSpan.FromMilliseconds(2000));
                Assert.IsNotNull(msg);
                Assert.IsTrue(msg.NMSRedelivered);
                Assert.AreEqual(2, msg.Properties.GetLong("NMSXDeliveryCount"));
                redispatchSession.Commit();

                Assert.IsNull(redispatchConsumer.Receive(TimeSpan.FromMilliseconds(500)));
                redispatchSession.Close();
            }
        }
    }
}