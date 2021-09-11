using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSimple {
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

        public static bool Debug { get; set; } = true;
        public static bool ListenPacket { get; set; } = true;

        private static bool _isReadingLine = false;

        public static string ToMessageFormat(string pseudo, string msg) {
            return $"[{pseudo}] {msg}";
        }

        public static void WriteLine(string pseudo, string msg) {
            WriteLine($"[{pseudo}] {msg}", (ConsoleColor)MessageType.Normal);
        }
        public static void WriteLine(string msg) { WriteLine(msg, (ConsoleColor)MessageType.Normal); }
        public static void WriteLine(string msg, MessageType color) {
            if (color == MessageType.Debug && !Debug || color == MessageType.Packet && !ListenPacket) return;
            WriteLine(msg, (ConsoleColor)color);
        }
        public static void WriteLine(string msg, ConsoleColor color) {
            if (_isReadingLine) RemoveCharacters(ReadPrefix.Length);

            Console.ForegroundColor = color;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.White;

            if (_isReadingLine) Console.Write(ReadPrefix);
        }

        public static string ReadLine() {
            _isReadingLine = true;
            Console.Write(ReadPrefix);
            string res = Console.ReadLine();
            _isReadingLine = false;
            return res;
        }

        public static void RemoveCharacters(int count) {
            for (int i = 0; i < count; i++) Console.Write("\b \b");
        }
    }
}
