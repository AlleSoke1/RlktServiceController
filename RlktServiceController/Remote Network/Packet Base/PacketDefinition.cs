using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RlktServiceController.Remote_Network
{
    public abstract class PacketDefinition
    {
        public virtual NetworkPacketType GetPacketType() => NetworkPacketType.NOT_DEFINED;

        public abstract int GetPacketSize();

        public abstract int SetPacketData(byte[] data);
        public abstract byte[] GetPacketData();

        public Guid guid { get; set; }
    }
}
