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
using System.Reflection;

namespace Apache.NMS.Util
{
	// Set NMS properties via introspection
	public class MessagePropertyHelper : IPrimitiveMap
	{
		private static BindingFlags publicBinding = BindingFlags.Public | BindingFlags.Instance;
		private IMessage message;
		private IPrimitiveMap properties;
		private Type messageType;

		public MessagePropertyHelper(IMessage _message, IPrimitiveMap _properties)
		{
			this.message = _message;
			this.properties = _properties;
			this.messageType = _message.GetType();
		}

		protected object GetObjectProperty(string name)
		{
			PropertyInfo propertyInfo = this.messageType.GetProperty(name, publicBinding);

			if(null != propertyInfo && propertyInfo.CanRead)
			{
				return propertyInfo.GetValue(this.message, null);
			}
			else
			{
				FieldInfo fieldInfo = this.messageType.GetField(name, publicBinding);

				if(null != fieldInfo)
				{
					return fieldInfo.GetValue(this.message);
				}
			}

			return this.properties[name];
		}

		protected void SetObjectProperty(string name, object value)
		{
			PropertyInfo propertyInfo = this.messageType.GetProperty(name, publicBinding);

			if(null != propertyInfo && propertyInfo.CanWrite)
			{
				propertyInfo.SetValue(this.message, value, null);
			}
			else
			{
				FieldInfo fieldInfo = this.messageType.GetField(name, publicBinding);

				if(null != fieldInfo && !fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
				{
					fieldInfo.SetValue(this.message, value);
				}
				else
				{
					this.properties[name] = value;
				}
			}
		}

		#region IPrimitiveMap Members

		public void Clear()
		{
			this.properties.Clear();
		}

		public bool Contains(object key)
		{
			return this.properties.Contains(key);
		}

		public void Remove(object key)
		{
			this.properties.Remove(key);
		}

		public int Count
		{
			get { return this.properties.Count; }
		}

		public System.Collections.ICollection Keys
		{
			get { return this.properties.Keys; }
		}

		public System.Collections.ICollection Values
		{
			get { return this.properties.Values; }
		}

		public object this[string key]
		{
			get { return GetObjectProperty(key); }
			set { SetObjectProperty(key, value); }
		}

		public string GetString(string key)
		{
			return (string) GetObjectProperty(key);
		}

		public void SetString(string key, string value)
		{
			SetObjectProperty(key, value);
		}

		public bool GetBool(string key)
		{
			return (bool) GetObjectProperty(key);
		}

		public void SetBool(string key, bool value)
		{
			SetObjectProperty(key, value);
		}

		public byte GetByte(string key)
		{
			return (byte) GetObjectProperty(key);
		}

		public void SetByte(string key, byte value)
		{
			SetObjectProperty(key, value);
		}

		public char GetChar(string key)
		{
			return (char) GetObjectProperty(key);
		}

		public void SetChar(string key, char value)
		{
			SetObjectProperty(key, value);
		}

		public short GetShort(string key)
		{
			return (short) GetObjectProperty(key);
		}

		public void SetShort(string key, short value)
		{
			SetObjectProperty(key, value);
		}

		public int GetInt(string key)
		{
			return (int) GetObjectProperty(key);
		}

		public void SetInt(string key, int value)
		{
			SetObjectProperty(key, value);
		}

		public long GetLong(string key)
		{
			return (long) GetObjectProperty(key);
		}

		public void SetLong(string key, long value)
		{
			SetObjectProperty(key, value);
		}

		public float GetFloat(string key)
		{
			return (float) GetObjectProperty(key);
		}

		public void SetFloat(string key, float value)
		{
			SetObjectProperty(key, value);
		}

		public double GetDouble(string key)
		{
			return (double) GetObjectProperty(key);
		}

		public void SetDouble(string key, double value)
		{
			SetObjectProperty(key, value);
		}

		public System.Collections.IList GetList(string key)
		{
			return (System.Collections.IList) GetObjectProperty(key);
		}

		public void SetList(string key, System.Collections.IList list)
		{
			SetObjectProperty(key, list);
		}

		public System.Collections.IDictionary GetDictionary(string key)
		{
			return (System.Collections.IDictionary) GetObjectProperty(key);
		}

		public void SetDictionary(string key, System.Collections.IDictionary dictionary)
		{
			SetObjectProperty(key, dictionary);
		}

		#endregion
	}
}
