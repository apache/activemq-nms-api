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
using System.Threading.Tasks;

namespace Apache.NMS.Commands
{
    public abstract class TempDestination : Destination
    {
        /// <summary>
        /// Method CreateDestination
        /// </summary>
        /// <returns>An Destination</returns>
        /// <param name="name">A  String</param>
        public override Destination CreateDestination(String name)
        {
            return null;
        }

        abstract override public DestinationType DestinationType { get; }

        public TempDestination()
            : base()
        {
        }

        public TempDestination(String name)
            : base(name)
        {
        }

        public override Object Clone()
        {
            // Since we are a derived class use the base's Clone()
            // to perform the shallow copy. Since it is shallow it
            // will include our derived class. Since we are derived,
            // this method is an override.
            TempDestination o = (TempDestination) base.Clone();

            // Now do the deep work required
            // If any new variables are added then this routine will
            // likely need updating

            return o;
        }

        public Task DeleteAsync()
        {
            return Task.Factory.StartNew(Delete);
        }

        public void Delete()
        {
            throw new NotSupportedException("Stomp Cannot Delete Temporary Destinations");
        }
    }
}