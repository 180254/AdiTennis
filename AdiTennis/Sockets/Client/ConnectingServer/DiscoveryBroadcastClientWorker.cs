using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using AdiTennis.Logic;
using AdiTennis.Sockets.AbstractWorker;
using AdiTennis.Sockets.ClientServer;

namespace AdiTennis.Sockets.Client.ConnectingServer
{
    internal class DiscoveryBroadcastClientWorker : IDiscoveryClientWorker
    {
        private readonly GameState _gameState = GameState.GetInstance();
        private readonly UdpClient _sendClient = UdpClientFactory.GetNewSendClient();
        private readonly UdpClient _listenClient = UdpClientFactory.GetNewListenClient();

        private bool _firstDiscovered = true;
        private readonly object _firstDiscoveredLock = new object();
        private readonly bool _stopOnFirstFound;

        public DiscoveryBroadcastClientWorker(bool stopOnFirstFound)
        {
            _stopOnFirstFound = stopOnFirstFound;
        }

        public void DoWork()
        {
            // listen answers!
            lock (_listenClient)
            {
                _listenClient.BeginReceive(ReceiveCallback, null);
            }

            // send request!
            var discoveryRequestBytes = NetworkHelper.SerializeObject(GameState.Config.DiscoveryRequestText);
            var broadcastIps = NetworkHelper.GetAllBroadcastIps();

            do
            {
                foreach (var broadcastIp in broadcastIps)
                {
                    lock (_sendClient)
                    {
                        try
                        {
                            _sendClient.BeginSend(discoveryRequestBytes, discoveryRequestBytes.Length,
                                new IPEndPoint(broadcastIp, GameState.Config.Port),
                                SendCallback, _sendClient);
                        }
                        catch (ObjectDisposedException)
                        {
                            // expected. no problem, sir!
                            return;
                        }
                    }
                }

                Thread.Sleep(GameState.Config.DiscoveryRequestRepeatTimeMs);
            } while (true);
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
                _sendClient.EndSend(ar);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            var remoteEndPoint = new IPEndPoint(0, 0);
            lock (_listenClient)
            {
                try
                {
                    var rcvedBytes = _listenClient.EndReceive(ar, ref remoteEndPoint);
                    var serverInfoStruct = NetworkHelper.DeserializeObject<ServerInfoStruct>(rcvedBytes);

                    if (serverInfoStruct != null &&
                        serverInfoStruct.StructId == GameState.Config.DiscoveryServerInfoStructId)
                    {
                        _gameState.ClientVars.DiscoveredServers.Add(serverInfoStruct);

                        if (_stopOnFirstFound)
                        {
                            _gameState.ClientVars.ServersDiscoveredEvent.Set();
                            RequestStop();
                        }
                        else
                        {
                            lock (_firstDiscoveredLock)
                            {
                                if (!_firstDiscovered) return;
                                _firstDiscovered = false;

                                IWorker stopwatchWorker =
                                    new WatchWorker(GameState.Config.DiscoveryTimeoutAfterFirstReceiveMs,
                                        this, _gameState.ClientVars.ServersDiscoveredEvent);

                                var stopwatchThread = new Thread(stopwatchWorker.DoWork);
                                stopwatchThread.Start();
                            }
                        }
                    }

                    _listenClient.BeginReceive(ReceiveCallback, null);
                }
                catch (ObjectDisposedException)
                {
                    // expected. no problem, sir!
                }
            }
        }
    }
}