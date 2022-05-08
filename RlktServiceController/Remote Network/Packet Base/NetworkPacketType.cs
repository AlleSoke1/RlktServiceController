using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RlktServiceController.Remote_Network
{
    public enum NetworkPacketType
    {
        NOT_DEFINED,
        HANDSHAKE,
        KEEPALIVE,
        SERVICE_INFO,
        SERVICE_CONTROL,
        DUMMY_PACKET,
        LOG,
    }

    public static class NetworkPacketBase
    {
        public static PacketDefinition GetNetworkPacketClass(NetworkPacketType type)
        {
            PacketDefinition result = null;
            
            switch (type)
            {
                case NetworkPacketType.HANDSHAKE:       result = new PHandshake(); break;
                case NetworkPacketType.KEEPALIVE:       result = new PKeepAlive(); break;
                case NetworkPacketType.SERVICE_CONTROL: result = new PServiceControl(); break;
                case NetworkPacketType.SERVICE_INFO:    result = new PServiceInfo(); break;
                case NetworkPacketType.DUMMY_PACKET:    result = new PDummyPacket(); break;
            }

            return result;
        }
    }
}
