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
    // private string b = "tescxt";
    private SteamManager steamManager;
    [Export]
    public PackedScene LobbyElement;
    [Export]
    public PackedScene LobbyPlayer;
    [Export]
    public PackedScene SceneToLoad;
    [Export]
    public PackedScene Player;

    public static SceneManager manager;
    private bool playerReady;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        steamManager = GetNode<SteamManager>("Steam Manager");
       // steamManager.Connect("OnLobbyRefreshCompleted", this, "OnLobbyRefreshCompleted");
        SteamManager.OnLobbyRefreshCompleted += OnLobbyRefreshCompleted;
        SteamManager.OnPlayerJoinLobby += OnPlayerJoinLobby;
        SteamManager.OnPlayerLeftLobby += OnPlayerLeftLobby;
        PacketManager.OnHostStartGame += OnStartGame;
        PacketManager.OnPlayerReady += OnPlayerReady;
        PacketManager.OnChatMessage += OnChatMessage;
        manager = this;
        GameManager gameManager = new GameManager();

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

    private void _on_Send_Chat_Message_button_down()
    {
        LineEdit lineEdit = manager.GetNode("Control").GetNode<LineEdit>("LineEdit");

        if(lineEdit.Text == null || lineEdit.Text.Trim().Length == 0)
        {
            lineEdit.Text = string.Empty;
            return;
        }

        Dictionary<string, string> message = new Dictionary<string, string>()
        {
            { "playerId", steamManager.PlayerSteamId.AccountId.ToString() },
            { "playerName", steamManager.PlayerName },
            { "text", manager.GetNode("Control").GetNode<LineEdit>("LineEdit").Text }
        };

        if (steamManager.IsHost)
        {
            SteamManager.Broadcast(PacketIO.PackObject(PacketTypes.ChatMessage, message));
        }
        else 
        {
            SteamManager.steamConnectionManager.Connection.SendMessage(PacketIO.PackObject(PacketTypes.ChatMessage, message));
        }

        lineEdit.Text = string.Empty;

        if(steamManager.IsHost)
            OnChatMessage(message);
    }

    private void OnPlayerJoinLobby(Friend friend){
        var element = LobbyPlayer.Instance() as LobbyPlayer;
        element.Name = friend.Id.AccountId.ToString();
        element.SetPlayerInfo(friend.Name);
        GetNode<VBoxContainer>("Lobby").AddChild(element);
        GameManager.OnPlayerJoinedLobby(friend);
    }

    private void OnPlayerLeftLobby(Friend player){
        GetNode<LobbyPlayer>($"Lobby/{player.Id.AccountId.ToString()}").QueueFree();
        GameManager.OnPlayerLeftLobby(player);
    }

    private void _on_Ready_button_down(){
        playerReady = !playerReady;
        Dictionary<string, string> playerDict = new Dictionary<string, string>();
        playerDict.Add("playername", steamManager.PlayerSteamId.AccountId.ToString());
        playerDict.Add("isReady", playerReady ? "true" : "false");

        if (steamManager.IsHost){
           OnPlayerReady(playerDict);
         SteamManager.Broadcast(PacketIO.PackObject(PacketTypes.GuestReady, playerDict));
        }else{
            SteamManager.steamConnectionManager.Connection.SendMessage(PacketIO.PackObject(PacketTypes.GuestReady, playerDict));
        }
    }

    private void _on_LeaveLobby_button_down(){
        Lobby[] lobbies = SteamMatchmaking.LobbyList.WithMaxResults(20).RequestAsync().Result;
        if (lobbies != null)
        {
            foreach (Lobby lobby in lobbies.ToList())
            {
                lobby.Leave();
            }
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
        GetNode<LobbyPlayer>($"Lobby/{playerDict["playername"]}").SetReadyStatus(playerDict["isReady"] == "true" ? true : false);
        GD.Print(playerDict);
        GD.Print(playerDict["playername"]);
        GD.Print(playerDict["isReady"]);
        GameManager.OnPlayerReady(playerDict);
    }

    private void OnChatMessage(Dictionary<string, string> dict){
        GetNode<RichTextLabel>("Control/RichTextLabel").Text = GetNode<RichTextLabel>("Control/RichTextLabel").Text + System.Environment.NewLine + dict["playerName"] + ": " + dict["text"];
    }

    public void OnStartGame(Dictionary<string, string> dict)
    {
        string scene = dict["sceneName"];

        // send self into game
        // PackedScene instance with name "scene"
        var s = SceneToLoad.Instance();
        GetTree().Root.AddChild(s);
        foreach(var currentPlayer in GameManager.CurrentPlayers){
            var p = Player.Instance() as Player;
            s.AddChild(p);
            p.Name = currentPlayer.FriendData.Id.AccountId.ToString();
            if(p.Name == SteamManager.Instance.PlayerSteamId.AccountId.ToString()){
                p.Controlled = true;
            }

        }
    }
}
