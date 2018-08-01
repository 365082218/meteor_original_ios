//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using System.Diagnostics;
using System.Text;
using Outfit7.Threading;
using UnityEngine;

namespace Outfit7.Util {

    /// <summary>
    /// Logger.
    /// </summary>
    public static class O7Log {

        public enum LogLevel {
            VERBOSE,
            DEBUG,
            INFO,
            WARN,
            ERROR,
            None,
        }

        private static int MainThreadId = -1;

        public static int MaxUnityLogLength = 1024 * 5;

        public static LogLevel Level { get; set ; }

        public static Func<string> CustomLogDataCallback;

        private static readonly char[] StackTraceSplitChars = new char[] {'\n'};

        private static bool IsCustomStackTrace = false;

        //########
        // VERBOSE

        public static bool VerboseEnabled {
            get {
                return LogLevel.VERBOSE >= Level;
            }
        }

        public static void SetMainThread() {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (BuildConfig.IsDevel) {
                Application.stackTraceLogType = StackTraceLogType.None;
                IsCustomStackTrace = true;
            }
#endif

            MainThreadId = O7Thread.CurrentThreadId;
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Verbose(string message) {
            Log(LogLevel.VERBOSE, null, message, null, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Verbose(string message, params object[] args) {
            Log(LogLevel.VERBOSE, null, message, null, args);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Verbose(Exception e, string message) {
            Log(LogLevel.VERBOSE, null, message, e, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Verbose(Exception e, string message, params object[] args) {
            Log(LogLevel.VERBOSE, null, message, e, args);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void VerboseT(string tag, string message) {
            Log(LogLevel.VERBOSE, tag, message, null, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void VerboseT(string tag, string message, params object[] args) {
            Log(LogLevel.VERBOSE, tag, message, null, args);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void VerboseT(string tag, Exception e, string message) {
            Log(LogLevel.VERBOSE, tag, message, e, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void VerboseT(string tag, Exception e, string message, params object[] args) {
            Log(LogLevel.VERBOSE, tag, message, e, args);
        }

        //######
        // DEBUG

        public static bool DebugEnabled {
            get {
                return LogLevel.DEBUG >= Level;
            }
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Debug(string message) {
            Log(LogLevel.DEBUG, null, message, null, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Debug(string message, params object[] args) {
            Log(LogLevel.DEBUG, null, message, null, args);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Debug(Exception e, string message) {
            Log(LogLevel.DEBUG, null, message, e, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Debug(Exception e, string message, params object[] args) {
            Log(LogLevel.DEBUG, null, message, e, args);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void DebugT(string tag, string message) {
            Log(LogLevel.DEBUG, tag, message, null, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void DebugT(string tag, string message, params object[] args) {
            Log(LogLevel.DEBUG, tag, message, null, args);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void DebugT(string tag, Exception e, string message) {
            Log(LogLevel.DEBUG, tag, message, e, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void DebugT(string tag, Exception e, string message, params object[] args) {
            Log(LogLevel.DEBUG, tag, message, e, args);
        }

        //######
        // INFO

        public static bool InfoEnabled {
            get {
                return LogLevel.INFO >= Level;
            }
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Info(string message) {
            Log(LogLevel.INFO, null, message, null, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Info(string message, params object[] args) {
            Log(LogLevel.INFO, null, message, null, args);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Info(Exception e, string message) {
            Log(LogLevel.INFO, null, message, e, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Info(Exception e, string message, params object[] args) {
            Log(LogLevel.INFO, null, message, e, args);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void InfoT(string tag, string message) {
            Log(LogLevel.INFO, tag, message, null, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void InfoT(string tag, string message, params object[] args) {
            Log(LogLevel.INFO, tag, message, null, args);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void InfoT(string tag, Exception e, string message) {
            Log(LogLevel.INFO, tag, message, e, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void InfoT(string tag, Exception e, string message, params object[] args) {
            Log(LogLevel.INFO, tag, message, e, args);
        }

        //########
        // WARNING

        public static bool WarnEnabled {
            get {
                return LogLevel.WARN >= Level;
            }
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Warn(string message) {
            Log(LogLevel.WARN, null, message, null, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Warn(string message, params object[] args) {
            Log(LogLevel.WARN, null, message, null, args);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Warn(Exception e, string message) {
            Log(LogLevel.WARN, null, message, e, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Warn(Exception e, string message, params object[] args) {
            Log(LogLevel.WARN, null, message, e, args);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void WarnT(string tag, string message) {
            Log(LogLevel.WARN, tag, message, null, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void WarnT(string tag, string message, params object[] args) {
            Log(LogLevel.WARN, tag, message, null, args);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void WarnT(string tag, Exception e, string message) {
            Log(LogLevel.WARN, tag, message, e, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void WarnT(string tag, Exception e, string message, params object[] args) {
            Log(LogLevel.WARN, tag, message, e, args);
        }

        //######
        // ERROR

        public static bool ErrorEnabled {
            get {
                return LogLevel.ERROR >= Level;
            }
        }

        public static void Error(string message) {
            Log(LogLevel.ERROR, null, message, null, null);
        }

        public static void Error(string message, params object[] args) {
            Log(LogLevel.ERROR, null, message, null, args);
        }

        public static void Error(Exception e, string message) {
            Log(LogLevel.ERROR, null, message, e, null);
        }

        public static void Error(Exception e, string message, params object[] args) {
            Log(LogLevel.ERROR, null, message, e, args);
        }

        public static void ErrorT(string tag, string message) {
            Log(LogLevel.ERROR, tag, message, null, null);
        }

        public static void ErrorT(string tag, string message, params object[] args) {
            Log(LogLevel.ERROR, tag, message, null, args);
        }

        public static void ErrorT(string tag, Exception e, string message) {
            Log(LogLevel.ERROR, tag, message, e, null);
        }

        public static void ErrorT(string tag, Exception e, string message, params object[] args) {
            Log(LogLevel.ERROR, tag, message, e, args);
        }

        //#######
        // COMMON

        private static void Log(LogLevel level, string tag, string format, Exception e, params object[] args) {
            if (level < Level) return;

            StringBuilder sb = new StringBuilder(512);

#if !UNITY_EDITOR
            // Add log level
            sb.AppendFormat("{0,-8}", level);
#endif

            // Add date & time
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            sb.Append(now);

            // Add thread ID
            sb.Append(" (t");
            sb.Append(O7Thread.CurrentThreadId);
            // Add frame count
            if (O7Thread.CurrentThreadId == MainThreadId) {
                sb.Append(" f");
                sb.Append(Time.frameCount);
            }
            // Add custom data if any
            if (CustomLogDataCallback != null) {
                string customData = CustomLogDataCallback();
                if (!string.IsNullOrEmpty(customData)) {
                    sb.Append(" ");
                    sb.Append(customData);
                }
            }
            sb.Append(") -- ");

            if (tag != null) {
                sb.Append("#");
                sb.Append(tag);
                sb.Append(" -- ");
            }

            // Format string with args if not null
            string msg = format;
            if (args != null) {
                msg = string.Format(format, args);
            }
            sb.Append(msg);

            // Append stack trace if exception is not null
            if (e != null) {
                sb.Append(" <");
                sb.Append(e.ToString());
                sb.Append(">");
            }

            if (IsCustomStackTrace) {
                // get own stack trace to remove first 3 useless lines of stack trace that are always the same (from Unity)
                string stackTrace = StackTraceUtility.ExtractStackTrace();
                stackTrace = stackTrace.Split(StackTraceSplitChars, 3)[2];
                sb.AppendLine();
                sb.Append(stackTrace);
            }

            string log = sb.ToString();

            // Pass to file and flush immediately if error, because app may crash when passing error to Unity
            FileLogger.Log(log, level == LogLevel.ERROR);

            // Strip log if too long
            if (log.Length > MaxUnityLogLength) {
                log = log.Substring(0, MaxUnityLogLength);
            }

            // Pass to Unity
            switch (level) {
                case LogLevel.VERBOSE:
                case LogLevel.DEBUG:
                case LogLevel.INFO:
                    UnityEngine.Debug.Log(log);
                    break;
                case LogLevel.WARN:
                    UnityEngine.Debug.LogWarning(log);
                    break;
                case LogLevel.ERROR:
                    UnityEngine.Debug.LogError(log);
                    break;
            }
        }
    }
}
