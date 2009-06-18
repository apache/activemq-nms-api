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
	/// <summary>
	/// Implementation of a factory for <see cref="IConnection" /> instances.
	/// </summary>
	public class NMSConnectionFactory : IConnectionFactory
	{
		protected readonly IConnectionFactory factory;

		/// <summary>
		/// The ConnectionFactory object must define a constructor that takes as a minimum a Uri object.
		/// Any additional parameters are optional, but will typically include a Client ID string.
		/// </summary>
		/// <param name="providerURI">The URI for the ActiveMQ provider.</param>
		/// <param name="constructorParams">Optional parameters to use when creating the ConnectionFactory.</param>
		public NMSConnectionFactory(string providerURI, params object[] constructorParams)
			: this(new Uri(providerURI), constructorParams)
		{
		}

		/// <summary>
		/// The ConnectionFactory object must define a constructor that takes as a minimum a Uri object.
		/// Any additional parameters are optional, but will typically include a Client ID string.
		/// </summary>
		/// <param name="uriProvider">The URI for the ActiveMQ provider.</param>
		/// <param name="constructorParams">Optional parameters to use when creating the ConnectionFactory.</param>
		public NMSConnectionFactory(Uri uriProvider, params object[] constructorParams)
		{
			this.factory = CreateConnectionFactory(uriProvider, constructorParams);
		}

		/// <summary>
		/// Create a connection factory that can create connections for the given scheme in the URI.
		/// </summary>
		/// <param name="uriProvider">The URI for the ActiveMQ provider.</param>
		/// <param name="constructorParams">Optional parameters to use when creating the ConnectionFactory.</param>
		/// <returns>A <see cref="IConnectionFactory" /> implementation that will be used.</returns>
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
					// Compact framework does not allow the activator ta pass parameters to a constructor.
					connectionFactory = (IConnectionFactory) Activator.CreateInstance(factoryType);
					connectionFactory.BrokerUri = uriProvider;
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
		/// Finds the <see cref="System.Type" /> associated with the given scheme.
		/// </summary>
		/// <param name="scheme">The scheme (e.g. <c>tcp</c>, <c>activemq</c> or <c>stomp</c>).</param>
		/// <returns>The <see cref="System.Type" /> of the ConnectionFactory that will be used
		/// to create the connection for the specified <paramref name="scheme" />.</returns>
		private static Type GetTypeForScheme(string scheme)
		{
			string[] paths = GetConfigSearchPaths();
			string assemblyFileName;
			string factoryClassName;
			Type factoryType = null;

			Tracer.Debug("Locating provider for scheme: " + scheme);

			if(LookupConnectionFactoryInfo(paths, scheme, out assemblyFileName, out factoryClassName))
			{
				Assembly assembly = null;

				Tracer.Debug("Attempting to locate provider assembly: " + assemblyFileName);
				foreach(string path in paths)
				{
					string fullpath = Path.Combine(path, assemblyFileName);
					Tracer.Debug("\tScanning folder: " + path);

					if(File.Exists(fullpath))
					{
						Tracer.Debug("\tAssembly found!");
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
		///     (?xml version="1.0" encoding="utf-8" ?)
		///     (configuration)
		///         (provider assembly="MyCompany.NMS.JMSProvider.dll" classFactory="MyCompany.NMS.JMSProvider.ConnectionFactory"/)
		///     (/configuration)
		///
		/// This configuration file would be loaded and parsed when a connection uri with a scheme of 'jms'
		/// is used for the provider.  In this example the connection string might look like:
		///     jms://localhost:7222
		///
		/// </summary>
		/// <param name="paths">Folder paths to look in.</param>
		/// <param name="scheme">The scheme.</param>
		/// <param name="assemblyFileName">Name of the assembly file.</param>
		/// <param name="factoryClassName">Name of the factory class.</param>
		/// <returns><c>true</c> if the configuration file for the specified <paramref name="scheme" /> could
		/// be found; otherwise, <c>false</c>.</returns>
		private static bool LookupConnectionFactoryInfo(string[] paths, string scheme, out string assemblyFileName, out string factoryClassName)
		{
			string configFileName = String.Format("nmsprovider-{0}.config", scheme.ToLower());
			bool foundFactory = false;

			assemblyFileName = String.Empty;
			factoryClassName = String.Empty;

			Tracer.Debug("Attempting to locate provider configuration: " + configFileName);
			foreach(string path in paths)
			{
				string fullpath = Path.Combine(path, configFileName);
				Tracer.Debug("\tScanning folder: " + path);

				try
				{
					if(File.Exists(fullpath))
					{
						Tracer.Debug("\tConfiguration file found!");
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
		/// <returns>
		/// A collection of search paths, including the current directory, the current AppDomain's
		/// BaseDirectory and the current AppDomain's RelativeSearchPath.
		/// </returns>
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
		/// Converts a <c>params object[]</c> collection into a plain <c>object[]</c>s, to pass to the constructor.
		/// </summary>
		/// <param name="firstParam">The first parameter in the collection.</param>
		/// <param name="varParams">The remaining parameters.</param>
		/// <returns>An array of <see cref="Object" /> instances.</returns>
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
		/// Creates a new connection.
		/// </summary>
		/// <returns>An <see cref="IConnection" /> created by the requested ConnectionFactory.</returns>
		public IConnection CreateConnection()
		{
			return this.factory.CreateConnection();
		}

		/// <summary>
		/// Creates a new connection with the given <paramref name="userName" /> and <paramref name="password" /> credentials.
		/// </summary>
		/// <param name="userName">The username to use when establishing the connection.</param>
		/// <param name="password">The password to use when establishing the connection.</param>
		/// <returns>An <see cref="IConnection" /> created by the requested ConnectionFactory.</returns>
		public IConnection CreateConnection(string userName, string password)
		{
			return this.factory.CreateConnection(userName, password);
		}

		/// <summary>
		/// Get/or set the broker Uri.
		/// </summary>
		public Uri BrokerUri
		{
			get { return ConnectionFactory.BrokerUri; }
			set { ConnectionFactory.BrokerUri = value; }
		}

		/// <summary>
		/// The actual IConnectionFactory implementation that is being used.  This implementation
		/// depends on the scheme of the URI used when constructed.
		/// </summary>
		public IConnectionFactory ConnectionFactory
		{
			get { return factory; }
		}
	}
}
