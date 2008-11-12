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
using NUnit.Framework.Extensions;

namespace Apache.NMS.Test
{
	[TestFixture]
	public class NMSPropertyTest : NMSTestSupport
	{
		protected static string DESTINATION_NAME = "NMSPropsDestination";
		protected static string TEST_CLIENT_ID = "NMSPropsClientId";

		// standard NMS properties
		protected string expectedText = "Hey this works!";
		protected string correlationID = "FooBar";
		protected byte priority = 4;
		protected String type = "FooType";
		protected String groupID = "BarGroup";
		protected int groupSeq = 1;

		[RowTest]
		[Row(true)]
		[Row(false)]
		public void SendReceiveNMSProperties(bool persistent)
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
						producer.Priority = priority;
						producer.Persistent = persistent;
						producer.RequestTimeout = receiveTimeout;
						ITextMessage request = session.CreateTextMessage(expectedText);
						ITemporaryQueue replyTo = session.CreateTemporaryQueue();

						// Set the headers
						request.NMSCorrelationID = correlationID;
						request.NMSReplyTo = replyTo;
						request.NMSType = type;
						request.Properties["NMSXGroupID"] = groupID;
						request.Properties["NMSXGroupSeq"] = groupSeq;

						producer.Send(request);

						ITextMessage message = consumer.Receive(receiveTimeout) as ITextMessage;

						Assert.IsNotNull(message, "Did not receive an ITextMessage!");
						Assert.AreEqual(expectedText, message.Text, "Message text does not match.");

						// compare standard NMS headers
						Assert.AreEqual(correlationID, message.NMSCorrelationID, "NMSCorrelationID does not match");
						Assert.AreEqual(persistent, message.NMSPersistent, "NMSPersistent does not match");
						Assert.AreEqual(priority, message.NMSPriority, "NMSPriority does not match");
						Assert.AreEqual(type, message.NMSType, "NMSType does not match");
						Assert.AreEqual(groupID, message.Properties["NMSXGroupID"], "NMSXGroupID does not match");
						Assert.AreEqual(groupSeq, message.Properties["NMSXGroupSeq"], "NMSXGroupSeq does not match");
						Assert.AreEqual(replyTo, message.NMSReplyTo, "NMSReplyTo does not match");
					}
				}
			}
		}
	}
}
