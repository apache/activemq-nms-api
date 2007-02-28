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
using System.Reflection;
using ActiveMQ.Commands;
using ActiveMQ.OpenWire.V1;
using ActiveMQ.Transport;
using NMS;
using System;
using System.Collections;
using System.IO;
using System.Text;

namespace ActiveMQ.Transport.Stomp
{
    /// <summary>
    /// A Stream for writing a <a href="http://stomp.codehaus.org/">STOMP</a> Frame
    /// </summary>
    public class StompFrameStream
    {
		public const String NEWLINE = "\n";
		public const String SEPARATOR = ":";
		public const char NULL = (char) 0;
		
		private StringBuilder builder = new StringBuilder();
		private BinaryWriter ds;
		private byte[] content;
		private int contentLength = -1;
		private Encoding encoding;
		
		public StompFrameStream(BinaryWriter ds, Encoding encoding)
		{
			this.ds = ds;
			this.encoding = encoding;
		}

		
		public byte[] Content
		{
			get { return content; }
			set { content = value; }
		}
		
		public int ContentLength
		{
			get { return contentLength; }
			set
			{
				contentLength = value;
				WriteHeader("content-length", contentLength);
			}
		}
		
		public void WriteCommand(Command command, String name)
		{
			builder.Append(name);
			builder.Append(NEWLINE);
			if (command.ResponseRequired)
			{
				WriteHeader("receipt", command.CommandId);
			}
		}
		
		public void WriteHeader(String name, Object value)
		{
			if (value != null) {
				builder.Append(name);
				builder.Append(SEPARATOR);
				builder.Append(value);
				builder.Append(NEWLINE);
			}
		}
		
		public void WriteHeader(String name, bool value)
		{
			if (value) {
				builder.Append(name);
				builder.Append(SEPARATOR);
				builder.Append("true");
				builder.Append(NEWLINE);
			}
		}
		
		public void Flush()
		{
			builder.Append(NEWLINE);
			ds.Write(encoding.GetBytes(builder.ToString()));
			
			if (content != null)
			{
				ds.Write(content);
			}
			
			// if no content length then lets write a null
			if (contentLength < 0)
			{
				ds.Write(NULL);
			}
		}

		
    }
}
