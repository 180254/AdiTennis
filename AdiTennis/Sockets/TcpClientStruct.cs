using System;
using System.IO;
using System.Net.Sockets;
using AdiTennis.Sockets.ClientServer;

namespace AdiTennis.Sockets
{
    public class TcpClientStruct
    {
        public const int BuffersSize = 4096;
        public byte[] ReadBuffer = new byte[BuffersSize];
        public TcpClient TcpClient;
        public ServerToClientResponseEnum ResponseStatus;

        public TcpClientStruct(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
        }

        public NetworkStream NetworkStream
        {
            get
            {
                try
                {
                    return TcpClient.GetStream();
                }
                catch (InvalidOperationException ex)
                {
                    throw new IOException(null, ex);
                }
            }
        }
    }
}