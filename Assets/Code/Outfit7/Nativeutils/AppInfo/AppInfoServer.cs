#if !SILVERLIGHT

using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

namespace Outfit7.Util.AppInfo {

    public class AppInfoServer : MonoBehaviour {

        private ClientServer Server;
        private AppInfoNetworkProtocol.ClientParametersMessage ClientParametersMessage;
        private AppInfoNetworkProtocol.ClientLogMessage ClientLogMessage;
        private AppInfoNetworkProtocol.ClientInitMessage ClientInitMessage;
        private AppInfoNetworkProtocol.ServerGameObjectsMessage ServerGameObjectsMessage;
        private AppInfoNetworkProtocol.ServerCommandMessage ServerCommandMessage;

        private List<AppInfoNetworkProtocol.SyncParameter> SyncParameters = new List<AppInfoNetworkProtocol.SyncParameter>();
        private List<AppInfoNetworkProtocol.SyncParameter> ActiveSyncParameters = new List<AppInfoNetworkProtocol.SyncParameter>();
        private Dictionary<string,AppInfoNetworkProtocol.Command> Commands = new Dictionary<string, AppInfoNetworkProtocol.Command>();

        private void OnConnected(IntPtr client) {
            // Reset sync parameters
            for (int i = 0; i < SyncParameters.Count; i++) {
                SyncParameters[i].Reset();
            }
            AppInfoNetworkProtocol.SendMessage(Server, ClientInitMessage, Commands);
        }

        private void OnDisconnected(IntPtr client) {
        }

        private void OnReceive(IntPtr client, byte[] data) {
            MemoryStream memoryStream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(memoryStream);
            AppInfoNetworkProtocol.MessageType messageType = (AppInfoNetworkProtocol.MessageType) reader.ReadByte();
            switch (messageType) {
                case AppInfoNetworkProtocol.MessageType.ServerGameObjects:
                    ServerGameObjectsMessage.Receive(reader);
                    break;
                case AppInfoNetworkProtocol.MessageType.ServerCommand:
                    ServerCommandMessage.Receive(reader);
                    break;
            }
        }

        private void OnLog(string condition, string stackTrace, LogType logType) {
            if (!Server.IsServerConnected()) {
                return;
            }
            if (Application.isEditor) {
                return;
            }
            lock (ClientLogMessage) {
                AppInfoNetworkProtocol.SendMessage(Server, ClientLogMessage, new AppInfoNetworkProtocol.SyncLog(logType, condition, stackTrace));
            }
        }

        private void UpdateParameters() {
            if (!Server.IsServerConnected()) {
                return;
            }
            float deltaTime = Time.unscaledDeltaTime;
            ActiveSyncParameters.Clear();
            for (int i = 0; i < SyncParameters.Count; i++) {
                AppInfoNetworkProtocol.SyncParameter syncParameter = SyncParameters[i];
                if (syncParameter.UpdateSync(deltaTime)) {
                    ActiveSyncParameters.Add(syncParameter);
                }
            }
            if (ActiveSyncParameters.Count > 0) {
                AppInfoNetworkProtocol.SendMessage(Server, ClientParametersMessage, ActiveSyncParameters);
            }
        }

        private void OnCommand(string name) {
            AppInfoNetworkProtocol.Command command;
            if (!Commands.TryGetValue(name, out command)) {
                return;
            }
            command.OnInvokeCommand();
        }

        public static AppInfoServer Create() {
            GameObject appInfoServerGameObject = new GameObject("AppInfoServer");
            GameObject.DontDestroyOnLoad(appInfoServerGameObject);
            return appInfoServerGameObject.AddComponent<AppInfoServer>();           
        }

        public void RegisterParameter(string name, AppInfoNetworkProtocol.SyncParameter.GetParameterDelegate getParameter, float time) {
            // Add new
            AppInfoNetworkProtocol.SyncParameter syncParameter = new AppInfoNetworkProtocol.SyncParameter(name, getParameter, time);
            SyncParameters.Add(syncParameter);
        }

        public void RegisterCommand(string name, AppInfoNetworkProtocol.Command.OnInvokeCommandDelegate onInvokeCommand) {
            Commands.Add(name, new AppInfoNetworkProtocol.Command(name, onInvokeCommand));
        }

        // Monobehaviour
        private void Start() {
            NativeUtils.Init();
            // Start server
            Server = new ClientServer(AppInfoNetworkProtocol.MaxReceiveSize);
            NativeUtils.LogFormat(NativeUtils.LogLevel.Verbose, "Creating AppInfo Server...");
            if (!Server.CreateServer(AppInfoNetworkProtocol.Port, AppInfoNetworkProtocol.BroadcastPort, 32, string.Format("{0} - {1} - {2}", Application.productName, SystemInfo.deviceModel, SystemInfo.deviceUniqueIdentifier))) {
                NativeUtils.LogFormat(NativeUtils.LogLevel.Warning, "AppInfoServer server failed!");
                return;
            }
            Server.OnConnected += OnConnected;
            Server.OnDisconnected += OnDisconnected;
            Server.OnReceive += OnReceive;
            // Create messages
            ClientParametersMessage = new AppInfoNetworkProtocol.ClientParametersMessage();
            ClientLogMessage = new AppInfoNetworkProtocol.ClientLogMessage();
            ClientInitMessage = new AppInfoNetworkProtocol.ClientInitMessage();
            ServerGameObjectsMessage = new AppInfoNetworkProtocol.ServerGameObjectsMessage();
            ServerCommandMessage = new AppInfoNetworkProtocol.ServerCommandMessage();
            ServerCommandMessage.OnCommand = OnCommand;
            // Register log callback
            Application.logMessageReceivedThreaded += OnLog;
        }

        // Monobehaviour
        private void LateUpdate() {
            UpdateParameters();
            Server.Update();
        }

        private void OnDestroy() {
            Server.Disconnect();
        }
    }

}

#endif