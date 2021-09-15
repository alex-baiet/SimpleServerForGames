using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerSimple {
    /// <summary>Handle received data and do action depending of the data.</summary>
    class ServerReceive {
        private static int spamCount = 0;
        private static Database db = Database.Instance;

        /// <summary>Treat the packet received depending of his content.</summary>
        public static void HandlePacket(Packet packet, Client client) {
            if (packet.TargetId == (ushort)SpecialId.Null) {
                throw new NotSupportedException("A packet with no target client can't be managed.");
            }

            Packet toSend;
            switch (packet.Name) {
                case "allConnectionDataReceived":
                    toSend = new Packet(SpecialId.Broadcast, "idName");
                    toSend.Write(client.Id);
                    toSend.Write(client.Pseudo);
                    Server.SendPacket(SpecialId.Broadcast, toSend);
                    Server.SendMessage(SpecialId.Broadcast, $"{client.Pseudo} join the server.");
                    break;

                case "connected":
                    ConsoleServer.WriteLine($"{client.Pseudo} connecting to the server with id {client.Id}...", MessageType.Debug);
                    break;

                case "disconnect":
                    ConsoleServer.WriteLine($"{client.Pseudo} disconnected.");
                    Server.RemoveClient(client.Id, "Well it's you who disconnected but if you see this this is not normal :(");
                    break;

                case "msg":
                    string msg = packet.ReadString();
                    if (packet.TargetId == (ushort)SpecialId.Broadcast) Server.SendMessage(packet.TargetId, msg);
                    else Server.SendMessage(new ushort[] { packet.TargetId, packet.SenderId }, msg);
                    break;

                case "ping":
                    toSend = new Packet(client.Id, "pingReturn");
                    client.SendPacket(toSend);
                    if (packet.TargetId != (ushort)SpecialId.Server) {
                        Server.SendPacket(packet.TargetId, packet);
                    }
                    break;

                case "pingReturn":
                    client.EndPing();
                    break;

                case "pseudo": // Connection of the client with his pseudo
                    client.Pseudo = packet.ReadString();
                    if (!IdHandler.AddIdName(client.Id, client.Pseudo)) { // A client with the same name already exist
                        ConsoleServer.WriteLine($"Connection of {client.Pseudo} failed : another client with the same name already exist.");
                        Server.RemoveClient(client.Id, "Another client with the same name already exist.");
                        break;
                    }
                    // Sending all datas
                    toSend = new Packet(client.Id, "yourId");
                    client.SendPacket(toSend);

                    // Sending all clients name
                    foreach (ushort idConnected in Server.ConnectedClientsId) {
                        if (idConnected == client.Id) continue;
                        toSend = new Packet(client.Id, "idName");
                        toSend.Write(idConnected);
                        toSend.Write(Server.GetClient(idConnected).Pseudo);
                        client.SendPacket(toSend);
                    }

                    // Packet meaning all data has been sent.
                    toSend = new Packet(client.Id, "allConnectionDataSent");
                    client.SendPacket(toSend);
                    break;

                case "query":
                    ushort idTarget = packet.ReadUshort();
                    string name = packet.ReadString();
                    if (db.TryGet(idTarget, name, out Packet storedPacket)) {
                        storedPacket.SenderId = (ushort)SpecialId.Server;
                        storedPacket.TargetId = client.Id;
                        Server.SendPacket(client.Id, storedPacket);
                    } else {
                        ConsoleServer.WriteLine($"{IdHandler.IdToName(packet.SenderId)} tried to access in database [{idTarget}, {name}], but it does not exist.", MessageType.Warning);
                    }
                    break;

                case "spam":
                    ConsoleServer.WriteLine($"Spam count : {++spamCount}", MessageType.Debug);
                    break;

                default:
                    ConsoleServer.WriteLine($"The received packet \"{packet.Name}\" is not supported.", MessageType.Warning);
                    break;
            }
        }
    }
}
