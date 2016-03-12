using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using ProtoBuf;

namespace AdiTennis.Sockets
{
    internal static class NetworkHelper
    {
        #region Network public
        public static NetworkInterface GetNetworkInterfaceForIp(IPAddress ipAddress)
        {
            return (from networkInterface in NetworkInterface.GetAllNetworkInterfaces()
                where
                    networkInterface.OperationalStatus == OperationalStatus.Up &&
                    GetIpForNetworkInterface(networkInterface) != null
                let broadcastIp = GetBroadcastAddressForNetworkInterface(networkInterface)
                let networkIp = GetNetworkAddressForNetworkInterface(networkInterface)
                where CompareIPs(ipAddress, networkIp) > 0 && CompareIPs(ipAddress, broadcastIp) < 0
                select networkInterface).FirstOrDefault();
        }

        public static List<IPAddress> GetAllBroadcastIps()
        {
            return (from networkInterface in NetworkInterface.GetAllNetworkInterfaces()
                where networkInterface.OperationalStatus == OperationalStatus.Up
                select GetBroadcastAddressForNetworkInterface(networkInterface)
                into broadcastAddress
                where broadcastAddress != null
                select broadcastAddress).ToList();
        }

        public static IPAddress GetIpForNetworkInterface(NetworkInterface networkInterface)
        {
            return (from ipAddressInformation in networkInterface.GetIPProperties().UnicastAddresses
                where ipAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork
                select ipAddressInformation.Address).FirstOrDefault();
        }

        public static IPAddress GetBroadcastAddressForNetworkInterface(NetworkInterface networkInterface)
        {
            var ipAddress = GetIpForNetworkInterface(networkInterface);
            if (ipAddress == null) return null;

            var ipv4Mask = GetIPv4MaskForNetworkInterface(networkInterface);

            var ipAddressBytes = ipAddress.GetAddressBytes();
            var ipv4MaskBytes = ipv4Mask.GetAddressBytes();
            var broadcastBytes = new byte[ipv4MaskBytes.Length];
            for (var i = 0; i < ipv4MaskBytes.Length; i++)
                broadcastBytes[i] = (byte) (ipAddressBytes[i] | ~ipv4MaskBytes[i]);

            return new IPAddress(broadcastBytes);
        }
        #endregion

        #region (De)?[Ss]erialize (protobuf)
        public static byte[] SerializeObject(object serializable)
        {
            using (var memory = new MemoryStream())
            {
                Serializer.Serialize(memory, serializable);
                var resultBytes = memory.GetBuffer();
                return resultBytes.Take((int) memory.Length).ToArray();
            }
        }

        public static T DeserializeObject<T>(byte[] bytes)
        {
            return DeserializeObject<T>(bytes, bytes.Length);
        }

        public static T DeserializeObject<T>(byte[] bytes, int count)
        {
            try
            {
                var realBytes = bytes.Take(count).ToArray();
                return Serializer.Deserialize<T>(new MemoryStream(realBytes));
            }
            catch (ProtoException)
            {
                return default(T);
            }
            catch (InvalidOperationException)
            {
                return default(T);
            }
        }
        #endregion

        #region Network private
        private static int CompareIPs(IPAddress ip1, IPAddress ip2)
        {
            var ip1OctetsInt = ip1.ToString().Split('.').Select(int.Parse).ToArray();
            var ip2OctetsInt = ip2.ToString().Split('.').Select(int.Parse).ToArray();

            for (var i = 0; i < ip1OctetsInt.Length; i++)
            {
                if (ip1OctetsInt[i] > ip2OctetsInt[i])
                    return 1;
                if (ip2OctetsInt[i] > ip1OctetsInt[i])
                    return -1;
            }
            return 0;
        }

        private static IPAddress GetNetworkAddressForNetworkInterface(NetworkInterface networkInterface)
        {
            var ipAddress = GetIpForNetworkInterface(networkInterface);
            if (ipAddress == null) return null;

            var ipv4Mask = GetIPv4MaskForNetworkInterface(networkInterface);

            var ipAddressBytes = ipAddress.GetAddressBytes();
            var ipv4MaskBytes = ipv4Mask.GetAddressBytes();
            var broadcastBytes = new byte[ipv4MaskBytes.Length];
            for (var i = 0; i < ipv4MaskBytes.Length; i++)
                broadcastBytes[i] = (byte) (ipAddressBytes[i] & ipv4MaskBytes[i]);

            return new IPAddress(broadcastBytes);
        }

        private static IPAddress GetIPv4MaskForNetworkInterface(NetworkInterface networkInterface)
        {
            return (
                from ipAddressInformation in networkInterface.GetIPProperties().UnicastAddresses
                where ipAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork
                select ipAddressInformation.IPv4Mask)
                .FirstOrDefault();
        }
        #endregion
    }
}