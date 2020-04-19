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
    public class DurableTest : NMSTestSupport
    {
        protected static string DURABLE_TOPIC = "topic://TEST.DurableConsumerTopic";
        protected static string DURABLE_SELECTOR = "2 > 1";

        protected string TEST_CLIENT_AND_CONSUMER_ID;
        protected string SEND_CLIENT_ID;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            TEST_CLIENT_AND_CONSUMER_ID = GetTestClientId();
            SEND_CLIENT_ID = GetTestClientId();
        }

        [Test]
        public void TestSendWhileClosed(
            [Values(AcknowledgementMode.AutoAcknowledge, AcknowledgementMode.ClientAcknowledge,
                AcknowledgementMode.DupsOkAcknowledge, AcknowledgementMode.Transactional)]
            AcknowledgementMode ackMode)
        {
            try
            {
                using (IConnection connection = CreateConnection(TEST_CLIENT_AND_CONSUMER_ID))
                {
                    connection.Start();

                    using (ISession session = connection.CreateSession(ackMode))
                    {
                        ITopic topic = (ITopic) CreateDestination(session, DestinationType.Topic);
                        IMessageProducer producer = session.CreateProducer(topic);

                        producer.DeliveryMode = MsgDeliveryMode.Persistent;

                        ISession consumeSession = connection.CreateSession(ackMode);
                        IMessageConsumer consumer =
                            consumeSession.CreateDurableConsumer(topic, TEST_CLIENT_AND_CONSUMER_ID, null, false);
                        Thread.Sleep(1000);
                        consumer.Dispose();
                        consumer = null;

                        ITextMessage message = session.CreateTextMessage("DurableTest-TestSendWhileClosed");
                        message.Properties.SetString("test", "test");
                        message.NMSType = "test";
                        producer.Send(message);
                        if (AcknowledgementMode.Transactional == ackMode)
                        {
                            session.Commit();
                        }

                        Thread.Sleep(1000);
                        consumer = consumeSession.CreateDurableConsumer(topic, TEST_CLIENT_AND_CONSUMER_ID, null,
                            false);
                        ITextMessage msg = consumer.Receive(TimeSpan.FromMilliseconds(1000)) as ITextMessage;
                        msg.Acknowledge();
                        if (AcknowledgementMode.Transactional == ackMode)
                        {
                            consumeSession.Commit();
                        }

                        Assert.IsNotNull(msg);
                        Assert.AreEqual(msg.Text, "DurableTest-TestSendWhileClosed");
                        Assert.AreEqual(msg.NMSType, "test");
                        Assert.AreEqual(msg.Properties.GetString("test"), "test");
                    }
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                // Pause to allow Stomp to unregister at the broker.
                Thread.Sleep(500);

                UnregisterDurableConsumer(TEST_CLIENT_AND_CONSUMER_ID, TEST_CLIENT_AND_CONSUMER_ID);
            }
        }

        [Test]
        public void TestDurableConsumerSelectorChange(
            [Values(AcknowledgementMode.AutoAcknowledge, AcknowledgementMode.ClientAcknowledge,
                AcknowledgementMode.DupsOkAcknowledge, AcknowledgementMode.Transactional)]
            AcknowledgementMode ackMode)
        {
            try
            {
                using (IConnection connection = CreateConnection(TEST_CLIENT_AND_CONSUMER_ID))
                {
                    connection.Start();
                    using (ISession session = connection.CreateSession(ackMode))
                    {
                        ITopic topic = (ITopic) CreateDestination(session, DestinationType.Topic);
                        IMessageProducer producer = session.CreateProducer(topic);
                        IMessageConsumer consumer = session.CreateDurableConsumer(topic, TEST_CLIENT_AND_CONSUMER_ID,
                            "color='red'", false);

                        producer.DeliveryMode = MsgDeliveryMode.Persistent;

                        // Send the messages
                        ITextMessage sendMessage = session.CreateTextMessage("1st");
                        sendMessage.Properties["color"] = "red";
                        producer.Send(sendMessage);
                        if (AcknowledgementMode.Transactional == ackMode)
                        {
                            session.Commit();
                        }

                        ITextMessage receiveMsg = consumer.Receive(receiveTimeout) as ITextMessage;
                        Assert.IsNotNull(receiveMsg, "Failed to retrieve 1st durable message.");
                        Assert.AreEqual("1st", receiveMsg.Text);
                        Assert.AreEqual(MsgDeliveryMode.Persistent, receiveMsg.NMSDeliveryMode,
                            "NMSDeliveryMode does not match");
                        receiveMsg.Acknowledge();
                        if (AcknowledgementMode.Transactional == ackMode)
                        {
                            session.Commit();
                        }

                        // Change the subscription, allowing some time for the Broker to purge the
                        // consumers resources.
                        consumer.Dispose();
                        Thread.Sleep(1000);

                        consumer = session.CreateDurableConsumer(topic, TEST_CLIENT_AND_CONSUMER_ID, "color='blue'",
                            false);

                        sendMessage = session.CreateTextMessage("2nd");
                        sendMessage.Properties["color"] = "red";
                        producer.Send(sendMessage);
                        sendMessage = session.CreateTextMessage("3rd");
                        sendMessage.Properties["color"] = "blue";
                        producer.Send(sendMessage);
                        if (AcknowledgementMode.Transactional == ackMode)
                        {
                            session.Commit();
                        }

                        // Selector should skip the 2nd message.
                        receiveMsg = consumer.Receive(receiveTimeout) as ITextMessage;
                        Assert.IsNotNull(receiveMsg, "Failed to retrieve durable message.");
                        Assert.AreEqual("3rd", receiveMsg.Text, "Retrieved the wrong durable message.");
                        Assert.AreEqual(MsgDeliveryMode.Persistent, receiveMsg.NMSDeliveryMode,
                            "NMSDeliveryMode does not match");
                        receiveMsg.Acknowledge();
                        if (AcknowledgementMode.Transactional == ackMode)
                        {
                            session.Commit();
                        }

                        // Make sure there are no pending messages.
                        Assert.IsNull(consumer.ReceiveNoWait(), "Wrong number of messages in durable subscription.");
                    }
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                // Pause to allow Stomp to unregister at the broker.
                Thread.Sleep(500);

                UnregisterDurableConsumer(TEST_CLIENT_AND_CONSUMER_ID, TEST_CLIENT_AND_CONSUMER_ID);
            }
        }

        [Test]
        public void TestDurableConsumer(
            [Values(AcknowledgementMode.AutoAcknowledge, AcknowledgementMode.ClientAcknowledge,
                AcknowledgementMode.DupsOkAcknowledge, AcknowledgementMode.Transactional)]
            AcknowledgementMode ackMode)
        {
            string TEST_DURABLE_TOPIC = DURABLE_TOPIC + ":TestDurableConsumer";

            try
            {
                RegisterDurableConsumer(TEST_CLIENT_AND_CONSUMER_ID, TEST_DURABLE_TOPIC, TEST_CLIENT_AND_CONSUMER_ID,
                    null, false);
                RunTestDurableConsumer(TEST_DURABLE_TOPIC, ackMode);
                if (AcknowledgementMode.Transactional == ackMode)
                {
                    RunTestDurableConsumer(TEST_DURABLE_TOPIC, ackMode);
                }
            }
            finally
            {
                // Pause to allow Stomp to unregister at the broker.
                Thread.Sleep(500);

                UnregisterDurableConsumer(TEST_CLIENT_AND_CONSUMER_ID, TEST_CLIENT_AND_CONSUMER_ID);
            }
        }

        protected void RunTestDurableConsumer(string topicName, AcknowledgementMode ackMode)
        {
            SendDurableMessage(topicName);
            SendDurableMessage(topicName);

            using (IConnection connection = CreateConnection(TEST_CLIENT_AND_CONSUMER_ID))
            {
                connection.Start();
                using (ISession session = connection.CreateSession(ackMode))
                {
                    ITopic topic = SessionUtil.GetTopic(session, topicName);
                    using (IMessageConsumer consumer =
                        session.CreateDurableConsumer(topic, TEST_CLIENT_AND_CONSUMER_ID, null, false))
                    {
                        IMessage msg = consumer.Receive(receiveTimeout);
                        Assert.IsNotNull(msg, "Did not receive first durable message.");
                        msg.Acknowledge();

                        msg = consumer.Receive(receiveTimeout);
                        Assert.IsNotNull(msg, "Did not receive second durable message.");
                        msg.Acknowledge();

                        if (AcknowledgementMode.Transactional == ackMode)
                        {
                            session.Commit();
                        }
                    }
                }
            }
        }

        protected void SendDurableMessage(string topicName)
        {
            using (IConnection connection = CreateConnection(SEND_CLIENT_ID))
            {
                connection.Start();
                using (ISession session = connection.CreateSession())
                {
                    ITopic topic = SessionUtil.GetTopic(session, topicName);
                    using (IMessageProducer producer = session.CreateProducer(topic))
                    {
                        ITextMessage message = session.CreateTextMessage("Durable Hello");

                        producer.DeliveryMode = MsgDeliveryMode.Persistent;
                        producer.Send(message);
                    }
                }
            }
        }
    }
}