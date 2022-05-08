using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RlktServiceController.Remote_Network
{
    /// <summary>
    /// KeepAlive packet sent frequently each 10sec or so to keep the connection opened.
    /// </summary>
    public class PKeepAlive : PacketDefinition
    {
        //Packet members
        public string keepalive_txt;

        //Get packet type
        public override NetworkPacketType GetPacketType() => NetworkPacketType.KEEPALIVE;

        //Get packet structure size
        public override int GetPacketSize()
        {
            return keepalive_txt.Length;
        }

        //Getter/Setter for the packet data
        public override int SetPacketData(byte[] data)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
            {
                keepalive_txt = reader.ReadString();
            }

            return GetPacketSize();
        }

        public override byte[] GetPacketData()
        {
            MemoryStream ms = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write(keepalive_txt);
            }

            return ms.ToArray();
        }
    }
}
