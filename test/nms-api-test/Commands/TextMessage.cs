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
using System.IO;
using Apache.NMS;
using Apache.NMS.Util;

namespace Apache.NMS.Commands
{
    public class TextMessage : Message, ITextMessage
    {
        private String text = null;

        public TextMessage()
        {
        }

        public TextMessage(String text)
        {
            this.Text = text;
        }

        public override string ToString()
        {
            string text = this.Text;

            if (text != null && text.Length > 63)
            {
                text = text.Substring(0, 45) + "..." + text.Substring(text.Length - 12);
            }

            return base.ToString() + " Text = " + (text ?? "null");
        }

        public override void ClearBody()
        {
            base.ClearBody();
            this.text = null;
        }

        // Properties

        public string Text
        {
            get { return this.text; }
            set
            {
                FailIfReadOnlyBody();
                this.text = value;
                this.Content = null;
            }
        }
    }
}