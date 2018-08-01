#if !SILVERLIGHT

using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Outfit7.Util {

    public static partial class NativeUtils {

        private const string Tag = "NativeUtils";

#if !UNITY_EDITOR && UNITY_IPHONE
        private const string import = "__Internal";
#elif !UNITY_EDITOR && UNITY_ANDROID
        private const string import = "nativeutils";
#else
        private const string import = "libnativeutils";
#endif

        [Flags]
        private enum InitializeFlags {
            None = 0,
            Log = 1,
            CrashDump = 2,
        }

        public enum LogLevel {
            Verbose,
            Warning,
            Error,
            Exception,
        };

        [DllImport(import)]
        private static extern int NativeUtils_Initialize(InitializeFlags flags);

        [DllImport(import)]
        private static extern int NativeUtils_Finalize();

        [DllImport(import)]
        private static extern void NativeUtils_SetCallbacks(IntPtr log);

        [DllImport(import)]
        private static extern void NativeUtils_TestCrash();

        [DllImport(import)]
        private static extern uint NativeUtils_xxHash(IntPtr data, uint data_size, uint seed);

        [DllImport(import)]
        private static extern ulong NativeUtils_xxHash64(IntPtr data, uint data_size, ulong seed);

        [DllImport(import)]
        public static extern void UpdateClothStructs(IntPtr points, int pointCount, IntPtr constraints, int constraintCount, IntPtr colliders, int colliderCount, float deltaTime2, ref Vector3 force, ref Vector3 gravity, float maxVelocity, float scale, int iterations);

        [DllImport(import)]
        public static extern IntPtr NativeUtils_ClientServer_Create(int max_receive_buffer_size);

        [DllImport(import)]
        public static extern void NativeUtils_ClientServer_Destroy(IntPtr instance);

        [DllImport(import)]
        public static extern void NativeUtils_ClientServer_SetCallbacks(IntPtr instance, IntPtr onConnected, IntPtr onDisconnected, IntPtr onReceive, IntPtr on_serverInfoMessage);

        [DllImport(import)]
        public static extern int NativeUtils_ClientServer_CreateServer(IntPtr instance, int port, int broadcastPort, int maxConnections, string serverInfoMessage);

        [DllImport(import)]
        public static extern int NativeUtils_ClientServer_CreateClient(IntPtr instance, string hostName, int port);

        [DllImport(import)]
        public static extern int NativeUtils_ClientServer_CreateServerInfoClient(IntPtr instance, int broadcastPort);

        [DllImport(import)]
        public static extern int NativeUtils_ClientServer_Disconnect(IntPtr instance);

        [DllImport(import)]
        public static extern void NativeUtils_ClientServer_Update(IntPtr instance);

        [DllImport(import)]
        public static extern int NativeUtils_ClientServer_Send(IntPtr instance, IntPtr data, int length);

        [DllImport(import)]
        public static extern bool NativeUtils_ClientServer_IsClientConnected(IntPtr instance);

        [DllImport(import)]
        public static extern bool NativeUtils_ClientServer_IsServerConnected(IntPtr instance);


        private delegate void InternalOnLogDelegate(LogLevel logLevel, string log);

        private static InternalOnLogDelegate OnLogDelegateInstance;

        private static bool IsInitialized = false;

        // Internal delegates
        [AOT.MonoPInvokeCallback(typeof(InternalOnLogDelegate))]
        private static void InternalOnLog(LogLevel logLevel, string log) {
            Log(logLevel, log);
        }

        public static void Log(LogLevel logLevel, string log) {
            switch (logLevel) {
                case LogLevel.Verbose:
#if NATIVEUTILSSTANDALONE
                    Debug.Log(log);
#else
                    O7Log.DebugT(Tag, log);
#endif
                    break;
                case LogLevel.Warning:
#if NATIVEUTILSSTANDALONE
                    Debug.LogWarning(log);
#else
                    O7Log.WarnT(Tag, log);
#endif
                    break;
                case LogLevel.Error:
                case LogLevel.Exception:
#if NATIVEUTILSSTANDALONE
                    Debug.LogError(log);
#else
                    FileLogger.Log(log, true);
#endif
                    break;
            }
        }

        public static void LogFormat(LogLevel logLevel, string format, params object[] args) {
            Log(logLevel, string.Format(format, args));
        }

        public static void Init(bool crashDump = false) {
            if (IsInitialized) {
                return;
            }
            IsInitialized = true;
            // Set callbacks
            OnLogDelegateInstance = new InternalOnLogDelegate(InternalOnLog);
            IntPtr pOnLogPtr = Marshal.GetFunctionPointerForDelegate(OnLogDelegateInstance);
            NativeUtils_SetCallbacks(pOnLogPtr);
            // Initialize
            InitializeFlags flags = 0;
#if UNITY_EDITOR || DEVEL_BUILD || PROD_BUILD
            flags |= InitializeFlags.Log;
#endif
#if !UNITY_EDITOR && (DEVEL_BUILD || PROD_BUILD) && (UNITY_ANDROID || UNITY_IPHONE)
            if(crashDump) {
                flags |= InitializeFlags.CrashDump;
            }
#endif
            NativeUtils_Initialize(flags);
        }

        public static void Cleanup() {
            if (!IsInitialized) {
                return;
            }
            NativeUtils_Finalize();
        }

        public static uint xxHash(byte[] data, int length, uint seed) {
            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            IntPtr dataPtr = dataHandle.AddrOfPinnedObject();
            uint hash = NativeUtils_xxHash(dataPtr, (uint) length, seed);
            dataHandle.Free();
            return hash;
        }

        public static ulong xxHash64(byte[] data, int length, ulong seed) {
            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            IntPtr dataPtr = dataHandle.AddrOfPinnedObject();
            ulong hash = NativeUtils_xxHash64(dataPtr, (uint)length, seed);
            dataHandle.Free();
            return hash;
        }

        public static uint xxHash(string str, uint seed) {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            return xxHash(bytes, bytes.Length, seed);
        }

        public static ulong xxHash64(string str, ulong seed) {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            return xxHash64(bytes, bytes.Length, seed);
        }

        public static void OnUpdate() {
        }

        public static void OnAppPause() {
        }

        public static void OnAppResume() {
        }

        public static void Crash() {
            NativeUtils_TestCrash();
        }
    }

}

#endif
