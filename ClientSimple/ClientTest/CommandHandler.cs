using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSimple {
    class CommandHandler {
        public delegate void Command();
        public static string[] CommandsList { get => _commands.Keys.ToArray(); }
        
        private static bool _isInitialized = false;
        private static Dictionary<string, Command> _commands = null;

        /// <summary>Must be called once at the start of the program to use this class.</summary>
        public static void InitCommand() {
            if (!_isInitialized) {
                _commands = new Dictionary<string, Command>();

                // Here we initialize all default commands.
                _commands.Add("ping", () => {
                    ConsoleServer.WriteLine($"Ping sent to server...");
                    Client.Instance.Ping();
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

            if (_commands.ContainsKey(command)) {
                _commands[command]();
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
