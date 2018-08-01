//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Threading;
using Outfit7.Util;
using Outfit7.Threading;
using UnityEngine;

namespace Outfit7.Web {

    /// <summary>
    /// Represents a REST call triggered outside main thread.
    /// </summary>
    public class ThreadedRestCall : RestCall {

        private readonly object Lock = new object();

        public ThreadedRestCall(string url, Dictionary<string, string> headers) : base(url, headers) {
        }

        public ThreadedRestCall(string url, Dictionary<string, string> headers, Method method) : base(url, headers, method) {
        }

        public ThreadedRestCall(string url, string body, Dictionary<string, string> headers) : base(url, body, headers) {
        }

        /// <summary>
        /// Starts executing REST call from outside main thread.
        /// </summary>
        /// <param name='executor'>
        /// The main executor (null not permitted).
        /// </param>
        public void Start(MainExecutor executor) {
            lock (Lock) {
                Assert.State(!running, "Already started");
                running = true;
            }

            // Be sure to do WWW call on main thread, using corouting to yield main thread
            executor.Post(delegate {
                executor.StartCoroutine(ExecuteInternalThreaded());
            });
        }

        private IEnumerator<WWW> ExecuteInternalThreaded() {
            IEnumerator<WWW> e = ExecuteInternal();
            while (e.MoveNext()) {
                yield return e.Current;
            }

            lock (Lock) {
                Monitor.Pulse(Lock);
            }
        }

        /// <summary>
        /// Waits for response headers by blocking current thread.
        /// Do not call on main thread.
        /// </summary>
        /// <returns>
        /// The REST call response headers (should not be null).
        /// </returns>
        /// <exception cref='TimeoutException'>
        /// Is thrown when the timeout occurs.
        /// </exception>
        public Dictionary<string, string> WaitForResponse(int timeoutMillis) {
            lock (Lock) {
                Assert.State(running, "Not started yet");

                while (ResponseHeaders == null) {
                    bool ok = Monitor.Wait(Lock, timeoutMillis);
                    if (!ok) {
                        throw new TimeoutException();
                    }
                }
            }
            return ResponseHeaders;
        }
    }
}
