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
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Apache.NMS.Util
{
	/// <summary>
	/// Class to provide support for working with Xml objects.
	/// </summary>
	public class XmlUtil
	{
		public static string Serialize(object obj)
		{
			XmlSerializer serializer = new XmlSerializer(obj.GetType());
			StringWriter writer = new StringWriter(new StringBuilder(4096));

			/*
			 * If the XML document has been altered with unknown 
			 * nodes or attributes, handle them with the 
			 * UnknownNode and UnknownAttribute events.
			 */
			serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
			serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
			serializer.Serialize(writer, obj);
			return writer.ToString();
		}

		public static object Deserialize(Type objType, string text)
		{
			if(null == text)
			{
				return null;
			}

			XmlSerializer serializer = new XmlSerializer(objType);
			/*
			 * If the XML document has been altered with unknown 
			 * nodes or attributes, handle them with the 
			 * UnknownNode and UnknownAttribute events.
			 */
			serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
			serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
			return serializer.Deserialize(new StringReader(text));
		}

		private static void serializer_UnknownNode(object sender, XmlNodeEventArgs e)
		{
			Tracer.ErrorFormat("Unknown Node: {0}\t{1}", e.Name, e.Text);
		}

		private static void serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
		{
			Tracer.ErrorFormat("Unknown attribute: {0}='{1}'", e.Attr.Name, e.Attr.Value);
		}

	}
}

