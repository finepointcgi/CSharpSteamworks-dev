using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
namespace CSharpSteamworks.Networking
{
    public class PacketManager
    {
        public delegate void PacketHandler(uint senderId, Packet packet);
        public static event Action<Dictionary<string, string>> OnHostStartGame;
        public static event Action<Dictionary<string, string>> OnPlayerReady;
        public static event Action<Dictionary<string, string>> OnChatMessage;
        public static event Action<Dictionary<string, string>> OnUpdateReadyState;
        public static event Action<Dictionary<string, string>> OnUpdatePlayer;

        public static Dictionary<PacketTypes, PacketHandler> Handlers = new Dictionary<PacketTypes, PacketHandler>()
        {
            { PacketTypes.HostStartGame, HostStartGame },
            { PacketTypes.GuestReady, GuestReady },
            { PacketTypes.ChatMessage, ChatMessage },
            { PacketTypes.UpdateReadyState, UpdateReadyState },
            { PacketTypes.UpdatePlayerState, UpdatePlayerReadyState },
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
                    GD.Print($"Received unknown packet with id ({id}). Try parse failed.");
                }
            }
            catch(Exception e)
            {
                GD.Print($"Received unknown packet with id ({id}). " + e.Message);
            }
        }

        public static void HostStartGame(uint senderId, Packet packet)
        {
            // return here if host.

            if (!SteamManager.Instance.IsHost)
            {
                GD.Print($"Received a 'HostStartGame' packet.");
                Dictionary<string, string> StartGame = PacketIO.UnpackObject<Dictionary<string, string>>(packet);

                StartGame.Print();

                // send current player into game.
                OnHostStartGame.Invoke(StartGame);
            }
        }

        public static void GuestReady(uint senderId, Packet packet)
        {
            // return here if not host.

            GD.Print($"Received a 'GuestReady' packet.");

            Dictionary<string, string> ReadyPlayer = PacketIO.UnpackObject<Dictionary<string, string>>(packet);

            ReadyPlayer.Print();

            OnPlayerReady.Invoke(ReadyPlayer);
            // set ready state of this player.
        }


        public static void ChatMessage(uint senderId, Packet packet)
        {
            GD.Print($"Received a 'ChatMessage' packet.");

            Dictionary<string, string> message = PacketIO.UnpackObject<Dictionary<string, string>>(packet);

            // relay the message to other clients.
            SteamManager.Broadcast(PacketIO.PackObject(PacketTypes.ChatMessage, message));

            message.Print();

            OnChatMessage.Invoke(message);
        }

        public static void UpdateReadyState(uint senderId, Packet packet)
        {
            GD.Print($"Received a 'UpdateReadyState' packet.");

            if(!SteamManager.Instance.IsHost)
            {
                Dictionary<string, string> readyState = PacketIO.UnpackObject<Dictionary<string, string>>(packet);

                readyState.Print();

                OnUpdateReadyState.Invoke(readyState);
            }
        }

        public static void UpdatePlayerReadyState(uint senderId, Packet packet)
        {
            GD.Print($"Received a 'PlayerUpdate' packet.");

            Dictionary<string, string> playerStatus = PacketIO.UnpackObject<Dictionary<string, string>>(packet);

            // relay the message to other clients.
            SteamManager.Broadcast(PacketIO.PackObject(PacketTypes.UpdatePlayerState, playerStatus));

            playerStatus.Print();

            OnUpdatePlayer.Invoke(playerStatus);
        }
    }
}
