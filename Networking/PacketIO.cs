using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CSharpSteamworks.Networking
{
    public class PacketIO
    {
        public static T UnpackObject<T>(Packet packet)
        {
            string json = packet.ReadString();
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static byte[] PackObject<T>(PacketTypes type, T obj)
        {
            Packet packet = new Packet((int)type);
            packet.WriteString(JsonConvert.SerializeObject(obj, Formatting.None));
            return packet.ToArray();
        }
    }
}
