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
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using Apache.NMS.Util;
using NUnit.Framework;

namespace Apache.NMS.Test
{
    /// <summary>
    /// useful base class for test cases
    /// </summary>
    public abstract class NMSTestSupport
    {
        private NMSConnectionFactory NMSFactory;
        protected TimeSpan receiveTimeout = TimeSpan.FromMilliseconds(15000);
        protected string clientId;
        protected string passWord;
        protected string userName;
        protected int testRun;
        protected int idCounter;

        static NMSTestSupport()
        {
            Apache.NMS.Tracer.Trace = new NmsTracer();
        }

        public NMSTestSupport()
        {
        }

        [SetUp]
        public virtual void SetUp()
        {
            this.testRun++;
        }

        [TearDown]
        public virtual void TearDown()
        {
        }

        // Properties

        /// <summary>
        /// The connection factory interface property.
        /// </summary>
        public IConnectionFactory Factory
        {
            get
            {
                if (null == NMSFactory)
                {
                    Assert.IsTrue(CreateNMSFactory(), "Error creating factory.");
                }

                return NMSFactory.ConnectionFactory;
            }
        }

        /// <summary>
        /// Name of the connection configuration filename.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetConnectionConfigFileName()
        {
            return "nmsprovider-test.config";
        }

        /// <summary>
        /// The name of the connection configuration that CreateNMSFactory() will load.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetNameTestURI()
        {
            return "defaultURI";
        }

        /// <summary>
        /// Create the NMS Factory that can create NMS Connections.
        /// </summary>
        /// <returns></returns>
        protected bool CreateNMSFactory()
        {
            return CreateNMSFactory(GetNameTestURI());
        }

        /// <summary>
        /// Return the configured URI String.  This function loads the connection
        /// settings from the configuration file.
        /// </summary>
        /// <returns></returns>
        protected string GetConfiguredConnectionURI()
        {
            Uri brokerUri = null;
            string[] paths = GetConfigSearchPaths();
            string connectionConfigFileName = GetConnectionConfigFileName();
            bool configFound = false;

            foreach (string path in paths)
            {
                string fullpath = Path.Combine(path, connectionConfigFileName);
                Tracer.Debug("\tScanning folder: " + path);

                if (File.Exists(fullpath))
                {
                    Tracer.Debug("\tAssembly found!");
                    connectionConfigFileName = fullpath;
                    configFound = true;
                    break;
                }
            }

            Assert.IsTrue(configFound, "Connection configuration file does not exist.");
            XmlDocument configDoc = new XmlDocument();

            configDoc.Load(connectionConfigFileName);
            XmlElement uriNode =
                (XmlElement) configDoc.SelectSingleNode(String.Format("/configuration/{0}", GetNameTestURI()));

            if (null != uriNode)
            {
                // Replace any environment variables embedded inside the string.
                brokerUri = new Uri(ReplaceEnvVar(uriNode.GetAttribute("value")));
            }

            return brokerUri.ToString();
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
            string[] paths = GetConfigSearchPaths();
            object[] factoryParams = null;
            string connectionConfigFileName = GetConnectionConfigFileName();
            bool configFound = false;

            foreach (string path in paths)
            {
                string fullpath = Path.Combine(path, connectionConfigFileName);
                Tracer.Debug("\tScanning folder: " + path);

                if (File.Exists(fullpath))
                {
                    Tracer.Debug("\tAssembly found!");
                    connectionConfigFileName = fullpath;
                    configFound = true;
                    break;
                }
            }

            Assert.IsTrue(configFound, "Connection configuration file does not exist.");
            XmlDocument configDoc = new XmlDocument();

            configDoc.Load(connectionConfigFileName);
            XmlElement uriNode =
                (XmlElement) configDoc.SelectSingleNode(String.Format("/configuration/{0}", nameTestURI));

            if (null != uriNode)
            {
                // Replace any environment variables embedded inside the string.
                brokerUri = new Uri(ReplaceEnvVar(uriNode.GetAttribute("value")));
                factoryParams = GetFactoryParams(uriNode);
                clientId = ReplaceEnvVar(GetNodeValueAttribute(uriNode, "clientId", "NMSTestClientId"));
                userName = ReplaceEnvVar(GetNodeValueAttribute(uriNode, "userName", "guest"));
                passWord = ReplaceEnvVar(GetNodeValueAttribute(uriNode, "passWord", "guest"));
            }

            if (null == factoryParams)
            {
                NMSFactory = new Apache.NMS.NMSConnectionFactory(brokerUri);
            }
            else
            {
                NMSFactory = new Apache.NMS.NMSConnectionFactory(brokerUri, factoryParams);
            }

            return (null != NMSFactory);
        }

        private static string[] GetConfigSearchPaths()
        {
            ArrayList pathList = new ArrayList();

            // Check the current folder first.
            pathList.Add("");
#if !NETCF
            AppDomain currentDomain = AppDomain.CurrentDomain;

            // Check the folder the assembly is located in.
            pathList.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            if (null != currentDomain.BaseDirectory)
            {
                pathList.Add(currentDomain.BaseDirectory);
            }

            if (null != currentDomain.RelativeSearchPath)
            {
                pathList.Add(currentDomain.RelativeSearchPath);
            }
#endif

            return (string[]) pathList.ToArray(typeof(string));
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

            if (null != factoryParamsNode)
            {
                XmlNodeList nodeList = factoryParamsNode.SelectNodes("param");

                if (null != nodeList)
                {
                    foreach (XmlElement paramNode in nodeList)
                    {
                        string paramType = paramNode.GetAttribute("type");
                        string paramValue = ReplaceEnvVar(paramNode.GetAttribute("value"));

                        switch (paramType)
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

            if (factoryParams.Count > 0)
            {
                return factoryParams.ToArray();
            }

            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="nodeName"></param>
        /// <param name="dflt"></param>
        /// <returns></returns>
        protected static string GetNodeValueAttribute(XmlElement parentNode, string nodeName, string dflt)
        {
            XmlElement node = (XmlElement) parentNode.SelectSingleNode(nodeName);
            string val;

            if (null != node)
            {
                val = node.GetAttribute("value");
            }
            else
            {
                val = dflt;
            }

            return val;
        }

        /// <summary>
        /// Replace embedded variable markups with environment variable values.
        /// Variable markups are of the following form:
        ///		${varname}
        /// </summary>
        /// <param name="srcText"></param>
        /// <returns></returns>
        public static string ReplaceEnvVar(string srcText)
        {
            // NOTE: Might be able to refactor to be more generic and support full variable
            // names that can be pulled from the environment.  Currently, we only support limited
            // hard-coded variable names.

            string defaultBroker = GetEnvVar("NMSTestBroker", "activemqhost");

            srcText = ReplaceEnvVar(srcText, "ActiveMQHost", defaultBroker);
            srcText = ReplaceEnvVar(srcText, "ActiveMQBackupHost", defaultBroker);

            srcText = ReplaceEnvVar(srcText, "TIBCOHost", defaultBroker);
            srcText = ReplaceEnvVar(srcText, "TIBCOBackupHost", defaultBroker);

            srcText = ReplaceEnvVar(srcText, "MSMQHost", defaultBroker);
            srcText = ReplaceEnvVar(srcText, "MSMQBackupHost", defaultBroker);
            return srcText;
        }

        /// <summary>
        /// Replace the variable with environment variable.
        /// </summary>
        /// <param name="srcText"></param>
        /// <param name="varName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string ReplaceEnvVar(string srcText, string varName, string defaultValue)
        {
            string replacementValue = GetEnvVar(varName, defaultValue);
            return Regex.Replace(srcText, "\\${" + varName + "}", replacementValue, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Get environment variable value.
        /// </summary>
        /// <param name="varName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetEnvVar(string varName, string defaultValue)
        {
#if (PocketPC||NETCF||NETCF_2_0)
            string varValue = null;
#else
            string varValue = Environment.GetEnvironmentVariable(varName);
#endif
            if (null == varValue)
            {
                varValue = defaultValue;
            }

            return varValue;
        }

        public virtual string GetTestClientId()
        {
            System.Text.StringBuilder id = new System.Text.StringBuilder();

            id.Append("ID:");
            id.Append(this.GetType().Name);
            id.Append(":");
            id.Append(this.testRun);
            id.Append(":");
            id.Append(++idCounter);

            return id.ToString();
        }

        /// <summary>
        /// Create a new connection to the broker.
        /// </summary>
        /// <returns></returns>
        public virtual IConnection CreateConnection()
        {
            return CreateConnection(null);
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
            if (newClientId != null)
            {
                newConnection.ClientId = newClientId;
            }

            return newConnection;
        }

        /// <summary>
        /// Create a new connection to the broker, and start it.
        /// </summary>
        /// <returns></returns>
        public virtual IConnection CreateConnectionAndStart()
        {
            return CreateConnectionAndStart(null);
        }

        /// <summary>
        /// Create a new connection to the broker, and start it.
        /// </summary>
        /// <param name="newClientId">Client ID of the new connection.</param>
        /// <returns></returns>
        public virtual IConnection CreateConnectionAndStart(string newClientId)
        {
            IConnection newConnection = CreateConnection(newClientId);
            newConnection.Start();
            return newConnection;
        }

        public IDestination CreateDestination(ISession session, DestinationType type)
        {
            return CreateDestination(session, type, "");
        }

        public IDestination CreateDestination(ISession session, DestinationType type, string name)
        {
            switch (type)
            {
                case DestinationType.Queue:
                    if (string.IsNullOrEmpty(name))
                    {
                        name = "queue://TEST." + this.GetType().Name + "." + Guid.NewGuid().ToString();
                    }

                    break;

                case DestinationType.Topic:
                    if (string.IsNullOrEmpty(name))
                    {
                        name = "topic://TEST." + this.GetType().Name + "." + Guid.NewGuid().ToString();
                    }

                    break;

                case DestinationType.TemporaryQueue:
                    if (string.IsNullOrEmpty(name))
                    {
                        name = "temp-queue://TEST." + this.GetType().Name + "." + Guid.NewGuid().ToString();
                    }

                    break;

                case DestinationType.TemporaryTopic:
                    if (string.IsNullOrEmpty(name))
                    {
                        name = "temp-topic://TEST." + this.GetType().Name + "." + Guid.NewGuid().ToString();
                    }

                    break;

                default:
                    throw new ArgumentException("type: " + type);
            }

            return CreateDestination(session, name);
        }

        /// <summary>
        /// Create a destination.  This will delete an existing destination and re-create it.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="destinationName"></param>
        /// <returns></returns>
        public virtual IDestination CreateDestination(ISession session, string destinationName)
        {
            try
            {
                SessionUtil.DeleteDestination(session, destinationName);
            }
            catch (Exception)
            {
                // Can't delete it, so lets try and purge it.
                IDestination destination = SessionUtil.GetDestination(session, destinationName);

                using (IMessageConsumer consumer = session.CreateConsumer(destination))
                {
                    while (consumer.Receive(TimeSpan.FromMilliseconds(750)) != null)
                    {
                    }
                }
            }

            return SessionUtil.GetDestination(session, destinationName);
        }

        /// <summary>
        /// Register a durable consumer
        /// </summary>
        /// <param name="connectionID">Connection ID of the consumer.</param>
        /// <param name="destination">Destination name to register.  Supports embedded prefix names.</param>
        /// <param name="consumerID">Name of the durable consumer.</param>
        /// <param name="selector">Selector parameters for consumer.</param>
        /// <param name="noLocal"></param>
        protected void RegisterDurableConsumer(string connectionID, string destination, string consumerID,
            string selector, bool noLocal)
        {
            using (IConnection connection = CreateConnection(connectionID))
            {
                connection.Start();
                using (ISession session = connection.CreateSession(AcknowledgementMode.DupsOkAcknowledge))
                {
                    ITopic destinationTopic = (ITopic) SessionUtil.GetDestination(session, destination);
                    Assert.IsNotNull(destinationTopic, "Could not get destination topic.");
                    using (IMessageConsumer consumer =
                        session.CreateDurableConsumer(destinationTopic, consumerID, selector, noLocal))
                    {
                        Assert.IsNotNull(consumer, "Could not create durable consumer.");
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
            using (IConnection connection = CreateConnection(connectionID))
            {
                connection.Start();
                using (ISession session = connection.CreateSession(AcknowledgementMode.DupsOkAcknowledge))
                {
                    session.DeleteDurableConsumer(consumerID);
                }
            }
        }

        public static string ToHex(long value)
        {
            return String.Format("{0:x}", value);
        }

        public void SendMessages(IDestination destination, MsgDeliveryMode deliveryMode, int count)
        {
            IConnection connection = CreateConnection();
            connection.Start();
            SendMessages(connection, destination, deliveryMode, count);
            connection.Close();
        }

        public void SendMessages(IConnection connection, IDestination destination, MsgDeliveryMode deliveryMode,
            int count)
        {
            ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge);
            SendMessages(session, destination, deliveryMode, count);
            session.Close();
        }

        public void SendMessages(ISession session, IDestination destination, MsgDeliveryMode deliveryMode, int count)
        {
            IMessageProducer producer = session.CreateProducer(destination);
            producer.DeliveryMode = deliveryMode;
            for (int i = 0; i < count; i++)
            {
                producer.Send(session.CreateTextMessage("" + i));
            }

            producer.Close();
        }

        protected void AssertTextMessagesEqual(IMessage[] firstSet, IMessage[] secondSet)
        {
            AssertTextMessagesEqual(firstSet, secondSet, "");
        }

        protected void AssertTextMessagesEqual(IMessage[] firstSet, IMessage[] secondSet, string messsage)
        {
            Assert.AreEqual(firstSet.Length, secondSet.Length, "Message count does not match: " + messsage);

            for (int i = 0; i < secondSet.Length; i++)
            {
                ITextMessage m1 = firstSet[i] as ITextMessage;
                ITextMessage m2 = secondSet[i] as ITextMessage;

                AssertTextMessageEqual(m1, m2, "Message " + (i + 1) + " did not match : ");
            }
        }

        protected void AssertEquals(ITextMessage m1, ITextMessage m2)
        {
            AssertEquals(m1, m2, "");
        }

        protected void AssertTextMessageEqual(ITextMessage m1, ITextMessage m2, string message)
        {
            Assert.IsFalse(m1 == null ^ m2 == null, message + ": expected {" + m1 + "}, but was {" + m2 + "}");

            if (m1 == null)
            {
                return;
            }

            Assert.AreEqual(m1.Text, m2.Text, message);
        }

        protected void AssertEquals(IMessage m1, IMessage m2)
        {
            AssertEquals(m1, m2, "");
        }

        protected void AssertEquals(IMessage m1, IMessage m2, string message)
        {
            Assert.IsFalse(m1 == null ^ m2 == null, message + ": expected {" + m1 + "}, but was {" + m2 + "}");

            if (m1 == null)
            {
                return;
            }

            Assert.IsTrue(m1.GetType() == m2.GetType(), message + ": expected {" + m1 + "}, but was {" + m2 + "}");

            if (m1 is ITextMessage)
            {
                AssertTextMessageEqual((ITextMessage) m1, (ITextMessage) m2, message);
            }
            else
            {
                Assert.AreEqual(m1, m2, message);
            }
        }
    }
}