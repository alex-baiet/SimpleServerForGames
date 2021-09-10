using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSimple {
    class CommandHandler {
        public delegate void Command(string[] args);
        public static string[] CommandsList { get => _commands.Keys.ToArray(); }
        
        private static bool _isInitialized = false;
        private static Dictionary<string, Command> _commands = null;

        /// <summary>Must be called once at the start of the program to use this class.</summary>
        public static void InitCommand() {
            if (!_isInitialized) {
                _commands = new Dictionary<string, Command>();
                Client client = Client.Instance;

                // Here we initialize all default commands.
                _commands.Add("msg", (string[] args) => {
                    if (args.Length < 2) {
                        ConsoleServer.WriteLine("Missing arguments. The command must be \"msg targetName text\".", MessageType.Error);
                        return;
                    }
                    try {
                        client.SendMessage(IdHandler.NameToId(args[0]), string.Join(" ", args, 1, args.Length-1));
                    } catch {
                        ConsoleServer.WriteLine("Invalid command.", MessageType.Error);
                    }
                });

                _commands.Add("ping", (string[] args) => {
                    ConsoleServer.WriteLine($"Ping sent to server...");
                    client.Ping();
                });

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
            string[] args = new string[words.Length - 1];
            Array.Copy(words, 1, args, 0, args.Length);

            if (_commands.ContainsKey(words[0])) {
                _commands[words[0]](args);
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
