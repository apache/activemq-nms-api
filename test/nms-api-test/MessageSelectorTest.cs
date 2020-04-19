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
    [Category("LongRunning")]
    public class MessageSelectorTest : NMSTestSupport
    {
        protected const string QUEUE_DESTINATION_NAME = "queue://TEST.MessageSelectorQueue";
        protected const string TOPIC_DESTINATION_NAME = "topic://TEST.MessageSelectorTopic";

        private int receivedNonIgnoredMsgCount = 0;
        private int receivedIgnoredMsgCount = 0;
        private bool simulateSlowConsumer = false;

        [Test]
        public void FilterIgnoredMessagesTest(
            [Values(QUEUE_DESTINATION_NAME, TOPIC_DESTINATION_NAME)]
            string destinationName)
        {
            simulateSlowConsumer = false;
            RunFilterIgnoredMessagesTest(destinationName);
        }

        /// <summary>
        /// A slow consumer will trigger the producer flow control on the broker when the destination is
        /// a queue.  It will also trigger the consumer flow control by slowing down the feed to all of the
        /// consumers on the queue to only send messages as fast as the slowest consumer can run.
        /// When sending to a topic, the producer will not be slowed down, and consumers will be allowed
        /// to run as fast as they can go.
        /// Since this test can take a long time to run, it is marked as explicit.
        /// </summary>
        /// <param name="destinationName"></param>
        [Test]
        public void FilterIgnoredMessagesSlowConsumerTest(
            [Values(QUEUE_DESTINATION_NAME, TOPIC_DESTINATION_NAME)]
            string destinationName)
        {
            simulateSlowConsumer = true;
            RunFilterIgnoredMessagesTest(destinationName);
        }

        public void RunFilterIgnoredMessagesTest(string destinationName)
        {
            TimeSpan ttl = TimeSpan.FromMinutes(30);
            const int MaxNumRequests = 100000;

            using (IConnection connection1 = CreateConnection(GetTestClientId()))
            using (IConnection connection2 = CreateConnection(GetTestClientId()))
            using (IConnection connection3 = CreateConnection(GetTestClientId()))
            {
                connection1.Start();
                connection2.Start();
                connection3.Start();
                using (ISession session1 = connection1.CreateSession(AcknowledgementMode.AutoAcknowledge))
                using (ISession session2 = connection2.CreateSession(AcknowledgementMode.AutoAcknowledge))
                using (ISession session3 = connection3.CreateSession(AcknowledgementMode.AutoAcknowledge))
                {
                    IDestination destination1 = CreateDestination(session1, destinationName);
                    IDestination destination2 = CreateDestination(session2, destinationName);
                    IDestination destination3 = CreateDestination(session3, destinationName);

                    using (IMessageProducer producer = session1.CreateProducer(destination1))
                    using (IMessageConsumer consumer1 =
                        session2.CreateConsumer(destination2, "JMSType NOT LIKE '%IGNORE'"))
                    {
                        int numNonIgnoredMsgsSent = 0;
                        int numIgnoredMsgsSent = 0;

                        producer.DeliveryMode = MsgDeliveryMode.NonPersistent;

                        receivedNonIgnoredMsgCount = 0;
                        receivedIgnoredMsgCount = 0;
                        consumer1.Listener += new MessageListener(OnNonIgnoredMessage);
                        IMessageConsumer consumer2 = null;

                        for (int index = 1; index <= MaxNumRequests; index++)
                        {
                            IMessage request = session1.CreateTextMessage(String.Format("Hello World! [{0} of {1}]",
                                index, MaxNumRequests));

                            request.NMSTimeToLive = ttl;
                            if (0 == (index % 2))
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

                            if (2000 == index)
                            {
                                // Start the second consumer
                                if (destination3.IsTopic)
                                {
                                    // Reset the ignored message sent count, since all previous messages
                                    // will not have been consumed on a topic.
                                    numIgnoredMsgsSent = 0;
                                }

                                consumer2 = session3.CreateConsumer(destination3, "JMSType LIKE '%IGNORE'");
                                consumer2.Listener += new MessageListener(OnIgnoredMessage);
                            }
                        }

                        // Create a waiting loop that will coordinate the end of the test.  It checks
                        // to see that all intended messages were received.  It will continue to wait as
                        // long as new messages are being received.  If it stops receiving messages before
                        // it receives everything it expects, it will eventually timeout and the test will fail.
                        int waitCount = 0;
                        int lastReceivedINongnoredMsgCount = receivedNonIgnoredMsgCount;
                        int lastReceivedIgnoredMsgCount = receivedIgnoredMsgCount;

                        while (receivedNonIgnoredMsgCount < numNonIgnoredMsgsSent
                               || receivedIgnoredMsgCount < numIgnoredMsgsSent)
                        {
                            if (lastReceivedINongnoredMsgCount != receivedNonIgnoredMsgCount
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

                            Assert.IsTrue(waitCount <= 30, String.Format(
                                "Timeout waiting for all messages to be delivered. Only {0} of {1} non-ignored messages delivered.  Only {2} of {3} ignored messages delivered.",
                                receivedNonIgnoredMsgCount, numNonIgnoredMsgsSent, receivedIgnoredMsgCount,
                                numIgnoredMsgsSent));
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
            if (simulateSlowConsumer)
            {
                // Simulate a slow consumer  It doesn't have to be too slow in a high speed environment
                // in order to trigger producer flow control.
                Thread.Sleep(10);
            }
        }
    }
}