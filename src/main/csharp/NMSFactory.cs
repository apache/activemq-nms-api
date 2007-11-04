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
using System.Reflection;

namespace Apache.NMS
{
	public class NMSFactory
	{
		public static IConnectionFactory CreateConnectionFactory(string providerURLs, params object[] constructorParams)
		{
			return CreateConnectionFactory(new Uri(providerURLs), constructorParams);
		}

		/// <summary>
		/// The ConnectionFactory object must define a constructor that takes as a minimum a Uri object.
		/// Any additional parameters are optional, but will typically include a Client ID string.
		/// </summary>
		/// <param name="uriProvider"></param>
		/// <param name="constructorParams"></param>
		/// <returns></returns>
		public static IConnectionFactory CreateConnectionFactory(Uri uriProvider, params object[] constructorParams)
		{
			Apache.NMS.IConnectionFactory connectionFactory = null;

			try
			{
				ConnectionFactoryInfo cfi;

				if(LookupConnectionFactoryInfo(uriProvider.Scheme, out cfi))
				{
					Assembly assembly = Assembly.LoadFrom(cfi.assemblyFileName);
					Type factoryType = assembly.GetType(cfi.factoryClassName, true, true);
					object[] parameters = GetParameters(uriProvider, constructorParams);
					connectionFactory = (Apache.NMS.IConnectionFactory) Activator.CreateInstance(factoryType, parameters);
				}
			}
			catch(Exception ex)
			{
				Tracer.ErrorFormat("Error creating ConnectionFactory: {0}", ex.Message);
				connectionFactory = null;
			}

			return connectionFactory;
		}

		/// <summary>
		/// Create an object array containing the parameters to pass to the constructor.
		/// </summary>
		/// <param name="firstParam"></param>
		/// <param name="varParams"></param>
		/// <returns></returns>
		protected static object[] GetParameters(object firstParam, params object[] varParams)
		{
			ArrayList paramList = new ArrayList();

			paramList.Add(firstParam);
			foreach(object param in varParams)
			{
				paramList.Add(param);
			}

			return paramList.ToArray();
		}

		protected class ConnectionFactoryInfo
		{
			public string assemblyFileName;
			public string factoryClassName;
		}

		protected static bool LookupConnectionFactoryInfo(string scheme, out ConnectionFactoryInfo cfi)
		{
			cfi = new ConnectionFactoryInfo();

			// TODO: Read an external configuration file that maps scheme to provider implementation.
			if(String.Compare(scheme, "tibco", true) == 0)
			{
				cfi.assemblyFileName = "NMS.TIBCO.dll";
				cfi.factoryClassName = "Apache.NMS.EMS.ConnectionFactory";
			}
			else if(String.Compare(scheme, "msmq", true) == 0)
			{
				cfi.assemblyFileName = "NMS.MSMQ.dll";
				cfi.factoryClassName = "Apache.NMS.MSMQ.ConnectionFactory";
			}
			else
			{
				cfi.assemblyFileName = "NMS.ActiveMQ.dll";
				cfi.factoryClassName = "Apache.NMS.ActiveMQ.ConnectionFactory";
			}

			return true;
		}
	}
}
