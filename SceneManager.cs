using Godot;
using System;
using System.Collections.Generic;
using Steamworks.Data;
using Steamworks;

public class SceneManager : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    private SteamManager steamManager;
    [Export]
    public PackedScene LobbyElement;
    [Export]
    public PackedScene LobbyPlayer;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        steamManager = GetNode<SteamManager>("Steam Manager");
       // steamManager.Connect("OnLobbyRefreshCompleted", this, "OnLobbyRefreshCompleted");
        SteamManager.OnLobbyRefreshCompleted += OnLobbyRefreshCompleted;
        SteamManager.OnPlayerJoinLobby += OnPlayerJoinLobby;
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

    public void _on_Button_button_down(){
        steamManager.CreateLobby();
    }

    public void _on_Button2_button_down(){
        _ = steamManager.RefreshMultiplayerLobbies();
    }
    public void _on_Button3_button_down(){
        steamManager.OpenFriendOverlayForGameInvite();
    }

    private void OnLobbyRefreshCompleted(List<Lobby> lobbies){
        foreach (var lobby in lobbies)
        {
            var element = LobbyElement.Instance() as LobbyElement;
            element.SetLabels(lobby.Id.ToString(), lobby.Owner.Name);
            element.lobby = lobby;
            GetNode<VBoxContainer>("VBoxContainer").AddChild(element);
        }
    }

    private void _on_Send_Chat_Message_button_down(){
        
    }

    private void OnPlayerJoinLobby(string friend){
        var element = LobbyPlayer.Instance() as LobbyPlayer;
        element.SetPlayerInfo(friend);
        GetNode<VBoxContainer>("Lobby").AddChild(element);
    }
}
