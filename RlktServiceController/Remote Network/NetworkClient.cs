﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using LiteNetwork.Client;
using RlktServiceControllerCommon;

namespace RlktServiceController.Remote_Network
{
    internal class NetworkClient
    {
        int connectServerPort = RlktServiceControllerCommon.CommonConfig.DefaultPort;
        string connectServerIP = "127.0.0.1";

        NetworkClientUser liteClient = null;

        public async Task InitializeClient()
        {
            try
            {
                Logger.Add($"Client instance trying to connect at [{connectServerIP}:{connectServerPort}]");

                LiteClientOptions options = new LiteClientOptions()
                {
                    Host = connectServerIP,
                    Port = connectServerPort
                };
                liteClient = new NetworkClientUser(options);
                await liteClient.ConnectAsync();
            }
            catch (Exception ex)
            {
                //log connection status
                Logger.Add($"Client failed to connect at [{connectServerIP}:{connectServerPort}]. The remote service controller is offline!");
                MessageBox.Show(ex.Message);
            }
        }

        public void Tick()
        {

        }

        public void SendPacket(PacketDefinition packet)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                using (var writer = new BinaryWriter(ms))
                {
                    byte[] packetData = packet.GetPacketData();

                    writer.Write((short)packetData.Length);
                    writer.Write((short)packet.GetPacketType());
                    writer.Write(packetData);
                }
                liteClient.Send(ms.ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void SetConnectInfo(string ipAddr, int port) => (connectServerIP, connectServerPort) = (ipAddr, port);

        public static NetworkClient Instance = new NetworkClient();
        public static void Initialize() => Instance.InitializeClient();
        public static void Process() => Instance.Tick();
    }
}
