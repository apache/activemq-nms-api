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

using Apache.NMS.Policies;
using NUnit.Framework;

namespace Apache.NMS.Test
{
    [TestFixture]
    public class RedeliveryPolicyTest
    {
        [Test]
        public void Executes_redelivery_policy_with_backoff_enabled_correctly()
        {
            RedeliveryPolicy policy = new RedeliveryPolicy();

            policy.BackOffMultiplier = 2;
            policy.InitialRedeliveryDelay = 5;
            policy.UseExponentialBackOff = true;

            // simulate a retry of 10 times
            Assert.IsTrue(policy.RedeliveryDelay(0) == 0, "redelivery delay not 5 is " + policy.RedeliveryDelay(0));
            Assert.IsTrue(policy.RedeliveryDelay(1) == 5, "redelivery delay not 10 is " + policy.RedeliveryDelay(1));
            Assert.IsTrue(policy.RedeliveryDelay(2) == 10, "redelivery delay not 20 is " + policy.RedeliveryDelay(2));
            Assert.IsTrue(policy.RedeliveryDelay(3) == 20, "redelivery delay not 40 is " + policy.RedeliveryDelay(3));
            Assert.IsTrue(policy.RedeliveryDelay(4) == 40, "redelivery delay not 80 is " + policy.RedeliveryDelay(4));
            Assert.IsTrue(policy.RedeliveryDelay(5) == 80, "redelivery delay not 160 is " + policy.RedeliveryDelay(5));
            Assert.IsTrue(policy.RedeliveryDelay(6) == 160, "redelivery delay not 320 is " + policy.RedeliveryDelay(6));
            Assert.IsTrue(policy.RedeliveryDelay(7) == 320, "redelivery delay not 640 is " + policy.RedeliveryDelay(7));
            Assert.IsTrue(policy.RedeliveryDelay(8) == 640,
                "redelivery delay not 1280 is " + policy.RedeliveryDelay(8));
            Assert.IsTrue(policy.RedeliveryDelay(9) == 1280,
                "redelivery delay not 2560 is " + policy.RedeliveryDelay(9));
        }

        [Test]
        public void Executes_redelivery_policy_with_backoff_of_3_enabled_correctly()
        {
            RedeliveryPolicy policy = new RedeliveryPolicy();

            policy.BackOffMultiplier = 3;
            policy.InitialRedeliveryDelay = 3;
            policy.UseExponentialBackOff = true;

            // simulate a retry of 10 times
            Assert.IsTrue(policy.RedeliveryDelay(0) == 0, "redelivery delay not 5 is " + policy.RedeliveryDelay(0));
            Assert.IsTrue(policy.RedeliveryDelay(1) == 3, "redelivery delay not 10 is " + policy.RedeliveryDelay(1));
            Assert.IsTrue(policy.RedeliveryDelay(2) == 9, "redelivery delay not 20 is " + policy.RedeliveryDelay(2));
            Assert.IsTrue(policy.RedeliveryDelay(3) == 27, "redelivery delay not 40 is " + policy.RedeliveryDelay(3));
            Assert.IsTrue(policy.RedeliveryDelay(4) == 81, "redelivery delay not 80 is " + policy.RedeliveryDelay(4));
            Assert.IsTrue(policy.RedeliveryDelay(5) == 243, "redelivery delay not 160 is " + policy.RedeliveryDelay(5));
            Assert.IsTrue(policy.RedeliveryDelay(6) == 729, "redelivery delay not 320 is " + policy.RedeliveryDelay(6));
            Assert.IsTrue(policy.RedeliveryDelay(7) == 2187,
                "redelivery delay not 640 is " + policy.RedeliveryDelay(7));
            Assert.IsTrue(policy.RedeliveryDelay(8) == 6561,
                "redelivery delay not 1280 is " + policy.RedeliveryDelay(8));
            Assert.IsTrue(policy.RedeliveryDelay(9) == 19683,
                "redelivery delay not 2560 is " + policy.RedeliveryDelay(9));
        }

        [Test]
        public void Executes_redelivery_policy_without_backoff_enabled_correctly()
        {
            RedeliveryPolicy policy = new RedeliveryPolicy();

            policy.InitialRedeliveryDelay = 5;

            // simulate a retry of 10 times
            Assert.IsTrue(policy.RedeliveryDelay(0) == 0, "redelivery delay not 0 is " + policy.RedeliveryDelay(0));
            Assert.IsTrue(policy.RedeliveryDelay(1) == 5, "redelivery delay not 5 is " + policy.RedeliveryDelay(1));
            Assert.IsTrue(policy.RedeliveryDelay(2) == 5, "redelivery delay not 5 is " + policy.RedeliveryDelay(2));
            Assert.IsTrue(policy.RedeliveryDelay(3) == 5, "redelivery delay not 5 is " + policy.RedeliveryDelay(3));
            Assert.IsTrue(policy.RedeliveryDelay(4) == 5, "redelivery delay not 5 is " + policy.RedeliveryDelay(4));
            Assert.IsTrue(policy.RedeliveryDelay(5) == 5, "redelivery delay not 5 is " + policy.RedeliveryDelay(5));
            Assert.IsTrue(policy.RedeliveryDelay(6) == 5, "redelivery delay not 5 is " + policy.RedeliveryDelay(6));
            Assert.IsTrue(policy.RedeliveryDelay(7) == 5, "redelivery delay not 5 is " + policy.RedeliveryDelay(7));
            Assert.IsTrue(policy.RedeliveryDelay(8) == 5, "redelivery delay not 5 is " + policy.RedeliveryDelay(8));
            Assert.IsTrue(policy.RedeliveryDelay(9) == 5, "redelivery delay not 5 is " + policy.RedeliveryDelay(9));
        }

        [Test]
        public void Should_get_collision_percent_correctly()
        {
            RedeliveryPolicy policy = new RedeliveryPolicy();

            policy.CollisionAvoidancePercent = 45;

            Assert.IsTrue(policy.CollisionAvoidancePercent == 45);
        }

        [Test]
        public void Executes_redelivery_policy_with_collision_enabled_correctly()
        {
            RedeliveryPolicy policy = new RedeliveryPolicy();

            policy.BackOffMultiplier = 2;
            policy.InitialRedeliveryDelay = 5;
            policy.UseExponentialBackOff = true;
            policy.UseCollisionAvoidance = true;
            policy.CollisionAvoidancePercent = 10;

            // simulate a retry of 10 times
            int delay = policy.RedeliveryDelay(0);
            Assert.IsTrue(delay == 0, "not zero is " + policy.RedeliveryDelay(0));
            delay = policy.RedeliveryDelay(1);
            Assert.IsTrue(delay >= 4.5 && delay <= 5.5,
                "not delay >= 4.5 && delay <= 5.5 is " + policy.RedeliveryDelay(1));
            delay = policy.RedeliveryDelay(2);
            Assert.IsTrue(delay >= 9 && delay <= 11, "not delay >= 9 && delay <= 11 is " + policy.RedeliveryDelay(2));
            delay = policy.RedeliveryDelay(3);
            Assert.IsTrue(delay >= 18 && delay <= 22, "not delay >= 18 && delay <= 22 is " + policy.RedeliveryDelay(3));
            delay = policy.RedeliveryDelay(4);
            Assert.IsTrue(delay >= 36 && delay <= 44, "not delay >= 36 && delay <= 44 is " + policy.RedeliveryDelay(4));
            delay = policy.RedeliveryDelay(5);
            Assert.IsTrue(delay >= 72 && delay <= 88, "not delay >= 72 && delay <= 88 is " + policy.RedeliveryDelay(5));
            delay = policy.RedeliveryDelay(6);
            Assert.IsTrue(delay >= 144 && delay <= 176,
                "not delay >= 144 && delay <= 176 is " + policy.RedeliveryDelay(6));
            delay = policy.RedeliveryDelay(7);
            Assert.IsTrue(delay >= 288 && delay <= 352,
                "not delay >= 288 && delay <= 352 is " + policy.RedeliveryDelay(7));
            delay = policy.RedeliveryDelay(8);
            Assert.IsTrue(delay >= 576 && delay <= 704,
                "not delay >= 576 && delay <= 704 is " + policy.RedeliveryDelay(8));
            delay = policy.RedeliveryDelay(9);
            Assert.IsTrue(delay >= 1152 && delay <= 1408,
                "not delay >= 1152 && delay <= 1408 is " + policy.RedeliveryDelay(9));
            delay = policy.RedeliveryDelay(10);
            Assert.IsTrue(delay >= 2304 && delay <= 2816,
                "not delay >= 2304 && delay <= 2816 is " + policy.RedeliveryDelay(10));
        }
    }
}