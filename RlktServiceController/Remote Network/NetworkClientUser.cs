using LiteNetwork.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RlktServiceController.Remote_Network
{
    internal class NetworkClientUser : LiteClient
    {
        public NetworkClientUser(LiteClientOptions options, IServiceProvider serviceProvider = null) : base(options, serviceProvider) { }

        public override Task HandleMessageAsync(byte[] packet)
        {
            using (var stream = new BinaryReader(new MemoryStream(packet)))
            {
                int packetSize = stream.ReadInt16(); //Keeping it simple, this is the size of payload/packetData and it does not include header sizes (packetSize / packetType).
                int packetType = stream.ReadInt16();
                byte[] packetData = stream.ReadBytes(packetSize);

                ProcessPacket(packetType, packetData);
            }
            return Task.CompletedTask;
        }

        protected override void OnConnected()
        {
            Console.WriteLine($"Connected to server, ID {Id}.");

            //authentificate via handshake
            PacketManagerClientServer.Instance.SendHandshakePacket();

            base.OnConnected();
        }

        protected override void OnDisconnected()
        {
            Console.WriteLine("Disconnected.");
            base.OnDisconnected();
        }

        private void ProcessPacket(int packetType, byte[] data)
        {
            PacketDefinition packet = NetworkPacketBase.GetNetworkPacketClass((NetworkPacketType)packetType);
            if (packet != null)
            {
                packet.SetPacketData(data);
                packet.guid = Id;

                PacketManagerServerClient.OnRecvPacket(packet);
            }
        }
    }
}
