using System;
using System.Threading;

namespace ServerSimple {
    class Program {
        static void Main(string[] args) {
            Console.Title = "Test Server";

            Server.Start();

            InputHandler.StartReadLines();
        }
    }
}
