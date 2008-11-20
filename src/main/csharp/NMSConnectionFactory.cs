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
using System.Xml;

namespace Apache.NMS
{
	public class NMSConnectionFactory : IConnectionFactory
	{
		protected readonly IConnectionFactory factory;

		/// <summary>
		/// The ConnectionFactory object must define a constructor that takes as a minimum a Uri object.
		/// Any additional parameters are optional, but will typically include a Client ID string.
		/// </summary>
		/// <param name="providerURI"></param>
		/// <param name="constructorParams"></param>
		public NMSConnectionFactory(string providerURI, params object[] constructorParams)
			: this(new Uri(providerURI), constructorParams)
		{
		}

		/// <summary>
		/// The ConnectionFactory object must define a constructor that takes as a minimum a Uri object.
		/// Any additional parameters are optional, but will typically include a Client ID string.
		/// </summary>
		/// <param name="uriProvider"></param>
		/// <param name="constructorParams"></param>
		public NMSConnectionFactory(Uri uriProvider, params object[] constructorParams)
		{
			this.factory = CreateConnectionFactory(uriProvider, constructorParams);
		}

		/// <summary>
		/// Create a connection factory that can create connections for the given scheme in the URI.
		/// </summary>
		/// <param name="uriProvider"></param>
		/// <param name="constructorParams"></param>
		/// <returns></returns>
		public static IConnectionFactory CreateConnectionFactory(Uri uriProvider, params object[] constructorParams)
		{
			IConnectionFactory connectionFactory = null;

			try
			{
				Type factoryType = GetTypeForScheme(uriProvider.Scheme);

				// If an implementation was found, try to instantiate it.
				if(factoryType != null)
				{
#if NETCF
					connectionFactory = (IConnectionFactory) Activator.CreateInstance(factoryType);
#else
					object[] parameters = MakeParameterArray(uriProvider, constructorParams);
					connectionFactory = (IConnectionFactory) Activator.CreateInstance(factoryType, parameters);
#endif
				}

				if(null == connectionFactory)
				{
					throw new NMSConnectionException("No IConnectionFactory implementation found for connection URI: " + uriProvider);
				}
			}
			catch(NMSConnectionException)
			{
				throw;
			}
			catch(Exception ex)
			{
				throw new NMSConnectionException("Could not create the IConnectionFactory implementation: " + ex.Message, ex);
			}

			return connectionFactory;
		}

		/// <summary>
		/// Finds the Type associated with the given scheme.
		/// </summary>
		/// <param name="scheme"></param>
		/// <returns></returns>
		private static Type GetTypeForScheme(string scheme)
		{
			string[] paths = GetConfigSearchPaths();
			string assemblyFileName;
			string factoryClassName;
			Type factoryType = null;

			if(LookupConnectionFactoryInfo(paths, scheme, out assemblyFileName, out factoryClassName))
			{
				Assembly assembly = null;

				foreach(string path in paths)
				{
					string fullpath = Path.Combine(path, assemblyFileName);

					if(File.Exists(fullpath))
					{
						assembly = Assembly.LoadFrom(fullpath);
						break;
					}
				}

				if(null != assembly)
				{
#if NETCF
					factoryType = assembly.GetType(factoryClassName, true);
#else
					factoryType = assembly.GetType(factoryClassName, true, true);
#endif
				}
			}

			return factoryType;
		}

		/// <summary>
		/// Lookup the connection factory assembly filename and class name.
		/// Read an external configuration file that maps scheme to provider implementation.
		/// Load XML config files named: nmsprovider-{scheme}.config
		/// Following is a sample configuration file named nmsprovider-jms.config.  Replace
		/// the parenthesis with angle brackets for proper XML formatting.
		///
		///		(?xml version="1.0" encoding="utf-8" ?)
		///		(configuration)
		///			(provider assembly="MyCompany.NMS.JMSProvider.dll" classFactory="MyCompany.NMS.JMSProvider.ConnectionFactory"/)
		///		(/configuration)
		///
		/// This configuration file would be loaded and parsed when a connection uri with a scheme of 'jms'
		/// is used for the provider.  In this example the connection string might look like:
		///		jms://localhost:7222
		///
		/// </summary>
		/// <param name="paths">Folder paths to look in.</param>
		/// <param name="scheme"></param>
		/// <param name="assemblyFileName"></param>
		/// <param name="factoryClassName"></param>
		/// <returns></returns>
		private static bool LookupConnectionFactoryInfo(string[] paths, string scheme, out string assemblyFileName, out string factoryClassName)
		{
			string configFileName = String.Format("nmsprovider-{0}.config", scheme.ToLower());
			bool foundFactory = false;

			assemblyFileName = String.Empty;
			factoryClassName = String.Empty;

			foreach(string path in paths)
			{
				string fullpath = Path.Combine(path, configFileName);

				try
				{
					if(File.Exists(fullpath))
					{
						XmlDocument configDoc = new XmlDocument();

						configDoc.Load(fullpath);
						XmlElement providerNode = (XmlElement) configDoc.SelectSingleNode("/configuration/provider");

						if(null != providerNode)
						{
							assemblyFileName = providerNode.GetAttribute("assembly");
							factoryClassName = providerNode.GetAttribute("classFactory");
							if(String.Empty != assemblyFileName && String.Empty != factoryClassName)
							{
								foundFactory = true;
								break;
							}
						}
					}
				}
				catch
				{
				}
			}

			return foundFactory;
		}

		/// <summary>
		/// Get an array of search paths to look for config files.
		/// </summary>
		private static string[] GetConfigSearchPaths()
		{
			ArrayList pathList = new ArrayList();

			// Check the current folder first.
			pathList.Add("");
#if !NETCF
			AppDomain currentDomain = AppDomain.CurrentDomain;

			// Check the folder the assembly is located in.
			pathList.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
			if(null != currentDomain.BaseDirectory)
			{
				pathList.Add(currentDomain.BaseDirectory);
			}

			if(null != currentDomain.RelativeSearchPath)
			{
				pathList.Add(currentDomain.RelativeSearchPath);
			}
#endif

			return (string[]) pathList.ToArray(typeof(string));
		}

		/// <summary>
		/// Create an object array containing the parameters to pass to the constructor.
		/// </summary>
		/// <param name="firstParam"></param>
		/// <param name="varParams"></param>
		/// <returns></returns>
		private static object[] MakeParameterArray(object firstParam, params object[] varParams)
		{
			ArrayList paramList = new ArrayList();
			paramList.Add(firstParam);
			foreach(object param in varParams)
			{
				paramList.Add(param);
			}

			return paramList.ToArray();
		}

		/// <summary>
		/// Creates a new connection
		/// </summary>
		public IConnection CreateConnection()
		{
			return this.factory.CreateConnection();
		}

		/// <summary>
		/// Creates a new connection with the given user name and password
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public IConnection CreateConnection(string userName, string password)
		{
			return this.factory.CreateConnection(userName, password);
		}

		/// <summary>
		/// The actual IConnectionFactory implementation that is being used.  This implemenation
		/// depends on the scheme of the URI used when constructed.
		/// </summary>
		public IConnectionFactory ConnectionFactory
		{
			get { return factory; }
		}
	}
}
