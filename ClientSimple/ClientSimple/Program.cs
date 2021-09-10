using System;
using System.Threading;

namespace ClientSimple {
    class Program {
        private static Client _client = Client.Instance;

        static void Main(string[] args) {
            Console.Title ="Test Client";

            Test.Start();
            Connection();

            InputHandler.StartReadLines();
        }

        private static void Connection() {
            ConsoleServer.WriteLine("Write your pseudo : ");
            string pseudo = ConsoleServer.ReadLine();
            ConsoleServer.WriteLine("Write the host ip adress (127.0.0.1 by default) : ");
            string ip = ConsoleServer.ReadLine();
            if (ip == "") ip = "127.0.0.1";
            _client.Connect(ip, 26950, pseudo);
        }
    }
}
