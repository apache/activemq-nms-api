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

using System.Collections;

namespace Apache.TibcoEMS
{
	public class MessageProperties : Apache.NMS.IPrimitiveMap
	{
		public TIBCO.EMS.Message tibcoMessage;

		public MessageProperties(TIBCO.EMS.Message message)
		{
			this.tibcoMessage = message;
		}

		private class PropertyNameEnumerable : IEnumerable
		{
			private readonly TIBCO.EMS.Message tibcoMessage;
			public PropertyNameEnumerable(TIBCO.EMS.Message message)
			{
				this.tibcoMessage = message;
			}

			public IEnumerator GetEnumerator()
			{
				return tibcoMessage.PropertyNames;
			}
		}

		#region IPrimitiveMap Members

		public void Clear()
		{
			this.tibcoMessage.ClearProperties();
		}

		public bool Contains(object key)
		{
			return this.tibcoMessage.PropertyExists(key.ToString());
		}

		public void Remove(object key)
		{
			// Best guess at equivalent implementation.
			this.tibcoMessage.SetObjectProperty(key.ToString(), null);
		}

		public int Count
		{
			get
			{
				int count = 0;
				PropertyNameEnumerable propertyNames = new PropertyNameEnumerable(this.tibcoMessage);

				foreach(object propertyName in propertyNames)
				{
					count++;
				}

				return count;
			}
		}

		public ICollection Keys
		{
			get
			{
				ArrayList keys = new ArrayList();
				PropertyNameEnumerable propertyNames = new PropertyNameEnumerable(this.tibcoMessage);

				foreach(string propertyName in propertyNames)
				{
					keys.Add(propertyName);
				}

				return keys;
			}
		}

		public ICollection Values
		{
			get
			{
				ArrayList values = new ArrayList();
				PropertyNameEnumerable propertyNames = new PropertyNameEnumerable(this.tibcoMessage);

				foreach(string propertyName in propertyNames)
				{
					values.Add(this.tibcoMessage.GetObjectProperty(propertyName));
				}

				return values;
			}
		}

		public object this[string key]
		{
			get { return this.tibcoMessage.GetObjectProperty(key); }
			set { this.tibcoMessage.SetObjectProperty(key, value); }
		}

		public string GetString(string key)
		{
			return this.tibcoMessage.GetStringProperty(key);
		}

		public void SetString(string key, string value)
		{
			this.tibcoMessage.SetStringProperty(key, value);
		}

		public bool GetBool(string key)
		{
			return this.tibcoMessage.GetBooleanProperty(key);
		}

		public void SetBool(string key, bool value)
		{
			this.tibcoMessage.SetBooleanProperty(key, value);
		}

		public byte GetByte(string key)
		{
			return this.tibcoMessage.GetByteProperty(key);
		}

		public void SetByte(string key, byte value)
		{
			this.tibcoMessage.SetByteProperty(key, value);
		}

		public char GetChar(string key)
		{
			return (char) this.tibcoMessage.GetShortProperty(key);
		}

		public void SetChar(string key, char value)
		{
			this.tibcoMessage.SetShortProperty(key, (short) value);
		}

		public short GetShort(string key)
		{
			return this.tibcoMessage.GetShortProperty(key);
		}

		public void SetShort(string key, short value)
		{
			this.tibcoMessage.SetShortProperty(key, value);
		}

		public int GetInt(string key)
		{
			return this.tibcoMessage.GetIntProperty(key);
		}

		public void SetInt(string key, int value)
		{
			this.tibcoMessage.SetIntProperty(key, value);
		}

		public long GetLong(string key)
		{
			return this.tibcoMessage.GetLongProperty(key);
		}

		public void SetLong(string key, long value)
		{
			this.tibcoMessage.SetLongProperty(key, value);
		}

		public float GetFloat(string key)
		{
			return this.tibcoMessage.GetFloatProperty(key);
		}

		public void SetFloat(string key, float value)
		{
			this.tibcoMessage.SetFloatProperty(key, value);
		}

		public double GetDouble(string key)
		{
			return this.tibcoMessage.GetDoubleProperty(key);
		}

		public void SetDouble(string key, double value)
		{
			this.tibcoMessage.SetDoubleProperty(key, value);
		}

		public IList GetList(string key)
		{
			return (IList) this.tibcoMessage.GetObjectProperty(key);
		}

		public void SetList(string key, IList list)
		{
			this.tibcoMessage.SetObjectProperty(key, list);
		}

		public IDictionary GetDictionary(string key)
		{
			return (IDictionary) this.tibcoMessage.GetObjectProperty(key);
		}

		public void SetDictionary(string key, IDictionary dictionary)
		{
			this.tibcoMessage.SetObjectProperty(key, dictionary);
		}

		#endregion
	}
}
