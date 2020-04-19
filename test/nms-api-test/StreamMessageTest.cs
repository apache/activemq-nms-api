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
    public class StreamMessageTest : NMSTestSupport
    {
        protected bool a = true;
        protected byte b = 123;
        protected char c = 'c';
        protected short d = 0x1234;
        protected int e = 0x12345678;
        protected long f = 0x1234567812345678;
        protected string g = "Hello World!";
        protected bool h = false;
        protected byte i = 0xFF;
        protected short j = -0x1234;
        protected int k = -0x12345678;
        protected long l = -0x1234567812345678;
        protected float m = 2.1F;
        protected double n = 2.3;

        [Test]
        public void SendReceiveStreamMessage(
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
                        IStreamMessage request;

                        try
                        {
                            request = session.CreateStreamMessage();
                        }
                        catch (System.NotSupportedException)
                        {
                            return;
                        }

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
                        Assert.AreEqual(deliveryMode, message.NMSDeliveryMode, "NMSDeliveryMode does not match");

                        // use generic API to access entries
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
                    }
                }
            }
        }
    }
}