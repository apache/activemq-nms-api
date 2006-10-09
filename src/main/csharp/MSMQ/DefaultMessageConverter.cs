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
using System.Collections.Generic;
using System.Text;
using System.Messaging;
using NMS;

namespace MSMQ
{
    public class DefaultMessageConverter : IMessageConverter
	{
        public Message convertToMSMQMessage(IMessage message)
        {
            Message msg = new Message();
            MessageQueue responseQueue=null;
            if (message.NMSReplyTo != null)
            {
                responseQueue = new MessageQueue(((Destination)message.NMSReplyTo).Path);
            }
            if (message.NMSExpiration != null)
            {
                msg.TimeToBeReceived = message.NMSExpiration;
            }
            if (message.NMSCorrelationID != null)
            {
                msg.CorrelationId = message.NMSCorrelationID;
            }
            msg.Recoverable = message.NMSPersistent;
            msg.Priority = MessagePriority.Normal;
            msg.ResponseQueue = responseQueue;

            return msg;
        }
        public IMessage convertFromMSMQMessage(Message message) 
        {
            return null;
        }
	}
}
