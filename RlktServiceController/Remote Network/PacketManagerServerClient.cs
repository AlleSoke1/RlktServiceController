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

        #region Server -> Client PACKET RECEIVING
        void OnRecvHandshake(PHandshake handshake)
        {
            Logger.Add($"[Client] Connection handshake recv key {handshake.key}");
        }

        void OnRecvKeepAlive(PKeepAlive keepAlive)
        {
            Logger.Add($"[Client] KeepAlive received {keepAlive.keepalive_txt}");

            //Send keepalive back.
            SendKeepAlivePacket(keepAlive.guid);
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
        #endregion


        #region Server -> Client PACKET SENDING
        public void BroadcastUpdateServiceInfo(Service service)
        {
            PServiceInfo serviceInfo = new PServiceInfo();
            serviceInfo.serviceId = service.ID;
            serviceInfo.serviceName = "";               //For update purposes, service name is not needed!
            serviceInfo.serviceStatus = (int)service.Status;

            NetworkServer.Instance.BroadcastPacket(serviceInfo);
        }

        public void BroadcastKeepAlivePacket()
        {
            PKeepAlive keepAlive = new PKeepAlive();
            keepAlive.keepalive_txt = "KEEPALIVE";

            NetworkServer.Instance.BroadcastPacket(keepAlive);
        }

        public void BroadcastDummyPacket()
        {
            PDummyPacket dummy = new PDummyPacket();
            dummy.dummy_value = new Random().Next();

            NetworkServer.Instance.BroadcastPacket(dummy);
        }

        public void SendServiceInfo(Guid guid)
        {
            foreach (Service service in ServiceManager.GetServices())
            {
                PServiceInfo serviceInfo = new PServiceInfo();
                serviceInfo.serviceId = service.ID;
                serviceInfo.serviceName = service.Name;
                serviceInfo.serviceStatus = (int)service.Status;

                NetworkServer.Instance.SendPacket(serviceInfo, guid);
            }
        }

        public void SendKeepAlivePacket(Guid guid)
        {
            PKeepAlive keepAlive = new PKeepAlive();
            keepAlive.keepalive_txt = "KEEPALIVE";

            NetworkServer.Instance.SendPacket(keepAlive, guid);
        }

        #endregion

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
        public static PacketManagerServerClient Instance = new PacketManagerServerClient();
        public static void OnRecvPacket(PacketDefinition packet) => Instance.packets.Enqueue(packet);
        public static void Process() => Instance.Tick();
    }
}
