//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using UnityEngine;
using Outfit7.Util;

namespace Outfit7.Threading {

    /// <summary>
    /// The executor that runs an enumerator looper via Unity coroutine. Coroutines are being run on the main/Unity thread.
    /// Coroutines are always being run serially, one after another, but the order of execution depends on the their scheduled times.
    /// This executor supports only coroutines that are enumerators which current element is null.
    /// </summary>
    public class CoroutineExecutor {

        private readonly MonoBehaviour Component;
        private readonly EnumeratorLooper Looper;
        private bool Running;

        public CoroutineExecutor(MonoBehaviour component) {
            Assert.NotNull(component, "component");
            Component = component;
            Looper = new EnumeratorLooper();
        }

        private IEnumerator<YieldInstruction> Run() {
            Running = true;

            // Wait for one update to prevent starting task immediatelly
            yield return null;

            while (true) {
                bool moreWork = Looper.LoopOnce();
                if (!moreWork) {
                    // No more work to do (no more tasks)
                    break;
                }

                // More work to do (task in progress or next task waiting)
                yield return null;
            }
            Running = false;
        }

        /// <summary>
        /// Posts the specified coroutine to be run ASAP.
        /// </summary>
        /// <param name='coroutine'>
        /// The coroutine to be run ASAP (null not permitted).
        /// </param>
        public void Post(IEnumerator<Null> coroutine) {
            PostAtTime(coroutine, DateTime.UtcNow);
        }

        /// <summary>
        /// Posts the specified coroutine to be run after the specified seconds.
        /// </summary>
        /// <param name='coroutine'>
        /// The coroutine to be run after the specified seconds (null not permitted).
        /// </param>
        /// <param name='delaySecs'>
        /// The seconds to delay run of the coroutine. If <= 0, the coroutine will be run ASAP.
        /// </param>
        public void PostDelayed(IEnumerator<Null> coroutine, double delaySecs) {
            PostAtTime(coroutine, DateTime.UtcNow.AddSeconds(delaySecs));
        }

        /// <summary>
        /// Posts the specified coroutine to be run at the specified time.
        /// </summary>
        /// <param name='coroutine'>
        /// The coroutine to be run at the specified time (null not permitted).
        /// </param>
        /// <param name='time'>
        /// The time when the coroutine should be run. If <= now, the coroutine will be run ASAP.
        /// </param>
        public void PostAtTime(IEnumerator<Null> coroutine, DateTime time) {
            Looper.Schedule(coroutine, time);

            if (!Running) {
                Component.StartCoroutine(Run());
            }
        }

        /// <summary>
        /// Removes all schedules of the specified coroutine.
        /// </summary>
        /// <returns>
        /// The number of schedules of the specified coroutine that were removed.
        /// </returns>
        /// <param name='coroutine'>
        /// The coroutine of schedules to be removed (null not permitted).
        /// </param>
        public int RemoveAllSchedules(IEnumerator<Null> coroutine) {
            return Looper.RemoveAllSchedules(coroutine);
        }
    }
}
