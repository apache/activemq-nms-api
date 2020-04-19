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

using Apache.NMS.Util;
using NUnit.Framework;

namespace Apache.NMS.Test
{
    [TestFixture]
    public class TextMessageTest : NMSTestSupport
    {
        [Test]
        public void SendReceiveTextMessage(
            [Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
            MsgDeliveryMode deliveryMode)
        {
            using (IConnection connection = CreateConnection(GetTestClientId()))
            {
                connection.Start();
                using (ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
                {
                    IDestination destination = CreateDestination(session, DestinationType.Queue);
                    using (IMessageConsumer consumer = session.CreateConsumer(destination))
                    using (IMessageProducer producer = session.CreateProducer(destination))
                    {
                        producer.DeliveryMode = deliveryMode;
                        IMessage request = session.CreateTextMessage("Hello World!");
                        producer.Send(request);

                        IMessage message = consumer.Receive(receiveTimeout);
                        AssertTextMessageEqual(request, message);
                        Assert.AreEqual(deliveryMode, message.NMSDeliveryMode, "NMSDeliveryMode does not match");
                    }
                }
            }
        }

        /// <summary>
        /// Assert that two messages are ITextMessages and their text bodies are equal.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        protected void AssertTextMessageEqual(IMessage expected, IMessage actual)
        {
            ITextMessage expectedTextMsg = expected as ITextMessage;
            Assert.IsNotNull(expectedTextMsg, "'expected' message not a text message");
            ITextMessage actualTextMsg = actual as ITextMessage;
            Assert.IsNotNull(actualTextMsg, "'actual' message not a text message");
            Assert.AreEqual(expectedTextMsg.Text, actualTextMsg.Text, "Text message does not match.");
        }
    }
}