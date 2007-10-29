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
using System.Text;
using System.Messaging;
using Apache.NMS;

namespace Apache.MSMQ
{
    public class DefaultMessageConverter : IMessageConverter
	{
        public virtual Message ToMsmqMessage(IMessage message)
        {
            Message answer = new Message();
            MessageQueue responseQueue=null;
            if (message.NMSReplyTo != null)
            {
                IDestination destination = message.NMSReplyTo;
				responseQueue = ToMsmqDestination(destination);
            }
            //if (message.NMSExpiration != null)
            //{
                answer.TimeToBeReceived = message.NMSTimeToLive;
            //}
            if (message.NMSCorrelationID != null)
            {
                answer.CorrelationId = message.NMSCorrelationID;
            }
            answer.Recoverable = message.NMSPersistent;
            answer.Priority = MessagePriority.Normal;
            answer.ResponseQueue = responseQueue;
			answer.Label = message.NMSType;
            return answer;
        }
		
        public virtual IMessage ToNmsMessage(Message message)
        {
			BaseMessage answer = CreateNmsMessage(message);
			answer.NMSMessageId = message.Id;
			if (message.CorrelationId != null)
			{
				answer.NMSCorrelationID = message.CorrelationId;
			}
			answer.NMSDestination = ToNmsDestination(message.DestinationQueue);
			answer.NMSType = message.Label;
			answer.NMSReplyTo = ToNmsDestination(message.ResponseQueue);
			answer.NMSTimeToLive = message.TimeToBeReceived;
            return answer;
        }
		
		
		public MessageQueue ToMsmqDestination(IDestination destination)
		{
			return new MessageQueue((destination as Destination).Path);
		}

		protected virtual IDestination ToNmsDestination(MessageQueue destinationQueue)
		{
			if (destinationQueue == null)
			{
				return null;
			}
			return new Queue(destinationQueue.Path);
		}
	
		protected virtual BaseMessage CreateNmsMessage(Message message)
		{
			object body = message.Body;
			if (body == null)
			{
				return new BaseMessage();
			}
			else if (body is string)
			{
				return new TextMessage(body as string);
			}
			else
			{
				return new ObjectMessage(body);
			}
		}
	}
}
