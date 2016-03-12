using AdiTennis.StageAbstract;
using AdiTennis.StageAbstract.Stages;

namespace AdiTennis.GameStages.Server
{
    internal class AwaitingClientNonblockingStage : INonblockingStage
    {
        private readonly NotifyTemplate _me = new NotifyTemplate(
            "RESPONDING DISCOVERY REQUEST",
            "AWAITING FOR CLIENT");

        public void Go()
        {
            _me.Go();
        }
    }
}