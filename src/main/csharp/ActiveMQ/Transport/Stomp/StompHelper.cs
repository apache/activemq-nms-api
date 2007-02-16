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
    /// Some <a href="http://stomp.codehaus.org/">STOMP</a> protocol conversion helper methods.
    /// </summary>
    public class StompHelper
    {
		public static ActiveMQDestination ToDestination(string text) 
		{
			int type = ActiveMQDestination.ACTIVEMQ_QUEUE;
			if (text.StartsWith("/queue/")) 
			{
				text = text.Substring("/queue/".Length);
			}
			else if (text.StartsWith("/topic/"))
			{
				text = text.Substring("/topic/".Length);
				type = ActiveMQDestination.ACTIVEMQ_TOPIC;
			}
			else if (text.StartsWith("/temptopic/"))
			{
				text = text.Substring("/temptopic/".Length);
				type = ActiveMQDestination.ACTIVEMQ_TEMPORARY_TOPIC;
			}
			else if (text.StartsWith("/tempqueue/"))
			{
				text = text.Substring("/tempqueue/".Length);
				type = ActiveMQDestination.ACTIVEMQ_TEMPORARY_QUEUE;
			}
			return ActiveMQDestination.CreateDestination(type, text);
		}

		public static string ToStomp(ActiveMQDestination destination)
		{
			if (destination == null)
			{
				return null;
			}
			else 
			{
				switch (destination.GetDestinationType())
				{
					case ActiveMQDestination.ACTIVEMQ_TOPIC:
						return "/topic/" + destination.PhysicalName;
					
					case ActiveMQDestination.ACTIVEMQ_TEMPORARY_TOPIC:
						return "/temptopic/" + destination.PhysicalName;
					
					case ActiveMQDestination.ACTIVEMQ_TEMPORARY_QUEUE:
						return "/tempqueue/" + destination.PhysicalName;
					
					default:
						return "/queue/" + destination.PhysicalName;
				}
			}
		}
		
		public static string ToStomp(ConsumerId id)
		{
			return id.ConnectionId + ":" + id.SessionId + ":" + id.Value;
		}
		
		public static ConsumerId ToConsumerId(string text)
		{
			if (text == null)
			{
				return null;
			}
			ConsumerId answer = new ConsumerId();
			int idx = text.LastIndexOf(':');
			if (idx >= 0) {
				answer.Value = Int32.Parse(text.Substring(idx + 1));
				text = text.Substring(0, idx);
				idx = text.LastIndexOf(':');
				if (idx >= 0) {
					answer.SessionId = Int32.Parse(text.Substring(idx + 1));
					text = text.Substring(0, idx);
				}
			}
			answer.ConnectionId = text;
			return answer;
		}
		
		public static string ToStomp(ProducerId id)
		{
			return id.ConnectionId + ":" + id.SessionId + ":" + id.Value;
		}
		
		public static ProducerId ToProducerId(string text)
		{
			if (text == null)
			{
				return null;
			}
			ProducerId answer = new ProducerId();
			int idx = text.LastIndexOf(':');
			if (idx >= 0) {
				answer.Value = Int32.Parse(text.Substring(idx + 1));
				text = text.Substring(0, idx);
				idx = text.LastIndexOf(':');
				if (idx >= 0) {
					answer.SessionId = Int32.Parse(text.Substring(idx + 1));
					text = text.Substring(0, idx);
				}
			}
			answer.ConnectionId = text;
			return answer;
		}
		
		public static string ToStomp(MessageId id)
		{
			return ToStomp(id.ProducerId) + ":" + id.BrokerSequenceId + ":" + id.ProducerSequenceId;
		}
		
		public static MessageId ToMessageId(string text)
		{
			if (text == null)
			{
				return null;
			}
			MessageId answer = new MessageId();
			int idx = text.LastIndexOf(':');
			if (idx >= 0) {
				answer.ProducerSequenceId = Int32.Parse(text.Substring(idx + 1));
				text = text.Substring(0, idx);
				idx = text.LastIndexOf(':');
				if (idx >= 0) {
					answer.BrokerSequenceId = Int32.Parse(text.Substring(idx + 1));
					text = text.Substring(0, idx);
				}
			}
			answer.ProducerId = ToProducerId(text);
			return answer;
		}
    }
}
