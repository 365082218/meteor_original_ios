//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using System.Threading;
using Outfit7.Util;

namespace Outfit7.Threading {

    /// <summary>
    /// The thread executor of posted actions. Creates new thread and runs own looper on it.
    /// </summary>
    public class ThreadExecutor : Executor {

        private const string Tag = "ThreadExecutor";
        private readonly object Lock = new object();
        private int sleepMillis = 50;

        public string Name { get; private set; }

        // Not very strict value, no need for thread safety
        public int SleepMillis {
            get {
                return this.sleepMillis;
            }
            set {
                Assert.IsTrue(value >= 0, "value must be >= 0");
                this.sleepMillis = value;
            }
        }

        /// <value>
        /// <c>true</c> if this executor is about to quit or has already quit
        /// </value>
        // Not very strict value, no need for thread safety
        public bool IsQuit { get; protected set; }

        /// <summary>
        /// Creates new executor on a new thread with the specified name.
        /// </summary>
        /// <param name='name'>
        /// The name of the new thread.
        /// </param>
        public ThreadExecutor(string name) : base(false) {
            Name = name;
            O7Thread thread = new O7Thread(Run);
            thread.Name = name;
            thread.IsBackground = true;
            thread.Start();

            O7Log.InfoT(Tag, "Creating new, calling thread id={0}", O7Thread.CurrentThreadId);

            // Be sure that looper is created - block current thread until new thread starts
            lock (Lock) {
                if (thread.IsAlive && looper == null) {
                    Monitor.Wait(Lock);
                }
            }
        }

        protected virtual void Run() {
            O7Log.DebugT(Tag, "New thread, id={0}", O7Thread.CurrentThreadId);

            // New thread - create new looper
            lock (Lock) {
                looper = Looper.ThreadInstance;
                Monitor.Pulse(Lock);
            }

            while (!IsQuit) {
                try {
                    looper.LoopOnce();

                } catch (Exception e) {
                    // Manually handle exception because Unity simply consumes exceptions thrown in non main thread
                    // Assume that app crashes after this
                    UnityLogHandler.HandleException("Inside ThreadExecutor named '" + Name + "': " + e.Message, e);
                    Quit();
                }
                O7Thread.Sleep(sleepMillis);
            }

            O7Log.DebugT(Tag, "Quit thread, id={0}", O7Thread.CurrentThreadId);
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
        public override void PostAtTime(Action action, DateTime time) {
            if (IsQuit) {
                O7Log.WarnT(Tag, "Posting action after executor is quit");
                return;
            }
            base.PostAtTime(action, time);
        }

        /// <summary>
        /// Posts the specified action to be run ASAP before any other action on this executor's looper.
        /// </summary>
        /// <param name='action'>
        /// The action to be run ASAP before any other action (null not permitted).
        /// </param>
        public override void PostAtFrontQueue(Action action) {
            if (IsQuit) {
                O7Log.WarnT(Tag, "Posting action after executor is quit");
                return;
            }
            base.PostAtFrontQueue(action);
        }

        /// <summary>
        /// Gracefully stops this executor's thread and looper. After this is called, this executor becomes useless.
        /// </summary>
        public virtual void Quit() {
            O7Log.InfoT(Tag, "Quitting, calling thread id={0}", O7Thread.CurrentThreadId);
            IsQuit = true;
        }
    }
}
