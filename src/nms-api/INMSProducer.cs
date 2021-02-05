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
using System.Collections;
using System.Threading.Tasks;

namespace Apache.NMS
{
    /// <summary>
    /// An object capable of sending messages to some destination
    /// https://www.oracle.com/technical-resources/articles/java/jms20.html
    /// </summary>
    public interface INMSProducer : System.IDisposable
    {
        INMSProducer Send(IDestination destination, IMessage message);

        INMSProducer Send(IDestination destination, String body);

        INMSProducer Send(IDestination destination, IPrimitiveMap body);

        INMSProducer Send(IDestination destination, byte[] body);

        INMSProducer Send(IDestination destination, object body);

        Task<INMSProducer> SendAsync(IDestination destination, IMessage message);

        Task<INMSProducer> SendAsync(IDestination destination, String body);

        Task<INMSProducer> SendAsync(IDestination destination, IPrimitiveMap body);

        Task<INMSProducer> SendAsync(IDestination destination, byte[] body);

        Task<INMSProducer> SendAsync(IDestination destination, object body);

        /// <summary>
        /// Provides access to the message properties (headers).
        /// </summary>
        IPrimitiveMap Properties { get; }

        /// <summary>
        /// Clears a message's properties.
        ///
        /// The message's header fields and body are not cleared.
        /// </summary>
        INMSProducer ClearProperties();

        /// <summary>
        /// The correlation ID used to correlate messages from conversations or long running business processes.
        /// </summary>
        string NMSCorrelationID { get; set; }

        /// <summary>
        /// The destination that the consumer of this message should send replies to
        /// </summary>
        IDestination NMSReplyTo { get; set; }

        /// <summary>
        /// Specifies that messages sent using this NMSProducer will
        /// have their NMSType header value set to the specified message type.
        /// </summary>
        string NMSType { get; set; }


        /// <summary>
        /// A delegate that is called each time a Message is sent from this Producer which allows
        /// the application to perform any needed transformations on the Message before it is sent.
        /// </summary>
        ProducerTransformerDelegate ProducerTransformer { get; set; }

        MsgDeliveryMode DeliveryMode { get; set; }

        TimeSpan DeliveryDelay { get; set; }

        TimeSpan TimeToLive { get; set; }

        TimeSpan RequestTimeout { get; set; }

        MsgPriority Priority { get; set; }

        bool DisableMessageID { get; set; }

        bool DisableMessageTimestamp { get; set; }

        //Method chaining setters
        //Allows message delivery options, headers, and properties to be configured using method chaining
        INMSProducer SetDeliveryDelay(TimeSpan deliveryDelay);

        INMSProducer SetTimeToLive(TimeSpan timeToLive);

        INMSProducer SetDeliveryMode(MsgDeliveryMode deliveryMode);

        INMSProducer SetDisableMessageID(bool value);

        INMSProducer SetDisableMessageTimestamp(bool value);

        INMSProducer SetNMSCorrelationID(string correlationID);

        INMSProducer SetNMSReplyTo(IDestination replyTo);

        INMSProducer SetNMSType(string type);

        INMSProducer SetPriority(MsgPriority priority);

        INMSProducer SetProperty(string name, bool value);

        INMSProducer SetProperty(string name, byte value);

        INMSProducer SetProperty(string name, double value);

        INMSProducer SetProperty(string name, float value);

        INMSProducer SetProperty(string name, int value);

        INMSProducer SetProperty(string name, long value);

        INMSProducer SetProperty(string name, short value);

        INMSProducer SetProperty(string name, char value);

        INMSProducer SetProperty(string name, string value);

        INMSProducer SetProperty(string name, byte[] value);

        INMSProducer SetProperty(string name, IList value);

        INMSProducer SetProperty(string name, IDictionary value);


        #region Factory methods to create messages

        /// <summary>
        /// Creates a new message with an empty body
        /// </summary>
        IMessage CreateMessage();

        /// <summary>
        /// Creates a new text message with an empty body
        /// </summary>
        ITextMessage CreateTextMessage();

        /// <summary>
        /// Creates a new text message with the given body
        /// </summary>
        ITextMessage CreateTextMessage(string text);

        /// <summary>
        /// Creates a new Map message which contains primitive key and value pairs
        /// </summary>
        IMapMessage CreateMapMessage();

        /// <summary>
        /// Creates a new Object message containing the given .NET object as the body
        /// </summary>
        IObjectMessage CreateObjectMessage(object body);

        /// <summary>
        /// Creates a new binary message
        /// </summary>
        IBytesMessage CreateBytesMessage();

        /// <summary>
        /// Creates a new binary message with the given body
        /// </summary>
        IBytesMessage CreateBytesMessage(byte[] body);

        /// <summary>
        /// Creates a new stream message
        /// </summary>
        IStreamMessage CreateStreamMessage();

        #endregion

        /// <summary>
        /// Close the producer.
        /// </summary>
        void Close();

        /// <summary>
        /// Close the producer.
        /// </summary>
        Task CloseAsync();
    }
}