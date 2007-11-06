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
using System.IO;

namespace Apache.NMS
{
    public class NMSConnectionFactory : IConnectionFactory
	{
        IConnectionFactory factory;

        /// <summary>
        /// The ConnectionFactory object must define a constructor that takes as a minimum a Uri object.
        /// Any additional parameters are optional, but will typically include a Client ID string.
        /// </summary>
        public NMSConnectionFactory(string uri, params object[] constructorParams)
		{
            factory = CreateConnectionFactory(uri, constructorParams);
		}

        /// <summary>
        /// Finds the Type associated with the given scheme.  This searches all loaded assembiles
        /// for a resouce called Apache.NMS.NMSFactory.${scheme} and expects it to conatain 
        /// the name of the Type assoicated with the scheme.
        /// </summary>
        public static IConnectionFactory CreateConnectionFactory(string uri, params object[] constructorParams)
		{
            try
            {
                // TODO: perhaps we should not use Uri to parse the string.. some implemenations my use non- Uri parsable
                // URIs.
                string scheme = new Uri(uri).Scheme;
                Type type = GetTypeForScheme(scheme);

                // If an implementation was found.. try to instanciate it..
                if (type != null)
                {
                    object[] parameters = GetParameters(uri, constructorParams);
                    return (Apache.NMS.IConnectionFactory)Activator.CreateInstance(type, parameters);
                }
                else
                {
                    throw new NMSException("No IConnectionFactory implementation found for connection URI: " + uri);
                }
            }
            catch (NMSException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new NMSException("Could not create the IConnectionFactory implementation: " + ex.Message, ex);
            }
		}


		/// <summary>
		/// Finds the Type associated with the given scheme.  This searches all loaded assembiles
        /// for a resouce called Apache.NMS.NMSFactory.${scheme} and expects it to conatain 
        /// the name of the Type assoicated with the scheme.
		/// </summary>
        private static Type GetTypeForScheme(String scheme) {

            // TODO: if this scanning is too slow, we should cache the results in a scheme->Type map
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                string resourceFile = assembly.GetName().Name + "." + "Apache.NMS.NMSConnectionFactory." + scheme;
                Stream fs = assembly.GetManifestResourceStream(resourceFile);
                if (fs != null)
                {
                    // Found it..
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        try
                        {
                            String className = sr.ReadLine();
                            return assembly.GetType(className, true, true);
                        }
                        catch (Exception e)
                        {
                            Tracer.ErrorFormat("Error creating ConnectionFactory from resource file '{0}': {1}", resourceFile, e.Message);
                        }
                    }
                }
            }
            return null;
        }

		/// <summary>
		/// Create an object array containing the parameters to pass to the constructor.
		/// </summary>
		/// <param name="firstParam"></param>
		/// <param name="varParams"></param>
		/// <returns></returns>
        private static object[] GetParameters(object firstParam, params object[] varParams)
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
            return factory.CreateConnection();
        }

        /// <summary>
        /// Creates a new connection with the given user name and password
        /// </summary>
        public IConnection CreateConnection(string userName, string password)
        {
            return factory.CreateConnection(userName, password);
        }
        /// <summary>
        /// The actual IConnectionFactory implementation that is being used.  This implemenation 
        /// depends on the scheme of the URI used when constructed.
        /// </summary>
        public IConnectionFactory ConnectionFactory
        {
            get
            {
                return factory;
            }
        }
	}
}
