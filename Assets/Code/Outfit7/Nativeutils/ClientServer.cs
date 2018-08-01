#if !SILVERLIGHT

using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Outfit7.Util {

    public class ClientServer {

        private delegate void InternalOnConnectionDelegate(IntPtr instance, IntPtr client);

        private delegate void InternalOnReceiveDelegate(IntPtr instance, IntPtr client, IntPtr data, int length);

        private delegate void InternalOnServerDelegate(IntPtr instance, string address, int port, string message);

        private static Dictionary<IntPtr, ClientServer> ClientServerInstances;
        private static InternalOnConnectionDelegate OnConnectedInstance;
        private static InternalOnConnectionDelegate OnDisconnectedInstance;
        private static InternalOnReceiveDelegate OnReceiveInstance;
        private static InternalOnServerDelegate OnServerInstance;
        private static IntPtr OnConnectedPtr;
        private static IntPtr OnDisconnectedPtr;
        private static IntPtr OnReceivePtr;
        private static IntPtr OnServerPtr;

        [AOT.MonoPInvokeCallback(typeof(InternalOnConnectionDelegate))]
        private static void InternalOnConnected(IntPtr instance, IntPtr client) {
            ClientServer clientServer;
            if (!ClientServerInstances.TryGetValue(instance, out clientServer)) {
                NativeUtils.LogFormat(NativeUtils.LogLevel.Warning, "OnConnected: Instance {0} not found!", instance);
                return;
            }
            if (clientServer.OnConnected != null) {
                clientServer.OnConnected(client);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(InternalOnConnectionDelegate))]
        private static void InternalOnDisconnected(IntPtr instance, IntPtr client) {
            ClientServer clientServer;
            if (!ClientServerInstances.TryGetValue(instance, out clientServer)) {
                NativeUtils.LogFormat(NativeUtils.LogLevel.Warning, "OnDisconnected: Instance {0} not found!", instance);
                return;
            }
            if (clientServer.OnDisconnected != null) {
                clientServer.OnDisconnected(client);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(InternalOnReceiveDelegate))]
        private static void InternalOnReceive(IntPtr instance, IntPtr client, IntPtr data, int length) {
            ClientServer clientServer;
            if (!ClientServerInstances.TryGetValue(instance, out clientServer)) {
                NativeUtils.LogFormat(NativeUtils.LogLevel.Warning, "OnReceive: Instance {0} not found!", instance);
                return;
            }

            if (clientServer.OnReceive != null) {
                byte[] outData = null;
                if (length != 0) {
                    outData = new byte[length];
                    Marshal.Copy(data, outData, 0, (int) length);
                }
                clientServer.OnReceive(client, outData);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(InternalOnServerDelegate))]
        private static void InternalOnServer(IntPtr instance, string address, int port, string message) {
            ClientServer clientServer;
            if (!ClientServerInstances.TryGetValue(instance, out clientServer)) {
                NativeUtils.LogFormat(NativeUtils.LogLevel.Warning, "OnServer: Instance {0} not found!", instance);
                return;
            }

            if (clientServer.OnServer != null) {
                clientServer.OnServer(address, port, message);
            }
        }

        private IntPtr Instance;

        public delegate void OnConnectionDelegate(IntPtr client);

        public delegate void OnReceiveDelegate(IntPtr client, byte[] data);

        public delegate void OnServerDelegate(string address, int port, string message);

        public OnConnectionDelegate OnConnected;
        public OnConnectionDelegate OnDisconnected;
        public OnReceiveDelegate OnReceive;
        public OnServerDelegate OnServer;

        public bool IsValid { get { return Instance != IntPtr.Zero; } }

        public ClientServer(int maxReceiveBufferSize) {
            // Initialize
            if (ClientServerInstances == null) {
                ClientServerInstances = new Dictionary<IntPtr, ClientServer>();
                OnConnectedInstance = new InternalOnConnectionDelegate(InternalOnConnected);
                OnConnectedPtr = Marshal.GetFunctionPointerForDelegate(OnConnectedInstance);
                OnDisconnectedInstance = new InternalOnConnectionDelegate(InternalOnDisconnected);
                OnDisconnectedPtr = Marshal.GetFunctionPointerForDelegate(OnDisconnectedInstance);
                OnReceiveInstance = new InternalOnReceiveDelegate(InternalOnReceive);
                OnReceivePtr = Marshal.GetFunctionPointerForDelegate(OnReceiveInstance);
                OnServerInstance = new InternalOnServerDelegate(InternalOnServer);
                OnServerPtr = Marshal.GetFunctionPointerForDelegate(OnServerInstance);
            }
            Instance = NativeUtils.NativeUtils_ClientServer_Create(maxReceiveBufferSize);
            NativeUtils.NativeUtils_ClientServer_SetCallbacks(Instance, OnConnectedPtr, OnDisconnectedPtr, OnReceivePtr, OnServerPtr);
            ClientServerInstances.Add(Instance, this);
        }

        public bool CreateServer(int port, int broadcastPort, int maxConnections, string serverInfoMessage) {
            if (!IsValid) {
                return false;
            }
            int ret = NativeUtils.NativeUtils_ClientServer_CreateServer(Instance, broadcastPort, port, maxConnections, serverInfoMessage);
            NativeUtils.LogFormat(NativeUtils.LogLevel.Verbose, "CreateServer {0} {1} {2}", ret, port, broadcastPort);
            return ret == 0;
        }

        public bool CreateClient(string host, int port) {
            if (!IsValid) {
                return false;
            }
            int ret = NativeUtils.NativeUtils_ClientServer_CreateClient(Instance, host, port);
            NativeUtils.LogFormat(NativeUtils.LogLevel.Verbose, "CreateClient {0} {1} {2}", ret, host, port);
            return ret == 0;
        }

        public bool CreateServerInfoClient(int broadcastPort) {
            if (!IsValid) {
                return false;
            }
            int ret = NativeUtils.NativeUtils_ClientServer_CreateServerInfoClient(Instance, broadcastPort);
            NativeUtils.LogFormat(NativeUtils.LogLevel.Verbose, "CreateServerInfoClient {0} {1}", ret, broadcastPort);
            return ret == 0;
        }

        public bool Disconnect() {
            if (!IsValid) {
                return false;
            }
            return NativeUtils.NativeUtils_ClientServer_Disconnect(Instance) == 0;
        }

        public void Destroy() {
            if (!IsValid) {
                return;
            }
            NativeUtils.NativeUtils_ClientServer_Destroy(Instance);
            ClientServerInstances.Remove(Instance);
            Instance = IntPtr.Zero;
        }

        public void Update() {
            if (!IsValid) {
                return;
            }
            NativeUtils.NativeUtils_ClientServer_Update(Instance);
        }

        public int Send(byte[] data, int length) {
            if (!IsValid) {
                return 0;
            }
            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            IntPtr dataPtr = dataHandle.AddrOfPinnedObject();
            int ret = NativeUtils.NativeUtils_ClientServer_Send(Instance, dataPtr, length);
            dataHandle.Free();
            return ret;
        }

        public bool IsClientConnected() {
            return IsValid && NativeUtils.NativeUtils_ClientServer_IsClientConnected(Instance);
        }

        public bool IsServerConnected() {
            return IsValid && NativeUtils.NativeUtils_ClientServer_IsServerConnected(Instance);
        }
    }

}

#endif