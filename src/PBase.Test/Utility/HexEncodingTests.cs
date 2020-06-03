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
            public const string HexEncodedText = "4120737472696e67206f66207465787420746861742063616e20626520656e636f64656420746f20616e64206465636f6465642066726f6d2068657821";
        }

        [Fact]
        public void TestHexEncoding()
        {
            const string stringToHex = TestConstants.UnencodedText;
            var encodedString = stringToHex.ToHex();
            Assert.Equal(TestConstants.HexEncodedText, encodedString);
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
            Assert.Throws<ArgumentException>(() => { emptyString.ToHex();});
            Assert.Throws<ArgumentException>(() => { emptyString.FromHex();});
        }

        [Fact]
        public void TestHexEncodeDecodeThrowsOnNullString()
        {
            const string nullString = null;
            Assert.Throws<ArgumentException>(() => { nullString.ToHex();});
            Assert.Throws<ArgumentException>(() => { nullString.FromHex();});
        }

        [Fact]
        public void TestHexDecodeWithInvalidHexString()
        {
            const string hexString = "This Is An Invalid Hex String";
            Assert.Throws<FormatException>(() => { hexString.FromHex();});
        }
    }
}
