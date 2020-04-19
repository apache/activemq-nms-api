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
using Apache.NMS.Commands;

namespace Apache.NMS.Test
{
    [TestFixture]
    public class ForeignMessageTransformationTest : NMSTestSupport
    {
        private string propertyName = "Test-Property";
        private string propertyValue = "Test-Property-Value";
        private string mapElementName = "Test-Map-Property";
        private string mapElementValue = "Test-Map-Property-Value";
        private string textBody = "This is a TextMessage from a Foreign Provider";
        private byte[] bytesContent = {1, 2, 3, 4, 5, 6, 7, 8};

        private bool a = true;
        private byte b = 123;
        private char c = 'c';
        private short d = 0x1234;
        private int e = 0x12345678;
        private long f = 0x1234567812345678;
        private string g = "Hello World!";
        private bool h = false;
        private byte i = 0xFF;
        private short j = -0x1234;
        private int k = -0x12345678;
        private long l = -0x1234567812345678;
        private float m = 2.1F;
        private double n = 2.3;

        [Test]
        public void SendReceiveForeignMessageTest(
            [Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
            MsgDeliveryMode deliveryMode)
        {
            using (IConnection connection = CreateConnection())
            {
                connection.Start();
                using (ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
                {
                    IDestination destination = CreateDestination(session, DestinationType.Topic);
                    using (IMessageConsumer consumer = session.CreateConsumer(destination))
                    using (IMessageProducer producer = session.CreateProducer(destination))
                    {
                        try
                        {
                            producer.DeliveryMode = deliveryMode;
                            Message request = new Message();
                            request.Properties[propertyName] = propertyValue;

                            producer.Send(request);

                            IMessage message = consumer.Receive(receiveTimeout);
                            Assert.IsNotNull(message, "No message returned!");
                            Assert.AreEqual(request.Properties.Count, message.Properties.Count,
                                "Invalid number of properties.");
                            Assert.AreEqual(deliveryMode, message.NMSDeliveryMode, "NMSDeliveryMode does not match");

                            // use generic API to access entries
                            Assert.AreEqual(propertyValue, message.Properties[propertyName],
                                "generic map entry: " + propertyName);

                            // use type safe APIs
                            Assert.AreEqual(propertyValue, message.Properties.GetString(propertyName),
                                "map entry: " + propertyName);
                        }
                        catch (NotSupportedException)
                        {
                        }
                    }
                }
            }
        }

        [Test]
        public void SendReceiveForeignTextMessageTest(
            [Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
            MsgDeliveryMode deliveryMode)
        {
            using (IConnection connection = CreateConnection())
            {
                connection.Start();
                using (ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
                {
                    IDestination destination = CreateDestination(session, DestinationType.Topic);
                    using (IMessageConsumer consumer = session.CreateConsumer(destination))
                    using (IMessageProducer producer = session.CreateProducer(destination))
                    {
                        try
                        {
                            producer.DeliveryMode = deliveryMode;
                            TextMessage request = new TextMessage();
                            request.Properties[propertyName] = propertyValue;
                            request.Text = textBody;

                            producer.Send(request);

                            ITextMessage message = consumer.Receive(receiveTimeout) as ITextMessage;
                            Assert.IsNotNull(message, "No message returned!");
                            Assert.AreEqual(request.Properties.Count, message.Properties.Count,
                                "Invalid number of properties.");
                            Assert.AreEqual(deliveryMode, message.NMSDeliveryMode, "NMSDeliveryMode does not match");

                            // Check the body
                            Assert.AreEqual(textBody, message.Text, "TextMessage body was wrong.");

                            // use generic API to access entries
                            Assert.AreEqual(propertyValue, message.Properties[propertyName],
                                "generic map entry: " + propertyName);

                            // use type safe APIs
                            Assert.AreEqual(propertyValue, message.Properties.GetString(propertyName),
                                "map entry: " + propertyName);
                        }
                        catch (NotSupportedException)
                        {
                        }
                    }
                }
            }
        }

        [Test]
        public void SendReceiveForeignBytesMessageTest(
            [Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
            MsgDeliveryMode deliveryMode)
        {
            using (IConnection connection = CreateConnection())
            {
                connection.Start();
                using (ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
                {
                    IDestination destination = CreateDestination(session, DestinationType.Topic);
                    using (IMessageConsumer consumer = session.CreateConsumer(destination))
                    using (IMessageProducer producer = session.CreateProducer(destination))
                    {
                        try
                        {
                            producer.DeliveryMode = deliveryMode;
                            BytesMessage request = new BytesMessage();
                            request.Properties[propertyName] = propertyValue;
                            request.WriteBytes(bytesContent);

                            producer.Send(request);

                            IBytesMessage message = consumer.Receive(receiveTimeout) as IBytesMessage;
                            Assert.IsNotNull(message, "No message returned!");
                            Assert.AreEqual(request.Properties.Count, message.Properties.Count,
                                "Invalid number of properties.");
                            Assert.AreEqual(deliveryMode, message.NMSDeliveryMode, "NMSDeliveryMode does not match");

                            // Check the body
                            byte[] content = new byte[bytesContent.Length];
                            Assert.AreEqual(bytesContent.Length, message.ReadBytes(content));
                            Assert.AreEqual(bytesContent, content, "BytesMessage body was wrong.");

                            // use generic API to access entries
                            Assert.AreEqual(propertyValue, message.Properties[propertyName],
                                "generic map entry: " + propertyName);

                            // use type safe APIs
                            Assert.AreEqual(propertyValue, message.Properties.GetString(propertyName),
                                "map entry: " + propertyName);
                        }
                        catch (NotSupportedException)
                        {
                        }
                    }
                }
            }
        }

        [Test]
        public void SendReceiveForeignMapMessageTest(
            [Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
            MsgDeliveryMode deliveryMode)
        {
            using (IConnection connection = CreateConnection())
            {
                connection.Start();
                using (ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
                {
                    IDestination destination = CreateDestination(session, DestinationType.Topic);
                    using (IMessageConsumer consumer = session.CreateConsumer(destination))
                    using (IMessageProducer producer = session.CreateProducer(destination))
                    {
                        try
                        {
                            producer.DeliveryMode = deliveryMode;
                            MapMessage request = new MapMessage();
                            request.Properties[propertyName] = propertyValue;
                            request.Body[mapElementName] = mapElementValue;

                            producer.Send(request);

                            IMapMessage message = consumer.Receive(receiveTimeout) as IMapMessage;
                            Assert.IsNotNull(message, "No message returned!");
                            Assert.AreEqual(request.Properties.Count, message.Properties.Count,
                                "Invalid number of properties.");
                            Assert.AreEqual(deliveryMode, message.NMSDeliveryMode, "NMSDeliveryMode does not match");

                            // Check the body
                            Assert.AreEqual(request.Body.Count, message.Body.Count);
                            Assert.AreEqual(mapElementValue, message.Body[mapElementName],
                                "MapMessage body was wrong.");

                            // use generic API to access entries
                            Assert.AreEqual(propertyValue, message.Properties[propertyName],
                                "generic map entry: " + propertyName);

                            // use type safe APIs
                            Assert.AreEqual(propertyValue, message.Properties.GetString(propertyName),
                                "map entry: " + propertyName);
                        }
                        catch (NotSupportedException)
                        {
                        }
                    }
                }
            }
        }

        [Test]
        public void SendReceiveForeignStreamMessageTest(
            [Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
            MsgDeliveryMode deliveryMode)
        {
            using (IConnection connection = CreateConnection())
            {
                connection.Start();
                using (ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
                {
                    IDestination destination = CreateDestination(session, DestinationType.Topic);
                    using (IMessageConsumer consumer = session.CreateConsumer(destination))
                    using (IMessageProducer producer = session.CreateProducer(destination))
                    {
                        try
                        {
                            producer.DeliveryMode = deliveryMode;
                            StreamMessage request = new StreamMessage();
                            request.Properties[propertyName] = propertyValue;

                            request.WriteBoolean(a);
                            request.WriteByte(b);
                            request.WriteChar(c);
                            request.WriteInt16(d);
                            request.WriteInt32(e);
                            request.WriteInt64(f);
                            request.WriteString(g);
                            request.WriteBoolean(h);
                            request.WriteByte(i);
                            request.WriteInt16(j);
                            request.WriteInt32(k);
                            request.WriteInt64(l);
                            request.WriteSingle(m);
                            request.WriteDouble(n);

                            producer.Send(request);

                            IStreamMessage message = consumer.Receive(receiveTimeout) as IStreamMessage;
                            Assert.IsNotNull(message, "No message returned!");
                            Assert.AreEqual(request.Properties.Count, message.Properties.Count,
                                "Invalid number of properties.");
                            Assert.AreEqual(deliveryMode, message.NMSDeliveryMode, "NMSDeliveryMode does not match");

                            // Check the body
                            Assert.AreEqual(a, message.ReadBoolean(), "Stream Boolean Value: a");
                            Assert.AreEqual(b, message.ReadByte(), "Stream Byte Value: b");
                            Assert.AreEqual(c, message.ReadChar(), "Stream Char Value: c");
                            Assert.AreEqual(d, message.ReadInt16(), "Stream Int16 Value: d");
                            Assert.AreEqual(e, message.ReadInt32(), "Stream Int32 Value: e");
                            Assert.AreEqual(f, message.ReadInt64(), "Stream Int64 Value: f");
                            Assert.AreEqual(g, message.ReadString(), "Stream String Value: g");
                            Assert.AreEqual(h, message.ReadBoolean(), "Stream Boolean Value: h");
                            Assert.AreEqual(i, message.ReadByte(), "Stream Byte Value: i");
                            Assert.AreEqual(j, message.ReadInt16(), "Stream Int16 Value: j");
                            Assert.AreEqual(k, message.ReadInt32(), "Stream Int32 Value: k");
                            Assert.AreEqual(l, message.ReadInt64(), "Stream Int64 Value: l");
                            Assert.AreEqual(m, message.ReadSingle(), "Stream Single Value: m");
                            Assert.AreEqual(n, message.ReadDouble(), "Stream Double Value: n");

                            // use generic API to access entries
                            Assert.AreEqual(propertyValue, message.Properties[propertyName],
                                "generic map entry: " + propertyName);

                            // use type safe APIs
                            Assert.AreEqual(propertyValue, message.Properties.GetString(propertyName),
                                "map entry: " + propertyName);
                        }
                        catch (NotSupportedException)
                        {
                        }
                    }
                }
            }
        }
    }
}