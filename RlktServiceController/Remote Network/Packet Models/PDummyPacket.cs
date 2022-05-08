using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RlktServiceController.Remote_Network
{
    public class PDummyPacket : PacketDefinition
    {
        //Packet members
        public int dummy_value;

        //Get packet type
        public override NetworkPacketType GetPacketType() => NetworkPacketType.DUMMY_PACKET;

        //Get packet structure size
        public override int GetPacketSize()
        {
            return sizeof(int);
        }

        //Getter/Setter for the packet data
        public override int SetPacketData(byte[] data)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
            {
                dummy_value = reader.ReadInt32();
            }

            return GetPacketSize();
        }

        public override byte[] GetPacketData()
        {
            MemoryStream ms = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write(dummy_value);
            }

            return ms.ToArray();
        }
    }
}
