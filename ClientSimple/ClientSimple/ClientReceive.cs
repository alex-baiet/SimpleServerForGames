using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSimple {
    /*class ClientReceive {
        /// <summary>Treat the packet received depending of his content.</summary>
        public static void HandlePacketReceived(Packet packet) {
            if (packet.Id == (int)SpecialId.Null) {
                throw new NotSupportedException("A packet with no target client can't be managed.");
            }

            switch (packet.Name) {
                case "msg":
                    string msg = packet.ReadString();
                    ConsoleServer.WriteLine(msg);
                    break;

                case "yourId":
                    Id = packet.Id;
                    ConsoleServer.WriteLine($"Your assigned id : {Id}", MessageType.Debug);
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
                    SendPacket(toSend);
                    break;

                case "ping":
                    toSend = new Packet(SpecialId.Server, "pingReturn");
                    SendPacket(toSend);
                    break;

                case "pingReturn":
                    long res = _stopwatch.ElapsedMilliseconds;
                    _stopwatch.Stop();
                    ConsoleServer.WriteLine($"Ping returned in {res}ms.");
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }*/
}
