using AdiTennis.Logic;
using AdiTennis.StageAbstract;
using AdiTennis.StageAbstract.Stages;

namespace AdiTennis.GameStages.ServerClient
{
    internal class GameOverNonblockingStage : INonblockingStage
    {
        private readonly GameState _gameState = GameState.GetInstance();
        private readonly NotifyTemplate _me;

        public GameOverNonblockingStage()
        {
            _me = new NotifyTemplate(
                "GAME OVER",
                "YOU " +
                (_gameState.ClientServerVars.SelectedStartAs == _gameState.ClientServerVars.TennisState.WhoWon
                    ? "WON"
                    : "LOST"));
        }

        public void Go()
        {
            _me.Go();
        }
    }
}