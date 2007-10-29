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
using System.Collections;

namespace Apache.TibcoEMS
{
	class MapMessage : Apache.TibcoEMS.Message, Apache.NMS.IMapMessage, Apache.NMS.IPrimitiveMap
	{
		public TIBCO.EMS.MapMessage tibcoMapMessage
		{
			get { return this.tibcoMessage as TIBCO.EMS.MapMessage; }
			set { this.tibcoMessage = value; }
		}

		public MapMessage(TIBCO.EMS.MapMessage message)
			: base(message)
		{
		}

		#region IMapMessage Members

		public Apache.NMS.IPrimitiveMap Body
		{
			get { return this; }
		}

		#endregion

		private class MapEnumerable : IEnumerable
		{
			private readonly TIBCO.EMS.MapMessage tibcoMapMessage;
			public MapEnumerable(TIBCO.EMS.MapMessage message)
			{
				this.tibcoMapMessage = message;
			}

			public IEnumerator GetEnumerator()
			{
				return tibcoMapMessage.MapNames;
			}
		}

		#region IPrimitiveMap Members

		public void Clear()
		{
			this.tibcoMapMessage.ClearBody();
		}

		public bool Contains(object key)
		{
			return this.tibcoMapMessage.ItemExists(key.ToString());
		}

		public void Remove(object key)
		{
			// Best guess at equivalent implementation.
			this.tibcoMapMessage.SetObject(key.ToString(), null);
		}

		public int Count
		{
			get
			{
				int count = 0;
				MapEnumerable mapItems = new MapEnumerable(this.tibcoMapMessage);

				foreach(object item in mapItems)
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
				MapEnumerable mapItems = new MapEnumerable(this.tibcoMapMessage);

				foreach(string itemName in mapItems)
				{
					keys.Add(itemName);
				}

				return keys;
			}
		}

		public ICollection Values
		{
			get
			{
				ArrayList keys = new ArrayList();
				MapEnumerable mapItems = new MapEnumerable(this.tibcoMapMessage);

				foreach(string itemName in mapItems)
				{
					keys.Add(this.tibcoMapMessage.GetObject(itemName));
				}

				return keys;
			}
		}

		public object this[string key]
		{
			get
			{
				return this.tibcoMapMessage.GetObject(key);
			}
			set
			{
				this.tibcoMapMessage.SetObject(key, value);
			}
		}

		public string GetString(string key)
		{
			return this.tibcoMapMessage.GetString(key);
		}

		public void SetString(string key, string value)
		{
			this.tibcoMapMessage.SetString(key, value);
		}

		public bool GetBool(string key)
		{
			return this.tibcoMapMessage.GetBoolean(key);
		}

		public void SetBool(string key, bool value)
		{
			this.tibcoMapMessage.SetBoolean(key, value);
		}

		public byte GetByte(string key)
		{
			return this.tibcoMapMessage.GetByte(key);
		}

		public void SetByte(string key, byte value)
		{
			this.tibcoMapMessage.SetByte(key, value);
		}

		public char GetChar(string key)
		{
			return this.tibcoMapMessage.GetChar(key);
		}

		public void SetChar(string key, char value)
		{
			this.tibcoMapMessage.SetChar(key, value);
		}

		public short GetShort(string key)
		{
			return this.tibcoMapMessage.GetShort(key);
		}

		public void SetShort(string key, short value)
		{
			this.tibcoMapMessage.SetShort(key, value);
		}

		public int GetInt(string key)
		{
			return this.tibcoMapMessage.GetInt(key);
		}

		public void SetInt(string key, int value)
		{
			this.tibcoMapMessage.SetInt(key, value);
		}

		public long GetLong(string key)
		{
			return this.tibcoMapMessage.GetLong(key);
		}

		public void SetLong(string key, long value)
		{
			this.tibcoMapMessage.SetLong(key, value);
		}

		public float GetFloat(string key)
		{
			return this.tibcoMapMessage.GetFloat(key);
		}

		public void SetFloat(string key, float value)
		{
			this.tibcoMapMessage.SetFloat(key, value);
		}

		public double GetDouble(string key)
		{
			return this.tibcoMapMessage.GetDouble(key);
		}

		public void SetDouble(string key, double value)
		{
			this.tibcoMapMessage.SetDouble(key, value);
		}

		public IList GetList(string key)
		{
			return (IList) this.tibcoMapMessage.GetObject(key);
		}

		public void SetList(string key, IList list)
		{
			this.tibcoMapMessage.SetObject(key, list);
		}

		public IDictionary GetDictionary(string key)
		{
			return (IDictionary) this.tibcoMapMessage.GetObject(key);
		}

		public void SetDictionary(string key, IDictionary dictionary)
		{
			this.tibcoMapMessage.SetObject(key, dictionary);
		}

		#endregion
	}
}
