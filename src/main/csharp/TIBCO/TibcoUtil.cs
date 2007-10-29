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

namespace Apache.TibcoEMS
{
	public class TibcoUtil
	{
		static TibcoUtil()
		{
			epochDiff = (javaEpoch.ToFileTimeUtc() - windowsEpoch.ToFileTimeUtc())
							/ TimeSpan.TicksPerMillisecond;
		}

		public static Apache.NMS.IConnection ToNMSConnection(TIBCO.EMS.Connection tibcoConnection)
		{
			return (null != tibcoConnection
			        		? new Apache.TibcoEMS.Connection(tibcoConnection)
			        		: null);
		}

		public static Apache.NMS.ISession ToNMSSession(TIBCO.EMS.Session tibcoSession)
		{
			return (null != tibcoSession
			        		? new Apache.TibcoEMS.Session(tibcoSession)
			        		: null);
		}

		public static Apache.NMS.IMessageProducer ToNMSMessageProducer(Apache.TibcoEMS.Session session,
					TIBCO.EMS.MessageProducer tibcoMessageProducer)
		{
			return (null != tibcoMessageProducer
			        		? new Apache.TibcoEMS.MessageProducer(session, tibcoMessageProducer)
			        		: null);
		}

		public static Apache.NMS.IMessageConsumer ToNMSMessageConsumer(Apache.TibcoEMS.Session session,
					TIBCO.EMS.MessageConsumer tibcoMessageConsumer)
		{
			return (null != tibcoMessageConsumer
			        		? new Apache.TibcoEMS.MessageConsumer(session, tibcoMessageConsumer)
			        		: null);
		}

		public static Apache.NMS.IQueue ToNMSQueue(TIBCO.EMS.Queue tibcoQueue)
		{
			return (null != tibcoQueue
			        		? new Apache.TibcoEMS.Queue(tibcoQueue)
			        		: null);
		}

		public static Apache.NMS.ITopic ToNMSTopic(TIBCO.EMS.Topic tibcoTopic)
		{
			return (null != tibcoTopic
			        		? new Apache.TibcoEMS.Topic(tibcoTopic)
			        		: null);
		}

		public static Apache.NMS.ITemporaryQueue ToNMSTemporaryQueue(
				TIBCO.EMS.TemporaryQueue tibcoTemporaryQueue)
		{
			return (null != tibcoTemporaryQueue
			        		? new Apache.TibcoEMS.TemporaryQueue(tibcoTemporaryQueue)
			        		: null);
		}

		public static Apache.NMS.ITemporaryTopic ToNMSTemporaryTopic(
				TIBCO.EMS.TemporaryTopic tibcoTemporaryTopic)
		{
			return (null != tibcoTemporaryTopic
			        		? new Apache.TibcoEMS.TemporaryTopic(tibcoTemporaryTopic)
			        		: null);
		}

		public static Apache.NMS.IDestination ToNMSDestination(TIBCO.EMS.Destination tibcoDestination)
		{
			if(tibcoDestination is TIBCO.EMS.Queue)
			{
				return ToNMSQueue((TIBCO.EMS.Queue) tibcoDestination);
			}

			if(tibcoDestination is TIBCO.EMS.Topic)
			{
				return ToNMSTopic((TIBCO.EMS.Topic) tibcoDestination);
			}

			if(tibcoDestination is TIBCO.EMS.TemporaryQueue)
			{
				return ToNMSTemporaryQueue((TIBCO.EMS.TemporaryQueue) tibcoDestination);
			}

			if(tibcoDestination is TIBCO.EMS.TemporaryTopic)
			{
				return ToNMSTemporaryTopic((TIBCO.EMS.TemporaryTopic) tibcoDestination);
			}

			return null;
		}

		public static Apache.NMS.IMessage ToNMSMessage(TIBCO.EMS.Message tibcoMessage)
		{
			if(tibcoMessage is TIBCO.EMS.TextMessage)
			{
				return ToNMSTextMessage(tibcoMessage as TIBCO.EMS.TextMessage);
			}

			if(tibcoMessage is TIBCO.EMS.BytesMessage)
			{
				return ToNMSBytesMessage(tibcoMessage as TIBCO.EMS.BytesMessage);
			}

			if(tibcoMessage is TIBCO.EMS.MapMessage)
			{
				return ToNMSMapMessage(tibcoMessage as TIBCO.EMS.MapMessage);
			}

			if(tibcoMessage is TIBCO.EMS.ObjectMessage)
			{
				return ToNMSObjectMessage(tibcoMessage as TIBCO.EMS.ObjectMessage);
			}

			return (null != tibcoMessage
			        		? new Apache.TibcoEMS.Message(tibcoMessage)
			        		: null);
		}

		public static Apache.NMS.ITextMessage ToNMSTextMessage(TIBCO.EMS.TextMessage tibcoTextMessage)
		{
			return (null != tibcoTextMessage
			        		? new Apache.TibcoEMS.TextMessage(tibcoTextMessage)
			        		: null);
		}

		public static Apache.NMS.IBytesMessage ToNMSBytesMessage(
				TIBCO.EMS.BytesMessage tibcoBytesMessage)
		{
			return (null != tibcoBytesMessage
			        		? new Apache.TibcoEMS.BytesMessage(tibcoBytesMessage)
			        		: null);
		}

		public static Apache.NMS.IMapMessage ToNMSMapMessage(TIBCO.EMS.MapMessage tibcoMapMessage)
		{
			return (null != tibcoMapMessage
			        		? new Apache.TibcoEMS.MapMessage(tibcoMapMessage)
			        		: null);
		}

		public static Apache.NMS.IObjectMessage ToNMSObjectMessage(
				TIBCO.EMS.ObjectMessage tibcoObjectMessage)
		{
			return (null != tibcoObjectMessage
			        		? new Apache.TibcoEMS.ObjectMessage(tibcoObjectMessage)
			        		: null);
		}

		public static TIBCO.EMS.SessionMode ToSessionMode(Apache.NMS.AcknowledgementMode acknowledge)
		{
			TIBCO.EMS.SessionMode sessionMode = TIBCO.EMS.SessionMode.NoAcknowledge;

			switch(acknowledge)
			{
			case Apache.NMS.AcknowledgementMode.AutoAcknowledge:
				sessionMode = TIBCO.EMS.SessionMode.AutoAcknowledge;
				break;

			case Apache.NMS.AcknowledgementMode.AutoClientAcknowledge:
				sessionMode = TIBCO.EMS.SessionMode.ClientAcknowledge;
				break;

			case Apache.NMS.AcknowledgementMode.ClientAcknowledge:
				sessionMode = TIBCO.EMS.SessionMode.ExplicitClientAcknowledge;
				break;

			case Apache.NMS.AcknowledgementMode.DupsOkAcknowledge:
				sessionMode = TIBCO.EMS.SessionMode.DupsOkAcknowledge;
				break;

			case Apache.NMS.AcknowledgementMode.Transactional:
				sessionMode = TIBCO.EMS.SessionMode.SessionTransacted;
				break;
			}

			return sessionMode;
		}

		public static Apache.NMS.AcknowledgementMode ToAcknowledgementMode(
				TIBCO.EMS.SessionMode sessionMode)
		{
			Apache.NMS.AcknowledgementMode acknowledge = Apache.NMS.AcknowledgementMode.AutoAcknowledge;

			switch(sessionMode)
			{
			case TIBCO.EMS.SessionMode.AutoAcknowledge:
				acknowledge = Apache.NMS.AcknowledgementMode.AutoAcknowledge;
				break;

			case TIBCO.EMS.SessionMode.ClientAcknowledge:
				acknowledge = Apache.NMS.AcknowledgementMode.AutoClientAcknowledge;
				break;

			case TIBCO.EMS.SessionMode.ExplicitClientAcknowledge:
				acknowledge = Apache.NMS.AcknowledgementMode.ClientAcknowledge;
				break;

			case TIBCO.EMS.SessionMode.DupsOkAcknowledge:
				acknowledge = Apache.NMS.AcknowledgementMode.DupsOkAcknowledge;
				break;

			case TIBCO.EMS.SessionMode.SessionTransacted:
				acknowledge = Apache.NMS.AcknowledgementMode.Transactional;
				break;
			}

			return acknowledge;
		}

		public static Apache.NMS.IPrimitiveMap ToMessageProperties(TIBCO.EMS.Message tibcoMessage)
		{
			return (null != tibcoMessage
			        		? new Apache.TibcoEMS.MessageProperties(tibcoMessage)
			        		: null);
		}

		public static TIBCO.EMS.MessageDeliveryMode ToMessageDeliveryMode(bool persistent)
		{
			return (persistent
						? TIBCO.EMS.MessageDeliveryMode.Persistent
						: TIBCO.EMS.MessageDeliveryMode.NonPersistent);
		}

		public static bool ToPersistent(TIBCO.EMS.MessageDeliveryMode deliveryMode)
		{
			return (TIBCO.EMS.MessageDeliveryMode.NonPersistent != deliveryMode);
		}

		#region DateUtils
		// This secton of utility functions was taken from the ActiveMQ.DateUtils class.
		// These functions should be merged into a common code area that can be shared
		// between all provider implementations.
		
		/// <summary>
		/// The start of the Windows epoch
		/// </summary>
		public static readonly DateTime windowsEpoch = new DateTime(1601, 1, 1, 0, 0, 0, 0);
		/// <summary>
		/// The start of the Java epoch
		/// </summary>
		public static readonly DateTime javaEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);

		/// <summary>
		/// The difference between the Windows epoch and the Java epoch
		/// in milliseconds.
		/// </summary>
		public static readonly long epochDiff; /* = 1164447360000L; */

		public static long ToJavaTime(DateTime dateTime)
		{
			return (dateTime.ToFileTime() / TimeSpan.TicksPerMillisecond) - epochDiff;
		}

		public static DateTime ToDateTime(long javaTime)
		{
			return DateTime.FromFileTime((javaTime + epochDiff) * TimeSpan.TicksPerMillisecond);
		}

		public static long ToJavaTimeUtc(DateTime dateTime)
		{
			return (dateTime.ToFileTimeUtc() / TimeSpan.TicksPerMillisecond) - epochDiff;
		}

		public static DateTime ToDateTimeUtc(long javaTime)
		{
			return DateTime.FromFileTimeUtc((javaTime + epochDiff) * TimeSpan.TicksPerMillisecond);
		}
		#endregion

	}
}