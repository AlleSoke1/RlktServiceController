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
                BroadcastKeepAlivePacket();
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

        public void BroadcastUpdateServiceInfo(Service service)
        {
            if (liteServer == null)
                return;

            PServiceInfo serviceInfo = new PServiceInfo();
            serviceInfo.serviceId       = service.ID;
            serviceInfo.serviceName     = "";               //For update purposes, service name is not needed!
            serviceInfo.serviceStatus   = (int)service.Status;

            foreach (NetworkServerUser client in liteServer.ConnectedUsers) 
                client.SendPacket(serviceInfo);
        }

        public void BroadcastKeepAlivePacket()
        {
            if (liteServer == null)
                return;

            PKeepAlive keepAlive = new PKeepAlive();
            keepAlive.keepalive_txt = "KEEPALIVE";

            foreach (NetworkServerUser client in liteServer.ConnectedUsers)
                client.SendPacket(keepAlive);
        }

        public void BroadcastDummyPacket()
        {
            if (liteServer == null)
                return;

            PDummyPacket dummy = new PDummyPacket();
            dummy.dummy_value = new Random().Next();

            foreach (NetworkServerUser client in liteServer.ConnectedUsers)
                client.SendPacket(dummy);
        }

        public void SendServiceInfo(Guid guid)
        {
            foreach (Service service in ServiceManager.GetServices())
            {
                PServiceInfo serviceInfo = new PServiceInfo();
                serviceInfo.serviceId = service.ID;
                serviceInfo.serviceName = service.Name;
                serviceInfo.serviceStatus = (int)service.Status;

                var client = liteServer.GetUser(guid);
                if(client != null)
                   client.SendPacket(serviceInfo);
            }
        }

        public void SetListenInfo(string ipAddr, int port) => (listenServerIP, listenServerPort) = (ipAddr, port);

        public static NetworkServer server = new NetworkServer();
        public static void Initialize() => server.InitializeServer();
        public static void Process() => server.Tick();
    }
}
