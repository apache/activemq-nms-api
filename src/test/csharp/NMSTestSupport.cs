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
using Apache.NMS;
using NUnit.Framework;
using System;

namespace Apache.NMS.Test
{
	/// <summary>
	/// useful base class for test cases
	/// </summary>
	[TestFixture]
	public abstract class NMSTestSupport
	{
		protected static object destinationLock = new object();
		protected static int destinationCounter;

		// enable/disable logging of message flows
		protected bool logging = true;

		private IConnectionFactory factory;
		private IConnection connection;
		private ISession session;
		private IDestination destination;

		protected TimeSpan receiveTimeout = TimeSpan.FromMilliseconds(1000);
		protected string clientId;
		protected bool persistent = true;
		protected DestinationType destinationType = DestinationType.Queue;
		protected AcknowledgementMode acknowledgementMode = AcknowledgementMode.ClientAcknowledge;

		[SetUp]
		public virtual void SetUp()
		{
		}

		[TearDown]
		public virtual void TearDown()
		{
			destination = null;
			Disconnect();
		}

		// Properties
		public bool Connected
		{
			get { return connection != null; }
			set
			{
				if(value)
				{
					Connect();
				}
				else
				{
					Disconnect();
				}
			}
		}

		public IConnectionFactory Factory
		{
			get
			{
				if(factory == null)
				{
					factory = CreateConnectionFactory();
					Assert.IsNotNull(factory, "no factory created");
				}
				return factory;
			}
			set { this.factory = value; }
		}

		public IConnection Connection
		{
			get
			{
				if(connection == null)
				{
					Connect();
				}
				return connection;
			}
			set { this.connection = value; }
		}

		public ISession Session
		{
			get
			{
				if(session == null)
				{
					session = Connection.CreateSession(acknowledgementMode);
					Assert.IsNotNull(connection != null, "no session created");
				}
				return session;
			}
			set { this.session = value; }
		}

		protected virtual void Connect()
		{
			WriteLine("Connecting...");
			connection = CreateConnection();
			Assert.IsNotNull(connection, "no connection created");
			connection.Start();
			WriteLine("Connected.");
			Assert.IsNotNull(connection, "no connection created");
		}

		protected virtual void Disconnect()
		{
			if(session != null)
			{
				session.Dispose();
				session = null;
			}

			if(connection != null)
			{
				WriteLine("Disconnecting...");
				connection.Dispose();
				connection = null;
				WriteLine("Disconnected.");
			}
		}

		protected virtual void Reconnect()
		{
			Disconnect();
			Connect();
		}

		protected virtual void Drain()
		{
			using(ISession drainSession = Connection.CreateSession())
			{
				// Tries to consume any messages on the Destination
				IMessageConsumer consumer = drainSession.CreateConsumer(CreateDestination(drainSession));

				// Should only need to wait for first message to arrive due to the way
				// prefetching works.
				while(consumer.Receive(receiveTimeout) != null)
				{
				}
			}
		}

		public virtual void SendAndSyncReceive()
		{
			using(ISession sendSession = Connection.CreateSession())
			{
				IDestination sendDestination = CreateDestination(sendSession);
				IMessageConsumer consumer = sendSession.CreateConsumer(sendDestination);
				IMessageProducer producer = sendSession.CreateProducer(sendDestination);
				producer.Persistent = persistent;

				IMessage request = sendSession.CreateMessage();
				producer.Send(request);

				IMessage message = consumer.Receive(receiveTimeout);
				Assert.IsNotNull(message, "No message returned!");
				AssertValidMessage(message);
			}
		}

		protected abstract IConnectionFactory CreateConnectionFactory();

		protected virtual IConnection CreateConnection()
		{
			IConnection newConnection = Factory.CreateConnection();
			if(clientId != null)
			{
				newConnection.ClientId = clientId;
			}
			return newConnection;
		}

		protected virtual IMessageProducer CreateProducer()
		{
			return Session.CreateProducer(Destination);
		}

		protected virtual IMessageConsumer CreateConsumer()
		{
			return Session.CreateConsumer(Destination);
		}

		protected virtual IDestination CreateDestination()
		{
			return CreateDestination(Session);
		}

		protected virtual IDestination CreateDestination(ISession curSession)
		{
			if(destinationType == DestinationType.Queue)
			{
				return curSession.GetQueue(CreateDestinationName());
			}
			else if(destinationType == DestinationType.Topic)
			{
				return curSession.GetTopic(CreateDestinationName());
			}
			else if(destinationType == DestinationType.TemporaryQueue)
			{
				return curSession.CreateTemporaryQueue();
			}
			else if(destinationType == DestinationType.TemporaryTopic)
			{
				return curSession.CreateTemporaryTopic();
			}
			else
			{
				throw new Exception("Unknown destination type: " + destinationType);
			}
		}

		protected virtual string CreateDestinationName()
		{
			return "Test.DotNet." + GetType().Name + "." + NextNumber.ToString();
		}

		public static int NextNumber
		{
			get
			{
				lock(destinationLock)
				{
					return ++destinationCounter;
				}
			}
		}

		protected virtual IMessage CreateMessage()
		{
			return Session.CreateMessage();
		}

		protected virtual void AssertValidMessage(IMessage message)
		{
			Assert.IsNotNull(message, "Null Message!");
		}

		public IDestination Destination
		{
			get
			{
				if(destination == null)
				{
					destination = CreateDestination();
					Assert.IsNotNull(destination, "No destination available!");
					WriteLine("Using destination: " + destination);
				}
				return destination;
			}
			set { destination = value; }
		}

		protected virtual void WriteLine(string text)
		{
			if(logging)
			{
				Console.WriteLine();
				Console.WriteLine("#### : " + text);
			}
		}
	}
}