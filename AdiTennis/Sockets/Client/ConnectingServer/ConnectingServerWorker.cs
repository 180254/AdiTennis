using System;
using System.Net.Sockets;
using AdiTennis.Logic;
using AdiTennis.Sockets.AbstractWorker;
using AdiTennis.Sockets.ClientServer;

namespace AdiTennis.Sockets.Client.ConnectingServer
{
    internal class ConnectingServerWorker : IAutoClosableWorker
    {
        private readonly GameState _gameState = GameState.GetInstance();
        private TcpClientStruct _tcpClient;

        public void DoWork()
        {
            try
            {
                var tcp = new TcpClient();
                _tcpClient = new TcpClientStruct(tcp);
                _tcpClient.TcpClient.Connect(_gameState.ClientVars.SelectedServer);

                var nicknameBytes = NetworkHelper.SerializeObject(_gameState.ClientServerVars.OwnNickname);
                _tcpClient.NetworkStream.BeginWrite(nicknameBytes, 0, nicknameBytes.Length,
                    AsyncSendNicknameCallback, null);
            }
            catch (SocketException)
            {
                NotifyConnectingEnd(ServerToClientResponseEnum.ConnectionError);
            }
        }

        public void RequestStop()
        {
            throw new InvalidOperationException();
            // will be auto close if client connected
        }

        private void AsyncSendNicknameCallback(IAsyncResult ar)
        {
            _tcpClient.NetworkStream.EndWrite(ar);
            _tcpClient.NetworkStream.BeginRead(_tcpClient.ReadBuffer, 0, _tcpClient.ReadBuffer.Length,
                AsyncReadResponseCallback, null);
        }

        private void AsyncReadResponseCallback(IAsyncResult ar)
        {
            var read = _tcpClient.NetworkStream.EndRead(ar);

            if (read > 0)
            {
                var response =
                    NetworkHelper.DeserializeObject<ServerToClientResponseEnum>(_tcpClient.ReadBuffer, read);

                NotifyConnectingEnd(response == ServerToClientResponseEnum.None
                    ? ServerToClientResponseEnum.ConnectionError
                    : response);
            }
            else
            {
                NotifyConnectingEnd(ServerToClientResponseEnum.ConnectionError);
            }
        }

        private void NotifyConnectingEnd(ServerToClientResponseEnum serverToClientResponse)
        {
            _gameState.ClientVars.ServerResponse = serverToClientResponse;
            _gameState.ClientVars.ConnectedServer = _tcpClient;
            _gameState.ClientVars.ConnectionEstabilishedEvent.Set();
        }
    }
}