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

namespace ActiveMQ.Util
{
	internal class DateUtils
	{
		/// <summary>
		/// The difference between the Windows epoch (1601-01-01 00:00:00)
		/// and the Unix epoch (1970-01-01 00:00:00) in milliseconds.
		/// </summary>
		public static readonly long EPOCH_DIFF = 11644473600000L;

        /// <summary>
        /// The start of the UNIX epoch
        /// </summary>
        public static readonly DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, 0);

	    public static long ToJavaTime(DateTime dateTime)
		{
			return dateTime.ToFileTime() + EPOCH_DIFF;
		}

        public static DateTime ToDateTime(long dateTime)
        {
            return UNIX_EPOCH.AddMilliseconds(dateTime);
        }
	}
}
