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
    public class MapMessageTest : NMSTestSupport
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
        public void SendReceiveMapMessage(
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
                        IMapMessage request = session.CreateMapMessage();
                        request.Body["a"] = a;
                        request.Body["b"] = b;
                        request.Body["c"] = c;
                        request.Body["d"] = d;
                        request.Body["e"] = e;
                        request.Body["f"] = f;
                        request.Body["g"] = g;
                        request.Body["h"] = h;
                        request.Body["i"] = i;
                        request.Body["j"] = j;
                        request.Body["k"] = k;
                        request.Body["l"] = l;
                        request.Body["m"] = m;
                        request.Body["n"] = n;
                        request.Body["o"] = o;
                        producer.Send(request);

                        IMapMessage message = consumer.Receive(receiveTimeout) as IMapMessage;
                        Assert.IsNotNull(message, "No message returned!");
                        Assert.AreEqual(request.Body.Count, message.Body.Count, "Invalid number of message maps.");
                        Assert.AreEqual(deliveryMode, message.NMSDeliveryMode, "NMSDeliveryMode does not match");
                        Assert.AreEqual(ToHex(f), ToHex(message.Body.GetLong("f")), "map entry: f as hex");

                        // use generic API to access entries
                        Assert.AreEqual(a, message.Body["a"], "generic map entry: a");
                        Assert.AreEqual(b, message.Body["b"], "generic map entry: b");
                        Assert.AreEqual(c, message.Body["c"], "generic map entry: c");
                        Assert.AreEqual(d, message.Body["d"], "generic map entry: d");
                        Assert.AreEqual(e, message.Body["e"], "generic map entry: e");
                        Assert.AreEqual(f, message.Body["f"], "generic map entry: f");
                        Assert.AreEqual(g, message.Body["g"], "generic map entry: g");
                        Assert.AreEqual(h, message.Body["h"], "generic map entry: h");
                        Assert.AreEqual(i, message.Body["i"], "generic map entry: i");
                        Assert.AreEqual(j, message.Body["j"], "generic map entry: j");
                        Assert.AreEqual(k, message.Body["k"], "generic map entry: k");
                        Assert.AreEqual(l, message.Body["l"], "generic map entry: l");
                        Assert.AreEqual(m, message.Body["m"], "generic map entry: m");
                        Assert.AreEqual(n, message.Body["n"], "generic map entry: n");
                        Assert.AreEqual(o, message.Body["o"], "generic map entry: o");

                        // use type safe APIs
                        Assert.AreEqual(a, message.Body.GetBool("a"), "map entry: a");
                        Assert.AreEqual(b, message.Body.GetByte("b"), "map entry: b");
                        Assert.AreEqual(c, message.Body.GetChar("c"), "map entry: c");
                        Assert.AreEqual(d, message.Body.GetShort("d"), "map entry: d");
                        Assert.AreEqual(e, message.Body.GetInt("e"), "map entry: e");
                        Assert.AreEqual(f, message.Body.GetLong("f"), "map entry: f");
                        Assert.AreEqual(g, message.Body.GetString("g"), "map entry: g");
                        Assert.AreEqual(h, message.Body.GetBool("h"), "map entry: h");
                        Assert.AreEqual(i, message.Body.GetByte("i"), "map entry: i");
                        Assert.AreEqual(j, message.Body.GetShort("j"), "map entry: j");
                        Assert.AreEqual(k, message.Body.GetInt("k"), "map entry: k");
                        Assert.AreEqual(l, message.Body.GetLong("l"), "map entry: l");
                        Assert.AreEqual(m, message.Body.GetFloat("m"), "map entry: m");
                        Assert.AreEqual(n, message.Body.GetDouble("n"), "map entry: n");
                        Assert.AreEqual(o, message.Body.GetBytes("o"), "map entry: o");
                    }
                }
            }
        }

        [Test]
        public void SendReceiveNestedMapMessage(
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
                        try
                        {
                            producer.DeliveryMode = deliveryMode;
                            IMapMessage request = session.CreateMapMessage();
                            const string textFieldValue = "Nested Map Messages Rule!";

                            request.Body.SetString("textField", textFieldValue);

                            IDictionary grandChildMap = new Hashtable();
                            grandChildMap["x"] = "abc";
                            grandChildMap["y"] = new ArrayList(new object[] {"a", "b", "c"});

                            IDictionary nestedMap = new Hashtable();
                            nestedMap["a"] = "foo";
                            nestedMap["b"] = (int) 23;
                            nestedMap["c"] = (long) 45;
                            nestedMap["d"] = grandChildMap;

                            request.Body.SetDictionary("mapField", nestedMap);
                            request.Body.SetList("listField", new ArrayList(new Object[] {"a", "b", "c"}));

                            producer.Send(request);

                            IMapMessage message = consumer.Receive(receiveTimeout) as IMapMessage;
                            Assert.IsNotNull(message, "No message returned!");
                            Assert.AreEqual(request.Body.Count, message.Body.Count, "Invalid number of message maps.");
                            Assert.AreEqual(deliveryMode, message.NMSDeliveryMode, "NMSDeliveryMode does not match");

                            string textFieldResponse = message.Body.GetString("textField");
                            Assert.AreEqual(textFieldValue, textFieldResponse, "textField does not match.");

                            IDictionary nestedMapResponse = message.Body.GetDictionary("mapField");
                            Assert.IsNotNull(nestedMapResponse, "Nested map not returned.");
                            Assert.AreEqual(nestedMap.Count, nestedMapResponse.Count,
                                "nestedMap: Wrong number of elements");
                            Assert.AreEqual("foo", nestedMapResponse["a"], "nestedMap: a");
                            Assert.AreEqual(23, nestedMapResponse["b"], "nestedMap: b");
                            Assert.AreEqual(45, nestedMapResponse["c"], "nestedMap: c");

                            IDictionary grandChildMapResponse = nestedMapResponse["d"] as IDictionary;
                            Assert.IsNotNull(grandChildMapResponse, "Grand child map not returned.");
                            Assert.AreEqual(grandChildMap.Count, grandChildMapResponse.Count,
                                "grandChildMap: Wrong number of elements");
                            Assert.AreEqual(grandChildMapResponse["x"], "abc", "grandChildMap: x");

                            IList grandChildList = grandChildMapResponse["y"] as IList;
                            Assert.IsNotNull(grandChildList, "Grand child list not returned.");
                            Assert.AreEqual(3, grandChildList.Count, "grandChildList: Wrong number of list elements.");
                            Assert.AreEqual("a", grandChildList[0], "grandChildList: a");
                            Assert.AreEqual("b", grandChildList[1], "grandChildList: b");
                            Assert.AreEqual("c", grandChildList[2], "grandChildList: c");

                            IList listFieldResponse = message.Body.GetList("listField");
                            Assert.IsNotNull(listFieldResponse, "Nested list not returned.");
                            Assert.AreEqual(3, listFieldResponse.Count,
                                "listFieldResponse: Wrong number of list elements.");
                            Assert.AreEqual("a", listFieldResponse[0], "listFieldResponse: a");
                            Assert.AreEqual("b", listFieldResponse[1], "listFieldResponse: b");
                            Assert.AreEqual("c", listFieldResponse[2], "listFieldResponse: c");
                        }
                        catch (NotSupportedException)
                        {
                        }
                        catch (NMSException e)
                        {
                            Assert.IsTrue(e.InnerException.GetType() == typeof(NotSupportedException));
                        }
                    }
                }
            }
        }
    }
}