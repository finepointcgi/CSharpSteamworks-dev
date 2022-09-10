using Godot;
using System;
using Steamworks.Data;
using Steamworks;
public class LobbyPlayer : Control
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

    public void SetPlayerInfo(string player){
        GetNode<RichTextLabel>("RichTextLabel").Text = player;
    }

    public void SetReadyStatus(bool ready){
        if(ready){
            GetNode<RichTextLabel>("Ready").Text = "Ready";
        } else
        {
            GetNode<RichTextLabel>("Ready").Text = "Not Ready";
        }
    }
}
