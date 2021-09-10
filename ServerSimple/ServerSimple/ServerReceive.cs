using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerSimple {
    class ServerReceive {
        /// <summary>Treat the packet received depending of his content.</summary>
        public static void HandlePacket(Packet packet, Client client) {
            if (packet.TargetId == (int)SpecialId.Null) {
                throw new NotSupportedException("A packet with no target client can't be managed.");
            }

            Packet toSend;
            switch (packet.Name) {
                case "pseudo":
                    client.Pseudo = packet.ReadString();
                    IdHandler.AddIdName(client.Id, client.Pseudo);
                    // Sending all datas
                    toSend = new Packet(client.Id, "yourId");
                    client.SendPacket(toSend);

                    // Sending all clients name
                    foreach (ushort idConnected in Server.ConnectedClientsId) {
                        toSend = new Packet(client.Id, "idName");
                        toSend.Write(idConnected);
                        toSend.Write(Server.GetClient(idConnected).Pseudo);
                        client.SendPacket(toSend);
                    }

                    // Packet meaning all data has been sent.
                    toSend = new Packet(client.Id, "allConnectionDataSent");
                    client.SendPacket(toSend);
                    break;

                case "allConnectionDataReceived":
                    ConsoleServer.WriteLine($"{client.Pseudo} join the server.");
                    break;

                case "msg":
                    string msg = packet.ReadString();
                    Server.SendMessage(packet.TargetId, msg);
                    break;

                case "connected":
                    ConsoleServer.WriteLine($"{client.Pseudo} connecting to the server with id {client.Id}...", MessageType.Debug);
                    break;

                case "disconnect":
                    ConsoleServer.WriteLine($"{client.Pseudo} disconnected.");
                    Server.RemoveClient(client.Id);
                    break;

                case "ping":
                    toSend = new Packet(client.Id, "pingReturn");
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
