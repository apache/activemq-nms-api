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
using Apache.NMS.Util;
using NUnit.Framework;

namespace Apache.NMS.Test
{
	[TestFixture]
	public class EndianBinaryWriterTest
	{
		void writeString16TestHelper(char[] input, byte[] expect)
		{
			MemoryStream stream = new MemoryStream();
			EndianBinaryWriter writer = new EndianBinaryWriter(stream);

			String str = new String(input);

			writer.WriteString16(str);

			byte[] result = stream.GetBuffer();

			Assert.AreEqual(result[0], 0x00);
			Assert.AreEqual(result[1], expect.Length);

			for(int i = 4; i < expect.Length; ++i)
			{
				Assert.AreEqual(result[i], expect[i - 2]);
			}
		}

		[Test]
		public void testWriteString16_1byteUTF8encoding()
		{
			// Test data with 1-byte UTF8 encoding.
			char[] input = { '\u0000', '\u000B', '\u0048', '\u0065', '\u006C', '\u006C', '\u006F', '\u0020', '\u0057', '\u006F', '\u0072', '\u006C', '\u0064' };
			byte[] expect = { 0xC0, 0x80, 0x0B, 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x20, 0x57, 0x6F, 0x72, 0x6C, 0x64 };

			writeString16TestHelper(input, expect);
		}

		[Test]
		public void testWriteString16_2byteUTF8encoding()
		{
			// Test data with 2-byte UT8 encoding.
			char[] input = { '\u0000', '\u00C2', '\u00A9', '\u00C3', '\u00A6' };
			byte[] expect = { 0xC0, 0x80, 0xC3, 0x82, 0xC2, 0xA9, 0xC3, 0x83, 0xC2, 0xA6 };

			writeString16TestHelper(input, expect);
		}

		[Test]
		public void testWriteString16_1byteAnd2byteEmbeddedNULLs()
		{
			// Test data with 1-byte and 2-byte encoding with embedded NULL's.
			char[] input = { '\u0000', '\u0004', '\u00C2', '\u00A9', '\u00C3', '\u0000', '\u00A6' };
			byte[] expect = { 0xC0, 0x80, 0x04, 0xC3, 0x82, 0xC2, 0xA9, 0xC3, 0x83, 0xC0, 0x80, 0xC2, 0xA6 };

			writeString16TestHelper(input, expect);
		}

		[Test]
		public void testWriteString16_nullstring()
		{
			// test that a null string writes no output.
			MemoryStream stream = new MemoryStream();
			EndianBinaryWriter writer = new EndianBinaryWriter(stream);
			writer.WriteString16(null);
			Assert.AreEqual(0, stream.Length);
		}

		[Test]
		public void testWriteString16_emptystring()
		{
			// test that a null string writes no output.
			MemoryStream stream = new MemoryStream();
			EndianBinaryWriter writer = new EndianBinaryWriter(stream);
			writer.WriteString16("");

			stream.Seek(0, SeekOrigin.Begin);
			EndianBinaryReader reader = new EndianBinaryReader(stream);
			Assert.AreEqual(0, reader.ReadInt16());
		}

		[Test]
        public void testWriteString16_stringTooLong()
        {
            Assert.Throws<IOException>(_testWriteString16_stringTooLong);
        }

        private void _testWriteString16_stringTooLong()
		{
			// String of length 65536 of Null Characters.
			MemoryStream stream = new MemoryStream();
			EndianBinaryWriter writer = new EndianBinaryWriter(stream);
			String testStr = new String('a', 65536);
			writer.Write(testStr);
		}

		[Test]
		public void testWriteString16_maxStringLength()
		{
			// String of length 65535 of non Null Characters since Null encodes as UTF-8.
			MemoryStream stream = new MemoryStream();
			EndianBinaryWriter writer = new EndianBinaryWriter(stream);
			String testStr = new String('a', 65535);
			writer.Write(testStr);
		}

		[Test]
        public void testWriteString16_invalidEncodingHeader()
        {
            Assert.Throws<IOException>(_testWriteString16_invalidEncodingHeader);
        }

        private void _testWriteString16_invalidEncodingHeader()
		{
			// Set one of the 65535 bytes to a value that will result in a 2 byte UTF8 encoded sequence.
			// This will cause the string of length 65535 to have a utf length of 65536.
			MemoryStream stream = new MemoryStream();
			EndianBinaryWriter writer = new EndianBinaryWriter(stream);
			String testStr = new String('a', 65535);
			char[] array = testStr.ToCharArray();
			array[0] = '\u0000';
			testStr = new String(array);
			writer.Write(testStr);
		}

		void writeString32TestHelper(char[] input, byte[] expect)
		{
			MemoryStream stream = new MemoryStream();
			EndianBinaryWriter writer = new EndianBinaryWriter(stream);

			String str = new String(input);

			writer.WriteString32(str);

			byte[] result = stream.GetBuffer();

			Assert.AreEqual(result[0], 0x00);
			Assert.AreEqual(result[1], 0x00);
			Assert.AreEqual(result[2], 0x00);
			Assert.AreEqual(result[3], expect.Length);

			for(int i = 4; i < expect.Length; ++i)
			{
				Assert.AreEqual(result[i], expect[i - 4]);
			}
		}

		[Test]
		public void testWriteString32_1byteUTF8encoding()
		{
			// Test data with 1-byte UTF8 encoding.
			char[] input = { '\u0000', '\u000B', '\u0048', '\u0065', '\u006C', '\u006C', '\u006F', '\u0020', '\u0057', '\u006F', '\u0072', '\u006C', '\u0064' };
			byte[] expect = { 0xC0, 0x80, 0x0B, 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x20, 0x57, 0x6F, 0x72, 0x6C, 0x64 };

			writeString32TestHelper(input, expect);
		}

		[Test]
		public void testWriteString32_2byteUTF8encoding()
		{
			// Test data with 2-byte UT8 encoding.
			char[] input = { '\u0000', '\u00C2', '\u00A9', '\u00C3', '\u00A6' };
			byte[] expect = { 0xC0, 0x80, 0xC3, 0x82, 0xC2, 0xA9, 0xC3, 0x83, 0xC2, 0xA6 };

			writeString32TestHelper(input, expect);
		}

		[Test]
		public void testWriteString32_1byteAnd2byteEmbeddedNULLs()
		{
			// Test data with 1-byte and 2-byte encoding with embedded NULL's.
			char[] input = { '\u0000', '\u0004', '\u00C2', '\u00A9', '\u00C3', '\u0000', '\u00A6' };
			byte[] expect = { 0xC0, 0x80, 0x04, 0xC3, 0x82, 0xC2, 0xA9, 0xC3, 0x83, 0xC0, 0x80, 0xC2, 0xA6 };

			writeString32TestHelper(input, expect);
		}

		[Test]
		public void testWriteString32_nullstring()
		{
			// test that a null strings writes a -1
			MemoryStream stream = new MemoryStream();
			EndianBinaryWriter writer = new EndianBinaryWriter(stream);
			writer.WriteString32(null);

			stream.Seek(0, SeekOrigin.Begin);
			EndianBinaryReader reader = new EndianBinaryReader(stream);
			Assert.AreEqual(-1, reader.ReadInt32());
		}
	}
}
