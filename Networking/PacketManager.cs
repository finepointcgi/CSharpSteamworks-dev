using System;
using System.Collections.Generic;

namespace CSharpSteamworks.Networking
{
    public class PacketManager
    {
        public delegate void PacketHandler(uint senderId, Packet packet);
        public static event Action<Dictionary<string, string>> OnPlayerReady;

        public static Dictionary<PacketTypes, PacketHandler> Handlers = new Dictionary<PacketTypes, PacketHandler>()
        {
            { PacketTypes.HostStartGame, HostStartGame },
            { PacketTypes.GuestReady, GuestReady }
        };

        public static void Handle(uint senderId, Packet packet)
        {
            int id = packet.ReadInt();

            try
            {
                if(Handlers.TryGetValue((PacketTypes)id, out PacketHandler handler))
                {
                    handler.Invoke(senderId, packet);
                }
                else
                {
                    Console.WriteLine($"Received unknown packet with id ({id}).");
                }
            }
            catch(Exception)
            {
                Console.WriteLine($"Received unknown packet with id ({id}).");
            }
        }

        public static void HostStartGame(uint senderId, Packet packet)
        {
            // return here if host.

            if (!SteamManager.Instance.IsHost)
            {
                Console.WriteLine($"Received a 'HostStartGame' packet.");

                Dictionary<string, string> isReady = PacketIO.UnpackObject<Dictionary<string, string>>(packet);
            }
            else
            {
                Console.WriteLine("Recieved host packet from non host!");
            }
            
            // send current player into game.
        }

        public static void GuestReady(uint senderId, Packet packet)
        {
            // return here if not host.

            Console.WriteLine($"Received a 'GuestReady' packet.");

            Dictionary<string, string> ReadyPlayer = PacketIO.UnpackObject<Dictionary<string, string>>(packet);
            OnPlayerReady.Invoke(ReadyPlayer);
            // set ready state of this player.
        }
    }
}
