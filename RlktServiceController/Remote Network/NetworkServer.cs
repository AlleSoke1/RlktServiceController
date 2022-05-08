using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using LiteNetwork;
using LiteNetwork.Server;

namespace RlktServiceController.Remote_Network
{
    internal class NetworkServer
    {
        public int listenServerPort = RlktServiceControllerCommon.CommonConfig.DefaultPort;
        public string listenServerIP = RlktServiceControllerCommon.CommonConfig.DefaultListenIP;

        const double keepAliveInterval = 10; //seconds
        DateTime nextKeepAliveMsg = DateTime.Now.AddSeconds(keepAliveInterval);
        DateTime nextDummyMsg = DateTime.Now.AddSeconds(keepAliveInterval);

        LiteServer<NetworkServerUser> liteServer = null;

        public async Task InitializeServer()
        {
            try
            {
                var configuration = new LiteServerOptions()
                {
                    Host = listenServerIP,
                    Port = listenServerPort,
                    ReceiveStrategy = ReceiveStrategyType.Queued,
                };

                liteServer = new LiteServer<NetworkServerUser>(configuration);
                await liteServer.StartAsync(CancellationToken.None);

                //Log status
                Logger.Add($"Server instance listening on [{listenServerIP}:{listenServerPort}]");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void Tick()
        {
            if(DateTime.Now > nextKeepAliveMsg)
            {
                PacketManagerServerClient.Instance.BroadcastKeepAlivePacket();
                nextKeepAliveMsg = DateTime.Now.AddSeconds(keepAliveInterval);
            }

#if DEBUG_PACKETS
            if (DateTime.Now > nextDummyMsg)
            {
                BroadcastDummyPacket();
                nextDummyMsg = DateTime.Now.AddMilliseconds(10);
            }
#endif
        }

        public bool BroadcastPacket(PacketDefinition packet)
        {
            if (liteServer == null)
                return false;

            foreach(var client in liteServer.ConnectedUsers)
            {
                if (client == null)
                    continue;

                client.SendPacket(packet);
            }

            return true;
        }

        public bool SendPacket(PacketDefinition packet, Guid clientGuid)
        {
            if (liteServer == null)
                return false;

            var client = liteServer.GetUser(clientGuid);
            if (client == null)
                return false;

            client.SendPacket(packet);

            return true;
        }

        public bool Disconnect(Guid clientGuid)
        {
            if (liteServer.GetUser(clientGuid) == null)
                return false;

            liteServer.DisconnectUser(clientGuid);
            return true;
        }

        public void SetListenInfo(string ipAddr, int port) => (listenServerIP, listenServerPort) = (ipAddr, port);

        public static NetworkServer Instance = new NetworkServer();
        public static void Initialize() => Instance.InitializeServer();
        public static void Process() => Instance.Tick();
    }
}
