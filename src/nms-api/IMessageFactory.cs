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

using System.Threading.Tasks;

namespace Apache.NMS
{
    /// <summary>
    /// Factory methods to create messages
    /// </summary>
    public interface IMessageFactory
    {
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
    }
}