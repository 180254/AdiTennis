using AdiTennis.Logic;
using AdiTennis.StageAbstract.Menus;
using AdiTennis.StageAbstract.Stages;

namespace AdiTennis.GameStages.Client
{
    internal class ConnectionMethodBlockingStage : IBlockingStage
    {
        private readonly MenuTemplate _me =
            new MenuTemplate("CONNECTION METHOD.",
                "PLEASE CHOOSE SERVER SOURCE:",
                GameState.Config.MsgColorHeader)
            {
                new MenuItem("DISCOVERY (FIRST FOUND)"),
                new MenuItem("DISCOVERY (LIST ALL)"),
                new MenuItem("KNOWN IP")
            };

        public void Go()
        {
            _me.Go();

            var selectedMenuItemIndex = _me.Items.FindIndex(x => x.IsSelected);
            GameState.GetInstance().ClientVars.ConnectionMethod =
                (ConnectionMethodEnum) selectedMenuItemIndex;
        }
    }
}