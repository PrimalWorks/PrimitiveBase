using PBase.Networking;
using PBase.Test.Support;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace PBase.Test.Networking
{
    public class PSocketTests : BaseUnitTest
    {
        public PSocketTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [Fact]
        public void TestPSocketAsyncEventArgs()
        {
            PSocketAsyncEventArgs sut = new PSocketAsyncEventArgs();
            Assert.NotNull(sut);

            var data = new byte[10];
            sut.SetBuffer(data, 0, data.Length);

            Assert.Equal(0, sut.Offset);
            Assert.Equal(0, sut.BytesTransferred);
            Assert.Equal(10, sut.Buffer.Length);
        }

        [Fact]
        public void TestPSocket()
        {
            PSocket sut = new PSocket(
                System.Net.Sockets.AddressFamily.InterNetwork,
                System.Net.Sockets.SocketType.Stream,
                System.Net.Sockets.ProtocolType.Tcp
                );

            Assert.NotNull(sut);
        }

        [Fact]
        public void TestPSocketArgsFactory()
        {
            PSocketArgsFactory sut = new PSocketArgsFactory(3000, 1000);

            var res1 = sut.GetEmptyArgs();
            Assert.NotNull(res1);
            Assert.Null(res1.Buffer);

            var res2 = sut.AllocateArgs();
            Assert.NotNull(res2);
            Assert.NotNull(res2.Buffer);
            Assert.Equal(0, res2.Offset);
            Assert.Equal(1000, res2.Count);

            var res3 = sut.AllocateArgs();
            Assert.NotNull(res3);
            Assert.NotNull(res3.Buffer);
            Assert.Equal(1000, res3.Offset);
            Assert.Equal(1000, res3.Count);

            var res4 = sut.AllocateArgs();
            Assert.NotNull(res4);
            Assert.NotNull(res4.Buffer);
            Assert.Equal(2000, res4.Offset);
            Assert.Equal(1000, res4.Count);

            Assert.Throws<ArgumentException>(() => sut.AllocateArgs());

            sut.FreeArgs(res3);

            var res5 = sut.AllocateArgs();
            Assert.NotNull(res5);
            Assert.NotNull(res5.Buffer);
            Assert.Equal(1000, res5.Offset);
            Assert.Equal(1000, res5.Count);

            var t1 = new SocketAsyncEventArgs();
            Assert.Throws<ArgumentException>(() => sut.FreeArgs(t1));

            PSocketAsyncEventArgs t2 = new PSocketAsyncEventArgs();
            t2.SetBuffer(new byte[10]);
            Assert.Throws<ArgumentException>(() => sut.FreeArgs(t2 as ISocketAsyncEventArgs));
        }
    }
}
