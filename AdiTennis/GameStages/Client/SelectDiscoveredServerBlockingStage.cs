using AdiTennis.Logic;
using AdiTennis.StageAbstract.Menus;
using AdiTennis.StageAbstract.Stages;

namespace AdiTennis.GameStages.Client
{
    internal class SelectDiscoveredServerBlockingStage : IBlockingStage
    {
        private readonly GameState _gameState = GameState.GetInstance();

        private readonly MenuTemplate _me =
            new MenuTemplate("DISCOVERED SERVERS", "PLEASE CHOOSE SERVER:");

        public SelectDiscoveredServerBlockingStage()
        {
            foreach (var discoveredServer in _gameState.ClientVars.DiscoveredServersDistinct())
            {
                _me.Items.Add(
                    new MenuItem(discoveredServer.IpAddress.PadRight(15) + " | " +
                                 discoveredServer.PlayerNick.PadRight(GameState.Config.NickMaxLen)));
            }
        }

        public void Go()
        {
            _me.Go();

            var selectedServerIndex = _me.Items.FindIndex(x => x.IsSelected);
            _gameState.ClientVars.SelectedServer =
                _gameState.ClientVars.DiscoveredServers[selectedServerIndex].GetEndPoint();
        }
    }
}