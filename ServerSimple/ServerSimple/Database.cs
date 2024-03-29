﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerSimple {
    /// <summary>Contains packets sends by clients</summary>
    class Database {
        public static readonly Database Instance = new Database();
        
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
        /// <summary>Add the packet in the database.</summary>
        /// <remarks>If a packet with the same index exist, it will be override.</remarks>
        public void Set(ushort id, string name, Packet packet) {
            packet.WriteLength();
            if (!database.ContainsKey(id)) database.Add(id, new Dictionary<string, Packet>());
            if (!database[id].ContainsKey(name)) database[id].Add(name, packet);
            else database[id][name] = packet;
        }

        /// <summary>Return the contained packet, or false if it does not exist.</summary>
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
