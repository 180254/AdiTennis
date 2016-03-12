using System;
using System.IO;
using System.Threading;
using AdiTennis.Logic;
using AdiTennis.Sockets.AbstractWorker;

namespace AdiTennis.Sockets.Server.PlayingGame
{
    internal class TennisStageSenderActiveWorker : IActiveWorker
    {
        private readonly GameState _gameState = GameState.GetInstance();
        private readonly TcpClientStruct _client = GameState.GetInstance().ServerVars.ConnectedClient;

        private volatile bool _shouldStop;
        private volatile bool _oneMoreTime = true;

        public void DoWork()
        {
            try
            {
                do
                {
                    lock (GameState.Config.TennisStagePrinterLock)
                    {
                        var tennisState = _gameState.ClientServerVars.TennisState;
                        var tennisStateBytes = NetworkHelper.SerializeObject(tennisState);
                        _client.NetworkStream.BeginWrite(tennisStateBytes, 0, tennisStateBytes.Length,
                            AsyncWriteCallback,
                            null);

                        new TennisStage(tennisState).Go();
                    }
                    if (_shouldStop && _oneMoreTime)
                        _oneMoreTime = false;

                    else
                        Thread.Sleep(GameState.Config.FpsAsMilisecondsSleep);
                } while (!_shouldStop || _oneMoreTime);
            }
            catch (IOException)
            {
                _gameState.ServerVars.GameStateSent.Set();
            }
        }

        private void AsyncWriteCallback(IAsyncResult ar)
        {
            try
            {
                _client.NetworkStream.EndWrite(ar);
                if (_shouldStop && _oneMoreTime)
                    _gameState.ServerVars.GameStateSent.Set();
            }
            catch (IOException)
            {
                _gameState.ServerVars.GameStateSent.Set();
            }
        }

        public void RequestStop()
        {
            _shouldStop = true;
        }
    }
}