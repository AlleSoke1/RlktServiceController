using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RlktServiceController.Remote_Network
{
    /// <summary>
    /// This packet is used to get service information from the server, the server also sends status updates within this packet.
    /// </summary>
    public class PServiceInfo : PacketDefinition
    {
        //Packet members
        public int serviceId;
        public string serviceName;
        public int serviceStatus;

        //Get packet type
        public override NetworkPacketType GetPacketType() => NetworkPacketType.SERVICE_INFO;

        //Get packet structure size
        public override int GetPacketSize()
        {
            return serviceName.Length + sizeof(int) + sizeof(int);
        }

        //Getter/Setter for the packet data
        public override int SetPacketData(byte[] data)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
            {
                serviceId = reader.ReadInt32();
                serviceName = reader.ReadString();
                serviceStatus = reader.ReadInt32();
            }

            return GetPacketSize();
        }

        public override byte[] GetPacketData()
        {
            MemoryStream ms = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write(serviceId);
                writer.Write(serviceName);
                writer.Write(serviceStatus);
            }

            return ms.ToArray();
        }
    }
}
