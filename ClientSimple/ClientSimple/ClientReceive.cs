using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace ClientSimple {
    class ClientReceive {
        private static Dictionary<int, string> _idNames = new Dictionary<int, string>();

        /// <summary>Treat the packet received depending of his content.</summary>
        /// <param name="packet">The packet received.</param>
        /// <param name="client">The client receiving the packet.</param>
        public static void HandlePacket(Packet packet, Client client) {
            if (packet.Id == (int)SpecialId.Null) {
                throw new NotSupportedException("A packet with no target client can't be managed.");
            }

            switch (packet.Name) {
                case "msg":
                    string msg = packet.ReadString();
                    ConsoleServer.WriteLine(msg);
                    break;

                case "yourId":
                    client.Id = packet.Id;
                    ConsoleServer.WriteLine($"Your assigned id : {client.Id}", MessageType.Debug);
                    break;

                case "idName":
                    int id = packet.ReadInt();
                    string idName = packet.ReadString();
                    if (!_idNames.ContainsKey(id)) _idNames.Add(id, idName);
                    else _idNames[id] = idName;
                    break;

                case "allConnectionDataSent": // Connection finished
                    ConsoleServer.WriteLine("Connected successfully to server !", MessageType.Success);
                    ConsoleServer.WriteLine($"Connected clients' name : {Helper.ArrayToString(_idNames)}", MessageType.Debug);
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
