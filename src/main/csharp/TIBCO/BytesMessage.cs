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
	class BytesMessage : Apache.TibcoEMS.Message, Apache.NMS.IBytesMessage
	{
		public TIBCO.EMS.BytesMessage tibcoBytesMessage
		{
			get { return this.tibcoMessage as TIBCO.EMS.BytesMessage; }
			set { this.tibcoMessage = value; }
		}

		public BytesMessage(TIBCO.EMS.BytesMessage message)
			: base(message)
		{
		}

		#region IBytesMessage Members

		public byte[] Content
		{
			get
			{
				int contentLength = (int) this.tibcoBytesMessage.BodyLength;
				byte[] msgContent = new byte[contentLength];

				this.tibcoBytesMessage.Reset();
				this.tibcoBytesMessage.ReadBytes(msgContent, contentLength);
				return msgContent;
			}

			set
			{
				this.tibcoBytesMessage.ClearBody();
				this.tibcoBytesMessage.WriteBytes(value, 0, value.Length);
			}
		}

		#endregion
	}
}
