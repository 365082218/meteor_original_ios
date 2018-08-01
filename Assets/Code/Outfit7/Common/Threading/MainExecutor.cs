//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using UnityEngine;

namespace Outfit7.Threading {

    /// <summary>
    /// The Unity script/component/controller style executor that runs a looper on the main/Unity thread.
    /// More than one instance of this class can be created, but is not recommended.
    /// </summary>
    public class MainExecutor : MonoBehaviour {

        private Looper looper;
        private int mainThreadId;

        protected virtual void Awake() {
            looper = Looper.ThreadInstance;
            mainThreadId = O7Thread.CurrentThreadId;
        }

        protected virtual void Update() {
            looper.LoopOnce();
        }

        /// <value>
        /// <c>true</c> if the current thread is main/Unity thread.
        /// </value>
        public bool IsMainThread {
            get {
                return O7Thread.CurrentThreadId == mainThreadId;
            }
        }

        /// <summary>
        /// Runs the specified action on the main/Unity thread.
        /// Runs it immediately if the current thread is the main/Unity thread,
        /// otherwise posts it to be run on the main/Unity thread.
        /// </summary>
        /// <param name='action'>
        /// The action to be run on the main/Unity thread (null not permitted).
        /// </param>
        public void RunOnMainThread(Action action) {
            if (IsMainThread) {
                action();
            } else {
                Post(action);
            }
        }

        /// <summary>
        /// Posts the specified action to be run ASAP on the main/Unity thread.
        /// </summary>
        /// <param name='action'>
        /// The action to be run ASAP (null not permitted).
        /// </param>
        public void Post(Action action) {
            PostAtTime(action, DateTime.UtcNow);
        }

        /// <summary>
        /// Posts the specified action to be run after the specified seconds on the main/Unity thread.
        /// </summary>
        /// <param name='action'>
        /// The action to be run after the specified seconds (null not permitted).
        /// </param>
        /// <param name='delaySecs'>
        /// The seconds to delay run of the action. If <= 0, the action will be run ASAP.
        /// </param>
        public void PostDelayed(Action action, double delaySecs) {
            PostAtTime(action, DateTime.UtcNow.AddSeconds(delaySecs));
        }

        /// <summary>
        /// Posts the specified action to be run at the specified time on the main/Unity thread.
        /// </summary>
        /// <param name='action'>
        /// The action to be run at the specified time (null not permitted).
        /// </param>
        /// <param name='time'>
        /// The time when the action should be run. If <= now, the action will be run ASAP.
        /// </param>
        public void PostAtTime(Action action, DateTime time) {
            looper.Schedule(action, time, false);
        }

        /// <summary>
        /// Removes all schedules of the specified action.
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
