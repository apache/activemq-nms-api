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

namespace Apache.TibcoEMS
{
    /// <summary>
    /// A Factory that can estbalish NMS connections to TIBCO
    /// </summary>
    public class ConnectionFactory : Apache.NMS.IConnectionFactory
    {
    	public TIBCO.EMS.ConnectionFactory tibcoConnectionFactory;

		public ConnectionFactory()
		{
			try
			{
				this.tibcoConnectionFactory = new TIBCO.EMS.ConnectionFactory();
			}
			catch(Exception ex)
			{
				Apache.NMS.Tracer.DebugFormat("Exception instantiating TIBCO.EMS.ConnectionFactory: {0}", ex.Message);
			}

			VerifyConnectionFactory();
		}

		public ConnectionFactory(Uri uriProvider)
			: this(uriProvider.AbsolutePath)
		{
		}

		public ConnectionFactory(Uri uriProvider, string clientId)
			: this(uriProvider.AbsolutePath, clientId)
		{
		}

		public ConnectionFactory(string serverUrl)
			: this(serverUrl, Guid.NewGuid().ToString())
		{
		}

		public ConnectionFactory(string serverUrl, string clientId)
		{
			try
			{
				this.tibcoConnectionFactory = new TIBCO.EMS.ConnectionFactory(serverUrl, clientId);
			}
			catch(Exception ex)
			{
				Apache.NMS.Tracer.DebugFormat("Exception instantiating TIBCO.EMS.ConnectionFactory: {0}", ex.Message);
			}

			VerifyConnectionFactory();
		}

		public ConnectionFactory(string serverUrl, string clientId, Hashtable properties)
		{
			try
			{
				this.tibcoConnectionFactory = new TIBCO.EMS.ConnectionFactory(serverUrl, clientId, properties);
			}
			catch(Exception ex)
			{
				Apache.NMS.Tracer.DebugFormat("Exception instantiating TIBCO.EMS.ConnectionFactory: {0}", ex.Message);
			}

			VerifyConnectionFactory();
		}

		private void VerifyConnectionFactory()
		{
			if(null == this.tibcoConnectionFactory)
			{
				throw new Apache.NMS.NMSException("Error instantiating TIBCO connection factory object.");
			}
		}

		#region IConnectionFactory Members

		/// <summary>
		/// Creates a new connection to TIBCO.
		/// </summary>
		public Apache.NMS.IConnection CreateConnection()
        {
			return TibcoUtil.ToNMSConnection(this.tibcoConnectionFactory.CreateConnection());
        }

		/// <summary>
		/// Creates a new connection to TIBCO.
		/// </summary>
		public Apache.NMS.IConnection CreateConnection(string userName, string password)
        {
			return TibcoUtil.ToNMSConnection(this.tibcoConnectionFactory.CreateConnection(userName, password));
		}

		#endregion
	}
}
