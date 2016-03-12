using System;
using System.Net;
using System.Net.Sockets;
using AdiTennis.Logic;
using AdiTennis.Sockets.ClientServer;

namespace AdiTennis.Sockets.Server.AwaingClient
{
    internal class DiscoveryBroadcastServerWorker : IDiscoveryServerWorker
    {
        private readonly UdpClient _sendClient = UdpClientFactory.GetNewSendClient();
        private readonly UdpClient _listenClient = UdpClientFactory.GetNewListenClient();

        public void DoWork()
        {
            lock (_listenClient)
            {
                _listenClient.BeginReceive(ReceiveCallback, null);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            lock (_listenClient)
            {
                var remoteEndPoint = new IPEndPoint(0, 0);
                byte[] rcvedBytes;

                try
                {
                    rcvedBytes = _listenClient.EndReceive(ar, ref remoteEndPoint);
                }
                catch (ObjectDisposedException)
                {
                    return;
                }

                var discoveryRequest = NetworkHelper.DeserializeObject<string>(rcvedBytes);
                if (discoveryRequest == GameState.Config.DiscoveryRequestText)
                {
                    lock (_sendClient)
                    {
                        var networkInterface = NetworkHelper.GetNetworkInterfaceForIp(remoteEndPoint.Address);
                        var ipAddress = NetworkHelper.GetIpForNetworkInterface(networkInterface);
                        var broadcastAddress = NetworkHelper.GetBroadcastAddressForNetworkInterface(networkInterface);

                        var serverInfo = new ServerInfoStruct(ipAddress);
                        var serverInfoBytes = NetworkHelper.SerializeObject(serverInfo);

                        _sendClient.BeginSend(serverInfoBytes, serverInfoBytes.Length,
                            new IPEndPoint(broadcastAddress, GameState.Config.Port),
                            SendCallback, _sendClient);
                    }
                }
                _listenClient.BeginReceive(ReceiveCallback, null);
            }
        }


        public void RequestStop()
        {
            lock (_listenClient)
            {
                lock (_sendClient)
                {
                    _listenClient.Close();
                    _sendClient.Close();
                }
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            lock (_sendClient)
            {
                try
                {
                    _sendClient.EndSend(ar);
                }
                catch (ObjectDisposedException)
                {
                    // expected. no problem, sir!
                }
            }
        }
    }
}