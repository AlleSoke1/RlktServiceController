using LiteNetwork.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RlktServiceController.Remote_Network
{
    internal class NetworkServerUser : LiteServerUser
    {
        public override Task HandleMessageAsync(byte[] packet)
        {
            using (var stream = new BinaryReader(new MemoryStream(packet)))
            {
                while (stream.BaseStream.Position < packet.Length)
                {
                    int packetSize = stream.ReadInt16(); //Keeping it simple, this is the size of payload/packetData and it does not include header sizes (packetSize / packetType).
                    int packetType = stream.ReadInt16();
                    byte[] packetData = stream.ReadBytes(packetSize);

                    ProcessPacket(packetType, packetData);
                }
            }
            return Task.CompletedTask;
        }

        protected override void OnConnected()
        {
            Logger.Add($"Client {Id} connected.");

            Thread.Sleep(10);

            //Send service info first time an user connects.
            //Refactor and move to PacketManagerClientServer
            //SendServiceInfo();
        }

        protected override void OnDisconnected()
        {
            Logger.Add($"Client {Id} disconnected.");
        }

        protected override void OnError(object sender, Exception exception)
        {
            Logger.Add($"Client {exception.Message} disconnected.");
        }

        void ProcessPacket(int packetType, byte[] data)
        {
            PacketDefinition packet = NetworkPacketBase.GetNetworkPacketClass((NetworkPacketType)packetType);
            if (packet != null)
            {
                packet.SetPacketData(data);
                packet.guid = Id;

                PacketManagerClientServer.OnRecvPacket(packet);
            }
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

                Send(ms.ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}