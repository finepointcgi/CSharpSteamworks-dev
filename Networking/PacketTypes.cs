using System;
using System.Collections.Generic;

namespace CSharpSteamworks.Networking
{
    public enum PacketTypes
    {
        HostStartGame = 0,
        GuestReady = 1,
        ChatMessage = 2,
        UpdateReadyState = 3,
        UpdatePlayerState = 4,
    }
}
