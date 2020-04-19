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

namespace Apache.NMS.Test
{
    [TestFixture]
    public class ProducerTest : NMSTestSupport
    {
        [Test]
        public void TestProducerSendToNullDestinationWithoutDefault()
        {
            using (IConnection connection = CreateConnection(GetTestClientId()))
            {
                connection.Start();
                using (ISession session = connection.CreateSession())
                {
                    IMessageProducer producer = session.CreateProducer(null);

                    try
                    {
                        producer.Send(null, session.CreateTextMessage("Message"));
                        Assert.Fail("Producer should have thrown an NotSupportedException");
                    }
                    catch (NotSupportedException)
                    {
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail("Wrong Exception Type Thrown: " + ex.GetType().Name);
                    }
                }
            }
        }

        [Test]
        public void TestProducerSendToNullDestinationWithDefault()
        {
            using (IConnection connection = CreateConnection(GetTestClientId()))
            {
                connection.Start();
                using (ISession session = connection.CreateSession())
                {
                    IDestination unusedDest = session.CreateTemporaryQueue();

                    IMessageProducer producer = session.CreateProducer(unusedDest);

                    try
                    {
                        producer.Send(null, session.CreateTextMessage("Message"));
                        Assert.Fail("Producer should have thrown an InvalidDestinationException");
                    }
                    catch (InvalidDestinationException)
                    {
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail("Wrong Exception Type Thrown: " + ex.GetType().Name);
                    }
                }
            }
        }

        [Test]
        public void TestProducerSendToNonDefaultDestination()
        {
            using (IConnection connection = CreateConnection(GetTestClientId()))
            {
                connection.Start();
                using (ISession session = connection.CreateSession())
                {
                    IDestination unusedDest = session.CreateTemporaryQueue();
                    IDestination usedDest = session.CreateTemporaryQueue();

                    IMessageProducer producer = session.CreateProducer(unusedDest);

                    try
                    {
                        producer.Send(usedDest, session.CreateTextMessage("Message"));
                        Assert.Fail("Producer should have thrown an NotSupportedException");
                    }
                    catch (NotSupportedException)
                    {
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail("Wrong Exception Type Thrown: " + ex.GetType().Name);
                    }
                }
            }
        }
    }
}