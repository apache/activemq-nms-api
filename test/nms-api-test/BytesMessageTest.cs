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
    public class BytesMessageTest : NMSTestSupport
    {
        protected byte[] msgContent = {1, 2, 3, 4, 5, 6, 7, 8};

        [Test]
        public void SendReceiveBytesMessage(
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
                        IMessage request = session.CreateBytesMessage(msgContent);
                        producer.Send(request);

                        IMessage message = consumer.Receive(receiveTimeout);
                        AssertMessageIsReadOnly(message);
                        AssertBytesMessageEqual(request, message);
                        Assert.AreEqual(deliveryMode, message.NMSDeliveryMode, "NMSDeliveryMode does not match");
                    }
                }
            }
        }

        [Test]
        public void SendReceiveBytesMessageContent(
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
                        IBytesMessage request = session.CreateBytesMessage();

                        request.WriteBoolean(true);
                        request.WriteByte((byte) 1);
                        request.WriteBytes(new byte[1]);
                        request.WriteBytes(new byte[3], 0, 2);
                        request.WriteChar('a');
                        request.WriteDouble(1.5);
                        request.WriteSingle((float) 1.5);
                        request.WriteInt32(1);
                        request.WriteInt64(1);
                        request.WriteObject("stringobj");
                        request.WriteInt16((short) 1);
                        request.WriteString("utfstring");

                        producer.Send(request);

                        IMessage message = consumer.Receive(receiveTimeout);
                        AssertMessageIsReadOnly(message);
                        AssertBytesMessageEqual(request, message);
                        Assert.AreEqual(deliveryMode, message.NMSDeliveryMode, "NMSDeliveryMode does not match");
                    }
                }
            }
        }

        protected void AssertMessageIsReadOnly(IMessage message)
        {
            Type writeableExceptionType = typeof(MessageNotWriteableException);
            IBytesMessage theMessage = message as IBytesMessage;
            Assert.IsNotNull(theMessage);
            Assert.Throws(writeableExceptionType, delegate() { theMessage.WriteBoolean(true); });
            Assert.Throws(writeableExceptionType, delegate() { theMessage.WriteByte((byte) 1); });
            Assert.Throws(writeableExceptionType, delegate() { theMessage.WriteBytes(new byte[1]); });
            Assert.Throws(writeableExceptionType, delegate() { theMessage.WriteBytes(new byte[3], 0, 2); });
            Assert.Throws(writeableExceptionType, delegate() { theMessage.WriteChar('a'); });
            Assert.Throws(writeableExceptionType, delegate() { theMessage.WriteDouble(1.5); });
            Assert.Throws(writeableExceptionType, delegate() { theMessage.WriteSingle((float) 1.5); });
            Assert.Throws(writeableExceptionType, delegate() { theMessage.WriteInt32(1); });
            Assert.Throws(writeableExceptionType, delegate() { theMessage.WriteInt64(1); });
            Assert.Throws(writeableExceptionType, delegate() { theMessage.WriteObject("stringobj"); });
            Assert.Throws(writeableExceptionType, delegate() { theMessage.WriteInt16((short) 1); });
            Assert.Throws(writeableExceptionType, delegate() { theMessage.WriteString("utfstring"); });
        }

        /// <summary>
        /// Assert that two messages are IBytesMessages and their contents are equal.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        protected void AssertBytesMessageEqual(IMessage expected, IMessage actual)
        {
            IBytesMessage expectedBytesMsg = expected as IBytesMessage;
            expectedBytesMsg.Reset();
            Assert.IsNotNull(expectedBytesMsg, "'expected' message not a bytes message");
            IBytesMessage actualBytesMsg = actual as IBytesMessage;
            Assert.IsNotNull(actualBytesMsg, "'actual' message not a bytes message");
            Assert.AreEqual(expectedBytesMsg.Content, actualBytesMsg.Content, "Bytes message contents do not match.");
        }
    }
}