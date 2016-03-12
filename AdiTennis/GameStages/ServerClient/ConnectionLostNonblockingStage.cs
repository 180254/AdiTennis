using AdiTennis.StageAbstract;

namespace AdiTennis.GameStages.ServerClient
{
    internal class ConnectionLostNonblockingStage
    {
        private readonly NotifyTemplate _me = new NotifyTemplate(
            "END OF GAME",
            "CONNECTION LOST");

        public void Go()
        {
            _me.Go();
        }
    }
}