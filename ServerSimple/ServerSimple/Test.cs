using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace ServerSimple {
    class Test {
        public static void Start() {
            ConsoleServer.WriteLine("TEST");
            Database db = Database.Instance;
            Packet original = new Packet((ushort)SpecialId.Server, (ushort)SpecialId.Broadcast, "potato");
            original.Write(10);
            db.Set(original);

            Packet c1 = db[(ushort)SpecialId.Server, "potato"];
            ConsoleServer.WriteLine($"c1 int : {c1.ReadInt()}");
            Packet c2 = db[(ushort)SpecialId.Server, "potato"];
            ConsoleServer.WriteLine($"c2 int : {c2.ReadInt()}");

            ConsoleServer.WriteLine($"found 1 : {db.TryGet((ushort)SpecialId.Server, "potato", out _)}");
            ConsoleServer.WriteLine($"found 2 : {db.TryGet((ushort)SpecialId.Server, "nfofoes", out _)}");
            ConsoleServer.WriteLine($"found 3 : {db.TryGet(42, "potato", out _)}");

            ConsoleServer.WriteLine("TEST 2");
            ConsoleServer.WriteLine($"array 1 : {Helper.ArrayToString(original.ToArray())}");
            original.TargetId = 1;
            ConsoleServer.WriteLine($"array 2 : {Helper.ArrayToString(original.ToArray())}");
            original.SenderId = 1;
            ConsoleServer.WriteLine($"array 3 : {Helper.ArrayToString(original.ToArray())}");

            ConsoleServer.WriteLine("FIN TEST");
        }
    }
}
