using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ClientSimple {
    class InputHandler {
        /// <summary>Start reading lines entered by the user.</summary>
        public static void StartReadLines() {
            Thread thread = new Thread(new ThreadStart(ReadLines));
            thread.Start();
        }

        private static void ReadLines() {
            Client client = Client.Instance;
            while (true) {
                string input = ConsoleServer.ReadLine();

                if (input == "") continue;
                if (input == "exit") {
                    // Disconnecting
                    client.Disconnect();
                    Environment.Exit(0);
                    continue;
                }
                if (input.StartsWith("/")) {
                    string command = input.Substring(1, input.Length - 1);
                    if (!CommandHandler.ExecuteCommand(command)) {
                        ConsoleServer.WriteLine($"The command \"{command}\" does not exist.", MessageType.Error);
                    }
                    continue;
                }

                // Sending a global message.
                client.SendMessage(SpecialId.Broadcast, input);
            }
        }
    }
}
