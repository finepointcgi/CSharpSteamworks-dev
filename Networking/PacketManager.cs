using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpSteamworks.Networking
{
    public class PacketManager
    {
        public delegate void PacketHandler(uint senderId, Packet packet);
        public static event Action<Dictionary<string, string>> OnPlayerReady;
        public static event Action<Dictionary<string, string>> OnChatMessage;

        public static Dictionary<PacketTypes, PacketHandler> Handlers = new Dictionary<PacketTypes, PacketHandler>()
        {
            { PacketTypes.HostStartGame, HostStartGame },
            { PacketTypes.GuestReady, GuestReady },
            { PacketTypes.ChatMessage, ChatMessage }
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
                    Console.WriteLine($"Received unknown packet with id ({id}). Try parse failed.");
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"Received unknown packet with id ({id}). " + e.Message);
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

        public static void ChatMessage(uint senderId, Packet packet)
        {
            Console.WriteLine($"Received a 'ChatMessage' packet.");

            Dictionary<string, string> message = PacketIO.UnpackObject<Dictionary<string, string>>(packet);

            if (SteamManager.Instance.IsHost)
            {
                // relay the message to other clients.
                SteamManager.Broadcast(PacketIO.PackObject(PacketTypes.ChatMessage, message));
            }

            Console.WriteLine($"{message["playerName"]} ({message["playerId"]}): {message["text"]}");

            OnChatMessage.Invoke(message);
        }
    }
}
