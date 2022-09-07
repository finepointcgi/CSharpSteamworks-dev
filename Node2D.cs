using Godot;
using Steamworks;
using Steamworks.Data;
using System;
using System.Text;
public class Node2D : Godot.Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    [Export]
    public bool server;
    public MyServer MyServer;
    public Lobby Lobby;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        try
        {
            SteamClient.Init(2145350);
        }
        catch (System.Exception e)
        {
            Console.WriteLine(e.Message);
        }
        
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

    public void _on_Button_button_down(){
        Console.WriteLine(SteamClient.SteamId); // Your SteamId
        Console.WriteLine(SteamClient.Name); // Your Name
    }

    public void _on_Button2_button_down(){
        foreach ( var friend in SteamFriends.GetFriends() )
        {
            Console.WriteLine( $"{friend.Id}: {friend.Name}" );
            Console.WriteLine( $"{friend.IsOnline} / {friend.SteamLevel}" );
            if(friend.Name == "wash"){
                //friend.SendMessage("hello from steam integration");
            }
        }
    }

    public void _on_Get_Achevements_button_down(){
        foreach ( var a in SteamUserStats.Achievements )
        {
            Console.WriteLine( $"{a.Name} ({a.State})" );
        }	
    }

    public void _on_UnlockAchevo_button_down(){
         var ach = new Achievement("TestAchevement");
        ach.Trigger();
        ach.Clear();
    }

    public void _on_Load_File_button_down(){
        var fileContents = Encoding.ASCII.GetBytes("test");
        SteamRemoteStorage.FileWrite( "file.txt", fileContents );
    }

    public void _on_Save_file_button_down(){
        var fileContents = SteamRemoteStorage.FileRead( "file.txt" );
        Console.WriteLine(fileContents);
    }

    public void _on_Connect_To_Server_button_down(){
        SteamId id =  
        _ = SteamMatchmaking.JoinLobbyAsync(int.Parse(GetNode<TextEdit>("TextEdit").Text)).Result;
        }

    public void _on_Create_Lobby_To_Server_button_down(){
        Lobby Lobby = (Lobby)SteamMatchmaking.CreateLobbyAsync(5).Result;
        _ = Lobby.Join().Result;
        MyServer = SteamNetworkingSockets.CreateNormalSocket<MyServer>(Steamworks.Data.NetAddress.AnyIp(21893));
    }
    public void test()
    {
        
    }
}
