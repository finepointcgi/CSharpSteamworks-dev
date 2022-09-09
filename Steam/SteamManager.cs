﻿
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class SteamManager : Node
{
    public static SteamManager Instance { get; set; }
    private static uint gameAppId { get; set; } = 2145350;
    public string PlayerName { get; set; }
    public SteamId PlayerSteamId { get; set; }
    private string playerSteamIdString { get; set; }
    public string PlayerSteamIdString { get => playerSteamIdString; }
    private bool connectedToSteam { get; set; } = false;
    private Friend lobbyPartner { get; set; }
    public Friend LobbyPartner { get => lobbyPartner; set => lobbyPartner = value; }
    public SteamId OpponentSteamId { get; set; }
    public bool LobbyPartnerDisconnected { get; set; }
    public List<Lobby> activeUnrankedLobbies { get; set; }
    public List<Lobby> activeRankedLobbies { get; set; }
    public Lobby currentLobby { get; set; }
    private Lobby hostedMultiplayerLobby { get; set; }
    private bool applicationHasQuit { get; set; } = false;
    private bool daRealOne { get; set; } = false;
    public string PlayerEloDataString { get; set; } = "";
    public int PlayerElo { get; set; } = 0;

    public SteamManager()
    {
        if (Instance == null)
        {
            daRealOne = true;
            
            Instance = this;
            PlayerName = "";
            try
            {
                // Create client
                SteamClient.Init(gameAppId, true);

                if (!SteamClient.IsValid)
                {
                    Console.WriteLine("Steam client not valid");
                    throw new Exception();
                }

                PlayerName = SteamClient.Name;
                PlayerSteamId = SteamClient.SteamId;
                playerSteamIdString = PlayerSteamId.ToString();
                activeUnrankedLobbies = new List<Lobby>();
                activeRankedLobbies = new List<Lobby>();
                connectedToSteam = true;
                Console.WriteLine("Steam initialized: " + PlayerName);
            }
            catch (Exception e)
            {
                connectedToSteam = false;
                playerSteamIdString = "NoSteamId";
                Console.WriteLine("Error connecting to Steam");
                Console.WriteLine(e);
            }
        }
    }

    internal void RelaySocketMessageReceived(IntPtr data, int size, uint id)
    {
        throw new NotImplementedException();
    }

    public bool TryToReconnectToSteam()
    {
        Console.WriteLine("Attempting to reconnect to Steam");
        try
        {
            // Create client
            SteamClient.Init(gameAppId, true);

            if (!SteamClient.IsValid)
            {
                Console.WriteLine("Steam client not valid");
                throw new Exception();
            }

            PlayerName = SteamClient.Name;
            PlayerSteamId = SteamClient.SteamId;
            activeUnrankedLobbies = new List<Lobby>();
            activeRankedLobbies = new List<Lobby>();
            Console.WriteLine("Steam initialized: " + PlayerName);
            connectedToSteam = true;
            return true;
        }
        catch (Exception e)
        {
            connectedToSteam = false;
            Console.WriteLine("Error connecting to Steam");
            Console.WriteLine(e);
            return false;
        }
    }

    internal void ProcessMessageFromSocketServer(IntPtr data, int size)
    {
        throw new NotImplementedException();
    }

    public bool ConnectedToSteam()
    {
        return connectedToSteam;
    }


    public override void _Ready()
    {
        base._Ready();

        // Update Steam with achievements if was offline last play session
        //ReconcileMissedAchievements();

        // Callbacks
        SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreatedCallback;
        SteamMatchmaking.OnLobbyCreated += OnLobbyCreatedCallback;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEnteredCallback;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoinedCallback;
        SteamMatchmaking.OnChatMessage += OnChatMessageCallback;
        SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnectedCallback;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeaveCallback;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequestedCallback;
        SteamApps.OnDlcInstalled += OnDlcInstalledCallback;

        UpdateRichPresenceStatus("test" );
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        SteamClient.RunCallbacks();
    }
    // public override void _Notification(int what)
    // {
    //     if (what == MainLoop.NotificationWmQuitRequest)
    //         if (daRealOne)
    //         {
    //             gameCleanup();
    //         }
    //     GetTree().Quit(); // default behavior
    // }
    

    // Place where you can update saves, etc. on sudden game quit as well
    private void gameCleanup()
    {
        if (!applicationHasQuit)
        {
            applicationHasQuit = true;
            leaveLobby();
            SteamClient.Shutdown();
        }
    }

    void OnLobbyMemberDisconnectedCallback(Lobby lobby, Friend friend)
    {
        OtherLobbyMemberLeft(friend);
    }

    void OnLobbyMemberLeaveCallback(Lobby lobby, Friend friend)
    {
        OtherLobbyMemberLeft(friend);
    }

    private void OtherLobbyMemberLeft(Friend friend)
    {
        if (friend.Id != PlayerSteamId)
        {
            Console.WriteLine("Opponent has left the lobby");
            LobbyPartnerDisconnected = true;
            try
            {
                SteamNetworking.CloseP2PSessionWithUser(friend.Id);
                // Handle game / UI changes that need to happen when other player leaves
            }
            catch
            {
                Console.WriteLine("Unable to update disconnected player nameplate / process disconnect cleanly");
            }

        }
    }

    void OnLobbyGameCreatedCallback(Lobby lobby, uint ip, ushort port, SteamId steamId)
    {
        AcceptP2P(OpponentSteamId);
        //SceneManager.LoadScene("SceneToLoad");
    }

    private void AcceptP2P(SteamId opponentId)
    {
        try
        {
            // For two players to send P2P packets to each other, they each must call this on the other player
            SteamNetworking.AcceptP2PSessionWithUser(opponentId);
        }
        catch
        {
            Console.WriteLine("Unable to accept P2P Session with user");
        }
    }

    void OnChatMessageCallback(Lobby lobby, Friend friend, string message)
    {
        // Received chat message
        if (friend.Id != PlayerSteamId)
        {
            Console.WriteLine("incoming chat message");
            Console.WriteLine(message);

            // I used chat to setup game parameters on occasion like when player joined lobby with preselected playable bug family, prob not best way to do it
            // But after host received player chat message I set off the OnLobbyGameCreated callback with lobby.SetGameServer(PlayerSteamId)
            lobby.SetJoinable(false);
            lobby.SetGameServer(PlayerSteamId);
        }
    }

    // Called whenever you first enter lobby
    void OnLobbyEnteredCallback(Lobby lobby)
    {
        // You joined this lobby
        if (lobby.MemberCount != 1) // I do this because this callback triggers on host, I only wanted to use for players joining after host
        {
            // You will need to have gotten OpponentSteamId from various methods before (lobby data, joined invite, etc)
            AcceptP2P(OpponentSteamId);

            // Examples of things to do
            lobby.SendChatString("incoming player info");
            string v = lobby.GetData("isFriendLobby");
            Console.Write(v);
        }
    }

    // Accepted Steam Game Invite
    async void OnGameLobbyJoinRequestedCallback(Lobby joinedLobby, SteamId id)
    {
        // Attempt to join lobby
        RoomEnter joinedLobbySuccess = await joinedLobby.Join();
        if (joinedLobbySuccess != RoomEnter.Success)
        {
            Console.WriteLine("failed to join lobby");
        }
        else
        {
            // This was hacky, I didn't have clean way of getting lobby host steam id when joining lobby from game invite from friend 
            foreach (Friend friend in SteamFriends.GetFriends())
            {
                if (friend.Id == id)
                {
                    lobbyPartner = friend;
                    break;
                }
            }
            currentLobby = joinedLobby;
            OpponentSteamId = id;
            LobbyPartnerDisconnected = false;
            AcceptP2P(OpponentSteamId);
            //SceneManager.LoadScene("Scene to load");
        }
    }

    void OnLobbyCreatedCallback(Result result, Lobby lobby)
    {
        // Lobby was created
        LobbyPartnerDisconnected = false;
        if (result != Result.OK)
        {
            Console.WriteLine("lobby creation result not ok");
            Console.WriteLine(result.ToString());
        }
    }

    void OnLobbyMemberJoinedCallback(Lobby lobby, Friend friend)
    {
        // The lobby member joined
        Console.WriteLine("someone else joined lobby");
        if (friend.Id != PlayerSteamId)
        {
            LobbyPartner = friend;
            OpponentSteamId = friend.Id;
            AcceptP2P(OpponentSteamId);
            LobbyPartnerDisconnected = false;
        }
    }

    void OnDlcInstalledCallback(AppId appId)
    {
        // Do something if DLC installed
    }

    // I have a screen in game with UI that displays open multiplayer lobbies, I use this method to grab lobby data for UI and joining
    public async Task<bool> RefreshMultiplayerLobbies(bool ranked = false)
    {
        try
        {
            if (ranked)
            {
                activeRankedLobbies.Clear();
                Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithMaxResults(20).WithKeyValue("isRankedData", "TRUE").OrderByNear(PlayerEloDataString, PlayerElo).RequestAsync();
                if (lobbies != null)
                {
                    foreach (Lobby lobby in lobbies.ToList())
                    {
                        activeRankedLobbies.Add(lobby);
                    }
                }
                Console.WriteLine(activeRankedLobbies);
                return true;
            }
            else
            {
                activeUnrankedLobbies.Clear();
                Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithMaxResults(20).WithKeyValue("isRankedData", "FALSE").RequestAsync();
                if (lobbies != null)
                {
                    foreach (Lobby lobby in lobbies.ToList())
                    {
                        activeUnrankedLobbies.Add(lobby);
                    }
                }
                Console.WriteLine(activeUnrankedLobbies);
                return true;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            Console.WriteLine("Error fetching multiplayer lobbies");
            return true;
        }
    }

    public void leaveLobby()
    {
        try
        {
            currentLobby.Leave();
        }
        catch
        {
            Console.WriteLine("Error leaving current lobby");
        }
        try
        {
            SteamNetworking.CloseP2PSessionWithUser(OpponentSteamId);
        }
        catch
        {
            Console.WriteLine("Error closing P2P session with opponent");
        }
    }

    public async Task<bool> CreateFriendLobby()
    {
        try
        {
            var createLobbyOutput = await SteamMatchmaking.CreateLobbyAsync(2);
            if (!createLobbyOutput.HasValue)
            {
                Console.WriteLine("Lobby created but not correctly instantiated");
                throw new Exception();
            }

            LobbyPartnerDisconnected = false;

            hostedMultiplayerLobby = createLobbyOutput.Value;
            hostedMultiplayerLobby.SetData("isFriendLobby", "TRUE");
            hostedMultiplayerLobby.SetData("ownerNameDataString", PlayerName);
            hostedMultiplayerLobby.SetFriendsOnly();

            currentLobby = hostedMultiplayerLobby;

            return true;
        }
        catch (Exception exception)
        {
            Console.WriteLine("Failed to create multiplayer lobby");
            Console.WriteLine(exception.ToString());
            return false;
        }
    }

    public async Task<bool> CreateLobby()//int lobbyParameters)
    {
        try
        {
            Console.WriteLine("Creating Lobby");
            var createLobbyOutput = await SteamMatchmaking.CreateLobbyAsync(2);
            
            if (!createLobbyOutput.HasValue)
            {
                Console.WriteLine("Lobby created but not correctly instantiated");
                throw new Exception();
            }

            LobbyPartnerDisconnected = false;
            hostedMultiplayerLobby = createLobbyOutput.Value;
            hostedMultiplayerLobby.SetPublic();
            hostedMultiplayerLobby.SetJoinable(true);
            //hostedMultiplayerLobby.SetData("staticDataString", lobbyParameters.ToString());


            currentLobby = hostedMultiplayerLobby;
            Console.WriteLine("Lobby was created");
            return true;
        }
        catch (Exception exception)
        {
            Console.WriteLine("Failed to create multiplayer lobby");
            Console.WriteLine(exception.ToString());
            return false;
        }
    }

    // Allows you to open friends list where game invites will have lobby id
    public void OpenFriendOverlayForGameInvite()
    {
        SteamFriends.OpenGameInviteOverlay(currentLobby.Id);
    }

    private void ReconcileMissedAchievements()
    {
        //AchievementStatus localAchievementStatus = SaveSystem.LoadAchievementStatus();
        //List<Achievement> steamAchievementStatus = SteamUserStats.Achievements.ToList();
        //List<string> achievementsThatWereMissed = new List<string>();

        //if (localAchievementStatus.achievementToCheck)
        //{
        //    foreach (var achievement in steamAchievementStatus)
        //    {
        //        if (achievement.Name.Equals(achievementToCheck) && !achievement.State)
        //        {
        //            achievementsThatWereMissed.Add(achievementToCheck);
        //        }
        //    }
        //}

        //if (achievementsThatWereMissed.Count > 0)
        //{
        //    UnlockAchievements(achievementsThatWereMissed);
        //}
    }

    public void UnlockAchievements(List<string> achievementsToUnlock)
    {
        try
        {
            foreach (string achievement in achievementsToUnlock)
            {
                var ach = new Achievement(achievement);
                ach.Trigger();
            }
        }
        catch
        {
            Console.WriteLine("Unable to set unlocked achievement status on Steam");
        }
    }

    void OnSceneLoaded(string name)
    {
        UpdateRichPresenceStatus(name);
    }

    public void UpdateRichPresenceStatus(string SceneName)
    {
        if (connectedToSteam)
        {
            string richPresenceKey = "steam_display";

            if (SceneName.Equals("SillyScene"))
            {
                SteamFriends.SetRichPresence(richPresenceKey, "#SillyScene");
            }
            else if (SceneName.Contains("SillyScene2"))
            {
                SteamFriends.SetRichPresence(richPresenceKey, "#SillyScene2");
            }
        }
    }
}

