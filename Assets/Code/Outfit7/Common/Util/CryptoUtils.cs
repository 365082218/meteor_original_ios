//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using System.Text;

namespace Outfit7.Util {

    /// <summary>
    /// Cryptography utils.
    /// </summary>
    public static class CryptoUtils {

        /// <summary>
        /// Computes SHA1 hexadecimal hash of the given UTF-8 encoded value.
        /// </summary>
        /// <param name='value'>
        /// The UTF-8 encoded value.
        /// </param>
        public static string Sha1(string value) {
            byte[] valueBuffer = Encoding.UTF8.GetBytes(value);
            return Sha1(valueBuffer);
        }

        /// <summary>
        /// Computes SHA1 hexadecimal hash of the given bytes.
        /// </summary>
        /// <param name='value'>
        /// The bytes value.
        /// </param>
        public static string Sha1(byte[] value) {
#if NETFX_CORE
            var sha1 = MarkerMetro.Unity.WinLegacy.Security.Cryptography.SHA1.Create();
#else
            var sha1 = new System.Security.Cryptography.SHA1Managed();
#endif
            byte[] hashBuffer = sha1.ComputeHash(value);
            string delimitedHash = BitConverter.ToString(hashBuffer);
            return delimitedHash.Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
