using AdiTennis.StageAbstract;
using AdiTennis.StageAbstract.Stages;

namespace AdiTennis.GameStages.Client
{
    internal class DiscoveringServersNonblockingStage : INonblockingStage
    {
        private readonly NotifyTemplate _me = new NotifyTemplate(
            "WHERE ARE YOU?",
            "DISCOVERING SERVERS");

        public void Go()
        {
            _me.Go();
        }
    }
}