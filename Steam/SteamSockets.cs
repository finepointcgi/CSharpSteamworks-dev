using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace CSharpSteamworks.Steam
{

    // SteamManager.cs code/ methods that facilitate sending and receiving messages to and from Socket server
    public class SteamSockets : Node
    {
        public SteamSocketManager steamSocketManager { get; private set; }
        public bool activeSteamSocketServer { get; private set; }
        public bool activeSteamSocketConnection { get; private set; }
        public SteamConnectionManager steamConnectionManager { get; private set; }
        public SteamId PlayerSteamId { get; private set; }
        public SteamId OpponentSteamId { get; private set; }
        public bool NOT_HOST { get; private set; }

        public SteamSockets()
        {
            // Helpful to reduce time to use SteamNetworkingSockets later
            SteamNetworkingUtils.InitRelayNetworkAccess();
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

        SteamClient.RunCallbacks();
        try
        {
            if (activeSteamSocketServer)
            {
                steamSocketManager.Receive();
            }
            if (activeSteamSocketConnection)
            {
                steamConnectionManager.Receive();
            }
        }
        catch
        {
            Console.WriteLine("Error receiving data on socket/connection");
        }
    }

    private void CreateSteamSocketServer()
    {
        steamSocketManager = SteamNetworkingSockets.CreateRelaySocket<SteamSocketManager>(0);
        // Host needs to connect to own socket server with a ConnectionManager to send/receive messages
        // Relay Socket servers are created/connected to through SteamIds rather than "Normal" Socket Servers which take IP addresses
        steamConnectionManager = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(PlayerSteamId);
        activeSteamSocketServer = true;
        activeSteamSocketConnection = true;
    }

    private void JoinSteamSocketServer()
    {
        if (NOT_HOST)
        {
            Console.WriteLine("joining socket server");
            steamConnectionManager = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(OpponentSteamId, 0);
            activeSteamSocketServer = false;
            activeSteamSocketConnection = true;
        }
    }

    private void LeaveSteamSocketServer()
    {
        activeSteamSocketServer = false;
        activeSteamSocketConnection = false;
        try
        {
            // Shutdown connections/sockets. I put this in try block because if player 2 is leaving they don't have a socketManager to close, only connection
            steamConnectionManager.Close();
            steamSocketManager.Close();
        }
        catch
        {
            Console.WriteLine("Error closing socket server / connection manager");
        }
    }

    public void RelaySocketMessageReceived(IntPtr message, int size, uint connectionSendingMessageId)
    {
        try
        {
                var hash = steamSocketManager.Connected.ToList();
            // Loop to only send messages to sosteamSocketManager.Connectedcket server members who are not the one that sent the message
            for (int i = 0; i < hash.Count; i++)
            {
                    
                if (hash[i].Id != connectionSendingMessageId)
                {
                    Result success = hash[i].SendMessage(message, size);
                    if (success != Result.OK)
                    {
                        Result retry = hash[i].SendMessage(message, size);
                    }
                }
            }
        }
        catch
        {
            Console.WriteLine("Unable to relay socket server message");
        }
    }

    public bool SendMessageToSocketServer(byte[] messageToSend)
    {
        try
        {
            // Convert string/byte[] message into IntPtr data type for efficient message send / garbage management
            int sizeOfMessage = messageToSend.Length;
            IntPtr intPtrMessage = System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeOfMessage);
            System.Runtime.InteropServices.Marshal.Copy(messageToSend, 0, intPtrMessage, sizeOfMessage);
            Result success = steamConnectionManager.Connection.SendMessage(intPtrMessage, sizeOfMessage, SendType.Reliable);
            if (success == Result.OK)
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer
                return true;
            }
            else
            {
                // RETRY
                Result retry = steamConnectionManager.Connection.SendMessage(intPtrMessage, sizeOfMessage, SendType.Reliable);
                System.Runtime.InteropServices.Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer
                if (retry == Result.OK)
                {
                    return true;
                }
                return false;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine("Unable to send message to socket server");
            return false;
        }
    }

    public void ProcessMessageFromSocketServer(IntPtr messageIntPtr, int dataBlockSize)
    {
        try
        {
            byte[] message = new byte[dataBlockSize];
            System.Runtime.InteropServices.Marshal.Copy(messageIntPtr, message, 0, dataBlockSize);
            string messageString = System.Text.Encoding.UTF8.GetString(message);

            // Do something with received message

        }
        catch
        {
            Console.WriteLine("Unable to process message from socket server");
        }
    }
}
}
