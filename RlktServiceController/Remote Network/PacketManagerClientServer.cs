using RlktServiceController.Remote_Network;
using RlktServiceControllerCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RlktServiceController.Remote_Network
{
    /// <summary>
    /// Client -> Server packet logic goes in here.
    /// </summary>
    public class PacketManagerClientServer
    {
        Queue<PacketDefinition> packets = new Queue<PacketDefinition>();

        #region CLIENT -> SERVER PACKET RECEIVING
        void OnRecvHandshake(PHandshake handshake)
        {
            Logger.Add($"[Server] Connection handshake recv key {handshake.key}");
            if(handshake.key == CommonConfig.NetworkSharedKey)
            {
                Logger.Add($"[Server] Authentification successful.");

                //On successful auth, send the service list.
                PacketManagerServerClient.Instance.SendServiceInfo(handshake.guid);
            }
            else
            {
                Logger.Add($"[Server][ERROR] Authentification failed, shared key does not match.");

                //On failed auth, disconnect the client.
                NetworkServer.Instance.Disconnect(handshake.guid);
            }
        }

        void OnRecvKeepAlive(PKeepAlive keepAlive)
        {
            Logger.Add($"[Server] KeepAlive received {keepAlive.keepalive_txt}");
        }

        void OnRecvServiceInfo(PServiceInfo serviceInfo)
        {
            //Not needed.
        }

        void OnRecvServiceControl(PServiceControl serviceControl)
        {
            int id = serviceControl.serviceId;

            switch (serviceControl.operation)
            {
                case PServiceControl.ServiceOperation.START_ALL: ServiceManager.StartAll(); break;
                case PServiceControl.ServiceOperation.STOP_ALL:  ServiceManager.StopAll(); break;
                case PServiceControl.ServiceOperation.START:     ServiceManager.GetService(id)?.Control(ProcessEvent.START_PROCESS); break;
                case PServiceControl.ServiceOperation.STOP:      ServiceManager.GetService(id)?.Control(ProcessEvent.STOP_PROCESS); break;
            }
        }
        #endregion


        #region CLIENT -> SERVER PACKET SENDING
        public void SendHandshakePacket()
        {
            PHandshake handshake = new PHandshake();
            handshake.key = CommonConfig.NetworkSharedKey;

            NetworkClient.Instance.SendPacket(handshake);
        }

        public void SendKeepAlivePacket()
        {
            PKeepAlive keepAlive = new PKeepAlive();
            keepAlive.keepalive_txt = "KEEPALIVE";

            NetworkClient.Instance.SendPacket(keepAlive);
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
                    }
                }
            }
        }

        //
        public static PacketManagerClientServer Instance = new PacketManagerClientServer();
        public static void OnRecvPacket(PacketDefinition packet) => Instance.packets.Enqueue(packet);
        public static void Process() => Instance.Tick();
    }
}
