using PBase.Networking;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Xunit;

namespace PBase.Test.Networking
{
    public class NetAddressTests
    {
        [Fact]
        public void TestNetAddress()
        {
            var sut = new NetAddress();
            Assert.NotNull(sut);
            Assert.Equal(0, sut.Port);
            Assert.Equal(IPAddress.Any, sut.Address);

            sut = new NetAddress("10.10.10.10");
            Assert.NotNull(sut);
            Assert.Equal(0, sut.Port);
            Assert.Equal("10.10.10.10", sut.ToString());

            sut = new NetAddress("10.10.10.10:5555");
            Assert.NotNull(sut);
            Assert.Equal(5555, sut.Port);
            Assert.Equal("10.10.10.10:5555", sut.ToString());

            sut = new NetAddress(":1234");
            Assert.NotNull(sut);
            Assert.Equal(1234, sut.Port);
            Assert.Equal(IPAddress.Any, sut.Address);

            Assert.Throws<ArgumentException>(()=> new NetAddress(":"));
            Assert.Throws<ArgumentException>(() => new NetAddress(""));

            sut = new NetAddress("10.10.10.10", 54321);
            Assert.NotNull(sut);
            Assert.Equal(54321, sut.Port);
            Assert.Equal("10.10.10.10:54321", sut.ToString());

            sut = new NetAddress(IPAddress.Parse("10.10.10.10"), 1234);
            Assert.NotNull(sut);
            Assert.Equal(1234, sut.Port);
            Assert.Equal("10.10.10.10:1234", sut.ToString());

            sut = new NetAddress(7777);
            Assert.NotNull(sut);
            Assert.Equal(7777, sut.Port);
            Assert.Equal(IPAddress.Any, sut.Address);

            sut = new NetAddress(new IPEndPoint(IPAddress.Parse("1.2.3.4"), 4321));
            Assert.NotNull(sut);
            Assert.Equal(4321, sut.Port);
            Assert.Equal("1.2.3.4:4321", sut.ToString());

            sut = new NetAddress("255.255.255.255");
            Assert.NotNull(sut);
            Assert.True(sut.IsPossibleBroadcast);

            sut = new NetAddress("239.0.0.1");
            Assert.NotNull(sut);
            Assert.True(sut.IsMulticast);
        }

        [Theory]
        [InlineData("255.255.255.255", true)]
        [InlineData("10.255.255.255", true)]
        [InlineData("10.10.255.255", true)]
        [InlineData("10.10.10.255", true)]
        [InlineData("10.10.10.127", true)]
        [InlineData("10.10.10.63", true)]
        [InlineData("10.10.10.31", true)]
        [InlineData("10.10.10.15", true)]
        [InlineData("10.10.10.7", true)]
        [InlineData("10.10.10.3", true)]
        [InlineData("10.10.10.1", false)]
        [InlineData("255.255.255.254", false)]
        [InlineData("255.255.255.253", false)]
        public void TestBroadcastAddresses(string addr, bool isBroadcast)
        {
            Assert.Equal(isBroadcast, NetAddress.IsPossibleBroadcastAddress(new NetAddress(addr)));
        }

        [Theory]
        [InlineData("223.0.0.1", false)]
        [InlineData("224.0.0.1", true)]
        [InlineData("225.0.0.1", true)]
        [InlineData("238.0.0.1", true)]
        [InlineData("239.0.0.1", true)]
        [InlineData("240.0.0.1", false)]
        [InlineData("239.255.255.255", true)]
        [InlineData("239.240.240.1", true)]
        [InlineData("240.240.240.1", false)]
        [InlineData("255.0.0.0", false)]
        public void TestMulticastAddresses(string addr, bool isMulticast)
        {
            Assert.Equal(isMulticast, NetAddress.IsMulticastAddress(new NetAddress(addr)));
        }
    }
}
