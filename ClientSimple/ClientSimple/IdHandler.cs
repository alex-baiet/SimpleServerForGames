using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSimple {
    class IdHandler {

        private static Dictionary<ushort, string> _idNames = new Dictionary<ushort, string>() {
            { (ushort)SpecialId.Broadcast, "everyone" },
            { (ushort)SpecialId.Server, "server" }
        };

        private static Dictionary<string, ushort> _namesId = new Dictionary<string, ushort>() {
            { "everyone", (ushort)SpecialId.Broadcast },
            { "server", (ushort)SpecialId.Server }
        };

        public static bool ClientExist(string name) { return _namesId.ContainsKey(name); }
        public static bool ClientExist(ushort id) { return _idNames.ContainsKey(id); }

        public static string IdToName(ushort id) { return _idNames[id]; }
        public static ushort NameToId(string name) { return _namesId[name]; }

        public static void AddIdName(ushort id, string name) {
            _idNames.Add(id, name);
            _namesId.Add(name, id);
        }
    }
}
