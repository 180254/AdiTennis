using System;
using System.Threading;
using AdiTennis.Logic;
using AdiTennis.Sockets.AbstractWorker;

namespace AdiTennis.Sockets.Server.PlayingGame
{
    internal class ServerRacketPosWorker : IWorker
    {
        private readonly GameState _gameState = GameState.GetInstance();
        private volatile bool _shouldStop;

        public void DoWork()
        {
            do
            {
                while (!Console.KeyAvailable && !_shouldStop)
                {
                    Thread.Sleep(GameState.Config.ReadKeyFpsAsMilisecondsSleep);
                }
                if (_shouldStop) continue;

                lock (GameState.Config.TennisStagePrinterLock)
                {
                    var key = Console.ReadKey(false);
                    if (key.Key != ConsoleKey.UpArrow && key.Key != ConsoleKey.DownArrow) continue;


                    switch (key.Key)
                    {
                        case ConsoleKey.UpArrow:
                            _gameState.ClientServerVars.TennisState.RacketPosUp(PlayerTypeEnum.Server);
                            break;
                        case ConsoleKey.DownArrow:
                            _gameState.ClientServerVars.TennisState.RacketPosDown(PlayerTypeEnum.Server);
                            break;
                    }
                }
            } while (!_shouldStop);
        }

        public void RequestStop()
        {
            _shouldStop = true;
        }
    }
}