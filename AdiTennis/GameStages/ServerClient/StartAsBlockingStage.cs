using AdiTennis.Logic;
using AdiTennis.Sockets.ClientServer;
using AdiTennis.StageAbstract.Menus;
using AdiTennis.StageAbstract.Stages;

namespace AdiTennis.GameStages.ServerClient
{
    internal class StartAsBlockingStage : IBlockingStage
    {
        private const string TaskLine = "PLEASE SELECT YOUR ROLE:";
        private readonly GameState _gameState = GameState.GetInstance();
        private readonly MenuTemplate _me;

        public StartAsBlockingStage()
        {
            switch (_gameState.ClientVars.ServerResponse)
            {
                case ServerToClientResponseEnum.BusyServer:
                    _me = new MenuTemplate("SELECTED SERVER IS BUSY.", TaskLine);
                    break;

                case ServerToClientResponseEnum.ConnectionError:
                    _me = new MenuTemplate("CONNECTION ERROR.", TaskLine);
                    break;

                case ServerToClientResponseEnum.ConnectionRefused:
                    _me = new MenuTemplate("CONNECTION REFUSED BY SERVER.", TaskLine);
                    break;
            }


            switch (_gameState.ClientServerVars.ConnectionStatus)
            {
                case ConnectionStatusEnum.CannotListen:
                    _me = new MenuTemplate("CANNOT START SERVER.",
                        TaskLine);
                    break;
                case ConnectionStatusEnum.ConnectionLost:
                    _me = new MenuTemplate("CONNECTION HAS BEEN LOST.", TaskLine);
                    break;
            }

            if (_me == null)
            {
                _me = new MenuTemplate("HELLO " + _gameState.ClientServerVars.OwnNickname + ".",
                    TaskLine,
                    GameState.Config.MsgColorHeader);
            }


            _me.Items.Add(new MenuItem("SERVER"));
            _me.Items.Add(new MenuItem("CLIENT"));
        }

        public void Go()
        {
            _me.Go();

            var selectedMenuItemIndex = _me.Items.FindIndex(x => x.IsSelected);
            _gameState.ClientServerVars.SelectedStartAs = (PlayerTypeEnum) selectedMenuItemIndex;
        }
    }
}