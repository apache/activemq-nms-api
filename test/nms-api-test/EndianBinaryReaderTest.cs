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

using System.IO;
using Apache.NMS.Util;
using NUnit.Framework;

namespace Apache.NMS.Test
{
	[TestFixture]
	public class EndianBinaryReaderTest
	{
		public void readString16Helper(byte[] input, char[] expect)
		{
			MemoryStream stream = new MemoryStream(input);
			EndianBinaryReader reader = new EndianBinaryReader(stream);

			char[] result = reader.ReadString16().ToCharArray();

			for(int i = 0; i < expect.Length; ++i)
			{
				Assert.AreEqual(expect[i], result[i]);
			}
		}

		[Test]
		public void testReadString16_1byteUTF8encoding()
		{
			// Test data with 1-byte UTF8 encoding.
			char[] expect = { '\u0000', '\u000B', '\u0048', '\u0065', '\u006C', '\u006C', '\u006F', '\u0020', '\u0057', '\u006F', '\u0072', '\u006C', '\u0064' };
			byte[] input = { 0x00, 0x0E, 0xC0, 0x80, 0x0B, 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x20, 0x57, 0x6F, 0x72, 0x6C, 0x64 };

			readString16Helper(input, expect);
		}

		[Test]
		public void testReadString16_2byteUTF8encoding()
		{
			// Test data with 2-byte UT8 encoding.
			char[] expect = { '\u0000', '\u00C2', '\u00A9', '\u00C3', '\u00A6' };
			byte[] input = { 0x00, 0x0A, 0xC0, 0x80, 0xC3, 0x82, 0xC2, 0xA9, 0xC3, 0x83, 0xC2, 0xA6 };
			readString16Helper(input, expect);
		}

		[Test]
		public void testReadString16_1byteAnd2byteEmbeddedNULLs()
		{
			// Test data with 1-byte and 2-byte encoding with embedded NULL's.
			char[] expect = { '\u0000', '\u0004', '\u00C2', '\u00A9', '\u00C3', '\u0000', '\u00A6' };
			byte[] input = { 0x00, 0x0D, 0xC0, 0x80, 0x04, 0xC3, 0x82, 0xC2, 0xA9, 0xC3, 0x83, 0xC0, 0x80, 0xC2, 0xA6 };

			readString16Helper(input, expect);
		}

        [Test]
        public void testReadString16_UTF8Missing2ndByte()
        {
            Assert.Throws<IOException>(_testReadString16_UTF8Missing2ndByte);
        }

        private void _testReadString16_UTF8Missing2ndByte()
		{
			// Test with bad UTF-8 encoding, missing 2nd byte of two byte value
			byte[] input = { 0x00, 0x0D, 0xC0, 0x80, 0x04, 0xC3, 0x82, 0xC2, 0xC2, 0xC3, 0x83, 0xC0, 0x80, 0xC2, 0xA6 };

			MemoryStream stream = new MemoryStream(input);
			EndianBinaryReader reader = new EndianBinaryReader(stream);

			reader.ReadString16();
		}

		[Test]
        public void testReadString16_3byteEncodingMissingLastByte()
        {
            Assert.Throws<IOException>(_testReadString16_3byteEncodingMissingLastByte);
        }

        private void _testReadString16_3byteEncodingMissingLastByte()
		{
			// Test with three byte encode that's missing a last byte.
			byte[] input = { 0x00, 0x02, 0xE8, 0xA8 };

			MemoryStream stream = new MemoryStream(input);
			EndianBinaryReader reader = new EndianBinaryReader(stream);

			reader.ReadString16();
		}

		public void readString32Helper(byte[] input, char[] expect)
		{
			MemoryStream stream = new MemoryStream(input);
			EndianBinaryReader reader = new EndianBinaryReader(stream);

			char[] result = reader.ReadString32().ToCharArray();

			for(int i = 0; i < expect.Length; ++i)
			{
				Assert.AreEqual(expect[i], result[i]);
			}
		}

		[Test]
		public void testReadString32_1byteUTF8encoding()
		{
			// Test data with 1-byte UTF8 encoding.
			char[] expect = { '\u0000', '\u000B', '\u0048', '\u0065', '\u006C', '\u006C', '\u006F', '\u0020', '\u0057', '\u006F', '\u0072', '\u006C', '\u0064' };
			byte[] input = { 0x00, 0x00, 0x00, 0x0E, 0xC0, 0x80, 0x0B, 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x20, 0x57, 0x6F, 0x72, 0x6C, 0x64 };

			readString32Helper(input, expect);
		}

		[Test]
		public void testReadString32_2byteUTF8encoding()
		{
			// Test data with 2-byte UT8 encoding.
			char[] expect = { '\u0000', '\u00C2', '\u00A9', '\u00C3', '\u00A6' };
			byte[] input = { 0x00, 0x00, 0x00, 0x0A, 0xC0, 0x80, 0xC3, 0x82, 0xC2, 0xA9, 0xC3, 0x83, 0xC2, 0xA6 };
			readString32Helper(input, expect);
		}

		[Test]
		public void testReadString32_1byteAnd2byteEmbeddedNULLs()
		{
			// Test data with 1-byte and 2-byte encoding with embedded NULL's.
			char[] expect = { '\u0000', '\u0004', '\u00C2', '\u00A9', '\u00C3', '\u0000', '\u00A6' };
			byte[] input = { 0x00, 0x00, 0x00, 0x0D, 0xC0, 0x80, 0x04, 0xC3, 0x82, 0xC2, 0xA9, 0xC3, 0x83, 0xC0, 0x80, 0xC2, 0xA6 };

			readString32Helper(input, expect);
		}

        [Test]
        public void testReadString32_UTF8Missing2ndByte()
        {
            Assert.Throws<IOException>(_testReadString32_UTF8Missing2ndByte);
        }

        private void _testReadString32_UTF8Missing2ndByte()
		{
            
            // Test with bad UTF-8 encoding, missing 2nd byte of two byte value
            byte[] input = { 0x00, 0x00, 0x00, 0x0D, 0xC0, 0x80, 0x04, 0xC3, 0x82, 0xC2, 0xC2, 0xC3, 0x83, 0xC0, 0x80, 0xC2, 0xA6 };

			MemoryStream stream = new MemoryStream(input);
			EndianBinaryReader reader = new EndianBinaryReader(stream);

			reader.ReadString32();
		}

		[Test]
        public void testReadString32_3byteEncodingMissingLastByte()
        {
            Assert.Throws<IOException>(_testReadString32_3byteEncodingMissingLastByte);
        }

        private void _testReadString32_3byteEncodingMissingLastByte()
		{
			// Test with three byte encode that's missing a last byte.
			byte[] input = { 0x00, 0x00, 0x00, 0x02, 0xE8, 0xA8 };

			MemoryStream stream = new MemoryStream(input);
			EndianBinaryReader reader = new EndianBinaryReader(stream);

			reader.ReadString32();
		}
	}
}
