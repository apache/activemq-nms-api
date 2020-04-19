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

using Apache.NMS;
using Apache.NMS.Util;
using System;
using System.Collections;
using System.IO;

namespace Apache.NMS.Commands
{
    public class BytesMessage : Message, IBytesMessage
    {
        private EndianBinaryReader dataIn = null;
        private EndianBinaryWriter dataOut = null;
        private MemoryStream outputBuffer = null;
        private int length = 0;

        public override Object Clone()
        {
            StoreContent();
            return base.Clone();
        }

        public override void ClearBody()
        {
            base.ClearBody();
            this.outputBuffer = null;
            this.dataIn = null;
            this.dataOut = null;
            this.length = 0;
        }

        public long BodyLength
        {
            get
            {
                InitializeReading();
                return this.length;
            }
        }

        public byte ReadByte()
        {
            InitializeReading();
            try
            {
                return dataIn.ReadByte();
            }
            catch (EndOfStreamException e)
            {
                throw NMSExceptionSupport.CreateMessageEOFException(e);
            }
            catch (IOException e)
            {
                throw NMSExceptionSupport.CreateMessageFormatException(e);
            }
        }

        public void WriteByte(byte value)
        {
            InitializeWriting();
            try
            {
                dataOut.Write(value);
            }
            catch (Exception e)
            {
                throw NMSExceptionSupport.Create(e);
            }
        }

        public bool ReadBoolean()
        {
            InitializeReading();
            try
            {
                return dataIn.ReadBoolean();
            }
            catch (EndOfStreamException e)
            {
                throw NMSExceptionSupport.CreateMessageEOFException(e);
            }
            catch (IOException e)
            {
                throw NMSExceptionSupport.CreateMessageFormatException(e);
            }
        }

        public void WriteBoolean(bool value)
        {
            InitializeWriting();
            try
            {
                dataOut.Write(value);
            }
            catch (Exception e)
            {
                throw NMSExceptionSupport.Create(e);
            }
        }

        public char ReadChar()
        {
            InitializeReading();
            try
            {
                return dataIn.ReadChar();
            }
            catch (EndOfStreamException e)
            {
                throw NMSExceptionSupport.CreateMessageEOFException(e);
            }
            catch (IOException e)
            {
                throw NMSExceptionSupport.CreateMessageFormatException(e);
            }
        }

        public void WriteChar(char value)
        {
            InitializeWriting();
            try
            {
                dataOut.Write(value);
            }
            catch (Exception e)
            {
                throw NMSExceptionSupport.Create(e);
            }
        }

        public short ReadInt16()
        {
            InitializeReading();
            try
            {
                return dataIn.ReadInt16();
            }
            catch (EndOfStreamException e)
            {
                throw NMSExceptionSupport.CreateMessageEOFException(e);
            }
            catch (IOException e)
            {
                throw NMSExceptionSupport.CreateMessageFormatException(e);
            }
        }

        public void WriteInt16(short value)
        {
            InitializeWriting();
            try
            {
                dataOut.Write(value);
            }
            catch (Exception e)
            {
                throw NMSExceptionSupport.Create(e);
            }
        }

        public int ReadInt32()
        {
            InitializeReading();
            try
            {
                return dataIn.ReadInt32();
            }
            catch (EndOfStreamException e)
            {
                throw NMSExceptionSupport.CreateMessageEOFException(e);
            }
            catch (IOException e)
            {
                throw NMSExceptionSupport.CreateMessageFormatException(e);
            }
        }

        public void WriteInt32(int value)
        {
            InitializeWriting();
            try
            {
                dataOut.Write(value);
            }
            catch (Exception e)
            {
                throw NMSExceptionSupport.Create(e);
            }
        }

        public long ReadInt64()
        {
            InitializeReading();
            try
            {
                return dataIn.ReadInt64();
            }
            catch (EndOfStreamException e)
            {
                throw NMSExceptionSupport.CreateMessageEOFException(e);
            }
            catch (IOException e)
            {
                throw NMSExceptionSupport.CreateMessageFormatException(e);
            }
        }

        public void WriteInt64(long value)
        {
            InitializeWriting();
            try
            {
                dataOut.Write(value);
            }
            catch (Exception e)
            {
                throw NMSExceptionSupport.Create(e);
            }
        }

        public float ReadSingle()
        {
            InitializeReading();
            try
            {
                return dataIn.ReadSingle();
            }
            catch (EndOfStreamException e)
            {
                throw NMSExceptionSupport.CreateMessageEOFException(e);
            }
            catch (IOException e)
            {
                throw NMSExceptionSupport.CreateMessageFormatException(e);
            }
        }

        public void WriteSingle(float value)
        {
            InitializeWriting();
            try
            {
                dataOut.Write(value);
            }
            catch (Exception e)
            {
                throw NMSExceptionSupport.Create(e);
            }
        }

        public double ReadDouble()
        {
            InitializeReading();
            try
            {
                return dataIn.ReadDouble();
            }
            catch (EndOfStreamException e)
            {
                throw NMSExceptionSupport.CreateMessageEOFException(e);
            }
            catch (IOException e)
            {
                throw NMSExceptionSupport.CreateMessageFormatException(e);
            }
        }

        public void WriteDouble(double value)
        {
            InitializeWriting();
            try
            {
                dataOut.Write(value);
            }
            catch (Exception e)
            {
                throw NMSExceptionSupport.Create(e);
            }
        }

        public int ReadBytes(byte[] value)
        {
            InitializeReading();
            try
            {
                return dataIn.Read(value, 0, value.Length);
            }
            catch (EndOfStreamException e)
            {
                throw NMSExceptionSupport.CreateMessageEOFException(e);
            }
            catch (IOException e)
            {
                throw NMSExceptionSupport.CreateMessageFormatException(e);
            }
        }

        public int ReadBytes(byte[] value, int length)
        {
            InitializeReading();
            try
            {
                return dataIn.Read(value, 0, length);
            }
            catch (EndOfStreamException e)
            {
                throw NMSExceptionSupport.CreateMessageEOFException(e);
            }
            catch (IOException e)
            {
                throw NMSExceptionSupport.CreateMessageFormatException(e);
            }
        }

        public void WriteBytes(byte[] value)
        {
            InitializeWriting();
            try
            {
                dataOut.Write(value, 0, value.Length);
            }
            catch (Exception e)
            {
                throw NMSExceptionSupport.Create(e);
            }
        }

        public void WriteBytes(byte[] value, int offset, int length)
        {
            InitializeWriting();
            try
            {
                dataOut.Write(value, offset, length);
            }
            catch (Exception e)
            {
                throw NMSExceptionSupport.Create(e);
            }
        }

        public string ReadString()
        {
            InitializeReading();
            try
            {
                // JMS, CMS and NMS all encode the String using a 16 bit size header.
                return dataIn.ReadString16();
            }
            catch (EndOfStreamException e)
            {
                throw NMSExceptionSupport.CreateMessageEOFException(e);
            }
            catch (IOException e)
            {
                throw NMSExceptionSupport.CreateMessageFormatException(e);
            }
        }

        public void WriteString(string value)
        {
            InitializeWriting();
            try
            {
                // JMS, CMS and NMS all encode the String using a 16 bit size header.
                dataOut.WriteString16(value);
            }
            catch (Exception e)
            {
                throw NMSExceptionSupport.Create(e);
            }
        }

        public void WriteObject(System.Object value)
        {
            InitializeWriting();
            if (value is System.Byte)
            {
                this.dataOut.Write((byte) value);
            }
            else if (value is Char)
            {
                this.dataOut.Write((char) value);
            }
            else if (value is Boolean)
            {
                this.dataOut.Write((bool) value);
            }
            else if (value is Int16)
            {
                this.dataOut.Write((short) value);
            }
            else if (value is Int32)
            {
                this.dataOut.Write((int) value);
            }
            else if (value is Int64)
            {
                this.dataOut.Write((long) value);
            }
            else if (value is Single)
            {
                this.dataOut.Write((float) value);
            }
            else if (value is Double)
            {
                this.dataOut.Write((double) value);
            }
            else if (value is byte[])
            {
                this.dataOut.Write((byte[]) value);
            }
            else if (value is String)
            {
                this.dataOut.WriteString16((string) value);
            }
            else
            {
                throw new MessageFormatException("Cannot write non-primitive type:" + value.GetType());
            }
        }

        public new byte[] Content
        {
            get
            {
                byte[] buffer = null;
                InitializeReading();
                if (this.length != 0)
                {
                    buffer = new byte[this.length];
                    this.dataIn.Read(buffer, 0, buffer.Length);
                }

                return buffer;
            }

            set
            {
                InitializeWriting();
                this.dataOut.Write(value, 0, value.Length);
            }
        }

        public void Reset()
        {
            StoreContent();
            this.dataIn = null;
            this.dataOut = null;
            this.outputBuffer = null;
            this.ReadOnlyBody = true;
        }

        private void InitializeReading()
        {
            FailIfWriteOnlyBody();
            if (this.dataIn == null)
            {
                byte[] data = base.Content;

                if (base.Content == null)
                {
                    data = new byte[0];
                }

                Stream target = new MemoryStream(data, false);

                this.length = data.Length;
                this.dataIn = new EndianBinaryReader(target);
            }
        }

        private void InitializeWriting()
        {
            FailIfReadOnlyBody();
            if (this.dataOut == null)
            {
                this.outputBuffer = new MemoryStream();
                this.dataOut = new EndianBinaryWriter(this.outputBuffer);
            }
        }

        private void StoreContent()
        {
            if (this.dataOut != null)
            {
                this.dataOut.Close();
                base.Content = outputBuffer.ToArray();

                this.dataOut = null;
                this.outputBuffer = null;
            }
        }
    }
}