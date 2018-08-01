//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

namespace Outfit7.Util.Io {

    /// <summary>
    /// The wrapper for system's Directory, which does not exist on Windows Runtime, but is partly implemented by Unity.
    /// </summary>
    public static class O7Directory {

        public static bool Exists(string path) {
#if NETFX_CORE
            return MarkerMetro.Unity.WinLegacy.Plugin.IO.Directory.Exists(path);
#else
            return System.IO.Directory.Exists(path);
#endif
        }

        public static void CreateDirectory(string path) {
#if NETFX_CORE
            MarkerMetro.Unity.WinLegacy.Plugin.IO.Directory.CreateDirectory(path);
#else
            System.IO.Directory.CreateDirectory(path);
#endif
        }

        public static bool Delete(string path, bool recursive) {
            // If directory or any parent does not exist, exception is thrown
            // Not thread safe!
            if (!Exists(path)) return false;

#if NETFX_CORE
            MarkerMetro.Unity.WinLegacy.Plugin.IO.Directory.Delete(path, recursive);
#else
            System.IO.Directory.Delete(path, recursive);
#endif
            return true;
        }
    }
}
