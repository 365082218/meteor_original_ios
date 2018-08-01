//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//

using System;
using System.Text;

namespace Outfit7.Util {

    /// <summary>
    /// Unique identifier (UDID/UID) converter.
    /// Not available on real builds - highly sensitive information
    /// </summary>
    public static class UniqueIdConverter {

#if UNITY_EDITOR || NATIVE_SIM
        private static readonly int[] UidInsertPositions = { 2, 3, 5, 7, 11, 14, 17, 19, 23, 29, 31 };
        private static readonly int[] UidXor = { 212, 162, 187, 147, 207, 241, 106, 94, 64, 144, 173, 247, 86, 209,
            214, 248, 20, 216, 66, 37, 126, 159, 1, 200, 103, 198, 148, 149, 130, 210, 137, 7, 48, 22, 128, 96, 142, 14,
            97, 110, 182, 179, 226, 246, 253, 247, 48, 180, 65, 237, 148, 137, 140, 35, 0, 134, 171, 178, 35, 228
        };

        public static string Udid2Uid(string udid) {
            StringBuilder uid = new StringBuilder(udid, udid.Length + UidInsertPositions.Length);
            for (int i = UidInsertPositions.Length - 1; i >= 0; i--) {
                const char randomChar = '\0';
                if (UidInsertPositions[i] <= uid.Length) {
                    uid.Insert(UidInsertPositions[i], randomChar);
                }
            }

            // Reverse string
            byte[] data = Encoding.GetEncoding("iso-8859-1").GetBytes(uid.ToString());
            for (int i = 0; i < data.Length / 2; i++) {
                byte tmp = data[i];
                data[i] = data[data.Length - i - 1];
                data[data.Length - i - 1] = tmp;
            }

            // XOR
            for (int i = 0; (i < data.Length) && (i < UidXor.Length); i++) {
                data[i] = (byte) (data[i] ^ UidXor[i]);
            }

            return ToBase64UrlSafeString(data);
        }
#endif

        public static string ToBase64UrlSafeString(byte[] data) {
            string t = Convert.ToBase64String(data);
            t = t.Replace("=", string.Empty).Replace('+', '-').Replace('/', '_');
            t = t.PadRight(t.Length + (4 - t.Length % 4) % 4, '=');
            return t;
        }
    }
}
