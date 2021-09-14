using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;

namespace ClientSimple {
    class Test {
        public static void Start() {
            Client client = Client.Instance;
            /*string line = Console.ReadLine();
            string[] words = line.Split(' ');
            string[] args = new string[words.Length - 1];
            Array.Copy(words, 1, args, 0, args.Length);
            ConsoleServer.WriteLine($"args found : {Helper.ArrayToString(args)}");*/

            /*Thread threadTest = new Thread(new ThreadStart(() => {
                Thread.Sleep(5000);
                client.Query(SpecialId.Server, "potato");
                client.Query(SpecialId.Server, "bl");
            }));
            threadTest.Start();*/
        }
    }
}
