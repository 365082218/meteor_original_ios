#if !SILVERLIGHT

using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Outfit7.Util.AppInfo {

    public abstract class AppInfoClientParameterGUI {
        public abstract string ParameterName { get; }

        public abstract void OnParameter(string parameter);

        public abstract void OnGUI(string parameter);
    };

    public class AppInfoClient : EditorWindow {

        private float ServerInfoTimeout = 15.0f;

        private class ServerInfo {
            public string Address;
            public int Port;
            public string Message;
            public float Time;
            public int Hash;

            public ServerInfo(string address, int port, string message, int hash) {
                Address = address;
                Port = port;
                Message = message;
                Hash = hash;
            }
        }

        [MenuItem("Outfit7/Util/AppInfoClient")]
        public static void ShowAppInfoClient() {
            EditorWindow.GetWindow(typeof(AppInfoClient));
        }

        private bool IsConnecting = false;
        private ClientServer Client;
        private List<ServerInfo> ServerInfos;
        private string[] ServerInfoStrings;

        private AppInfoNetworkProtocol.ClientParametersMessage ClientParametersMessage = new AppInfoNetworkProtocol.ClientParametersMessage();
        private AppInfoNetworkProtocol.ClientLogMessage ClientLogMessage = new AppInfoNetworkProtocol.ClientLogMessage();
        private AppInfoNetworkProtocol.ClientInitMessage ClientInitMessage = new AppInfoNetworkProtocol.ClientInitMessage();
        private AppInfoNetworkProtocol.ServerCommandMessage ServerCommandMessage = new AppInfoNetworkProtocol.ServerCommandMessage();
        private AppInfoNetworkProtocol.ServerGameObjectsMessage ServerGameObjectsMessage = new AppInfoNetworkProtocol.ServerGameObjectsMessage();

        private string[] Commands = new string[0];
        private List<AppInfoNetworkProtocol.SyncParameter> SyncParameters = new List<AppInfoNetworkProtocol.SyncParameter>();

        private List<AppInfoNetworkProtocol.SyncGameObject> SyncGameObjects = new List<AppInfoNetworkProtocol.SyncGameObject>();

        private string SelectedServer = string.Empty;

        private bool ShowCommands = true;
        private bool ShowParameters = true;

        private Dictionary<string, AppInfoClientParameterGUI> ParameterGUIs;

        private void OnConnected(IntPtr client) {
            NativeUtils.LogFormat(NativeUtils.LogLevel.Warning, "OnConnected {0}", client);
        }

        private void OnDisconnected(IntPtr client) {
            NativeUtils.LogFormat(NativeUtils.LogLevel.Warning, "OnDisconnected {0}", client);
            IsConnecting = false;
        }

        private void OnReceive(IntPtr client, byte[] data) {
            MemoryStream memoryStream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(memoryStream);
            AppInfoNetworkProtocol.MessageType messageType = (AppInfoNetworkProtocol.MessageType) reader.ReadByte();
            switch (messageType) {
                case AppInfoNetworkProtocol.MessageType.ClientInit:
                    ClientInitMessage.Receive(reader);
                    break;
                case AppInfoNetworkProtocol.MessageType.ClientLog:
                    ClientLogMessage.Receive(reader);
                    break;
                case AppInfoNetworkProtocol.MessageType.ClientParameters:
                    ClientParametersMessage.Receive(reader);
                    break;
            }
        }

        private void UpdateServers() {
            float time = Time.realtimeSinceStartup;
            // Delete old server infos
            bool removed = false;
            for (int i = 0; i < ServerInfos.Count; i++) {
                float timeDiff = (time - ServerInfos[i].Time);
                if (timeDiff > ServerInfoTimeout) {
                    NativeUtils.LogFormat(NativeUtils.LogLevel.Verbose, "AppInfoServer timeout: {0}", ServerInfos[i].Address);
                    ServerInfos.RemoveAt(i--);
                    removed = true;
                }
            }
            // Rebuild string list
            if (removed || ServerInfoStrings.Length != ServerInfos.Count) {
                ServerInfoStrings = new string[ServerInfos.Count];
                for (int i = 0; i < ServerInfos.Count; i++) {
                    ServerInfo serverInfo = ServerInfos[i];
                    ServerInfoStrings[i] = string.Format("{0}({1}:{2})", serverInfo.Message, serverInfo.Address, serverInfo.Port);
                }
            }
        }

        private void OnServer(string address, int port, string message) {
            int hash = string.Format("{0}|{1}|{2}", address, port, message).GetHashCode();
            ServerInfo serverInfo = ServerInfos.Find((si) => si.Hash == hash);
            if (serverInfo == null) {                
                serverInfo = new ServerInfo(address, port, message, hash);
                ServerInfos.Add(serverInfo);
                NativeUtils.LogFormat(NativeUtils.LogLevel.Verbose, "Added AppInfoServer: {0}", serverInfo.Address);
            }
            serverInfo.Time = Time.realtimeSinceStartup;
            UpdateServers();
        }

        private void OnInit(string[] commands) {
            Commands = commands;
        }

        private void OnParameter(string name, string parameter) {
            int index = SyncParameters.FindIndex((obj) => obj.Name == name);
            if (index == -1) {
                SyncParameters.Add(new AppInfoNetworkProtocol.SyncParameter(name, parameter));
            } else {
                SyncParameters[index].Parameter = parameter;
            }
            AppInfoClientParameterGUI parameterGUI = null;
            if (ParameterGUIs.TryGetValue(name, out parameterGUI)) {
                parameterGUI.OnParameter(parameter);
            }
        }

        private void CreateParameterGUIs() {
            // Find types
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            Type[] possible = (types.Where(type => type.IsSubclassOf(typeof(AppInfoClientParameterGUI)))).ToArray();

            // Create exporters
            ParameterGUIs = new Dictionary<string, AppInfoClientParameterGUI>();
            foreach (Type type in possible) {
                if (type.IsAbstract) {
                    continue;
                }
                AppInfoClientParameterGUI parameterGUI = Activator.CreateInstance(type) as AppInfoClientParameterGUI;
                Debug.LogFormat("AppInfoClientParameterGUI {0} found!", parameterGUI.ParameterName);
                ParameterGUIs.Add(parameterGUI.ParameterName, parameterGUI);
            }
        }

        private void OnSelectionChange() {
            SyncGameObjects.Clear();
            for (int i = 0; i < Selection.gameObjects.Length; i++) {
                SyncGameObjects.Add(new AppInfoNetworkProtocol.SyncGameObject(Selection.gameObjects[i], AnimationUtility.CalculateTransformPath(Selection.gameObjects[i].transform, null)));
            }
        }

        private void CreateClient() {
            Debug.LogWarningFormat("CreateClient");
            ServerInfos = new List<ServerInfo>();
            ServerInfoStrings = new string[0];
            // Create client
            Client = new ClientServer(AppInfoNetworkProtocol.MaxReceiveSize);
            Client.CreateServerInfoClient(AppInfoNetworkProtocol.BroadcastPort);
            Client.OnConnected = OnConnected;
            Client.OnDisconnected = OnDisconnected;
            Client.OnReceive = OnReceive;
            Client.OnServer = OnServer;
            // Create messages
            ClientInitMessage.OnInit = OnInit;
            ClientParametersMessage.OnParameter = OnParameter;
        }

        private void DestroyClient() {
            Debug.LogWarningFormat("DestroyClient");
            Client.Destroy();
        }

        // EditorWindow
        private void OnEnable() {
            titleContent.text = "App Info Client";
            CreateClient();
            CreateParameterGUIs();
        }

        private void OnDisable() {
            DestroyClient();
        }

        private void OnGUI() {
            if (IsConnecting || Client.IsClientConnected()) {
                if (GUILayout.Button("Disconnect")) {
                    IsConnecting = false;
                    Client.Disconnect();
                }
            }
            if (!IsConnecting && !Client.IsClientConnected()) {
                if (ServerInfos.Count > 0) {
                    int index = ArrayUtility.FindIndex(ServerInfoStrings, (s) => SelectedServer == s);
                    if (index == -1) {
                        index = 0;
                    }
                    index = EditorGUILayout.Popup("Server", index, ServerInfoStrings);
                    if (index != -1) {
                        SelectedServer = ServerInfoStrings[index];
                        if (GUILayout.Button("Connect")) {
                            ServerInfo serverInfos = ServerInfos[index];
                            if (Client.CreateClient(serverInfos.Address, serverInfos.Port)) {
                                IsConnecting = true;
                            }
                        }
                    }
                }
            } else if (Client.IsClientConnected()) {
                ShowCommands = EditorGUILayout.Foldout(ShowCommands, "Commands");
                if (ShowCommands) {
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < Commands.Length; i++) {
                        if (GUILayout.Button(Commands[i])) {
                            AppInfoNetworkProtocol.SendMessage(Client, ServerCommandMessage, Commands[i]);
                        }
                    }
                    EditorGUI.indentLevel--;
                }
                ShowParameters = EditorGUILayout.Foldout(ShowParameters, "Parameters");
                if (ShowParameters) {
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < SyncParameters.Count; i++) {
                        AppInfoNetworkProtocol.SyncParameter syncParameter = SyncParameters[i];
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(syncParameter.Name, GUILayout.Width(200));
                        AppInfoClientParameterGUI parameterGUI = null;
                        if (ParameterGUIs.TryGetValue(syncParameter.Name, out parameterGUI)) {
                            parameterGUI.OnGUI(syncParameter.Parameter);
                        } else {
                            GUILayout.Label(syncParameter.Parameter);
                        }
                        GUILayout.EndHorizontal();
                    }
                    EditorGUI.indentLevel--;
                }
                // Check sync objects
                bool doSyncGameObjects = false;
                for (int i = 0; i < SyncGameObjects.Count; i++) {
                    if (SyncGameObjects[i].GameObject == null) {
                        SyncGameObjects.RemoveAt(i--);
                        continue;
                    }
                    if (SyncGameObjects[i].UpdateGameObject()) {
                        doSyncGameObjects = true;
                    }
                }
                if (doSyncGameObjects) {
                    AppInfoNetworkProtocol.SendMessage(Client, ServerGameObjectsMessage, SyncGameObjects);
                }
            }
        }

        private void Update() {
            Client.Update();
            UpdateServers();
            Repaint();
        }
    }

}

#endif