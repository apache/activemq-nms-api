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
    public class MessageTest : NMSTestSupport
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
        protected byte[] o = {1, 2, 3, 4, 5};

        [Test]
        public void SendReceiveMessageProperties(
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
                        IMessage request = session.CreateMessage();
                        request.Properties["a"] = a;
                        request.Properties["b"] = b;
                        request.Properties["c"] = c;
                        request.Properties["d"] = d;
                        request.Properties["e"] = e;
                        request.Properties["f"] = f;
                        request.Properties["g"] = g;
                        request.Properties["h"] = h;
                        request.Properties["i"] = i;
                        request.Properties["j"] = j;
                        request.Properties["k"] = k;
                        request.Properties["l"] = l;
                        request.Properties["m"] = m;
                        request.Properties["n"] = n;

                        try
                        {
                            request.Properties["o"] = o;
                            Assert.Fail("Should not be able to add a Byte[] to the Properties of a Message.");
                        }
                        catch
                        {
                            // Expected
                        }

                        try
                        {
                            request.Properties.SetBytes("o", o);
                            Assert.Fail("Should not be able to add a Byte[] to the Properties of a Message.");
                        }
                        catch
                        {
                            // Expected
                        }

                        producer.Send(request);

                        IMessage message = consumer.Receive(receiveTimeout);
                        Assert.IsNotNull(message, "No message returned!");
                        Assert.AreEqual(request.Properties.Count, message.Properties.Count,
                            "Invalid number of properties.");
                        Assert.AreEqual(deliveryMode, message.NMSDeliveryMode, "NMSDeliveryMode does not match");
                        Assert.AreEqual(ToHex(f), ToHex(message.Properties.GetLong("f")), "map entry: f as hex");

                        // use generic API to access entries
                        // Perform a string only comparison here since some NMS providers are type limited and
                        // may return only a string instance from the generic [] accessor.  Each provider should
                        // further test this functionality to determine that the correct type is returned if
                        // it is capable of doing so.
                        Assert.AreEqual(a.ToString(), message.Properties["a"].ToString(), "generic map entry: a");
                        Assert.AreEqual(b.ToString(), message.Properties["b"].ToString(), "generic map entry: b");
                        Assert.AreEqual(c.ToString(), message.Properties["c"].ToString(), "generic map entry: c");
                        Assert.AreEqual(d.ToString(), message.Properties["d"].ToString(), "generic map entry: d");
                        Assert.AreEqual(e.ToString(), message.Properties["e"].ToString(), "generic map entry: e");
                        Assert.AreEqual(f.ToString(), message.Properties["f"].ToString(), "generic map entry: f");
                        Assert.AreEqual(g.ToString(), message.Properties["g"].ToString(), "generic map entry: g");
                        Assert.AreEqual(h.ToString(), message.Properties["h"].ToString(), "generic map entry: h");
                        Assert.AreEqual(i.ToString(), message.Properties["i"].ToString(), "generic map entry: i");
                        Assert.AreEqual(j.ToString(), message.Properties["j"].ToString(), "generic map entry: j");
                        Assert.AreEqual(k.ToString(), message.Properties["k"].ToString(), "generic map entry: k");
                        Assert.AreEqual(l.ToString(), message.Properties["l"].ToString(), "generic map entry: l");
                        Assert.AreEqual(m.ToString(), message.Properties["m"].ToString(), "generic map entry: m");
                        Assert.AreEqual(n.ToString(), message.Properties["n"].ToString(), "generic map entry: n");

                        // use type safe APIs
                        Assert.AreEqual(a, message.Properties.GetBool("a"), "map entry: a");
                        Assert.AreEqual(b, message.Properties.GetByte("b"), "map entry: b");
                        Assert.AreEqual(c, message.Properties.GetChar("c"), "map entry: c");
                        Assert.AreEqual(d, message.Properties.GetShort("d"), "map entry: d");
                        Assert.AreEqual(e, message.Properties.GetInt("e"), "map entry: e");
                        Assert.AreEqual(f, message.Properties.GetLong("f"), "map entry: f");
                        Assert.AreEqual(g, message.Properties.GetString("g"), "map entry: g");
                        Assert.AreEqual(h, message.Properties.GetBool("h"), "map entry: h");
                        Assert.AreEqual(i, message.Properties.GetByte("i"), "map entry: i");
                        Assert.AreEqual(j, message.Properties.GetShort("j"), "map entry: j");
                        Assert.AreEqual(k, message.Properties.GetInt("k"), "map entry: k");
                        Assert.AreEqual(l, message.Properties.GetLong("l"), "map entry: l");
                        Assert.AreEqual(m, message.Properties.GetFloat("m"), "map entry: m");
                        Assert.AreEqual(n, message.Properties.GetDouble("n"), "map entry: n");
                    }
                }
            }
        }
    }
}