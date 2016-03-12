using ProtoBuf;

namespace AdiTennis.Sockets.ClientServer
{
    [ProtoContract]
    public enum ConnectionStatusEnum
    {
        None = 0,
        ConnectionLost = 1,
        CannotListen = 2
    }
}