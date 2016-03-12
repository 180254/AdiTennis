using ProtoBuf;

namespace AdiTennis.Sockets.ClientServer
{
    [ProtoContract]
    public enum ServerToClientResponseEnum
    {
        None = 0,
        ConnectionAccepted = 1,
        ConnectionRefused = 2,
        ConnectionError = 3,
        BusyServer = 4,
        ChangeNick = 5
    }
}