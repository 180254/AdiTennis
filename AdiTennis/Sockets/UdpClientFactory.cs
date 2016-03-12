using System.Net;
using System.Net.Sockets;
using AdiTennis.Logic;

namespace AdiTennis.Sockets
{
    internal static class UdpClientFactory
    {
        public static UdpClient GetNewListenClient()
        {
            var udpClient = new UdpClient();
            var listenEndPoint = new IPEndPoint(IPAddress.Any, GameState.Config.Port);
            udpClient.EnableBroadcast = true;
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.ExclusiveAddressUse = false;
            udpClient.Client.Bind(listenEndPoint);
            return udpClient;
        }

        public static UdpClient GetNewSendClient()
        {
            return new UdpClient
            {
                EnableBroadcast = true
            };
        }
    }
}