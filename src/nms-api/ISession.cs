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
using System.Threading.Tasks;

namespace Apache.NMS
{
    /// <summary>
    /// A delegate that is notified whenever a Transational evemt occurs for
    /// the specified session such as TX started, committed or rolled back.
    /// </summary>
    public delegate void SessionTxEventDelegate(ISession session);

    /// <summary>
    /// Represents a single unit of work on an IConnection.
    /// So the ISession can be used to perform transactional receive and sends
    /// </summary>
    public interface ISession : IDisposable
    {
        /// <summary>
        /// Creates a producer of messages
        /// </summary>
        IMessageProducer CreateProducer();
       
        /// <summary>
        /// Creates a producer of messages
        /// </summary>
        Task<IMessageProducer> CreateProducerAsync();

        /// <summary>
        /// Creates a producer of messages on a given destination
        /// </summary>
        IMessageProducer CreateProducer(IDestination destination);

        /// <summary>
        /// Creates a producer of messages on a given destination
        /// </summary>
        Task<IMessageProducer> CreateProducerAsync(IDestination destination);

        /// <summary>
        /// Creates a consumer of messages on a given destination
        /// </summary>
        IMessageConsumer CreateConsumer(IDestination destination);
        
        /// <summary>
        /// Creates a consumer of messages on a given destination
        /// </summary>
        Task<IMessageConsumer> CreateConsumerAsync(IDestination destination);

        /// <summary>
        /// Creates a consumer of messages on a given destination with a selector
        /// </summary>
        IMessageConsumer CreateConsumer(IDestination destination, string selector);
        
        /// <summary>
        /// Creates a consumer of messages on a given destination with a selector
        /// </summary>
        Task<IMessageConsumer> CreateConsumerAsync(IDestination destination, string selector);

        /// <summary>
        /// Creates a consumer of messages on a given destination with a selector
        /// </summary>
        IMessageConsumer CreateConsumer(IDestination destination, string selector, bool noLocal);

        /// <summary>
        /// Creates a consumer of messages on a given destination with a selector
        /// </summary>
        Task<IMessageConsumer> CreateConsumerAsync(IDestination destination, string selector, bool noLocal);

        IMessageConsumer CreateDurableConsumer(ITopic destination, string name);

        Task<IMessageConsumer> CreateDurableConsumerAsync(ITopic destination, string name);

        IMessageConsumer CreateDurableConsumer(ITopic destination, string name, string selector);
        
        Task<IMessageConsumer> CreateDurableConsumerAsync(ITopic destination, string name, string selector);

        /// <summary>
        /// Creates a named durable consumer of messages on a given destination with a selector
        /// </summary>
        IMessageConsumer CreateDurableConsumer(ITopic destination, string name, string selector, bool noLocal);
        
        /// <summary>
        /// Creates a named durable consumer of messages on a given destination with a selector
        /// </summary>
        Task<IMessageConsumer> CreateDurableConsumerAsync(ITopic destination, string name, string selector, bool noLocal);

        IMessageConsumer CreateSharedConsumer(ITopic destination, string name);

        Task<IMessageConsumer> CreateSharedConsumerAsync(ITopic destination, string name);

        IMessageConsumer CreateSharedConsumer(ITopic destination, string name, string selector);
        
        Task<IMessageConsumer> CreateSharedConsumerAsync(ITopic destination, string name, string selector);

        IMessageConsumer CreateSharedDurableConsumer(ITopic destination, string name);
        
        Task<IMessageConsumer> CreateSharedDurableConsumerAsync(ITopic destination, string name);

        IMessageConsumer CreateSharedDurableConsumer(ITopic destination, string name, string selector);
        
        Task<IMessageConsumer> CreateSharedDurableConsumerAsync(ITopic destination, string name, string selector);

        /// <summary>
        /// Deletes a durable consumer created with CreateDurableConsumer().
        /// </summary>
        /// <param name="name">Name of the durable consumer</param>
        [Obsolete("should use unsubscribe instead")]
        void DeleteDurableConsumer(string name);

        void Unsubscribe(string name);

        Task UnsubscribeAsync(string name);

        /// <summary>
        /// Creates a QueueBrowser object to peek at the messages on the specified queue.
        /// </summary>
        /// <param name="queue">
        /// A <see cref="IQueue"/>
        /// </param>
        /// <returns>
        /// A <see cref="IQueueBrowser"/>
        /// </returns>
        /// <exception cref="System.NotSupportedException">
        /// If the Prodiver does not support creation of Queue Browsers.
        /// </exception>
        IQueueBrowser CreateBrowser(IQueue queue);

        /// <summary>
        /// Creates a QueueBrowser object to peek at the messages on the specified queue.
        /// </summary>
        /// <param name="queue">
        /// A <see cref="IQueue"/>
        /// </param>
        /// <returns>
        /// A <see cref="IQueueBrowser"/>
        /// </returns>
        /// <exception cref="System.NotSupportedException">
        /// If the Prodiver does not support creation of Queue Browsers.
        /// </exception>
        Task<IQueueBrowser> CreateBrowserAsync(IQueue queue);

        /// <summary>
        /// Creates a QueueBrowser object to peek at the messages on the specified queue
        /// using a message selector.
        /// </summary>
        /// <param name="queue">
        /// A <see cref="IQueue"/>
        /// </param>
        /// <param name="selector">
        /// A <see cref="System.String"/>
        /// </param>
        /// <returns>
        /// A <see cref="IQueueBrowser"/>
        /// </returns>
        /// <exception cref="System.NotSupportedException">
        /// If the Prodiver does not support creation of Queue Browsers.
        /// </exception>
        IQueueBrowser CreateBrowser(IQueue queue, string selector);

        /// <summary>
        /// Creates a QueueBrowser object to peek at the messages on the specified queue
        /// using a message selector.
        /// </summary>
        /// <param name="queue">
        /// A <see cref="IQueue"/>
        /// </param>
        /// <param name="selector">
        /// A <see cref="System.String"/>
        /// </param>
        /// <returns>
        /// A <see cref="IQueueBrowser"/>
        /// </returns>
        /// <exception cref="System.NotSupportedException">
        /// If the Prodiver does not support creation of Queue Browsers.
        /// </exception>
        Task<IQueueBrowser> CreateBrowserAsync(IQueue queue, string selector);

        /// <summary>
        /// Returns the queue for the given name
        /// </summary>
        IQueue GetQueue(string name);
        
        /// <summary>
        /// Returns the queue for the given name
        /// </summary>
        Task<IQueue> GetQueueAsync(string name);

        /// <summary>
        /// Returns the topic for the given name
        /// </summary>
        ITopic GetTopic(string name);
        
        /// <summary>
        /// Returns the topic for the given name
        /// </summary>
        Task<ITopic> GetTopicAsync(string name);

        /// <summary>
        /// Creates a temporary queue
        /// </summary>
        ITemporaryQueue CreateTemporaryQueue();
        
        /// <summary>
        /// Creates a temporary queue
        /// </summary>
        Task<ITemporaryQueue> CreateTemporaryQueueAsync();

        /// <summary>
        /// Creates a temporary topic
        /// </summary>
        ITemporaryTopic CreateTemporaryTopic();

        /// <summary>
        /// Creates a temporary topic
        /// </summary>
        Task<ITemporaryTopic> CreateTemporaryTopicAsync();

        /// <summary>
        /// Delete a destination (Queue, Topic, Temp Queue, Temp Topic).
        /// </summary>
        void DeleteDestination(IDestination destination);
        
        /// <summary>
        /// Delete a destination (Queue, Topic, Temp Queue, Temp Topic).
        /// </summary>
        Task DeleteDestinationAsync(IDestination destination);

        // Factory methods to create messages

        /// <summary>
        /// Creates a new message with an empty body
        /// </summary>
        IMessage CreateMessage();

        /// <summary>
        /// Creates a new message with an empty body
        /// </summary>
        Task<IMessage> CreateMessageAsync();

        /// <summary>
        /// Creates a new text message with an empty body
        /// </summary>
        ITextMessage CreateTextMessage();

        /// <summary>
        /// Creates a new text message with an empty body
        /// </summary>
        Task<ITextMessage> CreateTextMessageAsync();

        /// <summary>
        /// Creates a new text message with the given body
        /// </summary>
        ITextMessage CreateTextMessage(string text);

        /// <summary>
        /// Creates a new text message with the given body
        /// </summary>
        Task<ITextMessage> CreateTextMessageAsync(string text);

        /// <summary>
        /// Creates a new Map message which contains primitive key and value pairs
        /// </summary>
        IMapMessage CreateMapMessage();

        /// <summary>
        /// Creates a new Map message which contains primitive key and value pairs
        /// </summary>
        Task<IMapMessage> CreateMapMessageAsync();

        /// <summary>
        /// Creates a new Object message containing the given .NET object as the body
        /// </summary>
        IObjectMessage CreateObjectMessage(object body);

        /// <summary>
        /// Creates a new Object message containing the given .NET object as the body
        /// </summary>
        Task<IObjectMessage> CreateObjectMessageAsync(object body);

        /// <summary>
        /// Creates a new binary message
        /// </summary>
        IBytesMessage CreateBytesMessage();

        /// <summary>
        /// Creates a new binary message
        /// </summary>
        Task<IBytesMessage> CreateBytesMessageAsync();

        /// <summary>
        /// Creates a new binary message with the given body
        /// </summary>
        IBytesMessage CreateBytesMessage(byte[] body);

        /// <summary>
        /// Creates a new binary message with the given body
        /// </summary>
        Task<IBytesMessage> CreateBytesMessageAsync(byte[] body);

        /// <summary>
        /// Creates a new stream message
        /// </summary>
        IStreamMessage CreateStreamMessage();

        /// <summary>
        /// Creates a new stream message
        /// </summary>
        Task<IStreamMessage> CreateStreamMessageAsync();

        /// <summary>
        /// Closes the session.  There is no need to close the producers and consumers
        /// of a closed session.
        /// </summary>
        void Close();
        Task CloseAsync();

        /// <summary>
        /// A Delegate that is called each time a Message is dispatched to allow the client to do
        /// any necessary transformations on the received message before it is delivered.
        /// The Session instance sets the delegate on each Consumer it creates.
        /// </summary>
        ConsumerTransformerDelegate ConsumerTransformer { get; set; }

        /// <summary>
        /// A delegate that is called each time a Message is sent from this Producer which allows
        /// the application to perform any needed transformations on the Message before it is sent.
        /// The Session instance sets the delegate on each Producer it creates.
        /// </summary>
        ProducerTransformerDelegate ProducerTransformer { get; set; }

        /// <summary>
        /// Stops all Message delivery in this session and restarts it again
        /// with the oldest unacknowledged message.  Messages that were delivered
        /// but not acknowledge should have their redelivered property set.
        /// This is an optional method that may not by implemented by all NMS
        /// providers, if not implemented an Exception will be thrown.
        /// Message redelivery is not requried to be performed in the original
        /// order.  It is not valid to call this method on a Transacted Session.
        /// </summary>
        void Recover();
        
        /// <summary>
        /// Stops all Message delivery in this session and restarts it again
        /// with the oldest unacknowledged message.  Messages that were delivered
        /// but not acknowledge should have their redelivered property set.
        /// This is an optional method that may not by implemented by all NMS
        /// providers, if not implemented an Exception will be thrown.
        /// Message redelivery is not requried to be performed in the original
        /// order.  It is not valid to call this method on a Transacted Session.
        /// </summary>
        Task RecoverAsync();

        void Acknowledge();
        
        Task AcknowledgeAsync();

        #region Transaction methods

        /// <summary>
        /// If this is a transactional session then commit all message
        /// send and acknowledgements for producers and consumers in this session
        /// </summary>
        void Commit();

        /// <summary>
        /// If this is a transactional session then commit all message
        /// send and acknowledgements for producers and consumers in this session
        /// </summary>
        Task CommitAsync();

        /// <summary>
        /// If this is a transactional session then rollback all message
        /// send and acknowledgements for producers and consumers in this session
        /// </summary>
        void Rollback();
        
        /// <summary>
        /// If this is a transactional session then rollback all message
        /// send and acknowledgements for producers and consumers in this session
        /// </summary>
        Task RollbackAsync();

        #endregion

        #region Session Events

        event SessionTxEventDelegate TransactionStartedListener;
        event SessionTxEventDelegate TransactionCommittedListener;
        event SessionTxEventDelegate TransactionRolledBackListener;

        #endregion

        #region Attributes

        TimeSpan RequestTimeout { get; set; }

        bool Transacted { get; }

        AcknowledgementMode AcknowledgementMode { get; }

        #endregion
    }
}