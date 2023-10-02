using System;
using System.Net;
using System.Net.Sockets;

namespace PBase.Networking
{
    public class PSocket : Socket, ISocket
    {
        private const int DEFAULT_BACKLOG = 128;

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

        public void Disconnect(bool reuseSocket)
        {
            base.Disconnect(reuseSocket);
        }

        public void Bind(NetAddress local)
        {
            base.Bind(local);
        }

        public void Listen()
        {
            base.Listen(DEFAULT_BACKLOG);
        }

        public void Listen(int backlog)
        {
            base.Listen(backlog);
        }

        public bool AcceptAsync(ISocketAsyncEventArgs args)
        {
            return base.AcceptAsync((SocketAsyncEventArgs)args);
        }
    }
}
