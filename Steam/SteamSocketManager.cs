using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpSteamworks.Steam
{
    // Code for SteamNetworkingSockets which replaced SteamNetworking P2P code SteamManager

    // SOCKET CLASS that creates socket server, only host of each match utilizes this
    public class SteamSocketManager : SocketManager
    {

        public override void OnConnecting(Connection connection, ConnectionInfo data)
        {
            base.OnConnecting(connection, data);//The base class will accept the connection
            Console.WriteLine("SocketManager OnConnecting");
        }

        public override void OnConnected(Connection connection, ConnectionInfo data)
        {
            base.OnConnected(connection, data);
            Console.WriteLine("New player connecting");
        }

        public override void OnDisconnected(Connection connection, ConnectionInfo data)
        {
            base.OnDisconnected(connection, data);
            Console.WriteLine("Player disconnected");
        }

        public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            // Socket server received message, forward on message to all members of socket server
            SteamManager.Instance.RelaySocketMessageReceived(data, size, connection.Id);
            Console.WriteLine("Socket message received");
        }
    }

}
