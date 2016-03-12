using System;
using System.IO;
using AdiTennis.Logic;
using AdiTennis.Sockets.AbstractWorker;

namespace AdiTennis.Sockets.Server.PlayingGame
{
    internal class ClientRacketPosListenerWorker : IAutoClosableWorker
    {
        private readonly byte[] _buffor = new byte[GameState.Config.BufforsLenght];
        private readonly GameState _gameState = GameState.GetInstance();
        private readonly TcpClientStruct _client = GameState.GetInstance().ServerVars.ConnectedClient;

        public void DoWork()
        {
            _client.NetworkStream.BeginRead(_buffor, 0, _buffor.Length, AsyncReadCallback, null);
        }

        private void AsyncReadCallback(IAsyncResult ar)
        {
            try
            {
                var read = _client.NetworkStream.EndRead(ar);
                if (read > 0)
                {
                    lock (GameState.Config.TennisStagePrinterLock)
                    {
                    var key = NetworkHelper.DeserializeObject<ConsoleKey>(_buffor, read);
                    
                        switch (key)
                        {
                            case ConsoleKey.UpArrow:
                                _gameState.ClientServerVars.TennisState.RacketPosUp(PlayerTypeEnum.Client);
                                break;
                            case ConsoleKey.DownArrow:
                                _gameState.ClientServerVars.TennisState.RacketPosDown(PlayerTypeEnum.Client);
                                break;
                        }
                    }

                    _client.NetworkStream.BeginRead(_buffor, 0, _buffor.Length, AsyncReadCallback, null);
                }
            }
            catch (IOException)
            {
                _gameState.ClientServerVars.TheGameEnd.Set();
            }
        }

        public void RequestStop()
        {
            throw new NotSupportedException();
        }
    }
}