//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;

namespace Outfit7.Util {

    /// <summary>
    /// Version utilities using System.Version
    /// </summary>
    public static class VersionUtils {

        public static Version ParseMissingToZero(string version) {
            Version v = new Version(version);

            // Fix missing sub-version fields to 0
            int major = (v.Major == -1) ? 0 : v.Major;
            int minor = (v.Minor == -1) ? 0 : v.Minor;
            int build = (v.Build == -1) ? 0 : v.Build;
            int rev = (v.Revision == -1) ? 0 : v.Revision;

            return new Version(major, minor, build, rev);
        }
    }
}
