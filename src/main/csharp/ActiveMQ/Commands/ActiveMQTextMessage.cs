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


namespace ActiveMQ.Commands
{
	public class ActiveMQTextMessage : ActiveMQMessage, ITextMessage
    {
        public const byte ID_ActiveMQTextMessage = 28;

		public const int SIZE_OF_INT = 4; // sizeof(int) - though causes unsafe issues with net 1.1
        
        private String text;

        private static System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
        
        public ActiveMQTextMessage()
        {
        }
        
        public ActiveMQTextMessage(String text)
        {
            this.Text = text;
        }
        
        // TODO generate Equals method
        // TODO generate GetHashCode method
        // TODO generate ToString method

	    public override string ToString()
	    {
	        return base.ToString() + " Text="+Text;
	    }

	    public override byte GetDataStructureType()
        {
            return ID_ActiveMQTextMessage;
        }
        
        
        // Properties
        
        public string Text
        {
            get {
                if (text == null)
                {
                    // now lets read the content
                    byte[] data = this.Content;
                    if (data != null)
                    {
                        text = encoder.GetString(data, SIZE_OF_INT, data.Length - SIZE_OF_INT);
                    }
                }
                return text;
            }
            
            set {
                this.text = value;
                byte[] data = null;
                if (text != null)
                {
					// TODO lets make the evaluation of the Content lazy!
					
					// TODO assume that the text is ASCII
                    byte[] utf8bytes = encoder.GetBytes( this.text );
                    byte[] sizePrefix = System.BitConverter.GetBytes(utf8bytes.Length);
                    data = new byte[utf8bytes.Length + sizePrefix.Length];  //int at the front of it
															
					// add the size prefix
					for (int j = 0; j < sizePrefix.Length; j++)
                    {
						// The bytes need to be encoded in big endian
						if ( BitConverter.IsLittleEndian ) {
							data[j] = sizePrefix[sizePrefix.Length - j - 1];
						} else {
							data[j] = sizePrefix[j];
						}
                    }
					
					// Add the data.
                    for (int i = 0; i < utf8bytes.Length; i++)
                    {
                        data[i + sizePrefix.Length] = (byte)utf8bytes[i];
                    }
				}
				this.Content = data;
					
            }
        }
    }
}

