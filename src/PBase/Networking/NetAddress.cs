using System;
using System.Linq;
using System.Net;

namespace PBase.Networking
{
    public class NetAddress : IPEndPoint
    {
        public NetAddress() : base(0, 0)
        {
        }

        public NetAddress(string addrAndOrPort) : base(0, 0)
        {
            string[] split = addrAndOrPort.Split(':');
            if (split.Length == 1 && !string.IsNullOrEmpty(split[0]))
            {
                this.Address = IPAddress.Parse(split[0]);
                this.Port = 0;
            }
            else if (split.Length == 2 && !string.IsNullOrEmpty(split[0]) && !string.IsNullOrEmpty(split[1]))
            {
                this.Address = IPAddress.Parse(split[0]);
                this.Port = Int32.Parse(split[1]);
            }
            else if (split.Length == 2 && string.IsNullOrEmpty(split[0]) && !string.IsNullOrEmpty(split[1]))
            {
                this.Address = IPAddress.Any;
                this.Port = Int32.Parse(split[1]);
            }
            else
            {
                throw new ArgumentException("Address Format is Unknown", "addrAndOrPort");
            }
        }

        public NetAddress(string addr, int port) : base(0, 0)
        {
            this.Address = IPAddress.Parse(addr);
            this.Port = port;
        }

        public NetAddress(IPAddress addr, int port) : base(0, 0)
        {
            this.Address = addr;
            this.Port = port;
        }

        public NetAddress(int port) : base(0, 0)
        {
            this.Address = IPAddress.Any;
            this.Port = port;
        }

        public NetAddress(IPEndPoint ep) : base(0, 0)
        {
            this.Address = ep.Address;
            this.Port = ep.Port;
        }

        public override string ToString()
        {
            if (Port > 0)
            {
                return string.Format("{0}:{1}", Address.ToString(), Port.ToString());
            }
            else
            {
                return Address.ToString();
            }
        }

        public bool IsPossibleBroadcast => NetAddress.IsPossibleBroadcastAddress(this);

        public bool IsMulticast => NetAddress.IsMulticastAddress(this);

        public static bool IsPossibleBroadcastAddress(NetAddress addr)
        {
            int sequentialOnes = 0;
            foreach(var oct in addr.Address.GetAddressBytes().Reverse())
            {
                for(int bit = 0; bit < 8; bit++)
                {
                    if(((oct >> bit) & 0x01) == 0x01)
                    {
                        sequentialOnes++;
                    }
                    else
                    {
                        return (sequentialOnes > 1);
                    }
                }
            }
            return true;
        }

        public static bool IsMulticastAddress(NetAddress addr)
        {
            var top = addr.Address.GetAddressBytes().First();

            return (top >= 224 && top <= 239);
        }
    }
}

