using System.Threading;
using AdiTennis.Sockets.AbstractWorker;

namespace AdiTennis.Logic
{
    internal class BallPhysicsWorker : IWorker
    {
        private readonly TennisState _state = GameState.GetInstance().ClientServerVars.TennisState;
        private volatile bool _shouldStop;
        private volatile int _extraSleep;
        private readonly object _extraSleepLock = new object();

        public void DoWork()
        {
            do
            {
                lock (_extraSleepLock)
                {
                    if (_extraSleep > 0)
                    {
                        Thread.Sleep(_extraSleep);
                        _extraSleep = 0;
                    }
                }
                _state.NextBallPos(this);
                Thread.Sleep(GameState.Config.BallSpeedMs);
            } while (!_shouldStop);
        }

        public void RequestStop()
        {
            _shouldStop = true;
        }

        public void Sleep(int ms)
        {
            lock (_extraSleepLock)
            {
                _extraSleep += ms;
            }
        }
    }
}