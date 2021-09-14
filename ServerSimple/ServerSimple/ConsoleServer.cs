using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerSimple {
    enum MessageType {
        Normal = ConsoleColor.White,
        Warning = ConsoleColor.Yellow,
        Error = ConsoleColor.Red,
        Success = ConsoleColor.Green,
        Debug = ConsoleColor.DarkGray,
        Packet = ConsoleColor.DarkMagenta
    }

    class ConsoleServer {
        private const string ReadPrefix = "> ";

        public static bool ShowDebug { get; set; } = true;
        public static bool ShowListenPacket { get; set; } = false;
        public static bool ShowWarning { get; set; } = true;

        private static bool _isReadingLine = false;
        private static object writer = new object();

        public static string ToMessageFormat(string pseudo, string msg) {
            return $"[{pseudo}] {msg}";
        }

        public static void WriteLine(string msg) { WriteLine(msg, (ConsoleColor)MessageType.Normal); }
        public static void WriteLine(string msg, MessageType color) {
            if (color == MessageType.Debug && !ShowDebug 
                || color == MessageType.Packet && !ShowListenPacket
                || color == MessageType.Warning && !ShowWarning
                ) return;
            WriteLine(msg, (ConsoleColor)color);
        }
        public static void WriteLine(string msg, ConsoleColor color) {
            lock (writer) {
                if (_isReadingLine) RemoveCharacters(ReadPrefix.Length);

                Console.ForegroundColor = color;
                Console.WriteLine(msg);
                Console.ForegroundColor = ConsoleColor.White;

                if (_isReadingLine) Console.Write(ReadPrefix);
            }
        }

        public static string ReadLine() {
            lock (writer) {
                _isReadingLine = true;
                Console.Write(ReadPrefix);
            }
            string res = Console.ReadLine();
            _isReadingLine = false;
            return res;
        }

        public static void RemoveCharacters(int count) {
            for (int i = 0; i < count; i++) Console.Write("\b \b");
        }
    }
}
