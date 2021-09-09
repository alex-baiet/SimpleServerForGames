using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace ServerSimple {
    class Server {
        public const int Port = 26950;
        public const int MaxClient = 4;
        public const string Name = "Server Test";

        public static HashSet<int> ConnectedClientsId { get => new HashSet<int>(_assignedId); }

        private static TcpListener _tcpListener;
        private static IPEndPoint _ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
        private static Client[] _clients = new Client[MaxClient + 1];
        private static int _connectedCount = 0;
        private static HashSet<int> _assignedId = new HashSet<int>();

        public static void Start() {
            ConsoleServer.WriteLine($"Starting server...");

            _tcpListener = new TcpListener(IPAddress.Any, Port);
            _tcpListener.Start();
            _tcpListener.BeginAcceptTcpClient(new AsyncCallback(ConnectCallback), null);

            ConsoleServer.WriteLine($"Server started on port {Port} !", MessageType.Success);
        }

        private static void ConnectCallback(IAsyncResult res) {
            ConsoleServer.WriteLine("Incoming connection...", MessageType.Debug);
            if (_connectedCount == MaxClient) {
                // Canceling connection.
                ConsoleServer.WriteLine("Connection refused : Server is already full !", MessageType.Warning);
                return;
            }

            TcpClient tcpClient = _tcpListener.EndAcceptTcpClient(res);
            if (tcpClient.Client.Connected) ConsoleServer.WriteLine($"Connected to {tcpClient.Client.RemoteEndPoint}.", MessageType.Debug);

            AddClient(tcpClient);
            _tcpListener.BeginAcceptTcpClient(new AsyncCallback(ConnectCallback), null);
        }

        private static void AddClient(TcpClient tcpClient) {
            for (int i = 1; i <= MaxClient; i++) {
                if (_clients[i] == null) {
                    _clients[i] = new Client(tcpClient, i);
                    _assignedId.Add(i);
                    _connectedCount++;
                    return;
                }
            }
        }

        public static void RemoveClient(int id) {
            _clients[id].Disconnect();
            _clients[id] = null;
            _assignedId.Remove(id);
            _connectedCount--;
        }

        public static Client GetClient(int id) {
            return _clients[id];
        }

        public static void SendMessage(SpecialId id, string msg) { SendMessage((int)id, msg); }
        public static void SendMessage(int id, string msg) {
            if (id == (int)SpecialId.Null) {
                ConsoleServer.WriteLine($"The message \"{msg}\" target no client.", MessageType.Error);
                return;
            }
            if (id == (int)SpecialId.Server) {
                ConsoleServer.WriteLine(msg);
                return;
            }
            if (id == (int)SpecialId.Broadcast) {
                SendMessage((int)SpecialId.Server, msg);
                foreach (int clientId in _assignedId) {
                    SendMessage(clientId, msg);
                }
                return;
            }

            Packet packet = new Packet(id, "msg");
            packet.Write(msg);
            _clients[id].SendPacket(packet);
        }

        public static void Ping() {
            foreach (int id in _assignedId) {
                _clients[id].Ping();
            }
        }
    }
}
