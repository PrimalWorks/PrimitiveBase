using System;
using System.Text;

namespace PBase.Utility
{
    public static class HexEncoding
    {
        /// <summary>Converts a text string into a hex encoded format.</summary>
        /// <param name="stringToHex">The string to be hex encoded.</param>
        /// <returns>The hex encoded string.</returns>
        /// <exception cref="FormatException"> Thrown when a null or empty stringToHex is provided</exception>
        public static string ToHex(this string stringToHex)
        {
            if (string.IsNullOrEmpty(stringToHex))
            {
                throw new System.ArgumentException("String to hex cannot be null or empty");
            }

            var bytes = Encoding.Default.GetBytes(stringToHex);
            var hexString = BitConverter.ToString(bytes);

            return hexString;
        }

        /// <summary>Decodes a hex encoded string.</summary>
        /// <param name="hexString">The hex encoded string to decode.</param>
        /// <returns>The decoded hex string.</returns>
        /// <exception cref="FormatException"> Thrown when a null, empty, or invalid hexString is provided</exception>
        public static string FromHex(this string hexString)
        {
            if (string.IsNullOrEmpty(hexString))
            {
                throw new System.ArgumentException("Hex string cannot be null or empty");
            }

            hexString = hexString.Replace("-", "");

            if (hexString.Length % 2 != 0)
            {
                throw new System.ArgumentException("Hex string length cannot be an odd number");
            }

            var bytes = new byte[hexString.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return Encoding.Default.GetString(bytes);
        }
    }
}