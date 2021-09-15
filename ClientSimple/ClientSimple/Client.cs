using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;

namespace ClientSimple {
    /// <summary>Used to communicate with the server.</summary>
    class Client {
        public const int BufferSize = 4096;
        public static Client Instance = new Client();

        public ushort Id { get; set; }
        public string Pseudo { get; private set; } = "Guest";
        public bool Connected { get; private set; } = false;

        private TcpClient _tcpClient;
        private byte[] _receiveBuffer = new byte[BufferSize];
        private NetworkStream _stream;
        private PacketManager _packetManager = new PacketManager();
        private Stopwatch _stopwatch = new Stopwatch();

        #region Connection
        /// <summary>Connect the client to the server.</summary>
        /// <returns>True if the connection is a success.</returns>
        /// <param name="ip">The host's ip.</param>
        /// <param name="port">The host's port.</param>
        /// <param name="pseudo">Your client's pseudo.</param>
        public void Connect(string ip, int port, string pseudo) {
            Pseudo = string.IsNullOrWhiteSpace(pseudo) ? "Guest" : pseudo;
            _tcpClient = new TcpClient();

            ConsoleServer.WriteLine($"Starting connexion to {ip}:{port}...");
            _tcpClient.BeginConnect(ip, port, new AsyncCallback(ConnectCallback), _tcpClient);
        }

        /// <summary>Disconnect the client.</summary>
        public void Disconnect() {
            _tcpClient.Close();
            Connected = false;
        /// <summary>Disconnect the client.</summary>
        }

        /// <summary>Called when ending the connection to the server.</summary>
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
                Connected = true;
            } else {
                ConsoleServer.WriteLine("Connection to server failed.", MessageType.Error);
            }
        }
        #endregion

        #region Sending
        /// <summary>Ask for an information to the server.</summary>
        public void Query(SpecialId id, string name) { Query((ushort)id, name); }
        /// <summary>Ask for an information to the server.</summary>
        public void Query(ushort id, string name) {
            Packet packet = new Packet(SpecialId.Server, "query");
            packet.Write(id).Write(name);
            SendPacket(packet);
        /// <summary>Ask for an information to the server.</summary>
        }

        /// <summary>Send a message to the distant connection.</summary>
        /// <remarks>The message is just a "msg" packet.</remarks>
        public void SendMessage(SpecialId toClientId, string msg) { SendMessage((ushort)toClientId, msg); }
        /// <summary>Send a message to the distant connection.</summary>
        /// <remarks>The message is just a "msg" packet.</remarks>
        public void SendMessage(ushort toClientId, string msg) {
            Packet packet = new Packet(toClientId, "msg");
            packet.Write(ConsoleServer.ToMessageFormat(Pseudo, msg));
            SendPacket(packet);

            ConsoleServer.WriteLine($"Message sent to {IdHandler.IdToName(toClientId)}.", MessageType.Debug);
        }
        
        /// <summary>Send a packet to the distant connection.</summary>
        public void SendPacket(Packet packet) {
            try {
                packet.WriteLength();
                ConsoleServer.WriteLine($"sent packet \"{packet.Name}\" with length : {packet.Length}", MessageType.Packet);
                _stream.BeginWrite(packet.ToArray(), 0, packet.Length, null, null);
            } catch (ObjectDisposedException) { }
        }

        /// <summary>Send a ping to the server.</summary>
        /// <remarks>The ping is just a "ping" packet.</remarks>
        public void Ping() {
            Ping((ushort)SpecialId.Server);
        }

        /// <summary>Send a ping to the specified client.</summary>
        /// <remarks>The ping is just a "ping" packet.</remarks>
        public void Ping(ushort id) {
            if (!_stopwatch.IsRunning) {
                Packet packet = new Packet(id, "ping");
                SendPacket(packet);
                _stopwatch.Restart();
            }
        }
        #endregion

        #region Receiving
        /// <summary>Called when a packet is received.</summary>
        private void ReceiveCallback(IAsyncResult res) {
            try {
                int byteLength = _stream.EndRead(res);

                byte[] data = new byte[byteLength];
                Array.Copy(_receiveBuffer, data, byteLength);

                Packet[] packets = _packetManager.GetPackets(data);

                foreach (Packet packet in packets) {
                    ConsoleServer.WriteLine($"Receiving packet from the server named \"{packet.Name}\" (size={packet.Length})", MessageType.Packet);

                    ClientReceive.HandlePacket(packet, this);
                }
                if (_tcpClient.Connected) _stream.BeginRead(_receiveBuffer, 0, BufferSize, new AsyncCallback(ReceiveCallback), null);

            } catch (System.IO.IOException) {
                ConsoleServer.WriteLine($"Lost connection to server.", MessageType.Error);
                Disconnect();
            } catch (ObjectDisposedException) { }
        }

        /// <summary>To call after receiving a ping answer.</summary>
        public void EndPing() {
            long res = _stopwatch.ElapsedMilliseconds;
            _stopwatch.Stop();
            ConsoleServer.WriteLine($"Ping returned in {res}ms.");
        }
        #endregion
    }
}
