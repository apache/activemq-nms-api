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
using NMS;
using NUnit.Framework;
using ActiveMQ.Commands;
using ActiveMQ.Transport.Stomp;
using System;

namespace Stomp
{
    [ TestFixture ]
    public class StompHelperTest
    {
		[ Test ]
		public void ConsumerIdMarshallingWorks()
		{
			ConsumerId id = new ConsumerId();
			id.ConnectionId = "cheese";
			id.SessionId = 2;
			id.Value = 3;
			
			string text = StompHelper.ToStomp(id);
			Assert.AreEqual("cheese:2:3", text, "ConsumerId as stomp");
			
			ConsumerId another = StompHelper.ToConsumerId("abc:5:6");
			Assert.AreEqual("abc", another.ConnectionId, "extracting consumerId.ConnectionId");
			Assert.AreEqual(5, another.SessionId, "extracting consumerId.SessionId");
			Assert.AreEqual(6, another.Value, "extracting consumerId.Value");
		}

		[ Test ]
		public void MessageIdMarshallingWorks()
		{
			ProducerId id = new ProducerId();
			id.ConnectionId = "cheese";
			id.SessionId = 2;
			id.Value = 3;
			
			MessageId mid = new MessageId();
			mid.ProducerId = id;
			mid.BrokerSequenceId = 5;
			mid.ProducerSequenceId = 6;
			
			string text = StompHelper.ToStomp(mid);
			Assert.AreEqual("cheese:2:3:5:6", text, "MessageId as stomp");
			
			MessageId mid2 = StompHelper.ToMessageId("abc:5:6:7:8");
			Assert.AreEqual(7, mid2.BrokerSequenceId, "extracting mid2.BrokerSequenceId");
			Assert.AreEqual(8, mid2.ProducerSequenceId, "extracting mid2.ProducerSequenceId");

			ProducerId another = mid2.ProducerId;
			Assert.AreEqual("abc", another.ConnectionId, "extracting producerId.ConnectionId");
			Assert.AreEqual(5, another.SessionId, "extracting producerId.SessionId");
			Assert.AreEqual(6, another.Value, "extracting producerId.Value");
		}

		// TODO destination stuff
		
		[ Test ]
		public void DestinationMarshallingWorks()
		{
			Assert.AreEqual("/queue/FOO.BAR", StompHelper.ToStomp(new ActiveMQQueue("FOO.BAR")), "queue");
			Assert.AreEqual("/topic/FOO.BAR", StompHelper.ToStomp(new ActiveMQTopic("FOO.BAR")), "topic");
			Assert.AreEqual("/temp-queue/FOO.BAR", StompHelper.ToStomp(new ActiveMQTempQueue("FOO.BAR")), "temporary queue");
			Assert.AreEqual("/temp-topic/FOO.BAR", StompHelper.ToStomp(new ActiveMQTempTopic("FOO.BAR")), "temporary topic");
			
			Assert.AreEqual(new ActiveMQQueue("FOO.BAR"), StompHelper.ToDestination("/queue/FOO.BAR"), "queue from Stomp");
			Assert.AreEqual(new ActiveMQTopic("FOO.BAR"), StompHelper.ToDestination("/topic/FOO.BAR"), "topic from Stomp");
			Assert.AreEqual(new ActiveMQTempQueue("FOO.BAR"), StompHelper.ToDestination("/temp-queue/FOO.BAR"), "temporary queue from Stomp");
			Assert.AreEqual(new ActiveMQTempTopic("FOO.BAR"), StompHelper.ToDestination("/temp-topic/FOO.BAR"), "temporary topic from Stomp");
		}
    }
}



