using Godot;
using System;
using Steamworks;
public class Player : KinematicBody2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
	public Friend FriendData;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        Vector2 movementVector = Vector2.Zero; 
		if(Input.IsActionPressed("ui_up")){
			movementVector += Vector2.Up;
		}
		if(Input.IsActionPressed("ui_down")){
			movementVector += Vector2.Down;
		}
		if(Input.IsActionPressed("ui_left")){
			movementVector += Vector2.Left;
		}
		if(Input.IsActionPressed("ui_right")){
			movementVector += Vector2.Right;
		}
		MoveAndSlide(movementVector * 100, Vector2.Up );
    }

    
}
