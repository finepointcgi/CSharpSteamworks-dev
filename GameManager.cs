using Godot;
using System;
using System.Collections.Generic;
using Steamworks;
public class GameManager : Node
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    public static GameManager Manager;
    private List<Friend> CurrentPlayers;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GameManager.Manager = this;
        SteamManager.OnPlayerJoinLobby += OnPlayerJoinedLobby;
        SteamManager.OnPlayerLeftLobby += OnPlayerLeftLobby;
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

    public void OnPlayerJoinedLobby(Friend player){
        CurrentPlayers.Add(player);
    }

    public void OnPlayerLeftLobby(Friend player){
        CurrentPlayers.Remove(player);
    }
}
