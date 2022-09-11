using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using CSharpSteamworks.Steam;
using System.Runtime.InteropServices;
using CSharpSteamworks.Networking;

public class SteamManager : Node
{
    public static SteamManager Instance { get; set; }
    private static uint gameAppId { get; set; } = 2145350;
    public string PlayerName { get; set; }
    public SteamId PlayerSteamId { get; set; }
    public string PlayerSteamIdString { get; set; }
    private bool connectedToSteam { get; set; } = false;
    public SteamId OpponentSteamId { get; set; }
    public bool LobbyPartnerDisconnected { get; set; }
    public List<Lobby> activeUnrankedLobbies { get; set; }
    public List<Lobby> activeRankedLobbies { get; set; }
    public Lobby currentLobby { get; set; }
    private Lobby hostedMultiplayerLobby { get; set; }
    public bool IsHost => currentLobby.IsOwnedBy(PlayerSteamId);
    public static SteamSocketManager steamSocketManager { get; set; }
    public static SteamConnectionManager steamConnectionManager { get; set; }

    public static event Action<List<Lobby>> OnLobbyRefreshCompleted;
    public static event Action<string> OnPlayerJoinLobby;
    //[Signal]
    //delegate void OnLobbyRefreshCompleted(List<Lobby> lobbies);

    public SteamManager()
    {
        if (Instance == null)
        {
            
            Instance = this;
            PlayerName = "";
            try
            {
                // Create client
                SteamClient.Init(gameAppId, true);

                if (!SteamClient.IsValid)
                {
                    GD.Print("Steam client not valid");
                    throw new Exception();
                }

                PlayerName = SteamClient.Name;
                PlayerSteamId = SteamClient.SteamId;
                PlayerSteamIdString = PlayerSteamId.ToString();
                activeUnrankedLobbies = new List<Lobby>();
                activeRankedLobbies = new List<Lobby>();
                connectedToSteam = true;
                GD.Print("Steam initialized: " + PlayerName);
            }
            catch (Exception e)
            {
                connectedToSteam = false;
                PlayerSteamIdString = "NoSteamId";
                GD.Print("Error connecting to Steam");
                GD.Print(e);
            }
        }
    }

    public static void Broadcast(byte[] data)
    {
        foreach (Connection connection in steamSocketManager.Connected.Skip(1).ToArray())
        {
            connection.SendMessage(data);
        }
    }

    internal void RelaySocketMessageReceived(IntPtr data, int size, uint id)
    {
        byte[] managedArray = new byte[size];
        Marshal.Copy(data, managedArray, 0, size);
        PacketManager.Handle(id, new Packet(managedArray));
    }

    public bool TryToReconnectToSteam()
    {
        GD.Print("Attempting to reconnect to Steam");
        try
        {
            // Create client
            SteamClient.Init(gameAppId, true);

            if (!SteamClient.IsValid)
            {
                GD.Print("Steam client not valid");
                throw new Exception();
            }

            PlayerName = SteamClient.Name;
            PlayerSteamId = SteamClient.SteamId;
            activeUnrankedLobbies = new List<Lobby>();
            activeRankedLobbies = new List<Lobby>();
            GD.Print("Steam initialized: " + PlayerName);
            connectedToSteam = true;
            return true;
        }
        catch (Exception e)
        {
            connectedToSteam = false;
            GD.Print("Error connecting to Steam");
            GD.Print(e);
            return false;
        }
    }

    internal void ProcessMessageFromSocketServer(IntPtr data, int size)
    {
        byte[] managedArray = new byte[size];
        Marshal.Copy(data, managedArray, 0, size);
        PacketManager.Handle(currentLobby.Owner.Id.AccountId, new Packet(managedArray));
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
        try
        {
            if (steamSocketManager != null )
            {
                steamSocketManager.Receive();
            }
            if(steamConnectionManager != null && steamConnectionManager.Connected){
                steamConnectionManager.Receive();
            }
        }
        catch(Exception e)
        {
            GD.Print("Error receiving data on socket/connection " + e.Message);
        }
    }

    public override void _Notification(int what)
    {
        if (what == MainLoop.NotificationWmQuitRequest){
            leaveLobby();
            SteamClient.Shutdown();
            GetTree().Quit(); // default behavior
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
            GD.Print("Opponent has left the lobby");
            LobbyPartnerDisconnected = true;
            try
            {
                SteamNetworking.CloseP2PSessionWithUser(friend.Id);
                // Handle game / UI changes that need to happen when other player leaves
            }
            catch
            {
                GD.Print("Unable to update disconnected player nameplate / process disconnect cleanly");
            }

        }
    }

    void OnLobbyGameCreatedCallback(Lobby lobby, uint ip, ushort port, SteamId steamId)
    {
        GD.Print("firing callback for on lobby game created.");
        //SceneManager.LoadScene("SceneToLoad");
    }

    void OnChatMessageCallback(Lobby lobby, Friend friend, string message)
    {
        // Received chat message
        if (friend.Id != PlayerSteamId)
        {
            GD.Print("incoming chat message");
            GD.Print(message);

            // I used chat to setup game parameters on occasion like when player joined lobby with preselected playable bug family, prob not best way to do it
            // But after host received player chat message I set off the OnLobbyGameCreated callback with lobby.SetGameServer(PlayerSteamId)
            //lobby.SetJoinable(false);
            lobby.SetGameServer(PlayerSteamId);
            //lobby.SendChatString("We are connected!");
            GD.Print(friend.Name + " has connected!");
        }
    }

    // Called whenever you first enter lobby
    void OnLobbyEnteredCallback(Lobby lobby)
    {
        // You joined this lobby
        if (lobby.MemberCount > 0) // I do this because this callback triggers on host, I only wanted to use for players joining after host
        {
            GD.Print($"You joined {lobby.Owner.Name}'s lobby");

            // Examples of things to do
            lobby.SendChatString("incoming player info");
            string v = lobby.GetData("isFriendLobby");
            Console.Write(v);
            foreach (var item in lobby.Members)
            {
                OnPlayerJoinLobby(item.Name);
            }
            JoinSteamSocketServer(lobby.Owner.Id);
        }
    }

    // Accepted Steam Game Invite
    async void OnGameLobbyJoinRequestedCallback(Lobby joinedLobby, SteamId id)
    {
        // Attempt to join lobby
        RoomEnter joinedLobbySuccess = await joinedLobby.Join();
        if (joinedLobbySuccess != RoomEnter.Success)
        {
            GD.Print("failed to join lobby " + joinedLobbySuccess.ToString());
        }
        else
        {
           
            currentLobby = joinedLobby;
            LobbyPartnerDisconnected = false;
            
            foreach (Friend friend in joinedLobby.Members){
                OnPlayerJoinLobby.Invoke(friend.Name);
            }
            
            //SceneManager.LoadScene("Scene to load");
        }
    }

    void OnLobbyCreatedCallback(Result result, Lobby lobby)
    {
        // Lobby was created
        LobbyPartnerDisconnected = false;
        if (result != Result.OK)
        {
            GD.Print("lobby creation result not ok");
            GD.Print(result.ToString());
        }

        CreateSteamSocketServer();
    }

    void OnLobbyMemberJoinedCallback(Lobby lobby, Friend friend)
    {
        // The lobby member joined
        GD.Print("someone else joined lobby");
        OnPlayerJoinLobby.Invoke(friend.Name);
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
            activeUnrankedLobbies.Clear();
            Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithMaxResults(20).RequestAsync();
            if (lobbies != null)
            {
                foreach (Lobby lobby in lobbies.ToList())
                {
                    activeUnrankedLobbies.Add(lobby);
                    lobby.Leave();
                }
            }
                
            GD.Print(activeUnrankedLobbies);
            OnLobbyRefreshCompleted.Invoke(lobbies.ToList());
            return true;
            
        }
        catch (Exception e)
        {
            GD.Print(e.ToString());
            GD.Print("Error fetching multiplayer lobbies");
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
            GD.Print("Error leaving current lobby");
        }
        
    }

    public async Task<bool> CreateFriendLobby()
    {
        try
        {
            var createLobbyOutput = await SteamMatchmaking.CreateLobbyAsync(5);
            if (!createLobbyOutput.HasValue)
            {
                GD.Print("Lobby created but not correctly instantiated");
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
            GD.Print("Failed to create multiplayer lobby");
            GD.Print(exception.ToString());
            return false;
        }
    }

    public async Task<bool> CreateLobby()//int lobbyParameters)
    {
        try
        {
            GD.Print("Creating Lobby");
            var createLobbyOutput = await SteamMatchmaking.CreateLobbyAsync(20);
            
            if (!createLobbyOutput.HasValue)
            {
                GD.Print("Lobby created but not correctly instantiated");
                throw new Exception();
            }

            LobbyPartnerDisconnected = false;
            hostedMultiplayerLobby = createLobbyOutput.Value;
            hostedMultiplayerLobby.SetPublic();
            hostedMultiplayerLobby.SetJoinable(true);
            hostedMultiplayerLobby.SetData("ownerNameDataString", PlayerName);
            //hostedMultiplayerLobby.SetData("staticDataString", lobbyParameters.ToString());

            currentLobby = hostedMultiplayerLobby;
            GD.Print("Lobby was created");
            //var friend = new Friend();
            //friend. = PlayerName;
            OnPlayerJoinLobby(PlayerName);

            return true;
        }
        catch (Exception exception)
        {
            GD.Print("Failed to create multiplayer lobby");
            GD.Print(exception.ToString());
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
            GD.Print("Unable to set unlocked achievement status on Steam");
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

    public void CreateSteamSocketServer()
    {
        steamSocketManager = SteamNetworkingSockets.CreateRelaySocket<SteamSocketManager>(0);
        // Host needs to connect to own socket server with a ConnectionManager to send/receive messages
        // Relay Socket servers are created/connected to through SteamIds rather than "Normal" Socket Servers which take IP addresses
        steamConnectionManager = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(PlayerSteamId);

        GD.Print("created socket server!");
    }

    public void JoinSteamSocketServer(SteamId host)
    {
        if (!IsHost)
        {
            GD.Print("joining socket server");
            steamConnectionManager = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(host, 0);
        }
    }
}