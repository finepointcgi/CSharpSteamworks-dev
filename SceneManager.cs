using Godot;
using System;
using System.Collections.Generic;
using Steamworks.Data;
using Steamworks;
using System.Linq;
using CSharpSteamworks.Networking;

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

    public static SceneManager manager;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        steamManager = GetNode<SteamManager>("Steam Manager");
       // steamManager.Connect("OnLobbyRefreshCompleted", this, "OnLobbyRefreshCompleted");
        SteamManager.OnLobbyRefreshCompleted += OnLobbyRefreshCompleted;
        SteamManager.OnPlayerJoinLobby += OnPlayerJoinLobby;
        PacketManager.OnPlayerReady += OnPlayerReady;
        manager = this;
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
        ClearLobbyList();
        _ = steamManager.RefreshMultiplayerLobbies();
    }
    public void _on_Button3_button_down(){
        steamManager.OpenFriendOverlayForGameInvite();
    }

    private void OnLobbyRefreshCompleted(List<Lobby> lobbies){
        foreach (var lobby in lobbies)
        {
            var element = LobbyElement.Instance() as LobbyElement;
            element.SetLabels(lobby.Id.ToString(), lobby.GetData("ownerNameDataString"));
            element.lobby = lobby;
            GetNode<VBoxContainer>("VBoxContainer").AddChild(element);
        }
    }

    private void _on_Send_Chat_Message_button_down(){
        if(steamManager.IsHost){
         SteamManager.steamConnectionManager.SendMessages(SteamManager.steamSocketManager.Connected.ToArray(),
                SteamManager.steamSocketManager.Connected.Count, "test");
        }else{
            SteamManager.steamConnectionManager.Connection.SendMessage("test");
        }
    }

    private void OnPlayerJoinLobby(string friend){
        var element = LobbyPlayer.Instance() as LobbyPlayer;
        element.Name = friend;
        element.SetPlayerInfo(friend);
        GetNode<VBoxContainer>("Lobby").AddChild(element);
    }

    private void _on_Ready_button_down(){
        
        Dictionary<string, string> playerDict = new Dictionary<string, string>();
        playerDict.Add("playername", steamManager.PlayerSteamId.Value.ToString());
        playerDict.Add("isReady", "true");

        if (steamManager.IsHost){
           
         SteamManager.steamConnectionManager.SendMessages(SteamManager.steamSocketManager.Connected.ToArray(),
                SteamManager.steamSocketManager.Connected.Count, PacketIO.PackObject(PacketTypes.HostStartGame, playerDict));
        }else{
            SteamManager.steamConnectionManager.Connection.SendMessage(PacketIO.PackObject(PacketTypes.GuestReady, playerDict));
        }
    }

    private void _on_LeaveLobby_button_down(){
        foreach (var item in steamManager.activeUnrankedLobbies)
        {
            item.Leave();
        }
        ClearLobby();
    }

    public void ClearLobby(){
        foreach (Node item in GetNode<VBoxContainer>("Lobby").GetChildren())
        {
            item.QueueFree();
        }
    }

    public void ClearLobbyList(){
        foreach (Node item in GetNode<VBoxContainer>("VBoxContainer").GetChildren())
        {
            item.QueueFree();
        }
    }

    private void OnPlayerReady(Dictionary<string, string> playerDict)
    {
        Console.WriteLine(playerDict);
        Console.WriteLine(playerDict["playername"]);
        Console.WriteLine(playerDict["isReady"]);
    }
}
