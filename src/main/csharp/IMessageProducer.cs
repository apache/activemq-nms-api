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

namespace Apache.NMS
{
	/// <summary>
	/// An object capable of sending messages to some destination
	/// </summary>
	public interface IMessageProducer : System.IDisposable
	{
		/// <summary>
		/// Sends the message to the default destination for this producer
		/// </summary>
		void Send(IMessage message);

		/// <summary>
		/// Sends the message to the default destination with the explicit QoS configuration
		/// </summary>
		void Send(IMessage message, MsgDeliveryMode deliveryMode, MsgPriority priority, TimeSpan timeToLive);

		/// <summary>
		/// Sends the message to the given destination
		/// </summary>
		void Send(IDestination destination, IMessage message);

		/// <summary>
		/// Sends the message to the given destination with the explicit QoS configuration
		/// </summary>
		void Send(IDestination destination, IMessage message, MsgDeliveryMode deliveryMode, MsgPriority priority, TimeSpan timeToLive);

		/// <summary>
		/// Close the producer.
		/// </summary>
		void Close();

		MsgDeliveryMode DeliveryMode { get; set; }

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

		#endregion
	}
}
