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
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Extensions;

namespace Apache.NMS.Test
{
	[TestFixture]
	public class ConnectionTest : NMSTestSupport
	{
        IConnection startedConnection = null;
        IConnection stoppedConnection = null;
        
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            startedConnection = CreateConnection(null);
            startedConnection.Start();
            stoppedConnection = CreateConnection(null);
        }

        [TearDown]
        public override void TearDown()
        {
            startedConnection.Close();
            stoppedConnection.Close();
            
            base.TearDown();            
        }
        
		/// <summary>
		/// Verify that it is possible to create multiple connections to the broker.
		/// There was a bug in the connection factory which set the clientId member which made
		/// it impossible to create an additional connection.
		/// </summary>
		[Test]
		public void TwoConnections()
		{
			using(IConnection connection1 = CreateConnection(null))
			{
				connection1.Start();
				using(IConnection connection2 = CreateConnection(null))
				{
					// with the bug present we'll get an exception in connection2.start()
					connection2.Start();
				}
			}
		}

		[RowTest]
		[Row(true)]
		[Row(false)]
		public void CreateAndDisposeWithConsumer(bool disposeConsumer)
		{
			using(IConnection connection = CreateConnection("DisposalTestConnection"))
			{
				connection.Start();

				using(ISession session = connection.CreateSession())
				{
					IQueue queue = session.GetQueue("DisposalTestQueue");
					IMessageConsumer consumer = session.CreateConsumer(queue);

					connection.Stop();
					if(disposeConsumer)
					{
						consumer.Dispose();
					}
				}
			}
		}

		[RowTest]
		[Row(true)]
		[Row(false)]
		public void CreateAndDisposeWithProducer(bool disposeProducer)
		{
			using(IConnection connection = CreateConnection("DisposalTestConnection"))
			{
				connection.Start();

				using(ISession session = connection.CreateSession())
				{
					IQueue queue = session.GetQueue("DisposalTestQueue");
					IMessageProducer producer = session.CreateProducer(queue);

					connection.Stop();
					if(disposeProducer)
					{
						producer.Dispose();
					}
				}
			}
		}

        /// <summary>
        /// Tests if the consumer receives the messages that were sent before the
        /// connection was started. 
        /// </summary>
        [Test]
        public void TestStoppedConsumerHoldsMessagesTillStarted()
        {
            ISession startedSession = startedConnection.CreateSession(AcknowledgementMode.AutoAcknowledge);
            ISession stoppedSession = stoppedConnection.CreateSession(AcknowledgementMode.AutoAcknowledge);
    
            // Setup the consumers.
            ITopic topic = startedSession.GetTopic("ConnectionTestTopic");
            IMessageConsumer startedConsumer = startedSession.CreateConsumer(topic);
            IMessageConsumer stoppedConsumer = stoppedSession.CreateConsumer(topic);
    
            // Send the message.
            IMessageProducer producer = startedSession.CreateProducer(topic);
            ITextMessage message = startedSession.CreateTextMessage("Hello");
            producer.Send(message);
    
            // Test the assertions.
            IMessage m = startedConsumer.Receive(TimeSpan.FromMilliseconds(1000));
            Assert.IsNotNull(m);
    
            m = stoppedConsumer.Receive(TimeSpan.FromMilliseconds(1000));
            Assert.IsNull(m);
    
            stoppedConnection.Start();
            m = stoppedConsumer.Receive(TimeSpan.FromMilliseconds(5000));
            Assert.IsNotNull(m);
    
            startedSession.Close();
            stoppedSession.Close();
        }
    
        /// <summary>
        /// Tests if the consumer is able to receive messages eveb when the
        /// connecction restarts multiple times.
        /// </summary>
        [Test]
        public void TestMultipleConnectionStops()
        {
            TestStoppedConsumerHoldsMessagesTillStarted();
            stoppedConnection.Stop();
            TestStoppedConsumerHoldsMessagesTillStarted();
            stoppedConnection.Stop();
            TestStoppedConsumerHoldsMessagesTillStarted();
        }        
	}
}
