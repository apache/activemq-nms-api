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

		INMSProducer SetAsync(CompletionListener completionListener);

		CompletionListener GetAsync();

		/// <summary>
		/// Provides access to the message properties (headers).
		/// </summary>
		IPrimitiveMap Properties { get; }
		
		/// <summary>
		/// Clears a message's properties.
		///
		/// The message's header fields and body are not cleared.
		/// </summary>
		void ClearProperties();
		
		/// <summary>
		/// The correlation ID used to correlate messages from conversations or long running business processes.
		/// </summary>
		string NMSCorrelationID { get; set; }

		/// <summary>
		/// The destination of the message.  This property is set by the IMessageProducer.
		/// </summary>
		IDestination NMSDestination { get; set; }

		/// <summary>
		/// The amount of time for which this message is valid.  Zero if this message does not expire.
		/// </summary>
		TimeSpan NMSTimeToLive { get; set; }

		/// <summary>
		/// The message ID which is set by the provider.
		/// </summary>
		string NMSMessageId { get; set; }

		/// <summary>
		/// Whether or not this message is persistent.
		/// </summary>
		MsgDeliveryMode NMSDeliveryMode { get; set; }

		/// <summary>
		/// The Priority of this message.
		/// </summary>
		MsgPriority NMSPriority { get; set; }

		/// <summary>
		/// Returns true if this message has been redelivered to this or another consumer before being acknowledged successfully.
		/// </summary>
		bool NMSRedelivered { get; set; }

		/// <summary>
		/// The destination that the consumer of this message should send replies to
		/// </summary>
		IDestination NMSReplyTo { get; set; }

		/// <summary>
		/// The timestamp of when the message was pubished in UTC time.  If the publisher disables setting
		/// the timestamp on the message, the time will be set to the start of the UNIX epoc (1970-01-01 00:00:00).
		/// </summary>
		DateTime NMSTimestamp { get; set; }

		/// <summary>
		/// The type name of this message.
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

	}
}
