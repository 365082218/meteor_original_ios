//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using UnityEngine;

namespace Outfit7.Util {

    /// <summary>
    /// Build configuration.
    /// </summary>
    public static class BuildConfig {

        public static bool IsDevel {
            get {
                return Debug.isDebugBuild;
            }
        }

        public static bool IsProd {
            get {
#if PROD_BUILD
                return true;
#else
                return false;
#endif
            }
        }

        public static bool IsRelease {
            get {
                return !IsProdOrDevel;
            }
        }

        public static bool IsProdOrDevel {
            get {
                return IsDevel || IsProd;
            }
        }
    }
}
