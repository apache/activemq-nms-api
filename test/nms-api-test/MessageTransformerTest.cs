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
    public class MessageTransformerTest : NMSTestSupport
    {
        private string propertyName = "ADDITIONAL-PROPERTY";
        private string propertyValue = "ADDITIONAL-PROPERTY-VALUE";

        [Test]
        public void TestProducerTransformer(
            [Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
            MsgDeliveryMode deliveryMode)
        {
            using (IConnection connection = CreateConnection())
            {
                connection.Start();
                using (ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
                {
                    IDestination destination = session.CreateTemporaryTopic();
                    using (IMessageConsumer consumer = session.CreateConsumer(destination))
                    using (IMessageProducer producer = session.CreateProducer(destination))
                    {
                        producer.DeliveryMode = deliveryMode;
                        producer.ProducerTransformer = DoProducerTransform;

                        IMessage message = session.CreateMessage();

                        message.Properties["Test"] = "Value";

                        producer.Send(message);

                        message = consumer.Receive(TimeSpan.FromMilliseconds(5000));

                        Assert.IsNotNull(message);
                        Assert.IsTrue(message.Properties.Count == 2);

                        Assert.AreEqual("Value", message.Properties["Test"]);
                        Assert.AreEqual(propertyValue, message.Properties[propertyName]);
                    }
                }
            }
        }

        [Test]
        public void TestConsumerTransformer(
            [Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
            MsgDeliveryMode deliveryMode)
        {
            using (IConnection connection = CreateConnection())
            {
                connection.Start();
                using (ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
                {
                    IDestination destination = session.CreateTemporaryTopic();
                    using (IMessageConsumer consumer = session.CreateConsumer(destination))
                    using (IMessageProducer producer = session.CreateProducer(destination))
                    {
                        producer.DeliveryMode = deliveryMode;

                        consumer.ConsumerTransformer = DoConsumerTransform;

                        IMessage message = session.CreateMessage();

                        message.Properties["Test"] = "Value";

                        producer.Send(message);

                        message = consumer.Receive(TimeSpan.FromMilliseconds(5000));

                        Assert.IsNotNull(message);
                        Assert.IsTrue(message.Properties.Count == 2, "Property Count should be 2");

                        Assert.AreEqual("Value", message.Properties["Test"], "Propert 'Value' was incorrect");
                        Assert.AreEqual(propertyValue, message.Properties[propertyName], "Property not inserted");
                    }
                }
            }
        }

        private IMessage DoProducerTransform(ISession session, IMessageProducer producer, IMessage message)
        {
            message.Properties[propertyName] = propertyValue;

            return message;
        }

        private IMessage DoConsumerTransform(ISession session, IMessageConsumer consumer, IMessage message)
        {
            IMessage newMessage = session.CreateMessage();

            MessageTransformation.CopyNMSMessageProperties(message, newMessage);

            newMessage.Properties[propertyName] = propertyValue;

            return newMessage;
        }
    }
}