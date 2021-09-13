using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerSimple {
    class Database {
        public static Database Instance = new Database();
        
        public Packet this[ushort id, string name] { 
            get {
                if (!database.ContainsKey(id) || !database[id].ContainsKey(name)) {
                    throw new KeyNotFoundException($"Trying to access inexisting data (id:{id}, name:{name}).");
                }
                return new Packet(database[id][name]);
            }
        }

        private Dictionary<ushort, Dictionary<string, Packet>> database = new Dictionary<ushort, Dictionary<string, Packet>>();

        private Database() { }

        /// <summary>Add the packet in the database.</summary>
        /// <remarks>If a packet with the same index exist, it will be override.</remarks>
        public void Set(Packet packet) { Set(packet.SenderId, packet.Name, packet); }
        public void Set(ushort id, string name, Packet packet) {
            packet.WriteLength();
            if (!database.ContainsKey(id)) database.Add(id, new Dictionary<string, Packet>());
            if (!database[id].ContainsKey(name)) database[id].Add(name, packet);
            else database[id][name] = packet;
        }

        public bool TryGet(ushort id, string name, out Packet packet) {
            try {
                packet = this[id, name];
                return true;
            } catch (KeyNotFoundException) {
                packet = null;
                return false;
            }
        }
    }
}
