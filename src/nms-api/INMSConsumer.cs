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
    /// An object capable of sending messages to some destination
    /// https://www.oracle.com/technical-resources/articles/java/jms20.html
    /// </summary>
    public interface INMSConsumer : System.IDisposable
    {
        /// <summary>
        /// Waits until a message is available and returns it
        /// </summary>
        IMessage Receive();
        
        /// <summary>
        /// Waits until a message is available and returns it
        /// </summary>
        Task<IMessage> ReceiveAsync();

        /// <summary>
        /// If a message is available within the timeout duration it is returned otherwise this method returns null
        /// </summary>
        IMessage Receive(TimeSpan timeout);
        
        /// <summary>
        /// If a message is available within the timeout duration it is returned otherwise this method returns null
        /// </summary>
        Task<IMessage> ReceiveAsync(TimeSpan timeout);

        /// <summary>
        /// Receives the next message if one is immediately available for delivery on the client side
        /// otherwise this method returns null. It is never an error for this method to return null, the
        /// time of Message availability varies so your client cannot rely on this method to receive a
        /// message immediately after one has been sent.
        /// </summary>
        IMessage ReceiveNoWait();


        T ReceiveBody<T>();
        
        Task<T> ReceiveBodyAsync<T>();

        T ReceiveBody<T>(TimeSpan timeout);
        
        Task<T> ReceiveBodyAsync<T>(TimeSpan timeout);

        T ReceiveBodyNoWait<T>();

        string MessageSelector { get; }

        /// <summary>
        /// An asynchronous listener which can be used to consume messages asynchronously
        /// </summary>
        event MessageListener Listener;

        /// <summary>
        /// Closes the message consumer.
        /// </summary>
        /// <remarks>
        /// Clients should close message consumers when they are not needed.
        /// This call blocks until a receive or message listener in progress has completed.
        /// A blocked message consumer receive call returns null when this message consumer is closed.
        /// </remarks>
        void Close();

        /// <summary>
        /// Closes the message consumer.
        /// </summary>
        /// <remarks>
        /// Clients should close message consumers when they are not needed.
        /// </remarks>
        Task CloseAsync();

        /// <summary>
        /// A Delegate that is called each time a Message is dispatched to allow the client to do
        /// any necessary transformations on the received message before it is delivered.
        /// </summary>
        ConsumerTransformerDelegate ConsumerTransformer { get; set; }
    }
}