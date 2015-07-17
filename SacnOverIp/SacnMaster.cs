using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

//using System.Net.Sockets;
using Crestron.SimplSharp.CrestronSockets;
using Crestron.SimplSharp;

namespace SacnOverIp
{
	public class SacnMaster
	{
		List<Connection> ConnectionList = new List<Connection>();
		//UdpClient[] Clients = new UdpClient[5];
		UDPServer[] Clients = new UDPServer[5];
		byte[] E131Packet; // big endian
		bool InitializedProtocol;
		bool AnInitializedConnection;

		///// <summary>
		///// SIMPL+ can only execute the default constructor.
		///// </summary>
		//public Sacn()
		//{
		//}

		public void InitializeConnection(string ipAddress)
		{
			Connection c = new Connection(ipAddress);
			if (c.UdpClient != null)
			{
				ConnectionList.Add(c);
				AnInitializedConnection = true;
			}
		}

		public void InitializeFields(string sourceName, ushort universe)
		{
			CommonE1_31.RootLayer Root = new CommonE1_31.RootLayer();
			byte[] RootLayerOctets = Root.RootLayerPacket;
			CommonE1_31.FramingLayer Frame = new CommonE1_31.FramingLayer(sourceName, universe);
			byte[] FramingLayerOctets = Frame.FramingLayerPacket;
			CommonE1_31.DmpLayer Dmp = new CommonE1_31.DmpLayer();
			byte[] DmpLayerOctets = Dmp.DmpLayerPacket;

			E131Packet = CommonE1_31.ConcatBytes(new byte[][] { RootLayerOctets, FramingLayerOctets, DmpLayerOctets });
			InitializedProtocol = true;
		}

		/// <summary>
		/// sACN Send systems
		/// </summary>
		/// <param name="universe">A set of up to 512 data slots identified by universe number. Note: In E1.31 there may be multiple sources for a universe. See also Slot.</param>
		/// <param name="activeDataSlots">When translating ANSI E1.11 DMX512-A [DMX] to E1.31, the active data slots are defined as ranging from data slot 1 to the maximum data slot in the most recently received packet with the corresponding START Code</param>
		public void Send(string activeDataSlotsAsString)
		{
			if (!string.IsNullOrEmpty(activeDataSlotsAsString))
			{
				try
				{
					char[] separator = new char[] { ',' };
					string[] sa = activeDataSlotsAsString.Split(separator);
					
					byte[] dmx512Packet = new byte[512];
					if (sa.Length > 512)
					{
						for (int i = 0; i < dmx512Packet.Length; i++)
							dmx512Packet[i] = byte.Parse(sa[i]);
					}
					else
					{
						for (int i = 0; i < sa.Length; i++)
							dmx512Packet[i] = Convert.ToByte(sa[i], 16);
					}

					Send(dmx512Packet, null, null, null);
				}
				catch { CrestronConsole.PrintLine("bad dmx512"); }
			}
		}

		/// <summary>
		/// sACN Send for Pro systems
		/// </summary>
		/// <param name="universe">A set of up to 512 data slots identified by universe number. Note: In E1.31 there may be multiple sources for a universe. See also Slot.</param>
		/// <param name="activeDataSlots">When translating ANSI E1.11 DMX512-A [DMX] to E1.31, the active data slots are defined as ranging from data slot 1 to the maximum data slot in the most recently received packet with the corresponding START Code</param>
		/// <param name="startCode">DMX512-A START Code</param>
		/// <param name="preview">indicates that the data in this packet is intended for use in visualization or media server preview applications and shall not be used to generate live output.</param>
		/// <param name="terminate">intended to allow E1.31 transmitters to terminate transmission of a stream without waiting for a timeout to occur and to indicate to receivers that such termination is not a fault condition</param>
		internal void Send(byte[] activeDataSlots, byte? startCode, bool? preview, bool? terminate)
		{
			if (InitializedProtocol && AnInitializedConnection)
			{
				E131Packet[111] += 1;

				//E131[112] = 0x00; // already
				if (preview != null && (bool)preview)
					E131Packet[112] = (byte)(E131Packet[112] | 0x80);
				if (terminate != null && (bool)terminate)
					E131Packet[112] = (byte)(E131Packet[112] | 0x40);

				//byte[] universeOctets = ConvertUShortToByteArray(universe);
				//Buffer.BlockCopy(universeOctets, 0, E131Packet, 113, universeOctets.Length);

				byte[] dmx512Packet = new byte[512];
				if (activeDataSlots.Length > 512)
					Buffer.BlockCopy(activeDataSlots, 0, dmx512Packet, 0, dmx512Packet.Length);
				else
					Buffer.BlockCopy(activeDataSlots, 0, dmx512Packet, 0, activeDataSlots.Length);

				//byte[] pkt = Concat(new byte[][] { E131, dmx512 });
				if (startCode != null)
					E131Packet[125] = (byte)startCode;

				Buffer.BlockCopy(dmx512Packet, 0, E131Packet, 126, dmx512Packet.Length);

				foreach (Connection c in ConnectionList)
				{
					//c.UdpClient.Connect(c.IpAddress, c.Port);
					//c.UdpClient.Send(E131Packet, E131Packet.Length);

					c.UdpClient.SendData(E131Packet, E131Packet.Length);
				}
			}
		}
	}

	internal class Connection
	{
		//internal UdpClient UdpClient;
		internal UDPServer UdpClient;

		public Connection(string ipAddress)
		{
			byte[] ip = new byte[4];

			if (!string.IsNullOrEmpty(ipAddress))
			{
				char[] separator = new char[] { '.', ':' };
				string[] sa = ipAddress.Split(separator);

				if (sa.Length == 4)
				{
					try
					{
						for (uint u = 0; u <= 3; u++)
							ip[u] = byte.Parse(sa[u]);
						
						//UdpClient = new UdpClient(System.Net.Sockets.AddressFamily.InterNetwork);
						//IpAddress = new System.Net.IPAddress(ip);

						IPAddress ipa = new IPAddress(ip);
						UdpClient = new UDPServer(ipa, 5568, 65534);
						UdpClient.EnableUDPServer();
					}
					catch { ErrorLog.Warn("Invalid sACN channel address: " + ipAddress); }
				}
			}
		}
	}
}