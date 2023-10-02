using PBase.Networking;
using PBase.Test.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PBase.Test.Networking
{
    public class SocketListenerTests : BaseUnitTest
    {
        public SocketListenerTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [Fact]
        public void TestListener()
        {
            int received = 0;
            var factory = new PSocketArgsFactory(8384, 512);

            var listener = new SocketListener(new PSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp), factory);

            var listen = listener.Listen(
                new NetAddress("0.0.0.0", 55667),
                (a) =>
                {
                    return true;
                }
            );
            listen.ContinueWith((res) =>
            {
                MakeConnection();
            });

            Thread.Sleep(3000);
        }

        private void MakeConnection()
        {
            int received = 0;
            var factory = new PSocketArgsFactory(8384, 512);

            using (var cli = new SocketConn(new PSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp), factory))
            {
                var connect = cli.Connect(
                   new NetAddress("127.0.0.1", 55667),
                   (a) =>
                   {
                       if (a.BytesTransferred > 0)
                       {
                           received += a.BytesTransferred;
                       }
                       return true;
                   }
                );
                connect.ContinueWith((res) =>
                {
                    Assert.True(res.Result);
                    Task.Run(() =>
                    {

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

                        var send = cli.SendBuffer(bytes);
                        send.ContinueWith((res2) =>
                        {
                            //Assert.True(res2.Result);
                        });
                    });
                });

                Thread.Sleep(3000);

                //Assert.NotEqual(0, received);
            }
        }
    }
}
