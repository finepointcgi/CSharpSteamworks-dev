using System;
using System.Collections.Generic;

namespace CSharpSteamworks.Networking
{
    public class PacketManager
    {
        public delegate void PacketHandler(int senderId, Packet packet);

        public static Dictionary<PacketTypes, PacketHandler> Handlers = new Dictionary<PacketTypes, PacketHandler>()
        {
            { PacketTypes.HostStartGame, HostStartGame },
            { PacketTypes.GuestReady, GuestReady }
        };

        public void Handle(int senderId, Packet packet)
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

        public static void HostStartGame(int senderId, Packet packet)
        {
            // return here if host.

            Console.WriteLine($"Received a 'HostStartGame' packet.");

            bool isReady = PacketIO.UnpackObject<bool>(packet);
            // send current player into game.
        }

        public static void GuestReady(int senderId, Packet packet)
        {
            // return here if not host.

            Console.WriteLine($"Received a 'GuestReady' packet.");

            bool isReady = PacketIO.UnpackObject<bool>(packet);
            // set ready state of this player.
        }
    }
}
