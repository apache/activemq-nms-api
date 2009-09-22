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
using System.Threading;

namespace Apache.NMS.Util
{
	/// <summary>
	/// Utility class for Tracking Memory Usage with an imposed limit on the amount
	/// available.  Provides methods for objects to wait on more space to become
	/// available if the memory limit is reached.
	/// </summary>
	public class MemoryUsage
	{
		private long limit = 0;
		private long usage = 0;
		private readonly object myLock = new object();

		public MemoryUsage()
		{
		}

		public MemoryUsage( long limit )
		{
			this.limit = limit;
		}

		#region Property Accessors

		public long Limit
		{
			get { return limit; }
			set { limit = value; }
		}

		public long Usage
		{
			get { return usage; }
			set { usage = value; }
		}

		#endregion

		/// <summary>
		/// If no space is available then this method blocks until more becomes available.
		/// </summary>
		public void WaitForSpace()
		{
			TimeSpan indefiniteWait = TimeSpan.FromMilliseconds(Timeout.Infinite);
			this.WaitForSpace(indefiniteWait);
		}

		/// <summary>
		/// If no space is available then this method blocks until more becomes available
		/// or until the specified timeout expires.
		/// </summary>
		/// <param name="timeout">
		/// A <see cref="System.TimeSpan"/>
		/// </param>
		public void WaitForSpace( TimeSpan timeout )
		{
			lock(this.myLock)
			{
				while(this.IsFull())
				{
#if !NETCF
					if( !Monitor.Wait(this.myLock, timeout ) )
#endif
					{
						return;
					}
				}
			}
		}

		/// <summary>
		/// Attempts to increase the amount of Memory Used, if non is available to fill
		/// then this method blocks until more is freed.
		/// </summary>
		/// <param name="usage">
		/// A <see cref="System.Int64"/>
		/// </param>
		public void EnqueueUsage( long usage )
		{
			this.WaitForSpace();
			this.IncreaseUsage(usage);
		}

		/// <summary>
		/// Increase the level of Usage.
		/// </summary>
		/// <param name="value">
		/// A <see cref="System.Int64"/>
		/// </param>
		public void IncreaseUsage( long value )
		{
			if(value == 0)
			{
				return;
			}

			lock(this.myLock)
			{
				this.Usage += value;
			}
		}

		/// <summary>
		/// Decrease the level of Usage.
		/// </summary>
		/// <param name="value">
		/// A <see cref="System.Int64"/>
		/// </param>
		public void DecreaseUsage( long value )
		{
			if(value == 0)
			{
				return;
			}

			lock(this.myLock)
			{
				if( value > this.Usage )
				{
					this.Usage = 0;
				}
				else
				{
					this.Usage -= value;
				}

#if !NETCF
				Monitor.PulseAll(this.myLock);
#endif
			}
		}

		public bool IsFull()
		{
			bool result = false;

			lock(this.myLock)
			{
				result = this.Usage >= this.Limit;
			}

			return result;
		}

	}
}
