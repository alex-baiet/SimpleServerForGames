using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace ClientSimple {
    class ClientReceive {
        /// <summary>Treat the packet received depending of his content.</summary>
        /// <param name="packet">The packet received.</param>
        /// <param name="client">The client receiving the packet.</param>
        public static void HandlePacket(Packet packet, Client client) {
            if (packet.TargetId == (int)SpecialId.Null) {
                throw new NotSupportedException("A packet with no target client can't be managed.");
            }

            switch (packet.Name) {
                case "msg":
                    string msg = packet.ReadString();
                    ConsoleServer.WriteLine(msg);
                    break;

                case "yourId":
                    client.Id = packet.TargetId;
                    ConsoleServer.WriteLine($"Your assigned id : {client.Id}", MessageType.Debug);
                    break;

                case "idName":
                    ushort id = packet.ReadUshort();
                    string idName = packet.ReadString();
                    IdHandler.AddIdName(id, idName);
                    ConsoleServer.WriteLine($"{idName} is connected to server with id {id}.", MessageType.Debug);
                    break;

                case "allConnectionDataSent": // Connection finished
                    ConsoleServer.WriteLine("Connected successfully to server !", MessageType.Success);
                    Packet toSend = new Packet(SpecialId.Server, "allConnectionDataReceived");
                    client.SendPacket(toSend);
                    break;

                case "ping":
                    toSend = new Packet(SpecialId.Server, "pingReturn");
                    client.SendPacket(toSend);
                    break;

                case "pingReturn":
                    client.EndPing();
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

    }
}
