using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using AdiTennis.GameStages.Client;
using AdiTennis.GameStages.Server;
using AdiTennis.Logic;
using AdiTennis.Sockets.AbstractWorker;
using AdiTennis.Sockets.ClientServer;

namespace AdiTennis.Sockets.Server.AwaingClient
{
    internal class AwaintingClientListenerWorker : IWorker, IConnectionLostNotifier
    {
        private readonly GameState _gameState = GameState.GetInstance();
        private readonly TcpListener _listener = new TcpListener(IPAddress.Any, GameState.Config.Port);
        private readonly ManualResetEvent _nickReceivedEvent = new ManualResetEvent(false);

        public void DoWork()
        {
            try
            {
                _listener.Start();
            }
            catch (SocketException)
            {
                _gameState.ClientServerVars.ConnectionStatus = ConnectionStatusEnum.CannotListen;
                _gameState.ServerVars.ClientConnectedEvent.Set();
                return;
            }

            try
            {
                do
                {
                    var tcpClient = _listener.AcceptTcpClient();
                    var newTcpClient = new TcpClientStruct(tcpClient);

                    lock (newTcpClient)
                    {
                        if (_gameState.ServerVars.ConnectedClient == null)
                        {
                            _nickReceivedEvent.Reset();
                            AsyncReceiveNickAndConnect(newTcpClient);
                            if (!_nickReceivedEvent.WaitOne(new TimeSpan(0, 0, 0, 0,
                                GameState.Config.ResponseTimeoutMs)))
                                continue; // all ok!
                            newTcpClient.ResponseStatus = ServerToClientResponseEnum.ConnectionError;
                            AsyncSendResponse(newTcpClient);
                        }
                        else
                        {
                            newTcpClient.ResponseStatus = ServerToClientResponseEnum.BusyServer;
                            AsyncSendResponse(newTcpClient);
                        }
                    }
                } while (true);
            }
            catch (SocketException)
            {
                _gameState.ClientServerVars.TheGameEnd.Set();
            }
        }

        public void RequestStop()
        {
            _listener.Stop();
        }

        private void AsyncReceiveNickAndConnect(TcpClientStruct newTcpClient)
        {
            lock (newTcpClient)
            {
                newTcpClient.NetworkStream.BeginRead(newTcpClient.ReadBuffer, 0, TcpClientStruct.BuffersSize,
                    AsyncReceiveNickAndConnectCallback, newTcpClient);
            }
        }

        private void AsyncReceiveNickAndConnectCallback(IAsyncResult ar)
        {
            var newTcpClient = (TcpClientStruct) ar.AsyncState;
            lock (newTcpClient)

            {
                int read;

                try
                {
                    read = newTcpClient.NetworkStream.EndRead(ar);
                }
                catch (ObjectDisposedException)
                {
                    // expected. no problem, sir!
                    return;
                }

                if (read > 0)
                {
                    var clientNick = NetworkHelper.DeserializeObject<string>(newTcpClient.ReadBuffer, read);
                    var selfNick = _gameState.ClientServerVars.OwnNickname;

                    if (clientNick == null)
                    {
                        newTcpClient.ResponseStatus = ServerToClientResponseEnum.ConnectionError;
                        AsyncSendResponse(newTcpClient);
                    }
                    else if (clientNick == selfNick)
                    {
                        newTcpClient.ResponseStatus = ServerToClientResponseEnum.ChangeNick;
                        AsyncSendResponse(newTcpClient);
                    }
                    else
                    {
                        new AcceptOrRefuseClientBlockingStage(newTcpClient, clientNick).Go();

                        if (newTcpClient.ResponseStatus == ServerToClientResponseEnum.ConnectionAccepted)
                        {
                            _gameState.ClientServerVars.TennisState.PlayerNickname[(int) PlayerTypeEnum.Client] =
                                clientNick;
                        }
                        else
                        {
                            new ConnectingServerNonblockingStage().Go();
                        }

                        AsyncSendResponse(newTcpClient);
                    }
                }
                else
                {
                    newTcpClient.TcpClient.Close();
                }
            }
        }

        public void AsyncSendResponse(TcpClientStruct tcpClient)
        {
            var serverStatusBytes = NetworkHelper.SerializeObject(tcpClient.ResponseStatus);
            lock (tcpClient)
            {
                try
                {
                    tcpClient.NetworkStream.BeginWrite(serverStatusBytes, 0, serverStatusBytes.Length,
                        AsyncSendResponseCallback, tcpClient);
                }
                catch (IOException)
                {
                    
                    _gameState.ClientServerVars.ConnectionStatus = ConnectionStatusEnum.ConnectionLost;
                    tcpClient.TcpClient.Close();
                    this.RequestStop();
                    _gameState.ServerVars.ClientConnectedEvent.Set();
                }
            }
        }

        private void AsyncSendResponseCallback(IAsyncResult ar)
        {
            var tcpClient = (TcpClientStruct) ar.AsyncState;
            lock (tcpClient)
            {
                tcpClient.NetworkStream.EndWrite(ar);
                if (tcpClient.ResponseStatus != ServerToClientResponseEnum.ConnectionAccepted)
                    tcpClient.TcpClient.Close();
                else
                {
                    _gameState.ServerVars.ConnectedClient = tcpClient;
                    _gameState.ServerVars.ClientConnectedEvent.Set();
                }
            }
        }
    }
}