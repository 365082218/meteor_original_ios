//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

using System;

#if NETFX_CORE
using MarkerMetro.Unity.WinLegacy.Threading;
#else
using System.Threading;
#endif

namespace Outfit7.Threading {

    /// <summary>
    /// The wrapper for system's Thread, which does not exist on Windows Runtime, but is partly implemented by MarkerMetro.
    /// </summary>
    public class O7Thread {

        public static int CurrentThreadId {
            get {
#if NETFX_CORE
                return Environment.CurrentManagedThreadId;
#else
                return Thread.CurrentThread.ManagedThreadId;
#endif
            }
        }

        public static void Sleep(int millisecondsTimeout) {
            Thread.Sleep(millisecondsTimeout);
        }

        public static void Sleep(TimeSpan timeout) {
            long tm = (long) timeout.TotalMilliseconds;
            if (tm < -1 || tm > (long) int.MaxValue) {
                throw new ArgumentOutOfRangeException("timeout");
            }
            Thread.Sleep((int) timeout.TotalMilliseconds);
        }

        private readonly Thread Thread;

        public O7Thread(Action job) {
            Thread = new Thread(new ThreadStart(job));
        }

        public string Name {
            get {
                return Thread.Name;
            }
            set {
                Thread.Name = value;
            }
        }

        public bool IsBackground {
            get {
                return Thread.IsBackground;
            }
            set {
                Thread.IsBackground = value;
            }
        }

        public bool IsAlive {
            get {
                return Thread.IsAlive;
            }
        }

        public void Start() {
            Thread.Start();
        }

        public void Join() {
            Join(System.Threading.Timeout.Infinite);
        }

        public void Join(int millisecondsTimeout) {
            Thread.Join(millisecondsTimeout);
        }

        public void Join(TimeSpan timeout) {
            long tm = (long) timeout.TotalMilliseconds;
            if (tm < -1 || tm > (long) int.MaxValue) {
                throw new ArgumentOutOfRangeException("timeout");
            }
            Join((int) timeout.TotalMilliseconds);
        }
    }
}
