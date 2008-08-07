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
using Apache.NMS.Util;
using NUnit.Framework;
using System;
using System.IO;
using System.Xml;
using System.Collections;

namespace Apache.NMS.Test
{
	/// <summary>
	/// useful base class for test cases
	/// </summary>
	[TestFixture]
	public abstract class NMSTestSupport
	{
		[Obsolete]
		protected static object destinationLock = new object();
		[Obsolete]
		protected static int destinationCounter;

		private NMSConnectionFactory NMSFactory;
		[Obsolete]
		private IConnection connection;
		[Obsolete]
		private ISession session;
		[Obsolete]
		private IDestination destination;

		protected TimeSpan receiveTimeout = TimeSpan.FromMilliseconds(5000);
		protected virtual string clientId { get; set; }
		protected virtual string passWord { get; set; }
		protected virtual string userName { get; set; }
		[Obsolete]
		protected bool persistent = true;
		[Obsolete]
		protected DestinationType destinationType = DestinationType.Queue;
		[Obsolete]
		protected AcknowledgementMode acknowledgementMode = AcknowledgementMode.ClientAcknowledge;

		public NMSTestSupport()
		{
			clientId = "NMSUnitTests";
		}

		[SetUp]
		public virtual void SetUp()
		{
		}

		[TearDown]
		public virtual void TearDown()
		{
			Disconnect();
		}

		// Properties
		[Obsolete]
		public bool Connected
		{
			get { return connection != null; }
			set
			{
				if(value)
				{
					if(connection == null)
					{
						Connect();
					}
				}
				else
				{
					Disconnect();
				}
			}
		}

		/// <summary>
		/// The connection factory interface property.
		/// </summary>
		public IConnectionFactory Factory
		{
			get
			{
				if(null == NMSFactory)
				{
					Assert.IsTrue(CreateNMSFactory(), "Error creating factory.");
				}

				return NMSFactory.ConnectionFactory;
			}
		}

		[Obsolete]
		public IConnection Connection
		{
			get
			{
				if(null == connection)
				{
					Connect();
				}

				return connection;
			}
		}

		[Obsolete]
		public ISession Session
		{
			get
			{
				if(null == session)
				{
					session = Connection.CreateSession(acknowledgementMode);
					Assert.IsNotNull(session, "no session created");
				}

				return session;
			}
		}

		[Obsolete]
		protected virtual void Connect()
		{
			Console.WriteLine("Connecting...");
			connection = CreateConnection(this.clientId);
			Assert.IsNotNull(connection, "no connection created");
			connection.Start();
			Console.WriteLine("Connected.");
		}

		[Obsolete]
		protected virtual void Disconnect()
		{
			destination = null;
			if(session != null)
			{
				session.Dispose();
				session = null;
			}

			if(connection != null)
			{
				Console.WriteLine("Disconnecting...");
				connection.Dispose();
				connection = null;
				Console.WriteLine("Disconnected.");
			}
		}

		[Obsolete]
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

		[Obsolete]
		public virtual void SendAndSyncReceive()
		{
			// IDestination sendDestination = CreateDestination(Session);
			IMessageConsumer consumer = Session.CreateConsumer(Destination);
			IMessageProducer producer = Session.CreateProducer(Destination);

			producer.Persistent = persistent;

			IMessage request = CreateMessage();
			producer.Send(request);

			IMessage message = consumer.Receive(receiveTimeout);
			Assert.IsNotNull(message, "No message returned!");
			AssertValidMessage(message);
		}

		/// <summary>
		/// Name of the connection configuration filename.
		/// </summary>
		/// <returns></returns>
		protected virtual string GetConnectionConfigFileName() { return "nmsprovider-test.config"; }

		/// <summary>
		/// The name of the connection configuration that CreateNMSFactory() will load.
		/// </summary>
		/// <returns></returns>
		protected virtual string GetNameTestURI() { return "defaultURI"; }

		/// <summary>
		/// Create the NMS Factory that can create NMS Connections.
		/// </summary>
		/// <returns></returns>
		protected bool CreateNMSFactory()
		{
			return CreateNMSFactory(GetNameTestURI());
		}

		/// <summary>
		/// Create the NMS Factory that can create NMS Connections.  This function loads the
		/// connection settings from the configuration file.
		/// </summary>
		/// <param name="nameTestURI">The named connection configuration.</param>
		/// <returns></returns>
		protected bool CreateNMSFactory(string nameTestURI)
		{
			Uri brokerUri = null;
			object[] factoryParams = null;
			string connectionConfigFileName = GetConnectionConfigFileName();

			Assert.IsTrue(File.Exists(connectionConfigFileName), "Connection configuration file does not exist.");
			XmlDocument configDoc = new XmlDocument();

			configDoc.Load(connectionConfigFileName);
			XmlElement uriNode = (XmlElement) configDoc.SelectSingleNode(String.Format("/configuration/{0}", nameTestURI));

			if(null != uriNode)
			{
				brokerUri = new Uri(uriNode.GetAttribute("value"));
				factoryParams = GetFactoryParams(uriNode);
	
				XmlElement userNameNode = (XmlElement) uriNode.SelectSingleNode("userName");

				if(null != userNameNode)
				{
					userName = userNameNode.GetAttribute("value");
				}
				else
				{
					userName = "guest";
				}

				XmlElement passWordNode = (XmlElement) uriNode.SelectSingleNode("passWord");

				if(null != passWordNode)
				{
					passWord = passWordNode.GetAttribute("value");
				}
				else
				{
					passWord = "guest";
				}

				if(null == factoryParams)
				{
					NMSFactory = new Apache.NMS.NMSConnectionFactory(brokerUri);
				}
				else
				{
					NMSFactory = new Apache.NMS.NMSConnectionFactory(brokerUri, factoryParams);
				}
			}

			return (null != NMSFactory);
		}

		/// <summary>
		/// Get the parameters for the ConnectionFactory from the configuration file.
		/// </summary>
		/// <param name="uriNode">Parent node of the factoryParams node.</param>
		/// <returns>Object array of parameter objects to be passsed to provider factory object.  Null if no parameters are specified in configuration file.</returns>
		protected object[] GetFactoryParams(XmlElement uriNode)
		{
			ArrayList factoryParams = new ArrayList();
			XmlElement factoryParamsNode = (XmlElement) uriNode.SelectSingleNode("factoryParams");

			if(null != factoryParamsNode)
			{
				XmlNodeList nodeList = factoryParamsNode.SelectNodes("param");

				if(null != nodeList)
				{
					foreach(XmlElement paramNode in nodeList)
					{
						string paramType = paramNode.GetAttribute("type");
						string paramValue = paramNode.GetAttribute("value");

						switch(paramType)
						{
						case "string":
							factoryParams.Add(paramValue);
						break;

						case "int":
							factoryParams.Add(int.Parse(paramValue));
						break;

						// TODO: Add more parameter types
						}
					}
				}
			}

			if(factoryParams.Count > 0)
			{
				return factoryParams.ToArray();
			}

			return null;
		}

		/// <summary>
		/// Create a new connection to the broker.
		/// </summary>
		/// <param name="newClientId">Client ID of the new connection.</param>
		/// <returns></returns>
		public virtual IConnection CreateConnection(string newClientId)
		{
			IConnection newConnection = Factory.CreateConnection(userName, passWord);
			Assert.IsNotNull(newConnection, "connection not created");
			if(newClientId != null)
			{
				newConnection.ClientId = newClientId;
			}

			return newConnection;
		}

		/// <summary>
		/// Register a durable consumer
		/// </summary>
		/// <param name="connectionID">Connection ID of the consumer.</param>
		/// <param name="destination">Destination name to register.  Supports embedded prefix names.</param>
		/// <param name="consumerID">Name of the durable consumer.</param>
		/// <param name="selector">Selector parameters for consumer.</param>
		/// <param name="noLocal"></param>
		protected void RegisterDurableConsumer(string connectionID, string destination, string consumerID, string selector, bool noLocal)
		{
			using(IConnection connection = CreateConnection(connectionID))
			{
				connection.Start();
				using(ISession session = connection.CreateSession(AcknowledgementMode.DupsOkAcknowledge))
				{
					ITopic destinationTopic = SessionUtil.GetTopic(session, destination);
					Assert.IsNotNull(destinationTopic, "Could not get destination topic.");
					using(IMessageConsumer consumer = session.CreateDurableConsumer(destinationTopic, consumerID, selector, noLocal, receiveTimeout))
					{
					}
				}
			}
		}

		/// <summary>
		/// Unregister a durable consumer for the given connection ID.
		/// </summary>
		/// <param name="connectionID">Connection ID of the consumer.</param>
		/// <param name="consumerID">Name of the durable consumer.</param>
		protected void UnregisterDurableConsumer(string connectionID, string consumerID)
		{
			using(IConnection connection = CreateConnection(connectionID))
			{
				connection.Start();
				using(ISession session = connection.CreateSession(AcknowledgementMode.DupsOkAcknowledge))
				{
					session.DeleteDurableConsumer(consumerID, receiveTimeout);
				}
			}
		}

		[Obsolete]
		protected virtual IMessageProducer CreateProducer()
		{
			return Session.CreateProducer(Destination);
		}

		[Obsolete]
		protected virtual IMessageConsumer CreateConsumer()
		{
			return Session.CreateConsumer(Destination);
		}

		[Obsolete]
		protected virtual IDestination CreateDestination()
		{
			return CreateDestination(Session);
		}

		[Obsolete]
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

		[Obsolete]
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

		[Obsolete]
		protected virtual IMessage CreateMessage()
		{
			return Session.CreateMessage();
		}

		protected virtual void AssertValidMessage(IMessage message)
		{
			Assert.IsNotNull(message, "Null Message!");
		}

		[Obsolete]
		public IDestination Destination
		{
			get
			{
				if(destination == null)
				{
					destination = CreateDestination();
					Assert.IsNotNull(destination, "No destination available!");
					Console.WriteLine("Using destination: " + destination);
				}
				return destination;
			}
			set { destination = value; }
		}
	}
}
