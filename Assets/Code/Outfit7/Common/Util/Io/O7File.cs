//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

using System;

namespace Outfit7.Util.Io {

    /// <summary>
    /// The wrapper for system's File, which does not exist on Windows Runtime, but is partly implemented by Unity &amp; MarkerMetro.
    /// </summary>
    public static class O7File {

        public static bool Exists(string path) {
#if NETFX_CORE
            return MarkerMetro.Unity.WinLegacy.Plugin.IO.File.Exists(path);
#else
            return System.IO.File.Exists(path);
#endif
        }

        public static bool Delete(string path) {
            // If any parent directory does not exist, exception is thrown
            // Not thread safe!
            if (!Exists(path)) return false;

#if NETFX_CORE
            MarkerMetro.Unity.WinLegacy.Plugin.IO.File.Delete(path);
#else
            System.IO.File.Delete(path);
#endif
            return true;
        }

        public static void Copy(string sourceFileName, string destFileName) {
#if NETFX_CORE
            MarkerMetro.Unity.WinLegacy.Plugin.IO.File.Copy(sourceFileName, destFileName);
#else
            System.IO.File.Copy(sourceFileName, destFileName);
#endif
        }


        public static DateTime GetLastWriteTime(string path) {
#if NETFX_CORE
            return MarkerMetro.Unity.WinLegacy.Plugin.IO.File.GetLastWriteTime(path);
#else
            return System.IO.File.GetLastWriteTime(path);
#endif
        }

        public static string ReadAllText(string path) {
#if NETFX_CORE
            return MarkerMetro.Unity.WinLegacy.Plugin.IO.File.ReadAllText(path);
#elif UNITY_WP8
            using (System.IO.StreamReader reader = System.IO.File.OpenText(path)) {
                return reader.ReadToEnd();
            }
#else
            return System.IO.File.ReadAllText(path);
#endif
        }

        public static void WriteAllText(string path, string data) {
#if NETFX_CORE
            MarkerMetro.Unity.WinLegacy.Plugin.IO.File.WriteAllText(path, data);
#elif UNITY_WP8
            using (System.IO.StreamWriter writer = System.IO.File.CreateText(path)) {
                writer.Write(data);
            }
#else
            System.IO.File.WriteAllText(path, data);
#endif
        }

        public static byte[] ReadAllBytes(string path) {
#if NETFX_CORE
            return MarkerMetro.Unity.WinLegacy.Plugin.IO.File.ReadAllBytes(path);
#elif UNITY_WP8
            using (System.IO.FileStream stream = System.IO.File.OpenRead(path))
            {
                byte[] data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
                return data;
            }
#else
            return System.IO.File.ReadAllBytes(path);
#endif
        }

        public static void WriteAllBytes(string path, byte[] data) {
#if NETFX_CORE
            MarkerMetro.Unity.WinLegacy.Plugin.IO.File.WriteAllBytes(path, data);
#elif UNITY_WP8
            using (System.IO.FileStream stream = System.IO.File.Create(path))
            {
                stream.Write(data, 0, data.Length);
            }
#else
            System.IO.File.WriteAllBytes(path, data);
#endif
        }
    }
}
