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
using NUnit.Framework;

namespace Apache.NMS.Test
{
	[TestFixture]
	public abstract class ConnectionTest : NMSTestSupport
	{
		/// <summary>
		/// Verify that it is possible to create multiple connections to the broker.
		/// There was a bug in the connection factory which set the clientId member which made
		/// it impossible to create an additional connection.
		/// </summary>
		[Test]
		public void TwoConnections()
		{
			IConnection connection1 = Factory.CreateConnection();
			connection1.Start();

			IConnection connection2 = Factory.CreateConnection();
			connection2.Start();

			connection1.Stop();
			connection1.Dispose();
			connection2.Stop();
			connection2.Dispose();
			// with the bug present we'll get an exception in connection2.start()
		}
	}
}
