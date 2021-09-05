using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace ClientSimple {
    class Client {
        public const int BufferSize = 4096;
        public static Client Instance = new Client();

        public int Id { get; private set; }
        public string Pseudo { get; private set; } = "Guest";

        private TcpClient _tcpClient;
        private byte[] _receiveBuffer = new byte[BufferSize];
        private NetworkStream _stream;
        private PacketManager _packetManager = new PacketManager();
        private Stopwatch _stopwatch = new Stopwatch();

        #region ClientSideOnly
        private Dictionary<int, string> _idNames = new Dictionary<int, string>();
        #endregion

        #region Connection
        /// <summary>Connect the client to the server.</summary>
        /// <returns>True if the connection is a success.</returns>
        public void Connect(string ip, int port) {
            _tcpClient = new TcpClient();

            ConsoleServer.WriteLine($"Starting connexion to {ip}:{port}...");
            _tcpClient.BeginConnect(ip, port, new AsyncCallback(ConnectCallback), _tcpClient);
        }

        public void Disconnect() {
            _tcpClient.Close();
        }

        private void ConnectCallback(IAsyncResult res) {
            if (_tcpClient.Connected) {
                ConsoleServer.WriteLine("Connection to server establish. Waiting for datas...", MessageType.Normal);
                _tcpClient.EndConnect(res);
                _stream = _tcpClient.GetStream();
                _stream.BeginRead(_receiveBuffer, 0, BufferSize, new AsyncCallback(ReceiveCallback), null);

                // Sending pseudo to server to finish the connection.
                Packet packet = new Packet(SpecialId.Server, "pseudo");
                packet.Write(Pseudo);
                SendPacket(packet);
            } else {
                ConsoleServer.WriteLine("Connection to server failed.", MessageType.Error);
            }
        }
        #endregion

        #region Sending
        public void SendMessage(SpecialId toClientId, string msg) { SendMessage((int)toClientId, msg); }
        public void SendMessage(int toClientId, string msg) {
            Packet packet = new Packet(toClientId, "msg");
            packet.Write(ConsoleServer.ToMessageFormat(Pseudo, msg));
            SendPacket(packet);

            ConsoleServer.WriteLine("Message send to server.", MessageType.Debug);
        }
        
        public void SendPacket(Packet packet) {
            packet.WriteLength();
            _stream.BeginWrite(packet.ToArray(), 0, packet.Length, null, null);
        }

        public void Ping() {
            if (!_stopwatch.IsRunning) {
                Packet packet = new Packet(SpecialId.Server, "ping");
                SendPacket(packet);
                _stopwatch.Start();
            }
        }
        #endregion

        #region Receiving
        private void ReceiveCallback(IAsyncResult res) {
            try {
                int byteLength = _stream.EndRead(res);

                byte[] data = new byte[byteLength];
                Array.Copy(_receiveBuffer, data, byteLength);

                Packet[] packets = _packetManager.GetPackets(data);

                foreach (Packet packet in packets) {
                    ConsoleServer.WriteLine($"Receiving packet from the server named \"{packet.Name}\" (size={packet.Length})", MessageType.Debug);

                    HandlePacketReceived(packet);
                }
                if (_tcpClient.Connected) _stream.BeginRead(_receiveBuffer, 0, BufferSize, new AsyncCallback(ReceiveCallback), null);

            } catch (System.IO.IOException) {
                ConsoleServer.WriteLine($"Lost connection to server.", MessageType.Error);
            }
        }

        /// <summary>Treat the packet received depending of his content.</summary>
        private void HandlePacketReceived(Packet packet) {
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
        #endregion
    }
}
