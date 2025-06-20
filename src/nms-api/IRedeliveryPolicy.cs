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
    public interface IRedeliveryPolicy : ICloneable
    {
        /// <summary>
        /// Gets or sets the collision avoidance percent.  This causes the redelivery delay
        /// to be adjusted in order to avoid possible collision when messages are redelivered
        /// to concurrent consumers.
        /// </summary>
        /// <value>The collision avoidance factor.</value>
        int CollisionAvoidancePercent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to [use collision avoidance].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [use collision avoidance]; otherwise, <c>false</c>.
        /// </value>
        bool UseCollisionAvoidance { get; set; }

        /// <summary>
        /// The time in milliseconds to initially delay a redelivery
        /// </summary>
        /// <value>The initial redelivery delay.</value>
        int InitialRedeliveryDelay { get; set; }

        /// <summary>
        /// Gets or sets the maximum redeliveries.  A value less than zero indicates
        /// that there is no maximum and the NMS provider should retry forever.
        /// </summary>
        /// <value>The maximum redeliveries.</value>
        int MaximumRedeliveries { get; set; }

        /// <summary>
        /// The time in milliseconds to delay a redelivery
        /// </summary>
        /// <param name="redeliveredCounter">The redelivered counter.</param>
        /// <returns></returns>
        int RedeliveryDelay(int redeliveredCounter);

        /// <summary>
        /// Gets or sets a value indicating whether [use exponential back off].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [use exponential back off]; otherwise, <c>false</c>.
        /// </value>
        bool UseExponentialBackOff { get; set; }

        /// <summary>
        /// Gets or sets the back off multiplier.
        /// </summary>
        /// <value>The back off multiplier.</value>
        int BackOffMultiplier { get; set; }
        
        /// <summary>
        /// Returns the provider-specific outcome to use when a message is rejected by the client
        /// after reaching the maximum redelivery threshold for the specified destination.
        ///
        /// The returned integer should map to an outcome defined by the specific provider implementation:
        ///
        /// For AMQP:
        ///   0 - ACCEPTED  
        ///   1 - REJECTED  
        ///   2 - RELEASED  
        ///   3 - MODIFIED_FAILED  
        ///   4 - MODIFIED_UNDELIVERABLE_HERE
        ///
        /// For OpenWire:
        ///   0 - DeliveredAck  
        ///   1 - PoisonAck  
        ///   2 - ConsumedAck  
        ///   3 - RedeliveredAck  
        ///   4 - IndividualAck  
        ///   5 - UnmatchedAck  
        ///   6 - ExpiredAck
        ///
        /// It is the responsibility of the provider to interpret the returned value appropriately.
        /// </summary>
        /// <param name="destination">The destination the message was received from.</param>
        /// <returns>An integer representing the provider-specific outcome code.</returns>
        int GetOutcome(IDestination destination);
    }
}