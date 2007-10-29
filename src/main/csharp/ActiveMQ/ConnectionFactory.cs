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
using Apache.ActiveMQ.Commands;
using Apache.ActiveMQ.Transport;
using Apache.ActiveMQ.Transport.Tcp;
using Apache.ActiveMQ.Util;
using Apache.NMS;
using System;

namespace Apache.ActiveMQ
{
    /// <summary>
    /// Represents a connection with a message broker
    /// </summary>
    public class ConnectionFactory : IConnectionFactory
    {
		public const string DEFAULT_BROKER_URL = "tcp://localhost:61616";
		public const string ENV_BROKER_URL = "ACTIVEMQ_BROKER_URL";
		
        private Uri brokerUri;
        private string connectionUserName;
        private string connectionPassword;
        private string clientId;
        
		public static string GetDefaultBrokerUrl()
		{
#if (PocketPC||NETCF||NETCF_2_0)
            return DEFAULT_BROKER_URL;
#else
            string answer = Environment.GetEnvironmentVariable(ENV_BROKER_URL);
			if (answer == null) {
				answer = DEFAULT_BROKER_URL;
			}
			return answer;
#endif
		}
		
        public ConnectionFactory()
			: this(GetDefaultBrokerUrl())
        {
        }

		public ConnectionFactory(string brokerUri)
			: this(brokerUri, CreateNewGuid())
		{
		}

		public ConnectionFactory(string brokerUri, string clientID)
			: this(new Uri(brokerUri), clientID)
		{
		}

		public ConnectionFactory(Uri brokerUri)
			: this(brokerUri, CreateNewGuid())
		{
		}

		public ConnectionFactory(Uri brokerUri, string clientID)
		{
			this.brokerUri = brokerUri;
			this.clientId = clientID;
		}

		public IConnection CreateConnection()
        {
            return CreateConnection(connectionUserName, connectionPassword);
        }

    	public IConnection CreateConnection(string userName, string password)
        {
			ConnectionInfo info = CreateConnectionInfo(userName, password);

			ITransportFactory tcpTransportFactory = new TcpTransportFactory();
			ITransport transport = tcpTransportFactory.CreateTransport(brokerUri);

			IConnection connection = new Connection(transport, info);
			connection.ClientId = info.ClientId;

			// Set properties on connection using parameters prefixed with "jms."
			System.Collections.Specialized.StringDictionary map = URISupport.ParseQuery(brokerUri.Query);
			URISupport.SetProperties(connection, map, "jms.");

			return connection;
        }
        
        // Properties
        
        public Uri BrokerUri
        {
            get { return brokerUri; }
            set { brokerUri = value; }
        }
                
        public string UserName
        {
            get { return connectionUserName; }
            set { connectionUserName = value; }
        }
        
        public string Password
        {
            get { return connectionPassword; }
            set { connectionPassword = value; }
        }

		public string ClientId
        {
            get { return clientId; }
            set { clientId = value; }
        }
        
        // Implementation methods
        
        protected virtual ConnectionInfo CreateConnectionInfo(string userName, string password)
        {
            ConnectionInfo answer = new ConnectionInfo();
            ConnectionId connectionId = new ConnectionId();
            connectionId.Value = CreateNewGuid();
            
            answer.ConnectionId = connectionId;
            answer.UserName = userName;
            answer.Password = password;
            answer.ClientId = clientId;
            if (clientId == null)
            {
                answer.ClientId = CreateNewGuid();
            }
            return answer;
        }
        
        protected static string CreateNewGuid()
        {
            return Guid.NewGuid().ToString();
        }
        
    }
}
