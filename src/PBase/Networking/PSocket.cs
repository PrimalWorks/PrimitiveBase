using System;
using System.Net;
using System.Net.Sockets;

namespace PBase.Networking
{
    public class PSocket : Socket, ISocket
    {
        public PSocket(AddressFamily family, SocketType socketType, ProtocolType protocolType)
            : base(family, socketType, protocolType)
        {

        }

        public bool ConnectAsync(ISocketAsyncEventArgs args)
        {
            return base.ConnectAsync((SocketAsyncEventArgs)args);
        }

        public bool ReceiveAsync(ISocketAsyncEventArgs args)
        {
            return base.ReceiveAsync((SocketAsyncEventArgs)args);
        }

        public bool SendAsync(ISocketAsyncEventArgs args)
        {
            return base.SendAsync((SocketAsyncEventArgs)args);
        }
    }
}
