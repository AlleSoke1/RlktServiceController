using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RlktServiceController.Remote_Network
{
    /// <summary>
    /// Packet that handles authentification via the shared key (the key must be defined in both client and server instance)
    /// </summary>
    public class PHandshake : PacketDefinition
    {
        //Packet members
        public string key;

        //Get packet type
        public override NetworkPacketType GetPacketType() => NetworkPacketType.HANDSHAKE;

        //Get packet structure size
        public override int GetPacketSize()
        {
            return key.Length;
        }

        //Getter/Setter for the packet data
        public override int SetPacketData(byte[] data)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
            {
                key = reader.ReadString();
            }

            return GetPacketSize(); 
        }

        public override byte[] GetPacketData()
        {
            MemoryStream ms = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write(key);
            }

            return ms.ToArray();
        }
    }
}
