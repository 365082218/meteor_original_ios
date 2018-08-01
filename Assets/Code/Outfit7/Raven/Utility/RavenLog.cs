using System;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Starlite.Raven.Internal {

    public static class RavenLog {

        public enum ELogLevel {
            VERBOSE,
            DEBUG,
            INFO,
            WARN,
            ERROR,
            NONE
        }

        public delegate string LogDelegate(string message, ELogLevel logLevel);

        public static int MaxUnityLogLength = 1024 * 5;

        public static ELogLevel Level { get; set; }

        public static Func<string> CustomLogDataCallback;

        private static LogDelegate LogCallback;

        // #######
        // CALLBACK

        public static void SetLogCallback(LogDelegate del) {
            LogCallback = del;
        }

        //########
        // VERBOSE

        public static bool VerboseEnabled {
            get {
                return ELogLevel.VERBOSE >= Level;
            }
        }

        public static void SetMainThread() {
#if STARLITE_EDITOR
#else
            Outfit7.Util.O7Log.SetMainThread();
#endif
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void Verbose(string message) {
            Log(ELogLevel.VERBOSE, null, message, null, null);
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void Verbose(string message, params object[] args) {
            Log(ELogLevel.VERBOSE, null, message, null, args);
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void Verbose(Exception e, string message) {
            Log(ELogLevel.VERBOSE, null, message, e, null);
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void Verbose(Exception e, string message, params object[] args) {
            Log(ELogLevel.VERBOSE, null, message, e, args);
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void VerboseT(string tag, string message) {
            Log(ELogLevel.VERBOSE, tag, message, null, null);
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void VerboseT(string tag, string message, params object[] args) {
            Log(ELogLevel.VERBOSE, tag, message, null, args);
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void VerboseT(string tag, Exception e, string message) {
            Log(ELogLevel.VERBOSE, tag, message, e, null);
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void VerboseT(string tag, Exception e, string message, params object[] args) {
            Log(ELogLevel.VERBOSE, tag, message, e, args);
        }

        //######
        // DEBUG

        public static bool DebugEnabled {
            get {
                return ELogLevel.DEBUG >= Level;
            }
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void Debug(string message) {
            Log(ELogLevel.DEBUG, null, message, null, null);
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void Debug(string message, params object[] args) {
            Log(ELogLevel.DEBUG, null, message, null, args);
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void Debug(Exception e, string message) {
            Log(ELogLevel.DEBUG, null, message, e, null);
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void Debug(Exception e, string message, params object[] args) {
            Log(ELogLevel.DEBUG, null, message, e, args);
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void DebugT(string tag, string message) {
            Log(ELogLevel.DEBUG, tag, message, null, null);
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void DebugT(string tag, string message, params object[] args) {
            Log(ELogLevel.DEBUG, tag, message, null, args);
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void DebugT(string tag, Exception e, string message) {
            Log(ELogLevel.DEBUG, tag, message, e, null);
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void DebugT(string tag, Exception e, string message, params object[] args) {
            Log(ELogLevel.DEBUG, tag, message, e, args);
        }

        //######
        // INFO

        public static bool InfoEnabled {
            get {
                return ELogLevel.INFO >= Level;
            }
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void Info(string message) {
            Log(ELogLevel.INFO, null, message, null, null);
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void Info(string message, params object[] args) {
            Log(ELogLevel.INFO, null, message, null, args);
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void Info(Exception e, string message) {
            Log(ELogLevel.INFO, null, message, e, null);
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void Info(Exception e, string message, params object[] args) {
            Log(ELogLevel.INFO, null, message, e, args);
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void InfoT(string tag, string message) {
            Log(ELogLevel.INFO, tag, message, null, null);
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void InfoT(string tag, string message, params object[] args) {
            Log(ELogLevel.INFO, tag, message, null, args);
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void InfoT(string tag, Exception e, string message) {
            Log(ELogLevel.INFO, tag, message, e, null);
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void InfoT(string tag, Exception e, string message, params object[] args) {
            Log(ELogLevel.INFO, tag, message, e, args);
        }

        //########
        // WARNING

        public static bool WarnEnabled {
            get {
                return ELogLevel.WARN >= Level;
            }
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void Warn(string message) {
            Log(ELogLevel.WARN, null, message, null, null);
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void Warn(string message, params object[] args) {
            Log(ELogLevel.WARN, null, message, null, args);
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void Warn(Exception e, string message) {
            Log(ELogLevel.WARN, null, message, e, null);
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void Warn(Exception e, string message, params object[] args) {
            Log(ELogLevel.WARN, null, message, e, args);
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void WarnT(string tag, string message) {
            Log(ELogLevel.WARN, tag, message, null, null);
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void WarnT(string tag, string message, params object[] args) {
            Log(ELogLevel.WARN, tag, message, null, args);
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void WarnT(string tag, Exception e, string message) {
            Log(ELogLevel.WARN, tag, message, e, null);
        }

#if STRIP_LOGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public static void WarnT(string tag, Exception e, string message, params object[] args) {
            Log(ELogLevel.WARN, tag, message, e, args);
        }

        //######
        // ERROR

        public static bool ErrorEnabled {
            get {
                return ELogLevel.ERROR >= Level;
            }
        }

        public static void Error(string message) {
            Log(ELogLevel.ERROR, null, message, null, null);
        }

        public static void Error(string message, params object[] args) {
            Log(ELogLevel.ERROR, null, message, null, args);
        }

        public static void Error(Exception e, string message) {
            Log(ELogLevel.ERROR, null, message, e, null);
        }

        public static void Error(Exception e, string message, params object[] args) {
            Log(ELogLevel.ERROR, null, message, e, args);
        }

        public static void ErrorT(string tag, string message) {
            Log(ELogLevel.ERROR, tag, message, null, null);
        }

        public static void ErrorT(string tag, string message, params object[] args) {
            Log(ELogLevel.ERROR, tag, message, null, args);
        }

        public static void ErrorT(string tag, Exception e, string message) {
            Log(ELogLevel.ERROR, tag, message, e, null);
        }

        public static void ErrorT(string tag, Exception e, string message, params object[] args) {
            Log(ELogLevel.ERROR, tag, message, e, args);
        }

        //#######
        // COMMON

        private static void Log(ELogLevel level, string tag, string format, Exception e, params object[] args) {
            if (level < Level) {
                return;
            }

            var logCallback = LogCallback;
            if (logCallback != null) {
                logCallback(string.Format(format, args), level);
                return;
            }

#if STARLITE_EDITOR
            switch (level) {
                case ELogLevel.VERBOSE:
                    O7Log.VerboseT(tag, e, format, args);
                    break;
                case ELogLevel.DEBUG:
                    O7Log.DebugT(tag, e, format, args);
                    break;
                case ELogLevel.INFO:
                    O7Log.InfoT(tag, e, format, args);
                    break;
                case ELogLevel.WARN:
                    O7Log.WarnT(tag, e, format, args);
                    break;
                case ELogLevel.ERROR:
                    O7Log.ErrorT(tag, e, format, args);
                    break;
            }
#else
            // Pass to Unity
            switch (level) {
                case ELogLevel.VERBOSE:
                    Outfit7.Util.O7Log.VerboseT(tag, e, format, args);
                    break;
                case ELogLevel.DEBUG:
                    Outfit7.Util.O7Log.DebugT(tag, e, format, args);
                    break;
                case ELogLevel.INFO:
                    Outfit7.Util.O7Log.InfoT(tag, e, format, args);
                    break;
                case ELogLevel.WARN:
                    Outfit7.Util.O7Log.WarnT(tag, e, format, args);
                    break;
                case ELogLevel.ERROR:
                    Outfit7.Util.O7Log.ErrorT(tag, e, format, args);
                    break;
            }
#endif
        }
    }
}
