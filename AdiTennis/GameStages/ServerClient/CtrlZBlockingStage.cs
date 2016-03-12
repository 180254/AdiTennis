using System;
using AdiTennis.StageAbstract;
using AdiTennis.StageAbstract.Stages;

namespace AdiTennis.GameStages.ServerClient
{
    internal class CtrlZBlockingStage : IBlockingStage
    {
        private readonly NotifyTemplate _me = new NotifyTemplate(
            "STANDARD INPUT HAS BEEN CLOSED",
            "CTRLZ EXCEPTION. PRESS ANY KEY TO EXIT.");

        public void Go()
        {
            _me.Go();
            Console.ReadKey();
        }
    }
}