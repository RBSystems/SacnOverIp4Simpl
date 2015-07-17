using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using Crestron.SimplSharp.CrestronSockets;

namespace SacnOverIp
{
	/// S+ callback match return types S# -> S+:
	/// short -> signed_integer
	/// ushort ->  integer
	/// int -> signed_long_integer
	/// uint -> long_integer
	/// string = > SimplSharpString
	//public delegate void DelegateFnString(SimplSharpString myString);
	public delegate void DelegateFn();

	public class SacnSlave
	{
		//public DelegateFnString SendSacn { get; set; }
		public DelegateFn ReadSacn { get; set; }

		CommonE1_31.RootLayer Root;
		CommonE1_31.FramingLayer Frame;
		CommonE1_31.DmpLayer Dmp;
		ushort Universe;
		byte LastSequence;
		//byte[] E131Packet; // big endian
		private static byte[] RxData = new byte[0];
		public ushort[] DmxData { get; private set; }

		//public SacnSlave()
		//{
		//}

		public void InitializeFields(ushort universe)
		{
			if (universe >= 1 && universe <= 63999)
			{
				Universe = universe;
				Root = new CommonE1_31.RootLayer();
				byte[] RootLayerOctets = Root.RootLayerPacket;
				Frame = new CommonE1_31.FramingLayer("", universe);
				byte[] FramingLayerOctets = Frame.FramingLayerPacket;
				Dmp = new CommonE1_31.DmpLayer();
				byte[] DmpLayerOctets = Dmp.DmpLayerPacket;
				//E131Packet = CommonE1_31.ConcatBytes(new byte[][] { RootLayerOctets, FramingLayerOctets, DmpLayerOctets });

				DmxData = new ushort[512];

				CrestronInvoke.BeginInvoke(StartUdpThread);
			}
			else
				ErrorLog.Error(this.ToString());
		}

		void StartUdpThread(object _)
		{
			CrestronConsole.PrintLine("UDP Read Thread Started");
			IPAddress ipa;

			ipa = new IPAddress(new byte[] { 0, 0, 0, 0 });
			UDPServer UdpServer;
			UdpServer = new UDPServer(ipa, 5568, 65534);

			UdpServer.EnableUDPServer();
			//server.EthernetAdapterToBindTo = EthernetAdapterType.EthernetUnknownAdapter;
			//server.SocketSendOrReceiveTimeOutInMs = 3000;

			// receive data
			while(true)
			{
			    int bytesRead = UdpServer.ReceiveData();
				if (bytesRead > 0)
				{
					CrestronConsole.PrintLine("Read: " + CrestronEnvironment.TickCount.ToString());

					byte[] data = new byte[RxData.Length + bytesRead];
					Buffer.BlockCopy(RxData, 0, data, 0, RxData.Length);
					Buffer.BlockCopy(UdpServer.IncomingDataBuffer, 0, data, RxData.Length, bytesRead);

					// gathered data large enough to be a valid sacn packet.
					if (data.Length >= 638)
					{
						CrestronConsole.PrintLine("Gathered: " + CrestronEnvironment.TickCount.ToString());

						//ErrorLog.Notice("\n data at least a packet size \n");
						byte[] pattern = new byte[22];
						Buffer.BlockCopy(Root.RootLayerPacket, 0, pattern, 0, 22);
						int? index = IndexOfPatternMatch(data, pattern, 0);
						if (index != null)
						{
							//ErrorLog.Notice("\n found root \n");
							// trim off dead bytes
							if ((int)index > 0)
							{
								//ErrorLog.Notice("\n" + index.ToString() + " dead bytes trimed \n");
								Buffer.BlockCopy(data, (int)index, data, 0, data.Length - (int)index);
								Array.Resize(ref data, data.Length - (int)index);
							}

							// enough for one full packet?
							if (data.Length < 638) // no, continue to gather
							{
								//ErrorLog.Notice("\n packet too small \n");
								Array.Resize(ref RxData, data.Length);
								RxData = data;
							}
							else // yes
							{
								// short circuit for universe, then frame, then dmp
								if (CommonE1_31.CompareUShortInByteArray(new byte[] { data[113], data[114] }, Universe))
								{
									pattern = new byte[6];
									Buffer.BlockCopy(Frame.FramingLayerPacket, 0, pattern, 0, pattern.Length);
									index = IndexOfPatternMatch(data, pattern, Root.RootLayerPacket.Length);
									if (index != null && index == Root.RootLayerPacket.Length)
									{
										pattern = new byte[10];
										Buffer.BlockCopy(Dmp.DmpLayerPacket, 0, pattern, 0, pattern.Length);
										index = IndexOfPatternMatch(data, pattern, Root.RootLayerPacket.Length + Frame.FramingLayerPacket.Length);
										if (index != null && index == Root.RootLayerPacket.Length + Frame.FramingLayerPacket.Length)
										{
											Array.Resize(ref RxData, data.Length - 638);
											Buffer.BlockCopy(data, 638, RxData, 0, data.Length - 638);	// should handle a no MOD buffers
											Array.Resize(ref data, 638);
											//CrestronInvoke.BeginInvoke(ProcessSacnAsString, data);
											CrestronConsole.PrintLine("Verified: " + CrestronEnvironment.TickCount.ToString());
											
											//CrestronInvoke.BeginInvoke(ProcessSacnAsInt, data);
											ProcessInline(data);
										}
									}
								}
							}
						}
					}
				}
				else
				{ }
			}
		}

		/// <summary>
		/// use to validate data in packet
		/// </summary>
		/// <param name="buffer">received packet</param>
		/// <param name="pattern">data to validate</param>
		/// <param name="startIndex">position to start comp in packet</param>
		/// <returns>position match is found</returns>
		int? IndexOfPatternMatch(byte[] buffer, byte[] pattern, int startIndex)    
		{
			int? position = null;
			int i = Array.IndexOf<byte>(buffer, pattern[0], startIndex);  
			while (i >= 0 && i <= buffer.Length - pattern.Length)  
			{
			  byte[] segment = new byte[pattern.Length];
			  Buffer.BlockCopy(buffer, i, segment, 0, pattern.Length);
			  if (segment.SequenceEqual<byte>(pattern))
			  {
				  position = i;
				  break;
			  }
			  i++;
			}
			return position;    
		}

		//bool ValidRootLayer(byte[] data)
		//{
		//    // check Preamble size, Postamble size, ACN pkt id, Flags&Len, Vector opctects only
		//    for (ushort u = 0; u < 22; u++)
		//    {
		//        if (data[u] != Root.RootLayerPacket[u])
		//            return false;
		//    }

		//    return true;
		//}

		//bool ValidFrammingLayer(byte[] data)
		//{
		//    // check Flags&Len, Vector
		//    for (ushort u = 0; u <= 4; u++)
		//    {
		//        if (data[u + Root.RootLayerPacket.Length] != Frame.FramingLayerPacket[u])
		//            return false;
		//    }

		//    return true;
		//}

		//bool ValidDmpLayer(byte[] data)
		//{
		//    // check Flags&Len, Vector, Addr&Data Type, First Prop Addr, Addr Inc, Prop value cnt
		//    for (ushort u = 0; u <= 10; u++)
		//    {
		//        if (data[u + Root.RootLayerPacket.Length + Frame.FramingLayerPacket.Length] != Dmp.DmpLayerPacket[u])
		//            return false;
		//    }

		//    return true;
		//}

		//void ProcessSacnAsString(object activeDataSlots)
		//{
		//    //ErrorLog.Notice("\n sending out \n");

		//    byte[] slots = (byte[])activeDataSlots;
		//    //string dmx512Packet = BitConverter.ToString(slots, 126, 512);
		//    string dmx512Packet = BitConverter.ToString(slots, 126, 8);
		//    //CrestronConsole.PrintLine(dmx512Packet);

		//    if (ValidSequence(slots[111]))
		//    {
		//        SimplSharpString sss = new SimplSharpString(dmx512Packet + "-");
		//        SendSacn(sss); // easier to parse in s+
		//    }
		//}

		//void ProcessSacnAsInt(object activeDataSlots)
		void ProcessInline(byte[] activeDataSlots)
		{
			byte[] slots = (byte[])activeDataSlots;

			CrestronConsole.PrintLine("populate dmx array: " + CrestronEnvironment.TickCount.ToString());
			for (int i = 0; i < 512; i++)
				DmxData[i] = (ushort)slots[i + 126];

			//DmxData = (ushort[])activeDataSlots;

			//if (ValidSequence(slots[111]))
			//{
				CrestronConsole.PrintLine("Callback start: " + CrestronEnvironment.TickCount.ToString());
				ReadSacn();
				CrestronConsole.PrintLine("Callback end: " + CrestronEnvironment.TickCount.ToString());
			//}
		}

		bool ValidSequence(byte sequence)
		{
			sbyte b = (sbyte)(sequence - LastSequence);

			if (b > -20 && b <= 0)
				return false;

			LastSequence = sequence;
			return true;
		}

		public int GetTime()
		{
			return CrestronEnvironment.TickCount;
		}
	}
}