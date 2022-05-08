using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RlktServiceController.Remote_Network
{
    /// <summary>
    /// This packet controls the state/status of a service, it is sent from the client instance.
    /// </summary>
    public class PServiceControl : PacketDefinition
    {        
        //Packet members
        public int serviceId;
        public ServiceOperation operation;

        //Get packet type
        public override NetworkPacketType GetPacketType() => NetworkPacketType.SERVICE_CONTROL;

        //Get packet structure size
        public override int GetPacketSize()
        {
            return sizeof(int) + sizeof(int);
        }

        //Enums..
        public enum ServiceOperation
        {
            START_ALL,
            STOP_ALL,
            START,
            STOP
        }

        //Getter/Setter for the packet data
        public override int SetPacketData(byte[] data)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
            {
                serviceId = reader.ReadInt32();
                operation = (ServiceOperation)reader.ReadInt32();
            }

            return GetPacketSize();
        }

        public override byte[] GetPacketData()
        {
            MemoryStream ms = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write(serviceId);
                writer.Write((int)operation);
            }

            return ms.ToArray();
        }
    }
}
