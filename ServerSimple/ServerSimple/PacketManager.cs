using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerSimple {
    class PacketManager {
        private byte[] _dataPart;

        public Packet[] GetPackets(byte[] data) {
            List<Packet> packets = new List<Packet>();
            if (data.Length == 0) {
                packets.Add(new Packet(SpecialId.Server, "disconnect"));
                return packets.ToArray();
            }

            int cursor = 0;
            if (_dataPart != null) {
                // Complete dataPart
                //ConsoleServer.WriteLine($"Completing data part {Helper.ArrayToString(_dataPart)}");

                byte[] lengthRaw = new byte[4];
                for (int i = 0; i < 4; i++) {
                    if (i < _dataPart.Length) lengthRaw[i] = _dataPart[i];
                    else lengthRaw[i] = data[i - _dataPart.Length];
                }

                int length = BitConverter.ToInt32(lengthRaw, 0) + 4;
                if (length > Client.BufferSize) throw new NotSupportedException($"Can't manage packet of more than {Client.BufferSize} bytes.");
                int missingLength = length - _dataPart.Length;
                byte[] dataPacket = new byte[length];
                Array.Copy(_dataPart, dataPacket, _dataPart.Length);
                Array.Copy(data, 0, dataPacket, _dataPart.Length, missingLength);
                packets.Add(new Packet(dataPacket));

                _dataPart = null;
                cursor += missingLength;
            }

            byte[] packetData;
            while (cursor < data.Length) {
                if (data.Length - cursor >= 4) {
                    int size = BitConverter.ToInt32(data, cursor) + 4; // Converts bytes to int
                    if (data.Length - cursor >= size) {
                        packetData = new byte[size];
                        Array.Copy(data, cursor, packetData, 0, size);
                        // ConsoleServer.WriteLine(Helper.ArrayToString(packetData), MessageType.Debug);
                        packets.Add(new Packet(packetData));
                        cursor += size;
                    } else { // The data left is not enough to create one full Packet.
                        StoreLeftData(data, cursor);
                        break;
                    }
                } else { // The data left is not enough to create at least the length of the Packet.
                    StoreLeftData(data, cursor);
                    break;
                }
            }

            return packets.ToArray();
        }

        private void StoreLeftData(byte[] data, int cursor) {
            int sizeLeft = data.Length - cursor;
            _dataPart = new byte[sizeLeft];
            Array.Copy(data, cursor, _dataPart, 0, sizeLeft);
        }
    }
}
