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

namespace Apache.NMS.Util
{
	/// <summary>
	/// Class to provide support for working with Session objects.
	/// </summary>
	public class SessionUtil
	{
		private static string QueuePrefix = "queue:";
		private static string TopicPrefix = "topic:";
		private static string TempQueuePrefix = "temp-queue:";
		private static string TempTopicPrefix = "temp-topic:";

		/// <summary>
		/// Get the destination by parsing the embedded type prefix.  Default is Queue if no prefix is
		/// embedded in the destinationName.
		/// </summary>
		/// <param name="session">Session object to use to get the destination.</param>
		/// <param name="destinationName">Name of destination with embedded prefix.  The embedded prefix can be one of the following:
		///		<list type="bullet">
		///			<item>queue:</item>
		///			<item>topic:</item>
		///			<item>temp-queue:</item>
		///			<item>temp-topic:</item>
		///		</list>
		///	</param>
		/// <returns></returns>
		public static IDestination GetDestination(ISession session, string destinationName)
		{
			return SessionUtil.GetDestination(session, destinationName, DestinationType.Queue);
		}

		/// <summary>
		/// Get the destination by parsing the embedded type prefix.
		/// </summary>
		/// <param name="session">Session object to use to get the destination.</param>
		/// <param name="destinationName">Name of destination with embedded prefix.  The embedded prefix can be one of the following:
		///		<list type="bullet">
		///			<item>queue:</item>
		///			<item>topic:</item>
		///			<item>temp-queue:</item>
		///			<item>temp-topic:</item>
		///		</list>
		///	</param>
		/// <param name="defaultType">Default type if no embedded prefix is specified.</param>
		/// <returns></returns>
		public static IDestination GetDestination(ISession session, string destinationName, DestinationType defaultType)
		{
			IDestination destination = null;
			DestinationType destinationType = defaultType;

			if(0 == String.Compare(destinationName.Substring(0, QueuePrefix.Length), QueuePrefix, false))
			{
				destinationType = DestinationType.Queue;
				destinationName = destinationName.Substring(QueuePrefix.Length);
			}
			else if(0 == String.Compare(destinationName.Substring(0, TopicPrefix.Length), TopicPrefix, false))
			{
				destinationType = DestinationType.Topic;
				destinationName = destinationName.Substring(TopicPrefix.Length);
			}
			else if(0 == String.Compare(destinationName.Substring(0, TempQueuePrefix.Length), TempQueuePrefix, false))
			{
				destinationType = DestinationType.TemporaryQueue;
				destinationName = destinationName.Substring(TempQueuePrefix.Length);
			}
			else if(0 == String.Compare(destinationName.Substring(0, TempTopicPrefix.Length), TempTopicPrefix, false))
			{
				destinationType = DestinationType.TemporaryTopic;
				destinationName = destinationName.Substring(TempTopicPrefix.Length);
			}

			switch(destinationType)
			{
			case DestinationType.Queue:
				destination = session.GetQueue(destinationName);
			break;

			case DestinationType.Topic:
				destination = session.GetTopic(destinationName);
			break;

			case DestinationType.TemporaryQueue:
				destination = session.CreateTemporaryQueue();
			break;

			case DestinationType.TemporaryTopic:
				destination = session.CreateTemporaryTopic();
			break;
			}

			return destination;
		}

		public static IQueue GetQueue(ISession session, string queueName)
		{
			return GetDestination(session, queueName, DestinationType.Queue) as IQueue;
		}

		public static ITopic GetTopic(ISession session, string topicName)
		{
			return GetDestination(session, topicName, DestinationType.Topic) as ITopic;
		}
	}
}

