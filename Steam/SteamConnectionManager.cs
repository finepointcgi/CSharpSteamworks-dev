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
    public class SteamConnectionManager : ConnectionManager
    {
        public override void OnConnected(ConnectionInfo info)
        {
            base.OnConnected(info);
            GD.Print("ConnectionOnConnected");
            
        }

        public override void OnConnecting(ConnectionInfo info)
        {
            base.OnConnecting(info);
            GD.Print("ConnectionOnConnecting");
        }

        public override void OnDisconnected(ConnectionInfo info)
        {
            base.OnDisconnected(info);
            GD.Print("ConnectionOnDisconnected");
        }

        public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            // Message received from socket server, delegate to method for processing
            SteamManager.Instance.ProcessMessageFromSocketServer(data, size);
            GD.Print("Connection Got A Message");
        }
    }
}
