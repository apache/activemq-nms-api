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
    public class RequestResponseTest : NMSTestSupport
    {
        [Test]
        [Category("RequestResponse")]
        public void TestRequestResponseMessaging()
        {
            using (IConnection connection = CreateConnection())
            {
                connection.Start();
                using (ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
                {
                    IDestination destination = CreateDestination(session, DestinationType.Queue);
                    ITemporaryQueue replyTo = session.CreateTemporaryQueue();

                    using (IMessageConsumer consumer = session.CreateConsumer(destination))
                    using (IMessageProducer producer = session.CreateProducer(destination))
                    {
                        IMessage request = session.CreateMessage();

                        request.NMSReplyTo = replyTo;

                        producer.Send(request);

                        request = consumer.Receive(TimeSpan.FromMilliseconds(3000));
                        Assert.IsNotNull(request);
                        Assert.IsNotNull(request.NMSReplyTo);

                        using (IMessageProducer responder = session.CreateProducer(request.NMSReplyTo))
                        {
                            IMessage response = session.CreateTextMessage("RESPONSE");
                            responder.Send(response);
                        }
                    }

                    using (IMessageConsumer consumer = session.CreateConsumer(replyTo))
                    {
                        ITextMessage response = consumer.Receive(TimeSpan.FromMilliseconds(3000)) as ITextMessage;
                        Assert.IsNotNull(response);
                        Assert.AreEqual("RESPONSE", response.Text);
                    }
                }
            }
        }
    }
}