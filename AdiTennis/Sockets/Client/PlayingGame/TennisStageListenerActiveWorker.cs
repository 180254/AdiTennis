using System;
using System.IO;
using AdiTennis.Logic;
using AdiTennis.Sockets.AbstractWorker;

namespace AdiTennis.Sockets.Client.PlayingGame
{
    internal class TennisStageListenerActiveWorker : IActiveWorker, IConnectionLostNotifier, IAutoClosableWorker
    {
        private readonly GameState _gameState = GameState.GetInstance();
        private readonly byte[] _buffor = new byte[GameState.Config.BufforsLenght];
        private readonly TcpClientStruct _server = GameState.GetInstance().ClientVars.ConnectedServer;

        public void DoWork()
        {
            _server.NetworkStream.BeginRead(_buffor, 0, _buffor.Length, AsyncReadCallback, null);
        }

        private void AsyncReadCallback(IAsyncResult ar)
        {
            try
            {
                var read = _server.NetworkStream.EndRead(ar);
                if (read > 0)
                {
                    var tennisState = NetworkHelper.DeserializeObject<TennisState>(_buffor, read);

                    if (tennisState != null)
                    {
                        lock (GameState.Config.TennisStagePrinterLock)
                        {
                            _gameState.ClientServerVars.TennisState = tennisState;
                            new TennisStage(tennisState).Go();

                            if (tennisState.GameOver)
                                _gameState.ClientServerVars.TheGameEnd.Set();
                        }
                    }
                }

                _server.NetworkStream.BeginRead(_buffor, 0, _buffor.Length, AsyncReadCallback, null);
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