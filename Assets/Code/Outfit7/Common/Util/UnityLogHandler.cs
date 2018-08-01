//
//  Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using Outfit7.Common;
using Outfit7.Threading;
using UnityEngine;

namespace Outfit7.Util {

    /// <summary>
    /// Unity log handler.
    /// </summary>
    public static class UnityLogHandler {

        private static bool IsExceptionAlreadyDispatched;

        public static MainExecutor MainExecutor { get; set; }

        public static void RegisterMe() {
// Removed warning for obsolete method. This method is the only working correctly
#pragma warning disable 618
            Application.RegisterLogCallbackThreaded(HandleLog);
#pragma warning restore 618
        }

        private static void HandleLog(string logString, string stackTrace, LogType type) {
            switch (type) {
                case LogType.Assert:
                case LogType.Exception:
                    // Dispatch exception only once, because Unity player never dies, just keeps popping exceptions
                    if (IsExceptionAlreadyDispatched)
                        return;

                    IsExceptionAlreadyDispatched = true;

                    O7Log.Error("{0} with message: '{1}' and stack trace: <{2}>", type, logString, stackTrace);

                    AppPlugin.DispatchException(logString, stackTrace);
                    break;
            }
        }

        public static void HandleException(string message, Exception e) {
            if (!object.ReferenceEquals(MainExecutor, null)) { // MainExecutor is MonoBehaviour, which has fucked up "== null" operator
                // Must run on main thread to dispatch exception to native
                MainExecutor.RunOnMainThread(delegate {
                    HandleLog(message, e.StackTrace, LogType.Exception);
                });
            }

            // Log now to be time-consistent with other logs,
            // because dispatching exception to main thread can happen a lot later if main thread is busy
            O7Log.Error(e, message);
        }
    }
}
