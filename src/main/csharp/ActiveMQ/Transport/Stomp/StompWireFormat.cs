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
    /// Implements the <a href="http://stomp.codehaus.org/">STOMP</a> protocol.
    /// </summary>
    public class StompWireFormat : IWireFormat
    {
		private Encoding encoding = new UTF8Encoding();
		private ITransport transport;
		
		public StompWireFormat()
		{
		}
		
		public ITransport Transport {
			get { return transport; }
			set { transport = value; }
		}
		
        public int Version {
            get { return 1; }
        }

        public void Marshal(Object o, BinaryWriter binaryWriter)
        {
			Console.WriteLine(">>>> " + o);
			//Console.Out.Flush();
			StompFrameStream ds = new StompFrameStream(binaryWriter, encoding);
			
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
			else if (o is TransactionInfo)
			{
				WriteTransactionInfo((TransactionInfo) o, ds);
			}
			else if (o is ShutdownInfo)
			{
				WriteShutdownInfo((ShutdownInfo) o, ds);
			}
			else if (o is RemoveInfo)
			{
				WriteRemoveInfo((RemoveInfo) o, ds);
			}
			else if (o is Command)
			{
				Command command = o as Command;
				if (command.ResponseRequired)
				{
					Response response = new Response();
					response.CorrelationId = command.CommandId;
					SendCommand(response);
				}
				Console.WriteLine("#### Ignored command: " + o.GetType());
                Console.Out.Flush();
			}
			else
			{
				Console.WriteLine("#### Ignored command: " + o.GetType());
                Console.Out.Flush();
			}
        }


        internal String ReadLine(BinaryReader dis)
        {
            MemoryStream ms = new MemoryStream();
            while (true)
            {
                int nextChar = dis.Read();
                if (nextChar < 0)
                {
                    throw new IOException("Peer closed the stream.");
                }
                if( nextChar == 10 )
                {
                    break;
                }
                ms.WriteByte((byte)nextChar);
            }
            return encoding.GetString(ms.ToArray());
        }
        
        public Object Unmarshal(BinaryReader dis)
        {
			string command;
			do {
                command = ReadLine(dis);
			}
			while (command == "");
			
			Console.WriteLine("<<<< command: " + command);
			
			IDictionary headers = new Hashtable();
			string line;
            while ((line = ReadLine(dis)) != "")
			{
				int idx = line.IndexOf(':');
				if (idx > 0)
				{
					string key = line.Substring(0, idx);
					string value = line.Substring(idx + 1);
					headers[key] = value;
					
					Console.WriteLine("<<<< header: " + key + " = " + value);
				}
				else
				{
					// lets ignore this bad header!
				}
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
                MemoryStream ms = new MemoryStream();
				int nextChar;
				while((nextChar = dis.Read()) != 0)
				{
				    if( nextChar < 0 )
				    {
				        // EOF ??
				        break;
				    }
					ms.WriteByte((byte)nextChar);
				}
                content = ms.ToArray();
			}
			Object answer = CreateCommand(command, headers, content);
			Console.WriteLine("<<<< received: " + answer);
			Console.Out.Flush();
			return answer;
        }

		protected virtual Object CreateCommand(string command, IDictionary headers, byte[] content)
		{
			if (command == "RECEIPT" || command == "CONNECTED")
			{
				string text = RemoveHeader(headers, "receipt-id");
				if (text != null)
				{
    				Response answer = new Response();
					answer.CorrelationId = Int32.Parse(text);
				    return answer;
				} else if( command == "CONNECTED") {
                    text = RemoveHeader(headers, "response-id");
                    if (text != null)
                    {
                        Response answer = new Response();
                        answer.CorrelationId = Int32.Parse(text);
                        return answer;
                    }
				}
			}
			else if (command == "ERROR")
			{
				ExceptionResponse answer = new ExceptionResponse();
				string text = RemoveHeader(headers, "receipt-id");
				if (text != null)
				{
					answer.CorrelationId = Int32.Parse(text);
				}
				
				BrokerError error = new BrokerError();
				error.Message = RemoveHeader(headers, "message");
				error.ExceptionClass = RemoveHeader(headers, "exceptionClass"); // TODO is this the right header?
				answer.Exception = error;
				return answer;
			}
			else if (command == "MESSAGE")
			{
				return ReadMessage(command, headers, content);
			}
			Console.WriteLine("Unknown command: " + command + " headers: " + headers);
			return null;
		}
		
		protected virtual Command ReadMessage(string command, IDictionary headers, byte[] content)
		{
			ActiveMQMessage message = null;
			if (headers.Contains("content-length"))
			{
				message = new ActiveMQBytesMessage();
				message.Content = content;
			}
			else
			{
				message = new ActiveMQTextMessage(encoding.GetString(content, 0, content.Length));
			}

			if (message is ActiveMQTextMessage)
			{
				ActiveMQTextMessage textMessage = message as ActiveMQTextMessage;
			}
			
			// TODO now lets set the various headers
			
			message.Type = RemoveHeader(headers, "type");
			message.Destination = StompHelper.ToDestination(RemoveHeader(headers, "destination"));
			message.ReplyTo = StompHelper.ToDestination(RemoveHeader(headers, "reply-to"));
			message.TargetConsumerId = StompHelper.ToConsumerId(RemoveHeader(headers, "subscription"));
			message.CorrelationId = ToString(headers["correlation-id"]);
			message.MessageId = StompHelper.ToMessageId(RemoveHeader(headers, "message-id"));
			message.Persistent = StompHelper.ToBool(RemoveHeader(headers, "persistent"), true);
			
			string header = RemoveHeader(headers, "priority");
			if (header != null) message.Priority = Byte.Parse(header);
			
			header = RemoveHeader(headers, "timestamp");
			if (header != null) message.Timestamp = Int64.Parse(header);

			header = RemoveHeader(headers, "expires");
			if (header != null) message.Expiration = Int64.Parse(header);
			
			header = RemoveHeader(headers, "timestamp");
			if (header != null) message.Timestamp = Int64.Parse(header);

			
			// now lets add the generic headers
			foreach (string key in headers.Keys)
			{
				Object value = headers[key];
				if (value != null)
				{
					// lets coerce some standard header extensions
					if (key == "NMSXGroupSeq")
					{
						value = Int32.Parse(value.ToString());
					}
				}
				message.Properties[key] = value;
			}
			MessageDispatch dispatch = new MessageDispatch();
			dispatch.Message = message;
			dispatch.ConsumerId = message.TargetConsumerId;
			dispatch.Destination = message.Destination;
			return dispatch;
		}
		
		
		
		
		protected virtual void WriteConnectionInfo(ConnectionInfo command, StompFrameStream ss)
		{
			// lets force a receipt
			command.ResponseRequired = true;
			
			ss.WriteCommand(command, "CONNECT");
			ss.WriteHeader("client-id", command.ClientId);
			ss.WriteHeader("login", command.UserName);
			ss.WriteHeader("passcode", command.Password);
		    
		    if (command.ResponseRequired)
			{
                ss.WriteHeader("request-id", command.CommandId);
			}

			ss.Flush();
		}
		
		protected virtual void WriteShutdownInfo(ShutdownInfo command, StompFrameStream ss)
		{
			ss.WriteCommand(command, "DISCONNECT");
			ss.Flush();
		}

		protected virtual void WriteConsumerInfo(ConsumerInfo command, StompFrameStream ss)
		{
			ss.WriteCommand(command, "SUBSCRIBE");
			ss.WriteHeader("destination", StompHelper.ToStomp(command.Destination));
			ss.WriteHeader("selector", command.Selector);
			ss.WriteHeader("id", StompHelper.ToStomp(command.ConsumerId));
			ss.WriteHeader("durable-subscriber-name", command.SubscriptionName);
			ss.WriteHeader("no-local", command.NoLocal);
			ss.WriteHeader("ack", "client");

			// ActiveMQ extensions to STOMP
			ss.WriteHeader("activemq.dispatchAsync", command.DispatchAsync);
			ss.WriteHeader("activemq.exclusive", command.Exclusive);
			ss.WriteHeader("activemq.maximumPendingMessageLimit", command.MaximumPendingMessageLimit);
			ss.WriteHeader("activemq.prefetchSize", command.PrefetchSize);
			ss.WriteHeader("activemq.priority ", command.Priority);
			ss.WriteHeader("activemq.retroactive", command.Retroactive);
			
			ss.Flush();
		}

		protected virtual void WriteRemoveInfo(RemoveInfo command, StompFrameStream ss)
		{
			object id = command.ObjectId;
			if (id is ConsumerId)
			{
				ConsumerId consumerId = id as ConsumerId;
				ss.WriteCommand(command, "UNSUBSCRIBE");
				ss.WriteHeader("id", StompHelper.ToStomp(consumerId));
				
				ss.Flush();
			}
		}
		
		
		protected virtual void WriteTransactionInfo(TransactionInfo command, StompFrameStream ss)
		{
			TransactionId id = command.TransactionId;
			if (id is LocalTransactionId)
			{
				string type = "BEGIN";
				TransactionType transactionType = (TransactionType) command.Type;
				switch (transactionType)
				{
					case TransactionType.CommitOnePhase:
						command.ResponseRequired = true;
						type = "COMMIT";
						break;
					case TransactionType.Rollback:
						command.ResponseRequired = true;
						type = "ABORT";
						break;
				}
				Console.WriteLine(">>> For transaction type: " + transactionType + " we are using command type: " + type);
				
				ss.WriteCommand(command, type);
				
				ss.WriteHeader("transaction", StompHelper.ToStomp(id));
				
				ss.Flush();
			}
		}
		
		protected virtual void WriteMessage(ActiveMQMessage command, StompFrameStream ss)
		{
			ss.WriteCommand(command, "SEND");
			ss.WriteHeader("destination", StompHelper.ToStomp(command.Destination));
			ss.WriteHeader("reply-to", StompHelper.ToStomp(command.ReplyTo));
			ss.WriteHeader("correlation-id", command.CorrelationId);
			ss.WriteHeader("expires", command.Expiration);
			ss.WriteHeader("priority", command.Priority);
			ss.WriteHeader("type", command.Type);
			ss.WriteHeader("transaction", StompHelper.ToStomp(command.TransactionId));
			ss.WriteHeader("persistent", command.Persistent);
			
			// lets force the content to be marshalled
			
			command.BeforeMarshall(null);
			if (command is ActiveMQTextMessage)
			{
				ActiveMQTextMessage textMessage = command as ActiveMQTextMessage;
				ss.Content = encoding.GetBytes(textMessage.Text);
			}
			else
			{
				ss.Content = command.Content;
				ss.ContentLength = command.Content.Length;
			}
	
			IPrimitiveMap map = command.Properties;
			foreach (string key in map.Keys)
			{
				ss.WriteHeader(key, map[key]);
			}
			ss.Flush();
		}
		
		protected virtual void WriteMessageAck(MessageAck command, StompFrameStream ss)
		{
			ss.WriteCommand(command, "ACK");
			
			// TODO handle bulk ACKs?
			ss.WriteHeader("message-id", command.FirstMessageId);
			ss.WriteHeader("transaction", command.TransactionId);

			ss.Flush();
		}
		
		protected virtual void SendCommand(Command command)
		{
			if (transport == null)
			{
				Console.WriteLine("No transport configured so cannot return command: " + command);
			}
			else
			{
				transport.Command(transport, command);
			}
		}
		
		protected virtual string RemoveHeader(IDictionary headers, string name)
		{
			object value = headers[name];
			if (value == null)
			{
				return null;
			}
			else
			{
				headers.Remove(name);
				return value.ToString();
			}
		}
		
		
		protected virtual string ToString(object value)
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
