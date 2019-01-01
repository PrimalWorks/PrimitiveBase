using System;
using System.Net.Sockets;

namespace PBase.Networking
{
    public class PSocketAsyncEventArgs : SocketAsyncEventArgs, ISocketAsyncEventArgs
    {
        public new int Offset => base.Offset;
        public new int Count => base.Count;
        public new int BytesTransferred => base.BytesTransferred;
        public new byte[] Buffer => base.Buffer;

        public new void SetBuffer(byte[] buffer, int offset, int count)
        {
            base.SetBuffer(buffer, offset, count);
        }
    }
}
