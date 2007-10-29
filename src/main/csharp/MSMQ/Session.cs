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
using Apache.NMS;
using System;
using System.Messaging;

namespace Apache.MSMQ
{
    /// <summary>
    /// MSQM provider of ISession
    /// </summary>
    public class Session : ISession
    {
        private Connection connection;
        private AcknowledgementMode acknowledgementMode;
        private MessageQueueTransaction messageQueueTransaction;
        private IMessageConverter messageConverter;

        public Session(Connection connection, AcknowledgementMode acknowledgementMode)
        {
            this.connection = connection;
            this.acknowledgementMode = acknowledgementMode;
            MessageConverter = connection.MessageConverter;
            if (this.acknowledgementMode == AcknowledgementMode.Transactional)
            {
                MessageQueueTransaction = new MessageQueueTransaction();
            }
        }

        public void Dispose()
        {
            if(MessageQueueTransaction!=null)
            {
                MessageQueueTransaction.Dispose();
            }
        }
        
        public IMessageProducer CreateProducer()
        {
            return CreateProducer(null);
        }
        
        public IMessageProducer CreateProducer(IDestination destination)
        {
            return new MessageProducer(this, (Destination) destination);
        }
        
        public IMessageConsumer CreateConsumer(IDestination destination)
        {
            return CreateConsumer(destination, null);
        }
        
        public IMessageConsumer CreateConsumer(IDestination destination, string selector)
        {
            return CreateConsumer(destination, selector, false);
        }

        public IMessageConsumer CreateConsumer(IDestination destination, string selector, bool noLocal)
        {
			if (selector != null)
			{
				throw new NotImplementedException("Selectors are not supported by MSQM");
			}
			MessageQueue queue = MessageConverter.ToMsmqDestination(destination);
            return new MessageConsumer(this, acknowledgementMode, queue);
        }

        public IMessageConsumer CreateDurableConsumer(ITopic destination, string name, string selector, bool noLocal)
        {
            throw new NotImplementedException("Durable Topic subscribers are not supported by MSMQ");
        }
        
        public IQueue GetQueue(string name)
        {
            return new Queue(name);
        }
        
        public ITopic GetTopic(string name)
        {
            throw new NotImplementedException("Topics are not supported by MSQM");
        }
        
        public ITemporaryQueue CreateTemporaryQueue()
        {
            throw new NotImplementedException("Tempoary Queues are not supported by MSQM");
        }
        
        public ITemporaryTopic CreateTemporaryTopic()
        {
            throw new NotImplementedException("Tempoary Topics are not supported by MSQM");
        }
        
        public IMessage CreateMessage()
        {
            BaseMessage answer = new BaseMessage();
            return answer;
        }
        
        
        public ITextMessage CreateTextMessage()
        {
            TextMessage answer = new TextMessage();
            return answer;
        }
        
        public ITextMessage CreateTextMessage(string text)
        {
            TextMessage answer = new TextMessage(text);
            return answer;
        }
        
        public IMapMessage CreateMapMessage()
        {
            return new MapMessage();
        }
        
        public IBytesMessage CreateBytesMessage()
        {
            return new BytesMessage();
        }
        
        public IBytesMessage CreateBytesMessage(byte[] body)
        {
            BytesMessage answer = new BytesMessage();
            answer.Content = body;
            return answer;
        }
		
		public IObjectMessage CreateObjectMessage(Object body)
		{
			ObjectMessage answer = new ObjectMessage();
			answer.Body = body;
			return answer;
		}
		
        public void Commit()
        {
            if (! Transacted )
            {
                throw new InvalidOperationException("You cannot perform a Commit() on a non-transacted session. Acknowlegement mode is: " + acknowledgementMode);
            }
            messageQueueTransaction.Commit();
        }
        
        public void Rollback()
        {
            if (! Transacted)
            {
                throw new InvalidOperationException("You cannot perform a Commit() on a non-transacted session. Acknowlegement mode is: " + acknowledgementMode);
            }
            messageQueueTransaction.Abort();
        }
        
        // Properties
        public Connection Connection
        {
            get { return connection; }
        }
        
        public bool Transacted
        {
            get { return acknowledgementMode == AcknowledgementMode.Transactional; }
        }

        public AcknowledgementMode AcknowledgementMode
        {
            get { throw new NotImplementedException(); }
        }

        public MessageQueueTransaction MessageQueueTransaction
        {
            get
            {
                if( messageQueueTransaction.Status != MessageQueueTransactionStatus.Pending )
                    messageQueueTransaction.Begin();
                return messageQueueTransaction;
            }
            set { messageQueueTransaction = value; }
        }

        public IMessageConverter MessageConverter
        {
            get { return messageConverter; }
            set { messageConverter = value; }
        }

        public void Close()
        {
            Dispose();
        }

    }
}
