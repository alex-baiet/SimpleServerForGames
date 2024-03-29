﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace ServerSimple {
    /// <summary>Used to communicate with distant clients.</summary>
    class Client {
        public const int BufferSize = 4096;

        /// <summary>Client's id. Each client has a different id.</summary>
        public ushort Id { get; set; }
        /// <summary>Name of the player connected trought this client.</summary>
        public string Pseudo { get; set; }

        private TcpClient _tcpClient;
        private byte[] _receiveBuffer;
        private byte[] _test { get => _receiveBuffer; set => _receiveBuffer = value; }
        private NetworkStream _stream; // on peut pas le recuperer de _tcpClient plutot que de le stocker ?
        private PacketManager _packetManager = new PacketManager();
        private Stopwatch _stopwatch = new Stopwatch();

        /// <summary>Create a connexion to a client.</summary>
        public Client(TcpClient tcpClient, ushort id) {
            // Tcp connection
            _tcpClient = tcpClient;
            _stream = _tcpClient.GetStream();
            _receiveBuffer = new byte[BufferSize];
            Pseudo = _tcpClient.Client.RemoteEndPoint.ToString();
            Id = id;

            _stream.BeginRead(_test, 0, BufferSize, new AsyncCallback(ReceiveCallback), null);
        }

        #region Connection
        /// <summary>Disconnect the client.</summary>
        /// <remarks>Must close the host point before closing the server's client.</remarks>
        public void Disconnect() {
            _tcpClient.Close();
        }
        #endregion

        #region Sending
        /// <summary>Send a message to the distant connection.</summary>
        /// <remarks>The message is just a "msg" packet.</remarks>
        public void SendMessage(string msg) {
            Packet packet = new Packet(Id, "msg");
            packet.Write(ConsoleServer.ToMessageFormat(Server.Name, msg));
            SendPacket(packet);

            ConsoleServer.WriteLine($"Message sent to {Pseudo}.", MessageType.Debug);
        }

        /// <summary>Send a packet to the distant connection.</summary>
        public void SendPacket(Packet packet) {
            if (!_tcpClient.Connected) return;
            if (packet.TargetId != Id && packet.TargetId != (ushort)SpecialId.Broadcast && packet.SenderId != Id) {
                throw new NotSupportedException($"The packet's id ({packet.TargetId}) must correspond to the client's id ({Id}).");
            }
            packet.WriteLength();
            ConsoleServer.WriteLine($"sent packet \"{packet.Name}\" with length : {packet.Length}", MessageType.Packet);
            _stream.BeginWrite(packet.ToArray(), 0, packet.Length, null, null);
        }

        /// <summary>Send a ping to the distant connection.</summary>
        /// <remarks>The ping is just a "ping" packet.</remarks>
        public void Ping() {
            if (!_stopwatch.IsRunning) {
                Packet packet = new Packet(Id, "ping");
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
                    ConsoleServer.WriteLine($"Receiving packet from {Pseudo} named \"{packet.Name}\" (size={packet.Length})", MessageType.Packet);
                    ServerReceive.HandlePacket(packet, this);
                }
                
                if (_tcpClient.Connected) _stream.BeginRead(_receiveBuffer, 0, BufferSize, new AsyncCallback(ReceiveCallback), null);
            } catch (System.IO.IOException) {
                ConsoleServer.WriteLine($"{Pseudo} lost connection.", MessageType.Error);
                Server.RemoveClient(Id, "Lost connection.");
            } catch (System.ObjectDisposedException) { }
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
