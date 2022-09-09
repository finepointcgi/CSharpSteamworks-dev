using Godot;
using System;

public class SceneManager : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

    public void _on_Button_button_down(){
        GetNode<SteamManager>("Steam Manager").CreateLobby();
    }

    public void _on_Button2_button_down(){
        _ = GetNode<SteamManager>("Steam Manager").RefreshMultiplayerLobbies();
    }
    public void _on_Button3_button_down(){
        GetNode<SteamManager>("Steam Manager").OpenFriendOverlayForGameInvite();
    }
}
