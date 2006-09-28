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
using System;
using System.Collections;
using System.Collections.Generic;

namespace MSMQ
{
    /// <summary>
    /// A default implementation of IPrimitiveMap
    /// </summary>
    [Serializable()]
    public class PrimitiveMap : IPrimitiveMap
    {
        public const byte NULL = 0;
        public const byte BOOLEAN_TYPE = 1;
        public const byte BYTE_TYPE = 2;
        public const byte CHAR_TYPE = 3;
        public const byte SHORT_TYPE = 4;
        public const byte INTEGER_TYPE = 5;
        public const byte LONG_TYPE = 6;
        public const byte DOUBLE_TYPE = 7;
        public const byte FLOAT_TYPE = 8;
        public const byte STRING_TYPE = 9;
        public const byte BYTE_ARRAY_TYPE = 10;
        public const byte MAP_TYPE = 11;
        public const byte LIST_TYPE = 12;
        public const byte BIG_STRING_TYPE = 13;

        private Dictionary<String, object> dictionary = new Dictionary<String, object>();

        public void Clear()
        {
            dictionary.Clear();
        }

        public bool Contains(Object key)
        {
            return dictionary.ContainsKey((string) key);
        }

        public void Remove(Object key)
        {
            dictionary.Remove((string) key);
        }


        public int Count
        {
            get
            {
                return dictionary.Count;
            }
        }

        public ICollection Keys
        {
            get
            {
                return dictionary.Keys;
            }
        }

        public ICollection Values
        {
            get
            {
                return dictionary.Values;
            }
        }

        public object this[string key]
        {
            get
            {
                return GetValue(key);
            }
            set
            {
                CheckValidType(value);
                SetValue(key, value);
            }
        }

        public string GetString(string key)
        {
            Object value = GetValue(key);
            CheckValueType(value, typeof(string));
            return (string)value;
        }

        public void SetString(string key, string value)
        {
            SetValue(key, value);
        }

        public bool GetBool(String key)
        {
            Object value = GetValue(key);
            CheckValueType(value, typeof(bool));
            return (bool)value;
        }

        public void SetBool(String key, bool value)
        {
            SetValue(key, value);
        }

        public byte GetByte(String key)
        {
            Object value = GetValue(key);
            CheckValueType(value, typeof(byte));
            return (byte)value;
        }

        public void SetByte(String key, byte value)
        {
            SetValue(key, value);
        }

        public char GetChar(String key)
        {
            Object value = GetValue(key);
            CheckValueType(value, typeof(char));
            return (char)value;
        }

        public void SetChar(String key, char value)
        {
            SetValue(key, value);
        }

        public short GetShort(String key)
        {
            Object value = GetValue(key);
            CheckValueType(value, typeof(short));
            return (short)value;
        }

        public void SetShort(String key, short value)
        {
            SetValue(key, value);
        }

        public int GetInt(String key)
        {
            Object value = GetValue(key);
            CheckValueType(value, typeof(int));
            return (int)value;
        }

        public void SetInt(String key, int value)
        {
            SetValue(key, value);
        }

        public long GetLong(String key)
        {
            Object value = GetValue(key);
            CheckValueType(value, typeof(long));
            return (long)value;
        }

        public void SetLong(String key, long value)
        {
            SetValue(key, value);
        }

        public float GetFloat(String key)
        {
            Object value = GetValue(key);
            CheckValueType(value, typeof(float));
            return (float)value;
        }

        public void SetFloat(String key, float value)
        {
            SetValue(key, value);
        }

        public double GetDouble(String key)
        {
            Object value = GetValue(key);
            CheckValueType(value, typeof(double));
            return (double)value;
        }

        public void SetDouble(String key, double value)
        {
            SetValue(key, value);
        }

        public IList GetList(String key)
        {
            Object value = GetValue(key);
            if (value != null && !(value is IList))
            {
                throw new NMSException("Property: " + key + " is not an IList but is: " + value);
            }
            return (IList)value;
        }

        public void SetList(String key, IList value)
        {
            SetValue(key, value);
        }

        public IDictionary GetDictionary(String key)
        {
            Object value = GetValue(key);
            if (value != null && !(value is IDictionary))
            {
                throw new NMSException("Property: " + key + " is not an IDictionary but is: " + value);
            }
            return (IDictionary)value;
        }

        public void SetDictionary(String key, IDictionary value)
        {
            SetValue(key, value);
        }


        protected virtual void SetValue(String key, Object value)
        {
            dictionary[key] = value;
        }


        protected virtual Object GetValue(String key)
        {
            return dictionary[key];
        }

        protected virtual void CheckValueType(Object value, Type type)
        {
            if (!type.IsInstanceOfType(value))
            {
                throw new NMSException("Expected type: " + type.Name + " but was: " + value);
            }
        }

        protected virtual void CheckValidType(Object value)
        {
            if (value != null && !(value is IList) && !(value is IDictionary))
            {
                Type type = value.GetType();
                if (!type.IsPrimitive && !type.IsValueType && !type.IsAssignableFrom(typeof(string)))
                {
                    throw new NMSException("Invalid type: " + type.Name + " for value: " + value);
                }
            }
        }

        /// <summary>
        /// Method ToString
        /// </summary>
        /// <returns>A string</returns>
        public override String ToString()
        {
            String s = "{";
            bool first = true;
            foreach (KeyValuePair<String, object> entry in dictionary)
            {
                if (!first)
                {
                    s += ", ";
                }
                first = false;
                String name = (String)entry.Key;
                Object value = entry.Value;
                s += name + "=" + value;
            }
            s += "}";
            return s;
        }
    }
}
