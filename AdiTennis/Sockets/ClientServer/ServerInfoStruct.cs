using System;
using System.Net;
using AdiTennis.Logic;
using ProtoBuf;

namespace AdiTennis.Sockets.ClientServer
{
    [ProtoContract]
    internal class ServerInfoStruct
    {
        [ProtoMember(1)] public string StructId = GameState.Config.DiscoveryServerInfoStructId;
        [ProtoMember(2)] public int Port;
        [ProtoMember(3)] public string PlayerNick;
        [ProtoMember(4)] public string IpAddress;

        public ServerInfoStruct()
        {
            // neccessary for protobuf
        }

        public ServerInfoStruct(IPAddress ipAddress)
        {
            PlayerNick = GameState.GetInstance().ClientServerVars.OwnNickname;
            IpAddress = ipAddress.ToString();
            Port = GameState.Config.Port;
        }

        public IPEndPoint GetEndPoint()
        {
            try
            {
                return new IPEndPoint(IPAddress.Parse(IpAddress), Port);
            }
            catch (FormatException)
            {
                return new IPEndPoint(0, 0);
            }
        }
    }
}