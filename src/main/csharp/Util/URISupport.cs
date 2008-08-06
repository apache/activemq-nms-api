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
using System.Collections.Specialized;
using System.Globalization;
using System.Reflection;

namespace Apache.NMS.Util
{
	/// <summary>
	/// Class to provide support for URI query parameters which uses .Net reflection
	/// to identify and set properties.
	/// </summary>
	public class URISupport
	{
		/// <summary>
		/// Parse a URI query string of the form ?x=y&amp;z=0
		/// into a map of name/value pairs.
		/// </summary>
		/// <param name="query">The query string to parse. This string should not contain
		/// URI escape characters.</param>
		public static StringDictionary ParseQuery(string query)
		{
			StringDictionary map = new StringDictionary();

			// strip the initial "?"
			if(query.StartsWith("?"))
			{
				query = query.Substring(1);
			}

			// split the query into parameters
			string[] parameters = query.Split('&');
			foreach(string pair in parameters)
			{
				if(pair.Length > 0)
				{
					string[] nameValue = pair.Split('=');

					if(nameValue.Length != 2)
					{
						throw new NMS.NMSException("Invalid URI parameter: " + query);
					}

					map[nameValue[0]] = nameValue[1];
				}
			}

			return map;
		}

		/// <summary>
		/// Sets the public properties of a target object using a string map.
		/// This method uses .Net reflection to identify public properties of
		/// the target object matching the keys from the passed map.
		/// </summary>
		/// <param name="target">The object whose properties will be set.</param>
		/// <param name="map">Map of key/value pairs.</param>
		/// <param name="prefix">Key value prefix.  This is prepended to the property name
		/// before searching for a matching key value.</param>
		public static void SetProperties(object target, StringDictionary map, string prefix)
		{
			Type type = target.GetType();

			foreach(string key in map.Keys)
			{
				if(key.ToLower().StartsWith(prefix.ToLower()))
				{
					string bareKey = key.Substring(prefix.Length);
					PropertyInfo prop = type.GetProperty(bareKey,
															BindingFlags.FlattenHierarchy
															| BindingFlags.Public
															| BindingFlags.Instance
															| BindingFlags.IgnoreCase);

					if(null == prop)
					{
						throw new NMS.NMSException(string.Format("no such property: {0} on class: {1}", bareKey, target.GetType().Name));
					}

					prop.SetValue(target, Convert.ChangeType(map[key], prop.PropertyType, CultureInfo.InvariantCulture), null);
				}
			}
		}
	}
}

