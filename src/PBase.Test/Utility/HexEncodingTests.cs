using System;
using PBase.Utility;
using Xunit;

namespace PBase.Test.Utility
{
    public class TestHexEncodingTests
    {
        private static class TestConstants
        {
            public const string UnencodedText = "A string of text that can be encoded to and decoded from hex!";

            public const string HexEncodedText = "41-20-73-74-72-69-6E-67-20-6F-66-20-74-65-78-74-20-74-68-61-74-20-63-61-6E-20-62-65-20-65-6E-63-6F-64-65-64-20-74-6F-20-61-6E-64-20-64-65-63-6F-64-65-64-20-66-72-6F-6D-20-68-65-78-21";
        }

        [Fact]
        public void TestHexDecodeWithInvalidHexString()
        {
            const string hexString = "This Is An Invalid Hex String";
            Assert.Throws<ArgumentException>(() => { hexString.FromHex(); });
        }

        [Fact]
        public void TestHexDecoding()
        {
            const string hexString = TestConstants.HexEncodedText;
            var decodedString = hexString.FromHex();
            Assert.Equal(TestConstants.UnencodedText, decodedString);
        }

        [Fact]
        public void TestHexEncodeDecodeThrowsOnEmptyString()
        {
            const string emptyString = "";
            Assert.Throws<ArgumentException>(() => { emptyString.ToHex(); });
            Assert.Throws<ArgumentException>(() => { emptyString.FromHex(); });
        }

        [Fact]
        public void TestHexEncodeDecodeThrowsOnNullString()
        {
            const string nullString = null;
            Assert.Throws<ArgumentException>(() => { nullString.ToHex(); });
            Assert.Throws<ArgumentException>(() => { nullString.FromHex(); });
        }

        [Fact]
        public void TestHexEncoding()
        {
            const string stringToHex = TestConstants.UnencodedText;
            var encodedString = stringToHex.ToHex();
            Assert.Equal(TestConstants.HexEncodedText, encodedString);
        }
    }
}