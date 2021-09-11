using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace ServerSimple {
    class Server {
        public const int Port = 26950;
        public const ushort MaxClient = 4;
        public const string Name = "Server Test";

        public static HashSet<ushort> ConnectedClientsId { get => new HashSet<ushort>(_assignedId); }
        public static bool IsOpen { get; private set; } = false;

        private static TcpListener _tcpListener;
        private static Client[] _clients = new Client[MaxClient + 1];
        private static int _connectedCount = 0;
        private static HashSet<ushort> _assignedId = new HashSet<ushort>();

        public static void Start() {
            ConsoleServer.WriteLine($"Starting server...");

            _tcpListener = new TcpListener(IPAddress.Any, Port);
            _tcpListener.Start();
            _tcpListener.BeginAcceptTcpClient(new AsyncCallback(ConnectCallback), null);
            IsOpen = true;

            ConsoleServer.WriteLine($"Server started on port {Port} !", MessageType.Success);
        }

        public static void Stop() {
            HashSet<ushort> copy = new HashSet<ushort>(_assignedId);
            foreach (ushort id in copy) {
                RemoveClient(id);
            }
            _tcpListener.Stop();
            IsOpen = false;
        }

        private static void ConnectCallback(IAsyncResult res) {
            try {
                if (_connectedCount == MaxClient) {
                    // Canceling connection.
                    ConsoleServer.WriteLine("Connection refused : Server is already full !", MessageType.Warning);
                    return;
                }

                TcpClient tcpClient = _tcpListener.EndAcceptTcpClient(res);
                ConsoleServer.WriteLine("Incoming connection...", MessageType.Debug);
                if (tcpClient.Client.Connected) ConsoleServer.WriteLine($"Connected to {tcpClient.Client.RemoteEndPoint}.", MessageType.Debug);

                AddClient(tcpClient);
                _tcpListener.BeginAcceptTcpClient(new AsyncCallback(ConnectCallback), null);
            } catch (ObjectDisposedException) { }
        }

        private static void AddClient(TcpClient tcpClient) {
            OpenCheck();
            for (ushort i = 1; i <= MaxClient; i++) {
            if (_clients[i] == null) {
                    _clients[i] = new Client(tcpClient, i);
                    _assignedId.Add(i);
                    _connectedCount++;
                    return;
                }
            }
        }

        public static void RemoveClient(ushort id) {
            OpenCheck();
            _clients[id].Disconnect();
            _clients[id] = null;
            _assignedId.Remove(id);
            IdHandler.RemoveIdName(id);
            _connectedCount--;
        }

        public static Client GetClient(ushort id) {
            return _clients[id];
        }

        public static void SendMessage(SpecialId id, string msg) { SendMessage((ushort)id, msg); }
        public static void SendMessage(ushort id, string msg) {
            OpenCheck();
            if (id == (ushort)SpecialId.Null) {
                ConsoleServer.WriteLine($"The message \"{msg}\" target no client.", MessageType.Error);
                return;
            }
            ConsoleServer.WriteLine(msg);
            if (id == (ushort)SpecialId.Broadcast) {
                foreach (ushort clientId in _assignedId) {
                    Packet packet = new Packet(clientId, "msg");
                    packet.Write(msg);
                    _clients[clientId].SendPacket(packet);
                }
                return;
            }
            if (id != (ushort)SpecialId.Server) {
                Packet packet = new Packet(id, "msg");
                packet.Write(msg);
                _clients[id].SendPacket(packet);
                return;
            }
        }

        public static void Ping() {
            OpenCheck();
            foreach (int id in _assignedId) {
                _clients[id].Ping();
            }
        }

        private static void OpenCheck() {
            if (!IsOpen) throw new NotSupportedException("The server must be open.");
        }
    }
}
