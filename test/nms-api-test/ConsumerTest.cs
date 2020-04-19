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
using NUnit.Framework;

namespace Apache.NMS.Test
{
    [TestFixture]
    public class ConsumerTest : NMSTestSupport
    {
        protected const int COUNT = 25;
        protected const string VALUE_NAME = "value";

        private bool dontAck;

// The .NET CF does not have the ability to interrupt threads, so this test is impossible.
#if !NETCF
        [Test]
        public void TestNoTimeoutConsumer(
            [Values(AcknowledgementMode.AutoAcknowledge, AcknowledgementMode.ClientAcknowledge,
                AcknowledgementMode.DupsOkAcknowledge, AcknowledgementMode.Transactional)]
            AcknowledgementMode ackMode)
        {
            // Launch a thread to perform IMessageConsumer.Receive().
            // If it doesn't fail in less than three seconds, no exception was thrown.
            Thread receiveThread = new Thread(new ThreadStart(TimeoutConsumerThreadProc));
            using (IConnection connection = CreateConnection())
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
            catch (ArgumentOutOfRangeException e)
            {
                // The test failed.  We will know because the timeout will expire inside TestNoTimeoutConsumer().
                Assert.Fail("Test failed with exception: " + e.Message);
            }
            catch (ThreadInterruptedException)
            {
                // The test succeeded!  We were still blocked when we were interrupted.
            }
            catch (Exception e)
            {
                // Some other exception occurred.
                Assert.Fail("Test failed with exception: " + e.Message);
            }
        }

        [Test]
        public void TestSyncReceiveConsumerClose(
            [Values(AcknowledgementMode.AutoAcknowledge, AcknowledgementMode.ClientAcknowledge,
                AcknowledgementMode.DupsOkAcknowledge, AcknowledgementMode.Transactional)]
            AcknowledgementMode ackMode)
        {
            // Launch a thread to perform IMessageConsumer.Receive().
            // If it doesn't fail in less than three seconds, no exception was thrown.
            Thread receiveThread = new Thread(new ThreadStart(TimeoutConsumerThreadProc));
            using (IConnection connection = CreateConnection())
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
                                Assert.Fail(
                                    "IMessageConsumer.Receive() thread is still alive, Close should have killed it.");
                            }
                        }
                    }
                }
            }
        }

        internal class ThreadArg
        {
            internal IConnection connection = null;
            internal ISession session = null;
            internal IDestination destination = null;
        }

        protected void DelayedProducerThreadProc(Object arg)
        {
            try
            {
                ThreadArg args = arg as ThreadArg;

                using (ISession session = args.connection.CreateSession())
                {
                    using (IMessageProducer producer = session.CreateProducer(args.destination))
                    {
                        // Give the consumer time to enter the receive.
                        Thread.Sleep(5000);

                        producer.Send(args.session.CreateTextMessage("Hello World"));
                    }
                }
            }
            catch (Exception e)
            {
                // Some other exception occurred.
                Assert.Fail("Test failed with exception: " + e.Message);
            }
        }

        [Test]
        public void TestDoChangeSentMessage(
            [Values(AcknowledgementMode.AutoAcknowledge, AcknowledgementMode.ClientAcknowledge,
                AcknowledgementMode.DupsOkAcknowledge, AcknowledgementMode.Transactional)]
            AcknowledgementMode ackMode,
            [Values(true, false)] bool doClear)
        {
            using (IConnection connection = CreateConnection())
            {
                connection.Start();
                using (ISession session = connection.CreateSession(ackMode))
                {
                    ITemporaryQueue queue = session.CreateTemporaryQueue();
                    using (IMessageConsumer consumer = session.CreateConsumer(queue))
                    {
                        IMessageProducer producer = session.CreateProducer(queue);
                        ITextMessage message = session.CreateTextMessage();

                        string prefix = "ConsumerTest - TestDoChangeSentMessage: ";

                        for (int i = 0; i < COUNT; i++)
                        {
                            message.Properties[VALUE_NAME] = i;
                            message.Text = prefix + Convert.ToString(i);

                            producer.Send(message);

                            if (doClear)
                            {
                                message.ClearBody();
                                message.ClearProperties();
                            }
                        }

                        if (ackMode == AcknowledgementMode.Transactional)
                        {
                            session.Commit();
                        }

                        for (int i = 0; i < COUNT; i++)
                        {
                            ITextMessage msg = consumer.Receive(TimeSpan.FromMilliseconds(2000)) as ITextMessage;
                            Assert.AreEqual(msg.Text, prefix + Convert.ToString(i));
                            Assert.AreEqual(msg.Properties.GetInt(VALUE_NAME), i);
                        }

                        if (ackMode == AcknowledgementMode.Transactional)
                        {
                            session.Commit();
                        }
                    }
                }
            }
        }

        [Test]
        public void TestConsumerReceiveBeforeMessageDispatched(
            [Values(AcknowledgementMode.AutoAcknowledge, AcknowledgementMode.ClientAcknowledge,
                AcknowledgementMode.DupsOkAcknowledge, AcknowledgementMode.Transactional)]
            AcknowledgementMode ackMode)
        {
            // Launch a thread to perform a delayed send.
            Thread sendThread = new Thread(DelayedProducerThreadProc);
            using (IConnection connection = CreateConnection())
            {
                connection.Start();
                using (ISession session = connection.CreateSession(ackMode))
                {
                    ITemporaryQueue queue = session.CreateTemporaryQueue();
                    using (IMessageConsumer consumer = session.CreateConsumer(queue))
                    {
                        ThreadArg arg = new ThreadArg();

                        arg.connection = connection;
                        arg.session = session;
                        arg.destination = queue;

                        sendThread.Start(arg);
                        IMessage message = consumer.Receive(TimeSpan.FromMinutes(1));
                        Assert.IsNotNull(message);
                    }
                }
            }
        }

        [Test]
        public void TestDontStart(
            [Values(MsgDeliveryMode.NonPersistent)]
            MsgDeliveryMode deliveryMode,
            [Values(DestinationType.Queue, DestinationType.Topic)]
            DestinationType destinationType)
        {
            using (IConnection connection = CreateConnection())
            {
                ISession session = connection.CreateSession();
                IDestination destination = CreateDestination(session, destinationType);
                IMessageConsumer consumer = session.CreateConsumer(destination);

                // Send the messages
                SendMessages(session, destination, deliveryMode, 1);

                // Make sure no messages were delivered.
                Assert.IsNull(consumer.Receive(TimeSpan.FromMilliseconds(1000)));
            }
        }

        [Test]
        public void TestSendReceiveTransacted(
            [Values(MsgDeliveryMode.NonPersistent, MsgDeliveryMode.Persistent)]
            MsgDeliveryMode deliveryMode,
            [Values(DestinationType.Queue, DestinationType.Topic, DestinationType.TemporaryQueue,
                DestinationType.TemporaryTopic)]
            DestinationType destinationType)
        {
            using (IConnection connection = CreateConnection())
            {
                // Send a message to the broker.
                connection.Start();
                ISession session = connection.CreateSession(AcknowledgementMode.Transactional);
                IDestination destination = CreateDestination(session, destinationType);
                IMessageConsumer consumer = session.CreateConsumer(destination);
                IMessageProducer producer = session.CreateProducer(destination);

                producer.DeliveryMode = deliveryMode;
                producer.Send(session.CreateTextMessage("Test"));

                // Message should not be delivered until commit.
                Thread.Sleep(1000);
                Assert.IsNull(consumer.ReceiveNoWait());
                session.Commit();

                // Make sure only 1 message was delivered.
                IMessage message = consumer.Receive(TimeSpan.FromMilliseconds(1000));
                Assert.IsNotNull(message);
                Assert.IsFalse(message.NMSRedelivered);
                Assert.IsNull(consumer.ReceiveNoWait());

                // Message should be redelivered is rollback is used.
                session.Rollback();

                // Make sure only 1 message was delivered.
                message = consumer.Receive(TimeSpan.FromMilliseconds(2000));
                Assert.IsNotNull(message);
                Assert.IsTrue(message.NMSRedelivered);
                Assert.IsNull(consumer.ReceiveNoWait());

                // If we commit now, the message should not be redelivered.
                session.Commit();
                Thread.Sleep(1000);
                Assert.IsNull(consumer.ReceiveNoWait());
            }
        }

        [Test]
        public void TestAckedMessageAreConsumed()
        {
            using (IConnection connection = CreateConnection())
            {
                connection.Start();
                ISession session = connection.CreateSession(AcknowledgementMode.ClientAcknowledge);
                IDestination destination = CreateDestination(session, DestinationType.Queue);
                IMessageProducer producer = session.CreateProducer(destination);
                producer.Send(session.CreateTextMessage("Hello"));

                // Consume the message...
                IMessageConsumer consumer = session.CreateConsumer(destination);
                IMessage msg = consumer.Receive(TimeSpan.FromMilliseconds(1000));
                Assert.IsNotNull(msg);
                msg.Acknowledge();

                // Reset the session.
                session.Close();
                session = connection.CreateSession(AcknowledgementMode.ClientAcknowledge);

                // Attempt to Consume the message...
                consumer = session.CreateConsumer(destination);
                msg = consumer.Receive(TimeSpan.FromMilliseconds(1000));
                Assert.IsNull(msg);

                session.Close();
            }
        }

        [Test]
        public void TestLastMessageAcked()
        {
            using (IConnection connection = CreateConnection())
            {
                connection.Start();
                ISession session = connection.CreateSession(AcknowledgementMode.ClientAcknowledge);
                IDestination destination = CreateDestination(session, DestinationType.Queue);
                IMessageProducer producer = session.CreateProducer(destination);
                producer.Send(session.CreateTextMessage("Hello"));
                producer.Send(session.CreateTextMessage("Hello2"));
                producer.Send(session.CreateTextMessage("Hello3"));

                // Consume the message...
                IMessageConsumer consumer = session.CreateConsumer(destination);
                IMessage msg = consumer.Receive(TimeSpan.FromMilliseconds(1000));
                Assert.IsNotNull(msg);
                msg = consumer.Receive(TimeSpan.FromMilliseconds(1000));
                Assert.IsNotNull(msg);
                msg = consumer.Receive(TimeSpan.FromMilliseconds(1000));
                Assert.IsNotNull(msg);
                msg.Acknowledge();

                // Reset the session.
                session.Close();
                session = connection.CreateSession(AcknowledgementMode.ClientAcknowledge);

                // Attempt to Consume the message...
                consumer = session.CreateConsumer(destination);
                msg = consumer.Receive(TimeSpan.FromMilliseconds(1000));
                Assert.IsNull(msg);

                session.Close();
            }
        }

        [Test]
        public void TestUnAckedMessageAreNotConsumedOnSessionClose()
        {
            using (IConnection connection = CreateConnection())
            {
                connection.Start();
                ISession session = connection.CreateSession(AcknowledgementMode.ClientAcknowledge);
                IDestination destination = CreateDestination(session, DestinationType.Queue);
                IMessageProducer producer = session.CreateProducer(destination);
                producer.Send(session.CreateTextMessage("Hello"));

                // Consume the message...
                IMessageConsumer consumer = session.CreateConsumer(destination);
                IMessage msg = consumer.Receive(TimeSpan.FromMilliseconds(1000));
                Assert.IsNotNull(msg);
                // Don't ack the message.

                // Reset the session.  This should cause the unacknowledged message to be re-delivered.
                session.Close();
                session = connection.CreateSession(AcknowledgementMode.ClientAcknowledge);

                // Attempt to Consume the message...
                consumer = session.CreateConsumer(destination);
                msg = consumer.Receive(TimeSpan.FromMilliseconds(2000));
                Assert.IsNotNull(msg);
                msg.Acknowledge();

                session.Close();
            }
        }

        [Test]
        public void TestAsyncAckedMessageAreConsumed()
        {
            using (IConnection connection = CreateConnection())
            {
                connection.Start();
                ISession session = connection.CreateSession(AcknowledgementMode.ClientAcknowledge);
                IDestination destination = CreateDestination(session, DestinationType.Queue);
                IMessageProducer producer = session.CreateProducer(destination);
                producer.Send(session.CreateTextMessage("Hello"));

                // Consume the message...
                IMessageConsumer consumer = session.CreateConsumer(destination);
                consumer.Listener += new MessageListener(OnMessage);

                Thread.Sleep(5000);

                // Reset the session.
                session.Close();

                session = connection.CreateSession(AcknowledgementMode.ClientAcknowledge);

                // Attempt to Consume the message...
                consumer = session.CreateConsumer(destination);
                IMessage msg = consumer.Receive(TimeSpan.FromMilliseconds(1000));
                Assert.IsNull(msg);

                session.Close();
            }
        }

        [Test]
        public void TestAsyncUnAckedMessageAreNotConsumedOnSessionClose()
        {
            using (IConnection connection = CreateConnection())
            {
                connection.Start();
                // don't aknowledge message on onMessage() call
                dontAck = true;
                ISession session = connection.CreateSession(AcknowledgementMode.ClientAcknowledge);
                IDestination destination = CreateDestination(session, DestinationType.Queue);
                IMessageProducer producer = session.CreateProducer(destination);
                producer.Send(session.CreateTextMessage("Hello"));

                // Consume the message...
                IMessageConsumer consumer = session.CreateConsumer(destination);
                consumer.Listener += new MessageListener(OnMessage);
                // Don't ack the message.

                // Reset the session. This should cause the Unacked message to be
                // redelivered.
                session.Close();

                Thread.Sleep(5000);
                session = connection.CreateSession(AcknowledgementMode.ClientAcknowledge);
                // Attempt to Consume the message...
                consumer = session.CreateConsumer(destination);
                IMessage msg = consumer.Receive(TimeSpan.FromMilliseconds(2000));
                Assert.IsNotNull(msg);
                msg.Acknowledge();

                session.Close();
            }
        }

        [Test]
        public void TestAddRemoveAsnycMessageListener()
        {
            using (IConnection connection = CreateConnection())
            {
                connection.Start();

                ISession session = connection.CreateSession(AcknowledgementMode.ClientAcknowledge);
                ITemporaryTopic topic = session.CreateTemporaryTopic();
                IMessageConsumer consumer = session.CreateConsumer(topic);

                consumer.Listener += OnMessage;
                consumer.Listener -= OnMessage;
                consumer.Listener += OnMessage;

                consumer.Close();
            }
        }

        public void OnMessage(IMessage message)
        {
            Assert.IsNotNull(message);

            if (!dontAck)
            {
                try
                {
                    message.Acknowledge();
                }
                catch (Exception)
                {
                }
            }
        }

        [Test]
        public void TestReceiveNoWait(
            [Values(AcknowledgementMode.AutoAcknowledge, AcknowledgementMode.ClientAcknowledge,
                AcknowledgementMode.DupsOkAcknowledge, AcknowledgementMode.Transactional)]
            AcknowledgementMode ackMode,
            [Values(MsgDeliveryMode.NonPersistent, MsgDeliveryMode.Persistent)]
            MsgDeliveryMode deliveryMode)
        {
            const int RETRIES = 20;

            using (IConnection connection = CreateConnection())
            {
                connection.Start();
                using (ISession session = connection.CreateSession(ackMode))
                {
                    IDestination destination = session.CreateTemporaryQueue();

                    using (IMessageProducer producer = session.CreateProducer(destination))
                    {
                        producer.DeliveryMode = deliveryMode;
                        ITextMessage message = session.CreateTextMessage("TEST");
                        producer.Send(message);

                        if (AcknowledgementMode.Transactional == ackMode)
                        {
                            session.Commit();
                        }
                    }

                    using (IMessageConsumer consumer = session.CreateConsumer(destination))
                    {
                        IMessage message = null;

                        for (int i = 0; i < RETRIES && message == null; ++i)
                        {
                            message = consumer.ReceiveNoWait();
                            Thread.Sleep(100);
                        }

                        Assert.IsNotNull(message);
                        message.Acknowledge();

                        if (AcknowledgementMode.Transactional == ackMode)
                        {
                            session.Commit();
                        }
                    }
                }
            }
        }

#endif
    }
}