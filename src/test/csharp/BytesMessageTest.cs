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
using NUnit.Framework.Extensions;

namespace Apache.NMS.Test
{
	[TestFixture]
	public class BytesMessageTest : NMSTestSupport
	{
		protected static string DESTINATION_NAME = "BytesMessageDestination";
		protected static string TEST_CLIENT_ID = "BytesMessageClientId";
		protected byte[] msgContent = {1, 2, 3, 4, 5, 6, 7, 8};

		[RowTest]
		[Row(true)]
		[Row(false)]
		public void SendReceiveBytesMessage(bool persistent)
		{
			using(IConnection connection = CreateConnection(TEST_CLIENT_ID))
			{
				connection.Start();
				using(ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
				{
					IDestination destination = SessionUtil.GetDestination(session, DESTINATION_NAME);
					using(IMessageConsumer consumer = session.CreateConsumer(destination))
					using(IMessageProducer producer = session.CreateProducer(destination))
					{
						producer.Persistent = persistent;
						producer.RequestTimeout = receiveTimeout;
						IMessage request = session.CreateBytesMessage(msgContent);
						producer.Send(request);

						IMessage message = consumer.Receive(receiveTimeout);
						AssertBytesMessageEqual(request, message);
						Assert.AreEqual(persistent, message.NMSPersistent, "NMSPersistent does not match");
					}
				}
			}
		}

		/// <summary>
		/// Assert that two messages are IBytesMessages and their contents are equal.
		/// </summary>
		/// <param name="expected"></param>
		/// <param name="actual"></param>
		protected void AssertBytesMessageEqual(IMessage expected, IMessage actual)
		{
			IBytesMessage expectedBytesMsg = expected as IBytesMessage;
			Assert.IsNotNull(expectedBytesMsg, "'expected' message not a bytes message");
			IBytesMessage actualBytesMsg = actual as IBytesMessage;
			Assert.IsNotNull(actualBytesMsg, "'actual' message not a bytes message");
			Assert.AreEqual(expectedBytesMsg.Content, actualBytesMsg.Content, "Bytes message contents do not match.");
		}
	}
}
