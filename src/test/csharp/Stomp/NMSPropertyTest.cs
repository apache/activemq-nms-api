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
using NMS;
using NUnit.Framework;
using Stomp;
using System;

namespace Stomp
{
    [ TestFixture ]
    public class NMSPropertyTest : NMS.Test.NMSPropertyTest
    {
        protected override IConnectionFactory CreateConnectionFactory()
        {
            return new ConnectionFactory();
        }
		
		protected override void AssertNonStringProperties(IMessage message)
		{
			// lets disable typesafe property testing as right now Stomp does not support them
		}
		
		
		protected override void AssertReplyToValid(IMessage message)
		{
			// TODO completely support temporary destinations in STOMP
			
			Assert.IsNotNull(message.NMSReplyTo, "NMSReplyTo");
			Assert.IsTrue(message.NMSReplyTo is ITemporaryQueue, "The reply to destination is not a TemporaryTopic!: " + message.NMSReplyTo);
		}
    }
}



