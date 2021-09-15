using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSimple {
    /// <summary>Manage commands.</summary>
    class CommandHandler {
        public static string[] CommandsList { get => _commands.Keys.ToArray(); }
        
        private static bool _isInitialized = false;
        private static Dictionary<string, Command> _commands = null;

        /// <summary>Must be called once at the start of the program to use this class.</summary>
        public static void InitCommand() {
            if (!_isInitialized) {
                _commands = new Dictionary<string, Command>();
                Client client = Client.Instance;
                
                // Here we initialize all default commands.
                Command command;

                command = new Command("help",
                    "help command\n\n" +
                    "Display the description of a specified command.\n" +
                    "Try another command than help as an argument now :).\n\n" +
                    "- command : The command from which you want the description.",
                    (string[] args) => {
                        if (args.Length > 1) {
                            ConsoleServer.WriteLine("Too much arguments. Need only 1 argument.", MessageType.Error);
                            return;
                        }
                        if (args.Length == 0) {
                            ConsoleServer.WriteLine(
                                $"########################################\n" +
                                $"{_commands["help"].Description}\n" +
                                $"########################################");
                            return;
                        }
                        if (!_commands.ContainsKey(args[0])) {
                            ConsoleServer.WriteLine($"The command {args[0]} does not exist.", MessageType.Error);
                            return;
                        }
                        ConsoleServer.WriteLine(
                            $"########################################\n" +
                            $"{_commands[args[0]].Description}\n" +
                            $"########################################");
                    });
                _commands.Add(command.Name, command);

                command = new Command("exit",
                    "exit\n\n" +
                    "Disconnect from the server.",
                    (string[] args) => {
                    if (args.Length > 2) {
                        ConsoleServer.WriteLine("Too much arguments. \"exit\" has no arguments.", MessageType.Error);
                        return;
                    }
                    ConsoleServer.WriteLine($"Disconnected from server.", MessageType.Success);
                    client.Disconnect();
                });
                _commands.Add(command.Name, command);

                command = new Command("msg",
                    "msg targetClient text\n\n" +
                    "Send a message private to the targetClient.\n\n" +
                    "- targetClient : Indicate here the pseudo of the player with which you want to chat.",
                    (string[] args) => {
                        if (args.Length < 2) {
                            ConsoleServer.WriteLine("Missing arguments. The command must be \"msg targetName text\".", MessageType.Error);
                            return;
                        }
                        if (!IdHandler.ClientExist(args[0])) {
                            ConsoleServer.WriteLine($"The client \"{args[0]}\" does not exist.", MessageType.Error);
                            return;
                        }
                        client.SendMessage(
                            IdHandler.NameToId(args[0]),
                            $"(whisper to {args[0]}) {string.Join(" ", args, 1, args.Length - 1)}"
                            );
                    });
                _commands.Add(command.Name, command);

                command = new Command("ping",
                    "ping [targetClient]\n\n" +
                    "Send a ping to server to test the speed of the connection.\n\n" +
                    "- targetClient : name of the client to ping.",
                    (string[] args) => {
                        try {
                            if (args.Length == 0) {
                                ConsoleServer.WriteLine($"Ping sent to server...");
                                client.Ping();
                            } else if (args.Length == 1) {
                                if (IdHandler.ClientExist(args[0])) {
                                    ConsoleServer.WriteLine($"Ping sent to {args[0]}...");
                                    client.Ping(IdHandler.NameToId(args[0]));
                                } else {
                                    ConsoleServer.WriteLine($"The client \"{args[0]}\" does not exist.", MessageType.Error);
                                }
                            } else {
                                ConsoleServer.WriteLine("Too much arguments.", MessageType.Error);
                            }

                        } catch {
                            ConsoleServer.WriteLine("Invalid command.", MessageType.Error);
                        }
                    });
                _commands.Add(command.Name, command);

                command = new Command("spam",
                    "spam count\n\n" +
                    "Send several packet to test the capacity of the server to handle massive amount of data. Only for test.\n" +
                    "Pls don't use this command (°-°).\n\n" +
                    "- count : The number of packet to send.",
                    (string[] args) => {
                        try {
                            if (args.Length == 0) {
                                ConsoleServer.WriteLine($"Missing argument : add the number of spam to send.", MessageType.Error);
                                return;
                            }
                            if (args.Length == 1) {
                                int count = int.Parse(args[0]);
                                Packet packet = new Packet(SpecialId.Server, "spam");
                                for (int i = 0; i < count; i++) { client.SendPacket(packet); }
                                return;
                            }
                            ConsoleServer.WriteLine("Too much arguments.", MessageType.Error);
                        }
                        catch { ConsoleServer.WriteLine("Invalid command.", MessageType.Error); }
                    });
                _commands.Add(command.Name, command);

                ConsoleServer.WriteLine("Commands initialized.", MessageType.Debug);
                _isInitialized = true;
            } else {
                throw new NotSupportedException("Can't initialize commands twice.");
            }
        }

        /// <summary>Execute the named command.</summary>
        /// <returns>True if the command exist.</returns>
        public static bool ExecuteCommand(string command) {
            if (!_isInitialized) InitCommand();

            string[] words = command.Split(' ');
            string commandName = words[0].ToLower();
            string[] args = new string[words.Length - 1];
            Array.Copy(words, 1, args, 0, args.Length);

            if (_commands.ContainsKey(commandName)) {
                try { _commands[commandName].Execute(args); } catch { ConsoleServer.WriteLine($"An unknown error occured with the command \"{command}\".", MessageType.Error); }
                return true;
            }

            return false;
        }

        public static void AddCommand(string name, Command command) {
            if (!_isInitialized) InitCommand();
            if (_commands.ContainsKey(name)) throw new ArgumentException($"The command \"{name}\" already exist.");
            _commands.Add(name, command);
        }
    }
}
