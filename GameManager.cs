using Godot;
using System;
using System.Collections.Generic;
using Steamworks;
using System.Linq;
using CSharpSteamworks.Networking;

public class GameManager
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    public static GameManager Manager;
    public static List<Player> CurrentPlayers = new List<Player>();

    // Called when the node enters the scene tree for the first time.
    public GameManager()
    {
        GameManager.Manager = this;
        //SteamManager.OnPlayerJoinLobby += OnPlayerJoinedLobby;
        //SteamManager.OnPlayerLeftLobby += OnPlayerLeftLobby;
        //PacketManager.OnPlayerReady += OnPlayerReady;
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

    public static void OnPlayerJoinedLobby(Friend player){
        Player p  = new Player();
        p.FriendData = player;
        CurrentPlayers.Add(p);
    }

    public static void OnPlayerLeftLobby(Friend player){
        CurrentPlayers.Remove(CurrentPlayers.Where(x => x.FriendData.Id.AccountId == player.Id.AccountId).FirstOrDefault());
    }


    public static void OnPlayerReady(Dictionary<string, string> dict){
        var v = CurrentPlayers;
        Player currentFriend = CurrentPlayers.Where(x => x.FriendData.Id.AccountId.ToString() == dict["playername"]).FirstOrDefault();
        currentFriend.isReady = dict["isReady"] == "true" ? true : false;
        if(SteamManager.Instance.IsHost){
            SteamManager.Broadcast(PacketIO.PackObject(PacketTypes.GuestReady, dict));

            if( CurrentPlayers.Count(x => x.isReady) == CurrentPlayers.Count)
            {
                // send self into game
                // PackedScene instance with name "sceneName"
                string scene = "GameScene";

                Dictionary<string, string> sceneData = new Dictionary<string, string>()
                {
                    { "sceneName", scene }
                };

                SceneManager.manager.OnStartGame(sceneData);

                // send others into game
                SteamManager.Broadcast(PacketIO.PackObject(PacketTypes.HostStartGame, sceneData));
            }
        }
    }
}
