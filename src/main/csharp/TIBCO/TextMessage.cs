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

namespace Apache.TibcoEMS
{
	class TextMessage : Apache.TibcoEMS.Message, Apache.NMS.ITextMessage
	{
		public TIBCO.EMS.TextMessage tibcoTextMessage
		{
			get { return this.tibcoMessage as TIBCO.EMS.TextMessage; }
			set { this.tibcoMessage = value; }
		}

		public TextMessage(TIBCO.EMS.TextMessage message)
			: base(message)
		{
		}

		#region ITextMessage Members

		public string Text
		{
			get { return this.tibcoTextMessage.Text; }
			set { this.tibcoTextMessage.Text = value; }
		}

		#endregion
	}
}
