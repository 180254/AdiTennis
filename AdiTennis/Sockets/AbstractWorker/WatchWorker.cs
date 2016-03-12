using System;
using System.Threading;

namespace AdiTennis.Sockets.AbstractWorker
{
    internal class WatchWorker : IWorker
    {
        private readonly int _msToDeduct;
        private readonly IWorker _workerToStop;
        private readonly ManualResetEvent _eventToSet;

        public WatchWorker(int msToDeduct, IWorker workerToStop, ManualResetEvent eventToSet)
        {
            _msToDeduct = msToDeduct;
            _workerToStop = workerToStop;
            _eventToSet = eventToSet;
        }

        public void DoWork()
        {
            Thread.Sleep(_msToDeduct);
            lock (_workerToStop)
            {
                _workerToStop.RequestStop();
            }
            _eventToSet.Set();
        }

        public void RequestStop()
        {
            throw new NotSupportedException();
        }
    }
}