using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Kit
{
    public static class IPAddressExtensions
    {
        public static string GetLocalIPAddress()
        {
            string localIP= "127.0.0.1";
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    localIP = endPoint.Address.ToString();
                }
            }catch(Exception ex)
            {
                Log.Logger.Error(ex, "GetLocalIPAddress");
            }
            return localIP;
        }
    }
}
