//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using Outfit7.Util;

#if NETFX_CORE
using MarkerMetro.Unity.WinLegacy.Threading;
#else
using System.Threading;
#endif

namespace Outfit7.Threading {

    /// <summary>
    /// The simple worker puts the specified action in the system's thread pool.
    /// </summary>
    public static class SimpleWorker {

        private const string Tag = "SimpleWorker";

        public static void RunAsync(Action job) {
            Assert.NotNull(job, "job");

            ThreadPool.QueueUserWorkItem(delegate {
                try {
                    job();

                } catch (Exception e) {
                    O7Log.WarnT(Tag, e, "Action threw Exception");
                }
            });
        }
    }
}
