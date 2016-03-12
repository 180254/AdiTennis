using System.Net;
using AdiTennis.Logic;
using AdiTennis.Sockets;
using AdiTennis.Sockets.ClientServer;
using AdiTennis.StageAbstract.Menus;
using AdiTennis.StageAbstract.Stages;

namespace AdiTennis.GameStages.Server
{
    internal class AcceptOrRefuseClientBlockingStage : IBlockingStage
    {
        private readonly TcpClientStruct _tcpClient;
        private readonly MenuTemplate _me;

        public AcceptOrRefuseClientBlockingStage(TcpClientStruct tcpClient, string clientNick)
        {
            _tcpClient = tcpClient;

            _me = new MenuTemplate("ACCEPT OR REFUSE?.",
                "CLIENT: " + clientNick + " (" + ((IPEndPoint) _tcpClient.TcpClient.Client.RemoteEndPoint).Address + ")",
                GameState.Config.MsgColorHeader)
            {
                new MenuItem("ACCEPT"),
                new MenuItem("REFUSE")
            };
        }


        public void Go()
        {
            _me.Go();

            var selectedMenuItemIndex = _me.Items.FindIndex(x => x.IsSelected);
            _tcpClient.ResponseStatus = selectedMenuItemIndex == 0
                ? ServerToClientResponseEnum.ConnectionAccepted
                : ServerToClientResponseEnum.ConnectionRefused;
        }
    }
}