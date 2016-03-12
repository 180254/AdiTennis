using System;
using System.Net;
using AdiTennis.Logic;
using AdiTennis.StageAbstract;
using AdiTennis.StageAbstract.Stages;

namespace AdiTennis.GameStages.Client
{
    internal class KnownServerBlockingStage : IBlockingStage
    {
        private readonly FreetextTemplate _me =
            new FreetextTemplate("SERVER HOSTNAME OR IP: ", "PLEASE TYPE SERVER INFO");

        public void Go()
        {
            _me.Go();

            try
            {
                GameState.GetInstance().ClientVars.SelectedServer =
                    new IPEndPoint(IPAddress.Parse(_me.Freetext), GameState.Config.Port);
            }
            catch (FormatException)
            {
                GameState.GetInstance().ClientVars.SelectedServer = new IPEndPoint(0, 0);
            }
        }
    }
}