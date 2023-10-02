using System;
using System.Net;
using System.Net.Sockets;

namespace PBase.Networking
{
    public interface ISocketAsyncEventArgs
    {
        int Offset { get; }
        int Count { get; }
        int BytesTransferred { get; }
        byte[] Buffer { get; }
        SocketAsyncOperation LastOperation { get; }
        void SetBuffer(byte[] buffer, int offset, int count);
        object UserToken { get; set; }
        EndPoint RemoteEndPoint { get; set; }
        event EventHandler<SocketAsyncEventArgs> Completed;
    }

    public interface ISocket : IDisposable
    {
        bool ConnectAsync(ISocketAsyncEventArgs args);
        bool ReceiveAsync(ISocketAsyncEventArgs args);
        bool SendAsync(ISocketAsyncEventArgs args);
        void Disconnect(bool reuseSocket);
    }

    public interface ISocketAsyncEventArgsFactory
    {
        ISocketAsyncEventArgs GetEmptyArgs();
        ISocketAsyncEventArgs AllocateArgs();
        void FreeArgs(ISocketAsyncEventArgs args);
        void FreeArgs(SocketAsyncEventArgs args);
    }
}
