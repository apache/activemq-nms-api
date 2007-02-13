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

namespace ActiveMQ.OpenWire
{
    /// <summary>
    /// Implements the <a href="http://stomp.codehaus.org/">STOMP</a> protocol.
    /// </summary>
    public class StompWireFormat : IWireFormat
    {
		protected const String NEWLINE = "\n";
		protected const String SEPARATOR = ":";
		protected const char NULL = (char) 0;
		protected Encoding encoding = new UTF8Encoding();
		
		public StompWireFormat()
		{
		}
		
        public int Version {
            get { return 1; }
        }

        public void Marshal(Object o, BinaryWriter ds)
        {
			if (o is ConnectionInfo) 
			{
				WriteConnectionInfo((ConnectionInfo) o, ds);
			}
			else if (o is ActiveMQMessage)
			{
				WriteMessage((ActiveMQMessage) o, ds);
			}
			else if (o is ConsumerInfo)
			{
				WriteConsumerInfo((ConsumerInfo) o, ds);
			}
			else if (o is MessageAck)
			{
				WriteMessageAck((MessageAck) o, ds);
			}
			else 
			{
				Console.WriteLine("Ignored command: " + o);
			}
        }
        
        public Object Unmarshal(BinaryReader dis)
        {
			StreamReader socketReader = new StreamReader(dis.BaseStream);
			string command;
			do {
				command = socketReader.ReadLine();
			}
			while (command == "");
			
			IDictionary headers = new Hashtable();
			string line;
			while((line = socketReader.ReadLine()) != "") 
			{
				string[] split = line.Split(new char[] {':'}, 2);
				headers[split[0]] = split[1];
			}

			byte[] content = null;
			string length = ToString(headers["content-length"]);
			if (length != null) 
			{
				int size = Int32.Parse(length);
				content = dis.ReadBytes(size);
			}
			else
			{
				StringBuilder body = new StringBuilder();
				int nextChar;
				while((nextChar = socketReader.Read()) != 0) 
				{
					body.Append((char)nextChar);
				}
				string text = body.ToString().TrimEnd('\r', '\n');
				content = encoding.GetBytes(text);
			}
			return CreateCommand(command, headers, content);
        }

		protected Object CreateCommand(string command, IDictionary headers, byte[] content) 
		{
			if (command == "RECEIPT")
			{
				Response answer = new Response();
				answer.CorrelationId = Int32.Parse(ToString(headers["receipt-id"]));
				return answer; 
			}
			else if (command == "MESSAGE")
			{
				return ReadMessage(command, headers, content);
			}
			else if (command == "ERROR")
			{
				ExceptionResponse answer = new ExceptionResponse();
				BrokerError error = new BrokerError();
				error.Message = ToString(headers["message"]);
				error.ExceptionClass = ToString(headers["exceptionClass"]); // TODO is this the right header?
				answer.Exception = error;
				return answer; 
			}
			else
			{
				Console.WriteLine("Unknown command: " + command + " headers: " + headers);
				return null;
			}
		}
		
		protected ActiveMQMessage ReadMessage(string command, IDictionary headers, byte[] content) 
		{
			ActiveMQMessage message = null;
			if (headers.Contains("content-length"))
			{
				message = new ActiveMQBytesMessage();
			}
			else 
			{
				message = new ActiveMQTextMessage();
			}
			message.Content = content;
			
			// TODO now lets set the various headers
			message.Type = ToString(headers["type"]);
			message.Destination = ActiveMQDestination.CreateDestination(ActiveMQDestination.ACTIVEMQ_QUEUE, ToString(headers["destination"]));
			message.ReplyTo = ActiveMQDestination.CreateDestination(ActiveMQDestination.ACTIVEMQ_QUEUE, ToString(headers["reply-to"]));
			
			// lets remove all the standard headers as unfortunately there's no Remove method which returns the removed value
			string[] standardHeaders = { "type", "destination", "reply-to", "receipt-id" };
			foreach (string header in standardHeaders) 
			{
				headers.Remove(header);
			}
			
			// now lets add the generic headers
			foreach (string key in headers.Keys)
			{
				message.Properties[key] = headers[key];
			}
			return message;
		}
		
		protected void WriteConnectionInfo(ConnectionInfo command, BinaryWriter ds)
		{
			WriteCommand(ds, "CONNECT");
			WriteHeader(ds, "client-id", command.ClientId);
			WriteHeader(ds, "login", command.UserName);
			WriteHeader(ds, "passcode", command.Password);
			WriteCommonHeaders(ds, command);
			WriteFrameEnd(ds);
		}

		protected void WriteConsumerInfo(ConsumerInfo command, BinaryWriter ds)
		{
			WriteCommand(ds, "SUBSCRIBE");
			WriteHeader(ds, "destination", command.Destination);
			WriteHeader(ds, "selector", command.Selector);
			WriteHeader(ds, "id", command.ConsumerId);
			WriteHeader(ds, "durable-subscriber-name", command.SubscriptionName);
			WriteHeader(ds, "no-local", command.NoLocal);
			WriteHeader(ds, "ack", "client");
			
			// ActiveMQ extensions to STOMP
			WriteHeader(ds, "activemq.dispatchAsync", command.DispatchAsync);
			WriteHeader(ds, "activemq.exclusive", command.Exclusive);
			WriteHeader(ds, "activemq.maximumPendingMessageLimit", command.MaximumPendingMessageLimit);
			WriteHeader(ds, "activemq.prefetchSize", command.PrefetchSize);
			WriteHeader(ds, "activemq.priority ", command.Priority);
			WriteHeader(ds, "activemq.retroactive", command.Retroactive);
			
			WriteCommonHeaders(ds, command);
			WriteFrameEnd(ds);
		}

		protected void WriteMessage(ActiveMQMessage command, BinaryWriter ds)
		{
			WriteCommand(ds, "SEND");
			WriteHeader(ds, "correlation-id", command.CorrelationId);
			WriteHeader(ds, "reply-to", command.ReplyTo);
			WriteHeader(ds, "expires", command.Expiration);
			WriteHeader(ds, "priority", command.Priority);
			WriteHeader(ds, "type", command.Type);
			WriteHeader(ds, "transaction", command.TransactionId);
			
			if (!(command is ActiveMQTextMessage)) 
			{
				WriteHeader(ds, "content-length", command.Content.Length);
			}
	
			// TODO write content
			
			IPrimitiveMap map = command.Properties;
			foreach (string key in map.Keys)
			{
				WriteHeader(ds, key, map[key]);
			}
			WriteCommonHeaders(ds, command);
			ds.Write(NEWLINE);
			ds.Write(command.Content);
			ds.Write(NULL);
		}
		
		protected void WriteMessageAck(MessageAck command, BinaryWriter ds)
		{
			WriteCommand(ds, "ACK");
			
			// TODO handle bulk ACKs?
			WriteHeader(ds, "message-id", command.FirstMessageId);
			WriteHeader(ds, "transaction", command.TransactionId);
			
			WriteCommonHeaders(ds, command);
			WriteFrameEnd(ds);
		}
		
		protected void WriteCommand(BinaryWriter ds, String command)
		{
			ds.Write(command + NEWLINE);
		}
		
		protected void WriteFrameEnd(BinaryWriter ds)
		{
			ds.Write(NEWLINE);
			ds.Write(NULL);
		}
		
		protected void WriteHeader(BinaryWriter ds, String name, Object value)
		{
			if (value != null) {
				ds.Write(name + SEPARATOR + value + NEWLINE);
			}
		}
		
		protected void WriteCommonHeaders(BinaryWriter ds, Command command) 
		{
			if (command.ResponseRequired)
			{
				WriteHeader(ds, "receipt", command.CommandId);
			}
		}
		
		protected string ToString(object value) 
		{
			if (value != null) 
			{
				return value.ToString();
			}
			else 
			{
				return null;
			}
		}
    }
}
