using System;
using System.Threading;
using AdiTennis.Exception;
using AdiTennis.GameStages.Client;
using AdiTennis.GameStages.Server;
using AdiTennis.GameStages.ServerClient;
using AdiTennis.Sockets.Client.ConnectingServer;
using AdiTennis.Sockets.Client.PlayingGame;
using AdiTennis.Sockets.ClientServer;
using AdiTennis.Sockets.Server.AwaingClient;
using AdiTennis.Sockets.Server.PlayingGame;
using AdiTennis.StageAbstract.Stages;

namespace AdiTennis.Logic
{
    internal class PrimaryLogic : IStage
    {
        private volatile GameState _gameState = GameState.GetInstance();

        private AwaintingClientListenerWorker _listenerWorker;
        private BallPhysicsWorker _ballPhysicsWorker;
        private ClientRacketPosListenerWorker _clientRacketPosListenerWorker;
        private ClientRacketPosSenderWorker _clientRacketPosSenderWorker;
        private ConnectingServerWorker _connectingWorker;
        private IDiscoveryClientWorker _discoveryClient;
        private IDiscoveryServerWorker _discoveryServer;
        private ServerRacketPosWorker _serverRacketPosWorker;
        private TennisStageListenerActiveWorker _tennisStageListenerWorker;
        private TennisStageSenderActiveWorker _tennisStageSenderWorker;
        private Thread _ballPhysicsThread;
        private Thread _connectingThread;
        private Thread _discoveryListenerThread;
        private Thread _discoverySenderThread;
        private Thread _listenerThread;
        private Thread _racketPosListenerThread;
        private Thread _racketPosSenderThread;
        private Thread _serverRacketPosThread;
        private Thread _tennisStageListenerThread;
        private Thread _tennisStageSenderThread;

        // ReSharper disable once FunctionComplexityOverflow
        public void Go()
        {
            ResetLabel:

            ResetThreads();
            FlushStdin();
            SerCursorVisability(true);

            StartLabel:

            if (_gameState.ClientServerVars.OwnNickname == null)
                try
                {
                    new EnterNicknameBlockingStage().Go();
                }
                catch (CtrlZException)
                {
                    goto CtrlZExceptionLabel;
                }

            StartAsBlockingStageLabel:

            new StartAsBlockingStage().Go();

            _gameState.ServerVars.ClientConnectedEvent.Reset();

            _gameState.ClientServerVars.ConnectionStatus = ConnectionStatusEnum.None;
            _gameState.ClientVars.ServerResponse = ServerToClientResponseEnum.None;

            if (_gameState.ClientServerVars.SelectedStartAs == PlayerTypeEnum.Server)
            {
                _gameState.ClientServerVars.TennisState.PlayerNickname[(int) PlayerTypeEnum.Server] =
                    _gameState.ClientServerVars.OwnNickname;

                _listenerWorker = new AwaintingClientListenerWorker();
                _discoveryServer = new DiscoveryBroadcastServerWorker();
                _discoverySenderThread = new Thread(_discoveryServer.DoWork);
                _listenerThread = new Thread(_listenerWorker.DoWork);
                _discoverySenderThread.Start();
                _listenerThread.Start();

                new AwaitingClientNonblockingStage().Go();
                _gameState.ServerVars.ClientConnectedEvent.WaitOne();

                _discoveryServer.RequestStop();

                if (_gameState.ClientServerVars.ConnectionStatus == ConnectionStatusEnum.CannotListen ||
                    _gameState.ClientServerVars.ConnectionStatus == ConnectionStatusEnum.ConnectionLost)
                {
                    _listenerWorker.RequestStop();
                    goto StartAsBlockingStageLabel;
                }

                _clientRacketPosListenerWorker = new ClientRacketPosListenerWorker();
                _racketPosListenerThread = new Thread(_clientRacketPosListenerWorker.DoWork);

                _tennisStageSenderWorker = new TennisStageSenderActiveWorker();
                _tennisStageSenderThread = new Thread(_tennisStageSenderWorker.DoWork);

                _serverRacketPosWorker = new ServerRacketPosWorker();
                _serverRacketPosThread = new Thread(_serverRacketPosWorker.DoWork);

                _ballPhysicsWorker = new BallPhysicsWorker();
                _ballPhysicsThread = new Thread(_ballPhysicsWorker.DoWork);

                _racketPosListenerThread.Start();
                _tennisStageSenderThread.Start();
                _serverRacketPosThread.Start();
            }

            else if (_gameState.ClientServerVars.SelectedStartAs == PlayerTypeEnum.Client)
            {
                new ConnectionMethodBlockingStage().Go();

                if (_gameState.ClientVars.ConnectionMethod == ConnectionMethodEnum.DiscoveryListAll ||
                    _gameState.ClientVars.ConnectionMethod == ConnectionMethodEnum.DiscoveryFirstFound)
                {
                    var stopOnFirstFound = _gameState.ClientVars.ConnectionMethod ==
                                           ConnectionMethodEnum.DiscoveryFirstFound;

                    _discoveryClient = new DiscoveryBroadcastClientWorker(stopOnFirstFound);
                    _discoveryListenerThread = new Thread(_discoveryClient.DoWork);
                    _discoveryListenerThread.Start();

                    new DiscoveringServersNonblockingStage().Go();
                    _gameState.ClientVars.ServersDiscoveredEvent.WaitOne();
                    _discoveryClient.RequestStop();

                    if (stopOnFirstFound)
                        _gameState.ClientVars.SelectedServer =
                            _gameState.ClientVars.DiscoveredServers[0].GetEndPoint();
                    else
                        try
                        {
                            new SelectDiscoveredServerBlockingStage().Go();
                        }
                        catch (CtrlZException)
                        {
                            goto CtrlZExceptionLabel;
                        }
                }
                else if (_gameState.ClientVars.ConnectionMethod == ConnectionMethodEnum.KnownIp)
                {
                    new KnownServerBlockingStage().Go();
                }

                new ConnectingServerNonblockingStage().Go();
                _connectingWorker = new ConnectingServerWorker();
                _connectingThread = new Thread(_connectingWorker.DoWork);
                _connectingThread.Start();

                _gameState.ClientVars.ConnectionEstabilishedEvent.WaitOne();

                switch (_gameState.ClientVars.ServerResponse)
                {
                    case ServerToClientResponseEnum.ConnectionError:
                    case ServerToClientResponseEnum.ConnectionRefused:
                    case ServerToClientResponseEnum.BusyServer:
                        _gameState.ClientVars.ConnectionEstabilishedEvent.Reset();
                        goto StartAsBlockingStageLabel;
                    case ServerToClientResponseEnum.ChangeNick:
                        _gameState.ClientServerVars.OwnNickname = null;
                        _gameState.ClientVars.ConnectionEstabilishedEvent.Reset();
                        goto StartLabel;
                }

                _clientRacketPosSenderWorker = new ClientRacketPosSenderWorker();
                _racketPosSenderThread = new Thread(_clientRacketPosSenderWorker.DoWork);

                _tennisStageListenerWorker = new TennisStageListenerActiveWorker();
                _tennisStageListenerThread = new Thread(_tennisStageListenerWorker.DoWork);

                _racketPosSenderThread.Start();
                _tennisStageListenerThread.Start();
            }

            lock (GameState.Config.TennisStagePrinterLock)
            {
                SerCursorVisability(false);
                TennisStage.Init();
            }

            if (_gameState.ClientServerVars.SelectedStartAs == PlayerTypeEnum.Server)
            {
                Thread.Sleep(GameState.Config.StartGameDelayMs);
                _ballPhysicsThread.Start();
            }

            _gameState.ClientServerVars.TheGameEnd.WaitOne();

            if (_gameState.ClientServerVars.SelectedStartAs == PlayerTypeEnum.Server)
            {
                _tennisStageSenderWorker.RequestStop();
                _gameState.ServerVars.GameStateSent.WaitOne();

                _listenerWorker.RequestStop();
                _discoveryServer.RequestStop();
                _serverRacketPosWorker.RequestStop();
                _ballPhysicsWorker.RequestStop();

                //  _gameState.ServerVars.ConnectedClient.TcpClient.Close();
            }
            else
            {
                _clientRacketPosSenderWorker.RequestStop();
//                _gameState.ClientVars.ConnectedServer.TcpClient.Close();
            }

            if (_gameState.ClientServerVars.TennisState.GameOver == false)
            {
                new ConnectionLostNonblockingStage().Go();
            }
            else
            {
                new GameOverNonblockingStage().Go();
            }

            Thread.Sleep(GameState.Config.GameOverMsgMs);

            _gameState.ClientServerVars.ConnectionStatus = ConnectionStatusEnum.ConnectionLost;

            ResetGameState();
            goto ResetLabel;

            CtrlZExceptionLabel:
            new CtrlZBlockingStage().Go();
        }

        private void ResetGameState()
        {
            _gameState.ResetAll();
            _gameState = GameState.GetInstance();
        }

        private static void SerCursorVisability(bool visible)
        {
            Console.CursorVisible = visible;
        }

        private static void FlushStdin()
        {
            while (Console.KeyAvailable)
                Console.ReadKey(false);
        }

        private void ResetThreads()
        {
            if (_racketPosListenerThread != null)
            {
                _racketPosListenerThread.Abort();
                _racketPosListenerThread = null;
            }
            if (_tennisStageSenderThread != null)
            {
                _tennisStageSenderThread.Abort();
                _tennisStageSenderThread = null;
            }
            if (_serverRacketPosThread != null)
            {
                _serverRacketPosThread.Abort();
                _serverRacketPosThread = null;
            }
            if (_connectingThread != null)
            {
                _connectingThread.Abort();
                _connectingThread = null;
            }
            if (_racketPosSenderThread != null)
            {
                _racketPosSenderThread.Abort();
                _racketPosSenderThread = null;
            }
            if (_tennisStageListenerThread != null)
            {
                _tennisStageListenerThread.Abort();
                _tennisStageListenerThread = null;
            }
            if (_ballPhysicsThread != null)
            {
                _ballPhysicsThread.Abort();
                _ballPhysicsThread = null;
            }
            if (_discoverySenderThread != null)
            {
                _discoverySenderThread.Abort();
                _discoverySenderThread = null;
            }
            if (_listenerThread != null)
            {
                _listenerThread.Abort();
                _listenerThread = null;
            }
            if (_discoveryListenerThread != null)
            {
                _discoveryListenerThread.Abort();
                _discoveryListenerThread = null;
            }
        }
    }
}