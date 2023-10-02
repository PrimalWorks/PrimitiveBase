using System.Collections.Generic;
using PBase.Utility;
using Xunit;

namespace PBase.Test.Utility
{
    public class IpValidationTests
    {
        [Fact]
        public void TestValidateIpv4Address()
        {
            var invalidIpv4 = new List<string>
            {
                "Just a string with no ip address...",
                "a.b.c.d",
                "1.a.2.b",
                "2",
                "1.2.3",
                "255.256.267.300"
            };

            var validIpv4 = new List<string>
            {
                "0.0.0.1",
                "127.0.0.1",
                "255.255.255.255"
            };

            foreach (var ip in invalidIpv4)
            {
                var isValid = ip.IsValidIpv4();
                Assert.False(isValid);
            }

            foreach (var ip in validIpv4)
            {
                var isValid = ip.IsValidIpv4();
                Assert.True(isValid);
            }
        }
    }
}