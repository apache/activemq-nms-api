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
using System.Collections.Specialized;
using Apache.NMS.Util;

namespace Apache.NMS.Commands
{
    /// <summary>
    /// Summary description for Destination.
    /// </summary>
    public abstract class Destination : IDestination, ICloneable
    {
        /// <summary>
        /// Topic Destination object
        /// </summary>
        public const int TOPIC = 1;
        /// <summary>
        /// Temporary Topic Destination object
        /// </summary>
        public const int TEMPORARY_TOPIC = 2;
        /// <summary>
        /// Queue Destination object
        /// </summary>
        public const int QUEUE = 3;
        /// <summary>
        /// Temporary Queue Destination object
        /// </summary>
        public const int TEMPORARY_QUEUE = 4;

        private const String TEMP_PREFIX = "{TD{";
        private const String TEMP_POSTFIX = "}TD}";

        private String physicalName = "";
        private StringDictionary options = null;

		private bool disposed = false;

        /// <summary>
        /// The Default Constructor
        /// </summary>
        protected Destination()
        {
        }

        /// <summary>
        /// Construct the Destination with a defined physical name;
        /// </summary>
        /// <param name="name"></param>
        protected Destination(String name)
        {
            setPhysicalName(name);
        }

		~Destination()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if(disposed)
			{
				return;
			}

			if(disposing)
			{
				try
				{
					OnDispose();
				}
				catch(Exception ex)
				{
					Tracer.ErrorFormat("Exception disposing Destination {0}: {1}", this.physicalName, ex.Message);
				}
			}

			disposed = true;
		}

		/// <summary>
		/// Child classes can override this method to perform clean-up logic.
		/// </summary>
		protected virtual void OnDispose()
		{
		}

		public bool IsTopic
        {
            get
            {
                int destinationType = GetDestinationType();
                return TOPIC == destinationType
                    || TEMPORARY_TOPIC == destinationType;
            }
        }

        public bool IsQueue
        {
            get
            {
                int destinationType = GetDestinationType();
                return QUEUE == destinationType
                    || TEMPORARY_QUEUE == destinationType;
            }
        }

        public bool IsTemporary
        {
            get
            {
                int destinationType = GetDestinationType();
                return TEMPORARY_QUEUE == destinationType
                    || TEMPORARY_TOPIC == destinationType;
            }
        }

        /// <summary>
        /// Dictionary of name/value pairs representing option values specified
        /// in the URI used to create this Destination.  A null value is returned
        /// if no options were specified.
        /// </summary>
        internal StringDictionary Options
        {
            get { return this.options; }
        }

        private void setPhysicalName(string name)
        {
            this.physicalName = name;

            int p = name.IndexOf('?');
            if(p >= 0)
            {
                String optstring = physicalName.Substring(p + 1);
                this.physicalName = name.Substring(0, p);
                options = URISupport.ParseQuery(optstring);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="destination"></param>
        /// <returns></returns>
        public static Destination Transform(IDestination destination)
        {
            Destination result = null;
            if(destination != null)
            {
                if(destination is Destination)
                {
                    result = (Destination) destination;
                }
                else
                {
                    if(destination is ITemporaryQueue)
                    {
                        result = new TempQueue(((IQueue) destination).QueueName);
                    }
                    else if(destination is ITemporaryTopic)
                    {
                        result = new TempTopic(((ITopic) destination).TopicName);
                    }
                    else if(destination is IQueue)
                    {
                        result = new Queue(((IQueue) destination).QueueName);
                    }
                    else if(destination is ITopic)
                    {
                        result = new Topic(((ITopic) destination).TopicName);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Create a temporary name from the clientId
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public static String CreateTemporaryName(String clientId)
        {
            return TEMP_PREFIX + clientId + TEMP_POSTFIX;
        }

        /// <summary>
        /// From a temporary destination find the clientId of the Connection that created it
        /// </summary>
        /// <param name="destination"></param>
        /// <returns>the clientId or null if not a temporary destination</returns>
        public static String GetClientId(Destination destination)
        {
            String answer = null;
            if(destination != null && destination.IsTemporary)
            {
                String name = destination.PhysicalName;
                int start = name.IndexOf(TEMP_PREFIX);
                if(start >= 0)
                {
                    start += TEMP_PREFIX.Length;
                    int stop = name.LastIndexOf(TEMP_POSTFIX);
                    if(stop > start && stop < name.Length)
                    {
                        answer = name.Substring(start, stop);
                    }
                }
            }
            return answer;
        }

        /// <summary>
        /// </summary>
        /// <param name="o">object to compare</param>
        /// <returns>1 if this is less than o else 0 if they are equal or -1 if this is less than o</returns>
        public int CompareTo(Object o)
        {
            if(o is Destination)
            {
                return CompareTo((Destination) o);
            }
            return -1;
        }

        /// <summary>
        /// Lets sort by name first then lets sort topics greater than queues
        /// </summary>
        /// <param name="that">another destination to compare against</param>
        /// <returns>1 if this is less than o else 0 if they are equal or -1 if this is less than o</returns>
        public int CompareTo(Destination that)
        {
            int answer = 0;
            if(physicalName != that.physicalName)
            {
                if(physicalName == null)
                {
                    return -1;
                }
                else if(that.physicalName == null)
                {
                    return 1;
                }
                answer = physicalName.CompareTo(that.physicalName);
            }

            if(answer == 0)
            {
                if(IsTopic)
                {
                    if(that.IsQueue)
                    {
                        return 1;
                    }
                }
                else
                {
                    if(that.IsTopic)
                    {
                        return -1;
                    }
                }
            }
            return answer;
        }

        /// <summary>
        /// </summary>
        /// <returns>Returns the Destination type</returns>
        public abstract int GetDestinationType();

        public String PhysicalName
        {
            get { return this.physicalName; }
            set
            {
                this.physicalName = value;
            }
        }

        /// <summary>
        /// </summary>
        /// <returns>string representation of this instance</returns>
        public override String ToString()
        {
            switch(DestinationType)
            {
            case DestinationType.Topic:
            return "topic://" + PhysicalName;

            case DestinationType.TemporaryTopic:
            return "temp-topic://" + PhysicalName;

            case DestinationType.TemporaryQueue:
            return "temp-queue://" + PhysicalName;

            default:
            return "queue://" + PhysicalName;
            }
        }

        /// <summary>
        /// </summary>
        /// <returns>hashCode for this instance</returns>
        public override int GetHashCode()
        {
            int answer = 37;

            if(this.physicalName != null)
            {
                answer = physicalName.GetHashCode();
            }
            if(IsTopic)
            {
                answer ^= 0xfabfab;
            }
            return answer;
        }

        /// <summary>
        /// if the object passed in is equivalent, return true
        /// </summary>
        /// <param name="obj">the object to compare</param>
        /// <returns>true if this instance and obj are equivalent</returns>
        public override bool Equals(Object obj)
        {
            bool result = this == obj;
            if(!result && obj != null && obj is Destination)
            {
                Destination other = (Destination) obj;
                result = this.GetDestinationType() == other.GetDestinationType()
                    && this.physicalName.Equals(other.physicalName);
            }
            return result;
        }

        /// <summary>
        /// Factory method to create a child destination if this destination is a composite
        /// </summary>
        /// <param name="name"></param>
        /// <returns>the created Destination</returns>
        public abstract Destination CreateDestination(String name);

        public abstract DestinationType DestinationType
        {
            get;
        }

        public virtual Object Clone()
        {
            // Since we are the lowest level base class, do a
            // shallow copy which will include the derived classes.
            // From here we would do deep cloning of other objects
            // if we had any.
            return this.MemberwiseClone();
        }
    }
}

