using Godot;
using Steamworks;
using Steamworks.Data;
using System;
using System.Text;
using System.Threading.Tasks;

public class Node2D : Godot.Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    Leaderboard steamLeaderboard;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        try
        {
            SteamClient.Init(2145350);
        }
        catch (System.Exception e)
        {
            GD.Print(e.Message);
        }

    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        //SocketServerManager.Receive();
        //ConnectionManager.Receive();
    }

    public void _on_Button_button_down()
    {
        GD.Print(SteamClient.SteamId); // Your SteamId
        GD.Print(SteamClient.Name); // Your Name
    }

    public void _on_Button2_button_down()
    {
        foreach (var friend in SteamFriends.GetFriends())
        {
            GD.Print($"{friend.Id}: {friend.Name}");
            GD.Print($"{friend.IsOnline} / {friend.SteamLevel}");
            if (friend.Name == "wash")
            {
                //friend.SendMessage("hello from steam integration");
            }
        }
    }


    private void DebugOutput(NetDebugOutput arg1, string arg2)
    {
        GD.Print(arg1);
        GD.Print(arg2);
    }

    public void _on_Get_Achevements_button_down()
    {
        var s = SteamUserStats.Achievements;
        foreach (var a in SteamUserStats.Achievements)
        {
            GD.Print($"{a.Name} ({a.State})");
        }
    }

    public void _on_UnlockAchevo_button_down()
    {
        var ach = new Achievement("TestAchevement");
        ach.Trigger();
        ach.Clear();
    }

    public void _on_Load_File_button_down()
    {
        var fileContents = Encoding.ASCII.GetBytes("test");
        SteamRemoteStorage.FileWrite("file.txt", fileContents);
    }

    public void _on_Save_file_button_down()
    {
        var fileContents = SteamRemoteStorage.FileRead("file.txt");
        GD.Print(fileContents);
    }


    public void _on_CreateLeaderBoard_button_down()
    {
        createleaderboard();

    }

    private async void createleaderboard()
    {

        var leaderboard = await SteamUserStats.FindLeaderboardAsync("Scores");//, LeaderboardSort.Ascending, LeaderboardDisplay.Numeric);
        //var leaderboard = await SteamUserStats.FindLeaderboardAsync("Global Score");

        if (leaderboard.HasValue)
        {

            steamLeaderboard = leaderboard.Value;
            steamLeaderboard.SubmitScoreAsync(10);
            LeaderboardEntry[] entries = await steamLeaderboard.GetScoresAroundUserAsync(-5, 5);
            if (entries == null)
            {
                // or maybe just "return;"
                entries = new LeaderboardEntry[] { };
            }
            //foreach (var e in entries)
            //{
            //    GameObject temp = Instantiate(LeaderboardListItem, LeaderboardGlobal);
            //    temp.transform.Find("PlayerName").GetComponent<Text>().text = e.User.ToString();
            //    temp.transform.Find("PlayerRank").GetComponent<Text>().text = e.GlobalRank.ToString();
            //    temp.transform.Find("PlayerScore").GetComponent<Text>().text = e.Score.ToString();
            //    AssignFriendImage(temp, e.User.Id);

            //}
        }
        else { GD.Print("Leaderboard has no values!"); }

    }

    public void _on_AddScore_button_down()
    {
        var t = Steamworks.Ugc.Query.All;
    }

}


