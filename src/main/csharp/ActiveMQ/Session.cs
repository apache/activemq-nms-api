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
using ActiveMQ.Commands;
using NMS;
using System;
using System.Collections;

namespace ActiveMQ {
        /// <summary>
        /// Default provider of ISession
        /// </summary>
        public class Session : ISession
        {
                private Connection connection;
                private SessionInfo info;
                private AcknowledgementMode acknowledgementMode;
                private long consumerCounter;
                private long producerCounter;
                private int prefetchSize = 1000;
                private int maximumPendingMessageLimit;
                private byte priority;
                private bool dispatchAsync;
                private bool exclusive;
                private bool retroactive;
                private IDictionary consumers = Hashtable.Synchronized(new Hashtable());
                private TransactionContext transactionContext;
                private DispatchingThread dispatchingThread;
                
                public Session(Connection connection, SessionInfo info, AcknowledgementMode acknowledgementMode)
                {
                        this.connection = connection;
                        this.info = info;
                        this.acknowledgementMode = acknowledgementMode;
                        transactionContext = new TransactionContext(this);
                        dispatchingThread = new DispatchingThread(new DispatchingThread.DispatchFunction(DispatchAsyncMessages));
                        dispatchingThread.ExceptionListener += new DispatchingThread.ExceptionHandler(dispatchingThread_ExceptionListener);
                }

                void dispatchingThread_ExceptionListener(Exception exception)
                {
                        connection.OnSessionException(this, exception);
                }


                /// <summary>
                /// Sets the prefetch size, the maximum number of messages a broker will dispatch to consumers
                /// until acknowledgements are received.
                /// </summary>
                public int PrefetchSize {
                        get { return prefetchSize; } 
                        set { this.prefetchSize = value; } 
                }

                /// <summary>
                /// Sets the maximum number of messages to keep around per consumer
                /// in addition to the prefetch window for non-durable topics until messages
                /// will start to be evicted for slow consumers.
                /// Must be > 0 to enable this feature
                /// </summary>
                public int MaximumPendingMessageLimit {
                        get { return maximumPendingMessageLimit; } 
                        set { this.maximumPendingMessageLimit = value; } 
                }

                /// <summary>
                /// Enables or disables whether asynchronous dispatch should be used by the broker
                /// </summary>
                public bool DispatchAsync {
                        get { return dispatchAsync; } 
                        set { this.dispatchAsync = value; } 
                }

                /// <summary>
                /// Enables or disables exclusive consumers when using queues. An exclusive consumer means
                /// only one instance of a consumer is allowed to process messages on a queue to preserve order
                /// </summary>
                public bool Exclusive {
                        get { return exclusive; } 
                        set { this.exclusive = value; } 
                }

                /// <summary>
                /// Enables or disables retroactive mode for consumers; i.e. do they go back in time or not?
                /// </summary>
                public bool Retroactive {
                        get { return retroactive; } 
                        set { this.retroactive = value; } 
                }

                /// <summary>
                /// Sets the default consumer priority for consumers
                /// </summary>
                public byte Priority {
                        get { return priority; } 
                        set { this.priority = value; } 
                }

                public void Dispose()
                {
                        connection.DisposeOf(info.SessionId);
                }

                public IMessageProducer CreateProducer()
                {
                        return CreateProducer(null);
                }

                public IMessageProducer CreateProducer(IDestination destination)
                {
                        ProducerInfo command = CreateProducerInfo(destination);
                        connection.SyncRequest(command);
                        return new MessageProducer(this, command);
                }



                public IMessageConsumer CreateConsumer(IDestination destination)
                {
                        return CreateConsumer(destination, null);
                }

                public IMessageConsumer CreateConsumer(IDestination destination, string selector)
                {
	                return CreateConsumer(destination, null, false);
				}
				
                public IMessageConsumer CreateConsumer(IDestination destination, string selector, bool noLocal)
                {
                        ConsumerInfo command = CreateConsumerInfo(destination, selector);
                        command.NoLocal = noLocal;

                        ConsumerId consumerId = command.ConsumerId;

                        try
                        {
                                MessageConsumer consumer = new MessageConsumer(this, command, acknowledgementMode);
                                // lets register the consumer first in case we start dispatching messages immediately
                                connection.AddConsumer(consumerId, consumer);

                                connection.SyncRequest(command);

                                consumers[consumerId] = consumer;
                                return consumer;
                        }
                        catch (Exception e)
                        {
                                connection.RemoveConsumer(consumerId);
                                throw e;
                        }
                }

                public IMessageConsumer CreateDurableConsumer(
                        ITopic destination,
                        string name,
                        string selector,
                        bool noLocal)
                {
                        ConsumerInfo command = CreateConsumerInfo(destination, selector);
                        ConsumerId consumerId = command.ConsumerId;
                        command.SubscriptionName = name;
                        command.NoLocal = noLocal;

                        try
                        {
                                MessageConsumer consumer = new MessageConsumer(this, command, acknowledgementMode);
                                // lets register the consumer first in case we start dispatching messages immediately
                                connection.AddConsumer(consumerId, consumer);

                                connection.SyncRequest(command);

                                consumers[consumerId] = consumer;
                                return consumer;
                        }
                        catch (Exception e)
                        {
                                connection.RemoveConsumer(consumerId);
                                throw e;
                        }
                }

                public IQueue GetQueue(string name)
                {
                        return new ActiveMQQueue(name);
                }

                public ITopic GetTopic(string name)
                {
                        return new ActiveMQTopic(name);
                }

                public ITemporaryQueue CreateTemporaryQueue()
                {
                        ActiveMQTempQueue answer = new ActiveMQTempQueue(connection.CreateTemporaryDestinationName());
                        CreateTemporaryDestination(answer);
                        return answer;
                }

                public ITemporaryTopic CreateTemporaryTopic()
                {
                        ActiveMQTempTopic answer = new ActiveMQTempTopic(connection.CreateTemporaryDestinationName());
                        CreateTemporaryDestination(answer);
                        return answer;
                }

                protected void CreateTemporaryDestination(ActiveMQDestination tempDestination)
                {
                        DestinationInfo command = new DestinationInfo();
                        command.ConnectionId = connection.ConnectionId;
                        command.OperationType = 0; // 0 is add
                        command.Destination = tempDestination;

                        connection.SyncRequest(command);
                }

                protected void DestroyTemporaryDestination(ActiveMQDestination tempDestination)
                {
                        DestinationInfo command = new DestinationInfo();
                        command.ConnectionId = connection.ConnectionId;
                        command.OperationType = 1; // 1 is remove
                        command.Destination = tempDestination;

                        connection.SyncRequest(command);
                }


                public IMessage CreateMessage()
                {
                        ActiveMQMessage answer = new ActiveMQMessage();
                        Configure(answer);
                        return answer;
                }


                public ITextMessage CreateTextMessage()
                {
                        ActiveMQTextMessage answer = new ActiveMQTextMessage();
                        Configure(answer);
                        return answer;
                }

                public ITextMessage CreateTextMessage(string text)
                {
                        ActiveMQTextMessage answer = new ActiveMQTextMessage(text);
                        Configure(answer);
                        return answer;
                }

                public IMapMessage CreateMapMessage()
                {
                        return new ActiveMQMapMessage();
                }

                public IBytesMessage CreateBytesMessage()
                {
                        return new ActiveMQBytesMessage();
                }

                public IBytesMessage CreateBytesMessage(byte[] body)
                {
                        ActiveMQBytesMessage answer = new ActiveMQBytesMessage();
                        answer.Content = body;
                        return answer;
                }

                public void Commit()
                {
                        if (!Transacted)
                        {
                                throw new InvalidOperationException(
                                        "You cannot perform a Commit() on a non-transacted session. Acknowlegement mode is: "
                                                + acknowledgementMode);
                        }
                        transactionContext.Commit();
                }

                public void Rollback()
                {
                        if (!Transacted)
                        {
                                throw new InvalidOperationException(
                                        "You cannot perform a Commit() on a non-transacted session. Acknowlegement mode is: "
                                                + acknowledgementMode);
                        }
                        transactionContext.Rollback();

                        // lets ensure all the consumers redeliver any rolled back messages
                        foreach (MessageConsumer consumer in GetConsumers())
                        {
                                consumer.RedeliverRolledBackMessages();
                        }
                }



                // Properties

                public Connection Connection {
                        get { return connection; } 
                }

                public SessionId SessionId {
                        get { return info.SessionId; } 
                }

                public AcknowledgementMode AcknowledgementMode {
                        get { return acknowledgementMode; } 
                }

                public bool Transacted {
                        get { return acknowledgementMode == AcknowledgementMode.Transactional; } 
                }

                public TransactionContext TransactionContext {
                        get { return transactionContext; } 
                }

                // Implementation methods
                public void DoSend(IDestination destination, IMessage message, bool persistent, byte priority, TimeSpan timeToLive)
                {
                        ActiveMQMessage command = ActiveMQMessage.Transform(message);
						command.Persistent = persistent;
						command.Priority = priority;
						
                        // TODO add time to live

                        connection.SyncRequest(command);
                }

                public void Close()
                {
                        // To do: what about session id?
                        StopAsyncDelivery();
                }
                
                /// <summary>
                /// Ensures that a transaction is started
                /// </summary>
                public void DoStartTransaction()
                {
                        if (Transacted)
                        {
                                transactionContext.Begin();
                        }
                }

                public void DisposeOf(ConsumerId objectId)
                {
                        consumers.Remove(objectId);
                        connection.RemoveConsumer(objectId);
                        connection.DisposeOf(objectId);
                }

                /// <summary>
                /// Private method called by the dispatcher thread in order to perform
                /// asynchronous delivery of queued (inbound) messages.
                /// </summary>
                private void DispatchAsyncMessages()
                {
                        // lets iterate through each consumer created by this session
                        // ensuring that they have all pending messages dispatched
                        foreach (MessageConsumer consumer in GetConsumers())
                        {
                                consumer.DispatchAsyncMessages();
                        }
                }


                /// <summary>
                /// Returns a copy of the current consumers in a thread safe way to avoid concurrency
                /// problems if the consumers are changed in another thread
                /// </summary>
                protected ICollection GetConsumers()
                {
                        lock (consumers.SyncRoot)
                        {
                                return new ArrayList(consumers.Values);
                        }
                }

                protected virtual ConsumerInfo CreateConsumerInfo(IDestination destination, string selector)
                {
                        ConsumerInfo answer = new ConsumerInfo();
                        ConsumerId id = new ConsumerId();
                        id.ConnectionId = info.SessionId.ConnectionId;
                        id.SessionId = info.SessionId.Value;
                        lock (this)
                        {
                                id.Value = ++consumerCounter;
                        }
                        answer.ConsumerId = id;
                        answer.Destination = ActiveMQDestination.Transform(destination);
                        answer.Selector = selector;
                        answer.PrefetchSize = prefetchSize;
                        answer.Priority = priority;
                        answer.Exclusive = exclusive;
                        answer.DispatchAsync = dispatchAsync;
                        answer.Retroactive = retroactive;

                        // If the destination contained a URI query, then use it to set public properties
                        // on the ConsumerInfo
                        ActiveMQDestination amqDestination = destination as ActiveMQDestination;
                        if (amqDestination != null && amqDestination.Options != null)
                        {
                                Util.URISupport.SetProperties(answer, amqDestination.Options, "consumer.");
                        }

                        return answer;
                }

                protected virtual ProducerInfo CreateProducerInfo(IDestination destination)
                {
                        ProducerInfo answer = new ProducerInfo();
                        ProducerId id = new ProducerId();
                        id.ConnectionId = info.SessionId.ConnectionId;
                        id.SessionId = info.SessionId.Value;
                        lock (this)
                        {
                                id.Value = ++producerCounter;
                        }
                        answer.ProducerId = id;
                        answer.Destination = ActiveMQDestination.Transform(destination);

                        answer.Destination = ActiveMQDestination.Transform(destination);

                        // If the destination contained a URI query, then use it to set public
                        // properties on the ProducerInfo
                        ActiveMQDestination amqDestination = destination as ActiveMQDestination;
                        if (amqDestination != null && amqDestination.Options != null)
                        {
                                Util.URISupport.SetProperties(answer, amqDestination.Options, "producer.");
                        }

                        return answer;
                }

                /// <summary>
                /// Configures the message command
                /// </summary>
                protected void Configure(ActiveMQMessage message)
                {
                }

				internal void StopAsyncDelivery()
				{
					dispatchingThread.Stop();
				}

				internal void StartAsyncDelivery(Dispatcher dispatcher)
				{
					if(dispatcher != null)
						dispatcher.SetAsyncDelivery(dispatchingThread.EventHandle);
					dispatchingThread.Start();
				}
        }
}
