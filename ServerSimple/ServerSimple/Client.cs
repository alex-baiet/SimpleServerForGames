using System;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace ServerSimple {
    class Client {
        public const int BufferSize = 4096;

        /// <summary>Client's id. Each client has a different id.</summary>
        public int Id { get; set; }
        /// <summary>Name of the player connected trought this client.</summary>
        public string Pseudo { get; set; }

        private TcpClient _tcpClient;
        private byte[] _receiveBuffer;
        private byte[] _test { get => _receiveBuffer; set => _receiveBuffer = value; }
        private NetworkStream _stream; // on peut pas le recuperer de _tcpClient plutot que de le stocker ?
        private PacketManager _packetManager = new PacketManager();
        private Stopwatch _stopwatch = new Stopwatch();

        #region ServerOnlySide
        private bool _hasBasicData = false;
        #endregion

        /// <summary>Create a connexion to a client.</summary>
        public Client(TcpClient tcpClient, int id) {
            // Tcp connection
            _tcpClient = tcpClient;
            _stream = _tcpClient.GetStream();
            _receiveBuffer = new byte[BufferSize];
            Pseudo = _tcpClient.Client.RemoteEndPoint.ToString();
            Id = id;

            _stream.BeginRead(_test, 0, BufferSize, new AsyncCallback(ReceiveCallback), null);
        }

        #region Connection
        public void Disconnect() {
            _tcpClient.Close();
        }
        #endregion

        #region Sending
        public void SendMessage(string msg) {
            Packet packet = new Packet(Id, "msg");
            packet.Write(ConsoleServer.ToMessageFormat(Server.Name, msg));
            SendPacket(packet);

            ConsoleServer.WriteLine($"Message sent to {Pseudo}.", MessageType.Debug);
        }

        public void SendPacket(Packet packet) {
            if (packet.Id != Id) {
                throw new NotSupportedException("The packet's id must correspond to the client's id.");
            }
            packet.WriteLength();
            ConsoleServer.WriteLine($"sent packet \"{packet.Name}\" with length : {packet.Length}", MessageType.Packet);
            _stream.BeginWrite(packet.ToArray(), 0, packet.Length, null, null);
        }

        public void Ping() {
            if (!_stopwatch.IsRunning) {
                Packet packet = new Packet(Id, "ping");
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
                    ConsoleServer.WriteLine($"Receiving packet from {Pseudo} named \"{packet.Name}\" (size={packet.Length})", MessageType.Packet);
                    ServerReceive.HandlePacket(packet, this);
                }
                
                if (_tcpClient.Connected) _stream.BeginRead(_receiveBuffer, 0, BufferSize, new AsyncCallback(ReceiveCallback), null);
            } catch (System.IO.IOException) {
                ConsoleServer.WriteLine($"{Pseudo} lost connection.", MessageType.Error);
                Server.RemoveClient(Id);
            }
        }

        /// <summary>To call after receiving a ping answer.</summary>
        public void EndPing() {
            long res = _stopwatch.ElapsedMilliseconds;
            _stopwatch.Stop();
            ConsoleServer.WriteLine($"Ping to {Pseudo} returned in {res}ms.");
        }
        #endregion
    }
}
