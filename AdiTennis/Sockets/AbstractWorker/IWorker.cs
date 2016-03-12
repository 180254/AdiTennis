namespace AdiTennis.Sockets.AbstractWorker
{
    internal interface IWorker
    {
        void DoWork();
        void RequestStop();
    }
}