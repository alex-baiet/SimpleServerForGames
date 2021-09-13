using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace ClientSimple {
    class ClientReceive {
        private static int spamCount = 0;

        /// <summary>Treat the packet received depending of his content.</summary>
        /// <param name="packet">The packet received.</param>
        /// <param name="client">The client receiving the packet.</param>
        public static void HandlePacket(Packet packet, Client client) {
            ushort id;

            switch (packet.Name) {
                case "allConnectionDataSent": // Connection finished
                    ConsoleServer.WriteLine("Connected successfully to server !", MessageType.Success);
                    Packet toSend = new Packet(SpecialId.Server, "allConnectionDataReceived");
                    client.SendPacket(toSend);
                    break;

                case "clientDisconnect":
                    id = packet.ReadUshort();
                    ConsoleServer.WriteLine($"{IdHandler.IdToName(id)} disconnected.");
                    IdHandler.RemoveIdName(id);
                    break;

                case "disconnect": // Server closed
                    string errorMsg = packet.ReadString();
                    ConsoleServer.WriteLine($"Connection to server failed : {errorMsg}", MessageType.Error);
                    client.Disconnect();
                    break;

                case "idName":
                    id = packet.ReadUshort();
                    string idName = packet.ReadString();
                    if (!IdHandler.ClientExist(id)) IdHandler.AddIdName(id, idName);
                    ConsoleServer.WriteLine($"{idName} is connected to server with id {id}.", MessageType.Debug);
                    break;

                case "msg":
                    string msg = packet.ReadString();
                    ConsoleServer.WriteLine(msg);
                    break;

                case "ping":
                    if (packet.SenderId == (ushort)SpecialId.Server) {
                        toSend = new Packet(SpecialId.Server, "pingReturn");
                        client.SendPacket(toSend);
                    }
                    break;

                case "pingReturn":
                    client.EndPing();
                    break;

                case "spam":
                    ConsoleServer.WriteLine($"Spam count : {++spamCount}", MessageType.Debug);
                    break;

                case "yourId":
                    client.Id = packet.TargetId;
                    ConsoleServer.WriteLine($"Your assigned id : {client.Id}", MessageType.Debug);
                    break;

                default:
                    ConsoleServer.WriteLine($"The received packet \"{packet.Name}\" is not supported.", MessageType.Warning);
                    break;
            }
        }

    }
}
