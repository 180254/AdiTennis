using AdiTennis.Logic;
using AdiTennis.Sockets.ClientServer;
using AdiTennis.StageAbstract;
using AdiTennis.StageAbstract.Stages;

namespace AdiTennis.GameStages.ServerClient
{
    internal class EnterNicknameBlockingStage : IBlockingStage
    {
        private readonly GameState _gameState = GameState.GetInstance();
        private readonly FreetextTemplate _me;

        public EnterNicknameBlockingStage()
        {
            _me = _gameState.ClientVars.ServerResponse == ServerToClientResponseEnum.ChangeNick
                ? new FreetextTemplate("NICKNAME: ", "SERVER PLAYER HAS SAME NICK AS YOU. PLEASE CHANGE YOUR NICK.")
                : new FreetextTemplate("NICKNAME: ");
        }

        public void Go()
        {
            _me.Go();
            _gameState.ClientServerVars.OwnNickname = _me.Freetext;
        }
    }
}