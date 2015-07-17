using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace SacnOverIp
{
	internal class CommonE1_31
	{
		internal class RootLayer
		{
			public byte[] RootLayerPacket = new byte[38];

			public RootLayer()
			{
				byte[] preambleSizeOctets = { 0x0, 0x10 };

				byte[] postambleSizeOctets = { 0x00, 0x00 };

				byte[] acnPacketIdentifierOctets = { 0x41, 0x53, 0x43, 0x2d, 0x45, 0x31, 0x2e, 0x31, 0x37, 0x00, 0x00, 0x00 };

				//The RLP PDU length is computed starting with octet 16 and counting all octets in the packet through the
				//last Property Value provided in the DMP layer (Octet 637 for a full payload). This is the length of the RLP PDU.
				byte[] flagsAndLengthOctets = GetFlagsAndLength((ushort)638 - 16); // should be 622, x26F

				byte[] vectorOctets = { 0x00, 0x00, 0x00, 0x04 };

				byte[] CidOctets = new byte[] { 0x33, 0x4c, 0x33, 0xb9, 0x59, 0xbf, 0x47, 0x6a, 0xa3, 0x22, 0x49, 0xc6, 0x14, 0xb8, 0x2d, 0x5A };

				RootLayerPacket = ConcatBytes(new byte[][] { preambleSizeOctets, postambleSizeOctets, acnPacketIdentifierOctets, flagsAndLengthOctets, vectorOctets, CidOctets });
			}
		}

		internal class FramingLayer
		{
			public byte[] FramingLayerPacket = new byte[77];

			public FramingLayer(string sourceName, ushort universe)
			{
				//The E1.31 framing layer PDU length is computed starting with octet 38 and continuing through the last  property value
				// provided in the DMP PDU (octet 637 for a full payload). This is the length of the E1.31 framing layer PDU.
				byte[] flagsAndLengthOctets = GetFlagsAndLength((ushort)638 - 38); // 600 x

				byte[] vectorOctets = { 0x00, 0x00, 0x00, 0x02 };

				byte[] sourceNameOctets = new byte[64];
				Encoding.UTF8.GetBytes(sourceName, 0, sourceName.Length, sourceNameOctets, 0);

				//byte[] name = GetBytes(sourceName);
				//byte[] SourceName = new byte[64];
				//Buffer.BlockCopy(name, 0, SourceName, 0, name.Length);

				byte[] priorityOctets = { 100 };

				byte[] reservedOctets = { 0x00, 0x00 };

				byte[] sequenceNumberOctets = { 0x00 };

				byte[] optionsOctets = { 0x00 };

				byte[] universeOctets = ConvertUShortToByteArray(universe);

				FramingLayerPacket = ConcatBytes(new byte[][]
					{
						flagsAndLengthOctets,
						vectorOctets,
						sourceNameOctets,
						priorityOctets,
						reservedOctets,
						sequenceNumberOctets,
						optionsOctets,
						universeOctets
					});
			}
		}

		internal class DmpLayer
		{
			public byte[] DmpLayerPacket = new byte[523];

			public DmpLayer()
			{
				//The DMP layer PDU length is computed starting with octet 115 and continuing through the last property
				//value provided in the DMP PDU (octet 637 for a full payload). This is the length of the DMP PDU.
				byte[] flagsAndLengthOctets = GetFlagsAndLength((ushort)638 - 115); // should be 622, x26F

				byte[] vectorOctets = { 0x02 };

				byte[] addressTypeAndDataTypeOctets = { 0xa1 };

				byte[] firstPropertyAddressOctets = { 0x00, 0x00 };

				byte[] addressIncrementOctets = { 0x00, 0x01 };

				byte[] propertyValueCountOctets = ConvertUShortToByteArray(513);

				byte[] startCodeOctets = { 0x00 };

				byte[] propertyValuesOctets = new byte[512];

				DmpLayerPacket = ConcatBytes(new byte[][]
					{
						flagsAndLengthOctets,
						vectorOctets,
						addressTypeAndDataTypeOctets,
						firstPropertyAddressOctets,
						addressIncrementOctets,
						propertyValueCountOctets,
						startCodeOctets,
						propertyValuesOctets,
					});
			}
		}

		private static byte[] GetFlagsAndLength(ushort pduLength)
		{
			byte[] flagsAndLength = new byte[2];
			//FlagsAndLenth[1] = (byte)(PduLength & 0xFF);
			flagsAndLength[1] = (byte)(pduLength);
			byte mask = (byte)Convert.ToInt32("01110000", 2);
			flagsAndLength[0] = (byte)((pduLength >> 8) | (mask));

			return flagsAndLength;
		}

		private static byte[] ConvertUShortToByteArray(ushort @ushort)
		{
			byte[] array = new byte[2];
			//array[1] = (byte)(@ushort & 0xFF);
			array[1] = (byte)(@ushort);
			array[0] = (byte)(@ushort >> 8);

			return array;
		}

		/// <summary>
		/// big endian value compare
		/// </summary>
		/// <param name="value">value to copare</param>
		/// <param name="array">little endian byte to compare</param>
		/// <returns></returns>
		internal static bool CompareUShortInByteArray(byte[] array, ushort @ushort)
		{
			ushort u;
			u = (ushort)(array[0] << 8);
			u = (ushort)(array[1] | u);
			if (u == @ushort)
				return true;
			else
				return false;
		}

		internal static byte[] ConcatBytes(params byte[][] arrays)
		{
			byte[] array = new byte[arrays.Sum(x => x.Length)];
			int offset = 0;
			foreach (byte[] data in arrays)
			{
				Buffer.BlockCopy(data, 0, array, offset, data.Length);
				offset += data.Length;
			}

			return array;
		}

		internal static byte[] GetBytes(string @string)
		{
			byte[] bytes = new byte[@string.Length * sizeof(char)];
			Buffer.BlockCopy(@string.ToCharArray(), 0, bytes, 0, bytes.Length);
			return bytes;
		}

		//internal static string GetString(byte[] bytes)
		//{
		//    char[] chars = new char[bytes.Length / sizeof(char)];
		//    Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
		//    return new string(chars);
		//}

		internal static bool CompareSections(byte[] a, ushort aStart, byte[] b, ushort bStart, ushort lenth)
		{
			return true;
		}

	}
}