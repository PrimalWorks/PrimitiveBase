namespace PBase.Utility
{
    public static class IpValidation
    {
        /// <summary>
        ///     Extension method to check if a string is a valid IPV4 address.
        /// </summary>
        /// <param name="ipString">The ip address string to validate.</param>
        /// <returns>
        ///     True if ipString is a valid IPV4 address; false if it is not.
        /// </returns>
        public static bool IsValidIpv4(this string ipString)
        {
            var octets = ipString.Split('.');
            if (octets.Length != 4) return false;

            foreach (var octet in octets)
            {
                if (octet.Length > 3) return false;

                if (!int.TryParse(octet, out var temp)) return false;

                if (temp < 0 || temp > 255) return false;
            }

            return true;
        }
    }
}