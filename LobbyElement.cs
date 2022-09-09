using Godot;
using Steamworks.Data;
using System;

public class LobbyElement : Control
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    public Lobby lobby { get; set; }
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }

    public void SetLabels(string id, string name)
    {
        GetNode<RichTextLabel>("Lobby Name").Text = name;
        GetNode<RichTextLabel>("ID").Text = id;
    }

    public void _on_JoinButton_button_down()
    {
        lobby.Join();
    }
}
