using Godot;
using System;
using System.Collections.Generic;
using Steamworks;
using System.Linq;
using CSharpSteamworks.Networking;

public class GameManager : Node
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    public static GameManager Manager;
    private List<Player> CurrentPlayers;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GameManager.Manager = this;
        SteamManager.OnPlayerJoinLobby += OnPlayerJoinedLobby;
        SteamManager.OnPlayerLeftLobby += OnPlayerLeftLobby;
        PacketManager.OnPlayerReady += OnPlayerReady;
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

    public void OnPlayerJoinedLobby(Friend player){
        Player p  = new Player();
        p.FriendData = player;
        CurrentPlayers.Add(p);
    }

    public void OnPlayerLeftLobby(Friend player){
        CurrentPlayers.Remove(CurrentPlayers.Where(x => x.FriendData.Id.Value == player.Id.Value).FirstOrDefault());
    }


    public void OnPlayerReady(Dictionary<string, string> dict){
        Player currentFriend = CurrentPlayers.Where(x => x.FriendData.Id.Value.ToString() == dict["playername"]).FirstOrDefault();
        currentFriend.isReady = dict["isReady"] == "True" ? true : false;
        if(SteamManager.Instance.IsHost){
            SteamManager.Broadcast(PacketIO.PackObject(PacketTypes.GuestReady, dict));
            if( CurrentPlayers.Count(x => x.isReady) == CurrentPlayers.Count){
                SteamManager.Broadcast(PacketIO.PackObject(PacketTypes.HostStartGame, new Dictionary<string, string>()));
            }
        }
    }
}
