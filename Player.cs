using Godot;
using System;
using Steamworks;
using Godot.Collections;
using CSharpSteamworks.Networking;

public class Player : KinematicBody2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
	private int currentFrame;
	private int totalFrame = 5;

	public Friend FriendData;
	public bool isReady;
	[Export]
	public bool Controlled;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
		PacketManager.OnUpdatePlayer += OnUpdatePlayer;
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
		if(Controlled){
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
			currentFrame += 1;
			if(currentFrame >= totalFrame){
				updateRemoteLocation(Position, Rotation);
			}
		}
    }

    private void updateRemoteLocation(Vector2 position, float rotation){
		var dict = new Dictionary<string, string>() {
			{"playerId", SteamManager.Instance.PlayerSteamId.AccountId.ToString()},
			{"positionx", position.x.ToString()},
			{"positiony", position.y.ToString()},
			{"rotation", rotation.ToString()},

        };

		if (SteamManager.Instance.IsHost){
         SteamManager.Broadcast(PacketIO.PackObject(PacketTypes.UpdatePlayerState, dict));
        }else{
            SteamManager.steamConnectionManager.Connection.SendMessage(PacketIO.PackObject(PacketTypes.UpdatePlayerState, dict));
        }
		
	}

	private void OnUpdatePlayer(Dictionary<string, string> dict)
	{
		if (dict["playerId"] == FriendData.Id.AccountId.ToString())
		{
			Position = Vector2(float.Parse(dict["positionx"]), float.Parse(dict["positiony"]));
			Rotation = float.Parse(dict["rotation"]);

        }
	}
}
