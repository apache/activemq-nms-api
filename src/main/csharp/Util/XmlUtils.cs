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
using System.Xml;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

namespace Apache.NMS.Util
{
	/// <summary>
	/// Class to provide support for working with Xml objects.
	/// </summary>
	public class XmlUtil
	{
		public static string Serialize(object obj)
		{
			try
			{
				StringBuilder outputStringBuilder = new StringBuilder();
				XmlSerializer serializer = new XmlSerializer(obj.GetType());
				XmlWriter xmlWriter = XmlWriter.Create(outputStringBuilder);

				// Set the error handlers.
				serializer.UnknownNode += serializer_UnknownNode;
				serializer.UnknownElement += serializer_UnknownElement;
				serializer.UnknownAttribute += serializer_UnknownAttribute;
				serializer.Serialize(xmlWriter, obj);
				return outputStringBuilder.ToString();
			}
			catch(Exception ex)
			{
				Tracer.ErrorFormat("Error serializing object: {0}", ex.Message);
				return null;
			}
		}

		public static object Deserialize(Type objType, string text)
		{
			if(null == text)
			{
				return null;
			}

			try
			{
				XmlSerializer serializer = new XmlSerializer(objType);

				// Set the error handlers.
				serializer.UnknownNode += serializer_UnknownNode;
				serializer.UnknownElement += serializer_UnknownElement;
				serializer.UnknownAttribute += serializer_UnknownAttribute;
				return serializer.Deserialize(new StringReader(text));
			}
			catch(Exception ex)
			{
				Tracer.ErrorFormat("Error deserializing object: {0}", ex.Message);
				return null;
			}
		}

		/// <summary>
		/// From xml spec valid chars:
		/// #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]    
		/// any Unicode character, excluding the surrogate blocks, FFFE, and FFFF.
		/// </summary>
		private static string invalidXMLMatch = @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]";
		private static Regex regexInvalidXMLChars = new Regex(invalidXMLMatch);

		/// <summary>
		/// This removes characters that are invalid for xml encoding
		/// </summary>
		/// <param name="text">Text to be encoded.</param>
		/// <returns>Text with invalid xml characters removed.</returns>
		public static string CleanInvalidXmlChars(string text)
		{
			return regexInvalidXMLChars.Replace(text, "");
		}

		private static void serializer_UnknownNode(object sender, XmlNodeEventArgs e)
		{
			Tracer.ErrorFormat("Unknown Node: {0}\t{1}", e.Name, e.Text);
		}

		private static void serializer_UnknownElement(object sender, XmlElementEventArgs e)
		{
			Tracer.ErrorFormat("Unknown Element: {0}\t{1}", e.Element.Name, e.Element.Value);
		}

		private static void serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
		{
			Tracer.ErrorFormat("Unknown attribute: {0}='{1}'", e.Attr.Name, e.Attr.Value);
		}
	}
}
