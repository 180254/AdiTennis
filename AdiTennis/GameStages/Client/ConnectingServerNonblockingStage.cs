using AdiTennis.StageAbstract;
using AdiTennis.StageAbstract.Stages;

namespace AdiTennis.GameStages.Client
{
    internal class ConnectingServerNonblockingStage : INonblockingStage
    {
        private readonly NotifyTemplate _me = new NotifyTemplate(
            "PLEASE WAIT",
            "CONNECTING ...");

        public void Go()
        {
            _me.Go();
        }
    }
}