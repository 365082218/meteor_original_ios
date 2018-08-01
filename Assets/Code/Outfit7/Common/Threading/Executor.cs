//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;

namespace Outfit7.Threading {

    /// <summary>
    /// The simple executor of posted actions. Just uses current thread's looper to post actions on.
    /// Be sure that current thread is main/Unity thread with <see cref="MainExecutor"/> running or
    /// was created with <see cref="ThreadExecutor"/>.
    /// </summary>
    public class Executor {

        protected Looper looper;

        /// <summary>
        /// Creates new simple executor and attaches looper to it.
        /// </summary>
        /// <exception cref='InvalidOperationException'>
        /// Is thrown when looper does not exist for the current thread.
        /// </exception>
        public Executor() : this(true) {
        }

        protected Executor(bool attachLooper) {
            if (!attachLooper)
                return;

            if (!Looper.DoesLooperExistForCurrentThread) {
                throw new InvalidOperationException("Looper does not exist for current thread");
            }
            looper = Looper.ThreadInstance;
        }

        /// <summary>
        /// Posts the specified action to be run ASAP on this executor's looper.
        /// </summary>
        /// <param name='action'>
        /// The action to be run ASAP (null not permitted).
        /// </param>
        public virtual void Post(Action action) {
            PostAtTime(action, DateTime.UtcNow);
        }

        /// <summary>
        /// Posts the specified action to be run after the specified seconds on this executor's looper.
        /// </summary>
        /// <param name='action'>
        /// The action to be run after the specified seconds (null not permitted).
        /// </param>
        /// <param name='delaySecs'>
        /// The seconds to delay run of the action. If <= 0, the action will be run ASAP.
        /// </param>
        public virtual void PostDelayed(Action action, double delaySecs) {
            PostAtTime(action, DateTime.UtcNow.AddSeconds(delaySecs));
        }

        /// <summary>
        /// Posts the specified action to be run at the specified time on this executor's looper.
        /// </summary>
        /// <param name='action'>
        /// The action to be run at the specified time (null not permitted).
        /// </param>
        /// <param name='time'>
        /// The time when the action should be run. If <= now, the action will be run ASAP.
        /// </param>
        public virtual void PostAtTime(Action action, DateTime time) {
            looper.Schedule(action, time, false);
        }

        /// <summary>
        /// Posts the specified action to be run ASAP before any other action on this executor's looper.
        /// </summary>
        /// <param name='action'>
        /// The action to be run ASAP before any other action (null not permitted).
        /// </param>
        public virtual void PostAtFrontQueue(Action action) {
            looper.Schedule(action, DateTime.UtcNow, true);
        }

        /// <summary>
        /// Removes all schedules of the specified action from this executor's looper.
        /// </summary>
        /// <returns>
        /// The number of schedules of the specified action that were removed.
        /// </returns>
        /// <param name='action'>
        /// The action of schedules to be removed (null not permitted).
        /// </param>
        public int RemoveAllSchedules(Action action) {
            return looper.RemoveAllSchedules(action);
        }
    }
}
