using System;
using System.IO;
using System.Threading;
using AdiTennis.Logic;
using AdiTennis.Sockets.AbstractWorker;

namespace AdiTennis.Sockets.Client.PlayingGame
{
    internal class ClientRacketPosSenderWorker : IWorker
    {
        private readonly TcpClientStruct _server = GameState.GetInstance().ClientVars.ConnectedServer;
        private volatile bool _shouldStop;

        public void DoWork()
        {
            try
            {
                do
                {
                    while (!Console.KeyAvailable && !_shouldStop)
                    {
                        Thread.Sleep(GameState.Config.ReadKeyFpsAsMilisecondsSleep);
                    }
                    if (_shouldStop) continue;

                    ConsoleKeyInfo key;
                    lock (GameState.Config.TennisStagePrinterLock)
                    {
                        key = Console.ReadKey(false);
                    }

                    if (key.Key != ConsoleKey.UpArrow && key.Key != ConsoleKey.DownArrow) continue;
                    var keyBytes = NetworkHelper.SerializeObject(key.Key);

                    _server.NetworkStream.BeginWrite(keyBytes, 0, keyBytes.Length, AsyncWriteCallback, null);
                } while (!_shouldStop);
            }
            catch (IOException)
            {
            }
        }


        private void AsyncWriteCallback(IAsyncResult ar)
        {
            try
            {
                _server.NetworkStream.EndWrite(ar);
            }
            catch (IOException)
            {
            }
        }

        public void RequestStop()
        {
            _shouldStop = true;
        }
    }
}