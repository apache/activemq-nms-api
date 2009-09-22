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
using Apache.NMS.Util;
using NUnit.Framework;
using NUnit.Framework.Extensions;

namespace Apache.NMS.Test
{
	[TestFixture]
	public class MemoryUsageTest : NMSTestSupport
	{

		[Test]
		public void TestConstructors()
		{
			MemoryUsage usage = new MemoryUsage();

			Assert.That(usage.Limit == 0);
			Assert.That(usage.Usage == 0);

			usage = new MemoryUsage(1024);

			Assert.That(usage.Limit == 1024);
			Assert.That(usage.Usage == 0);
		}

		[Test]
		public void TestUsage()
		{
			MemoryUsage usage1 = new MemoryUsage( 2048 );

			Assert.That( !usage1.IsFull() );
			Assert.That( usage1.Usage == 0 );

			usage1.IncreaseUsage( 1024 );

			Assert.That( !usage1.IsFull() );
			Assert.That( usage1.Usage == 1024 );

			usage1.DecreaseUsage( 512 );

			Assert.That( !usage1.IsFull() );
			Assert.That( usage1.Usage == 512 );

			usage1.Usage = 2048;

			Assert.That( usage1.IsFull() );
			Assert.That( usage1.Usage == 2048 );

			usage1.IncreaseUsage( 1024 );
			Assert.That( usage1.IsFull() );
			Assert.That( usage1.Usage == 3072 );
		}

		[Test]
		public void TestTimedWait()
		{
			MemoryUsage usage = new MemoryUsage( 2048 );
			usage.IncreaseUsage( 5072 );

			DateTime start = DateTime.Now;

			usage.WaitForSpace( TimeSpan.FromMilliseconds(150) );

			DateTime end = DateTime.Now;

			TimeSpan timePassed = end - start;

			Assert.That( timePassed.TotalMilliseconds >= 125 );
		}

		[Test]
		public void TestWait()
		{
			MemoryUsage usage = new MemoryUsage( 2048 );
			usage.IncreaseUsage( 5072 );

			Thread thread1 = new Thread(delegate ()
			{
				Thread.Sleep( 100 );
				usage.DecreaseUsage( usage.Usage );
			});

			thread1.Start();

			usage.WaitForSpace();
			Assert.That( usage.Usage == 0 );

			thread1.Join();
		}
	}
}
