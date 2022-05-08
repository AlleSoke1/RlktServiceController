using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RlktServiceController.Remote_Network
{    
    /// <summary>
    /// Server -> Client packet logic goes in here.
    /// </summary>
    public class PacketManagerServerClient
    {
        Queue<PacketDefinition> packets = new Queue<PacketDefinition>();


        void OnRecvHandshake(PHandshake handshake)
        {
            Logger.Add($"[Client] Connection handshake recv key {handshake.key}");
        }

        void OnRecvKeepAlive(PKeepAlive keepAlive)
        {
            Logger.Add($"[Client] KeepAlive received {keepAlive.keepalive_txt}");

            //Send keepalive back.
            NetworkClient.client.SendKeepAlivePacket();
        }

        void OnRecvServiceInfo(PServiceInfo serviceInfo)
        {
            ServiceManager.manager.CreateRemoteService(serviceInfo);
        }

        void OnRecvServiceControl(PServiceControl serviceControl)
        {
            //Not needed.
        }

        void OnRecvDummyPacket(PDummyPacket dummyPacket)
        {
            Logger.Add($"[DUMMY] Received dummy packet with value {dummyPacket.dummy_value}");
        }

        //
        private void Tick()
        {
            while (packets.Count > 0)
            {
                PacketDefinition packet = packets.Dequeue();
                if (packet != null)
                {
                    NetworkPacketType type = packet.GetPacketType();

                    switch (type)
                    {
                        case NetworkPacketType.HANDSHAKE: OnRecvHandshake((PHandshake)packet); break;
                        case NetworkPacketType.KEEPALIVE: OnRecvKeepAlive((PKeepAlive)packet); break;
                        case NetworkPacketType.SERVICE_CONTROL: OnRecvServiceControl((PServiceControl)packet); break;
                        case NetworkPacketType.SERVICE_INFO: OnRecvServiceInfo((PServiceInfo)packet); break;
                        case NetworkPacketType.DUMMY_PACKET: OnRecvDummyPacket((PDummyPacket)packet); break;
                    }
                }
            }
        }

        //
        public static PacketManagerServerClient managerSCNetwork = new PacketManagerServerClient();
        public static void OnRecvPacket(PacketDefinition packet) => managerSCNetwork.packets.Enqueue(packet);
        public static void Process() => managerSCNetwork.Tick();
    }
}
