using PBase.Test.Support;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;
using PBase.Networking;
using System.Net.Sockets;
using System.Net;
using Xunit;
using System.Threading.Tasks;
using System.Threading;
using PBase.Logging;

namespace PBase.Test.Networking
{
    public class SocketConnTests : BaseUnitTest
    {
        public SocketConnTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        // Removing this test for now as it access the internet
        [Fact]
        public async void TestGETRequest()
        {
            int received = 0;
            var factory = new PSocketArgsFactory(8384, 512);

            using (var cli = new SocketConn(new PSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp), factory))
            {
                var connect = await cli.Connect(
                   //new NetAddress("178.62.88.250", 80),
                   new NetAddress("54.161.141.91", 80),
                   (a) =>
                   {
                       if (a.BytesTransferred > 0)
                       {
                           received += a.BytesTransferred;
                       }
                       return true;
                   }
                );

                Assert.True(connect);

                var sb = new StringBuilder();
                sb.Append("GET /anything HTTP/1.1\r\n");
                sb.Append("Host: ionot.com\r\n");
                sb.Append("Connection: keep-alive\r\n");
                sb.Append("Cache-Control: no-cache\r\n");
                sb.Append("User-Agent: PrimitiveBase\r\n");
                sb.Append("Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8\r\n");
                sb.Append("Accept-Encoding: gzip, deflate\r\n");
                sb.Append("Accept-Language: en-GB,en;q=0.9,en-US;q=0.8\r\n");
                sb.Append("\r\n");

                var bytes = ASCIIEncoding.ASCII.GetBytes(sb.ToString());

                var send = await cli.SendBuffer(bytes);
                Assert.True(send);

                Thread.Sleep(3000);

                Assert.NotEqual(0, received);
            }
        }
    }

    public class TestArgsFactory : ISocketAsyncEventArgsFactory
    {
        public TestArgsFactory()
        {

        }

        public ISocketAsyncEventArgs AllocateArgs()
        {
            return new TestSocketArgs();
        }

        public void FreeArgs(ISocketAsyncEventArgs args)
        {
            // Impl not required for test
        }

        public void FreeArgs(SocketAsyncEventArgs args)
        {
            // Impl not required for test
        }

        public ISocketAsyncEventArgs GetEmptyArgs()
        {
            return new TestSocketArgs();
        }
    }

    public class TestSocketArgs : ISocketAsyncEventArgs
    {
        public int Offset { get; set; }

        public int Count { get; set; }

        public int BytesTransferred { get; set; }

        public byte[] Buffer { get; set; }

        public SocketAsyncOperation LastOperation { get; set; }

        public object UserToken { get; set; }
        public EndPoint RemoteEndPoint { get; set; }

        public event EventHandler<SocketAsyncEventArgs> Completed;

        public ISocket AcceptSocket { get; set; }

        public void SetBuffer(byte[] buffer, int offset, int count)
        {
            Buffer = buffer;
            Offset = offset;
            Count = count;
        }
    }
}
