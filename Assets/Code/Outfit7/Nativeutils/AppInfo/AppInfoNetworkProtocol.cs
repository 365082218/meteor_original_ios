#if !SILVERLIGHT

using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Outfit7.Util.AppInfo {

    public static class AppInfoNetworkProtocol {

        public const int Port = 0;
        public const int BroadcastPort = 21337;
        public const int MaxReceiveSize = 0xffff;

        public enum MessageType {
            ClientInit,
            ClientParameters,
            ClientLog,
            ServerGameObjects,
            ServerCommand,
        };

        public class SyncGameObject {
            private const float Epsilon = 0.0001f;

            private static Dictionary<string, GameObject> GameObjectCache;

            public GameObject GameObject;
            public string GameObjectPath;
            public bool Active;
            public Vector3 LocalPosition;
            public Quaternion LocalRotation;
            public Vector3 LocalScale;

            public SyncGameObject() {
            }

            public SyncGameObject(GameObject gameObject, string path) {
                GameObject = gameObject;
                GameObjectPath = path;
            }

            public bool UpdateGameObject() {
                bool needSync = false;
                if (Active != GameObject.activeSelf) {
                    needSync = true;
                }
                if (Vector3.Distance(LocalPosition, GameObject.transform.localPosition) > Epsilon) {
                    needSync = true;
                }
                if (Quaternion.Dot(LocalRotation, GameObject.transform.localRotation) < 1.0f - Epsilon) {
                    needSync = true;
                }
                if (Vector3.Distance(LocalScale, GameObject.transform.localScale) > Epsilon) {
                    needSync = true;
                }
                if (needSync) {
                    Active = GameObject.activeSelf;
                    LocalPosition = GameObject.transform.localPosition;
                    LocalRotation = GameObject.transform.localRotation;
                    LocalScale = GameObject.transform.localScale;
                    return true;
                }
                return false;
            }

            public void Write(BinaryWriter writer) {
                writer.Write(GameObjectPath);
                writer.Write(Active);
                writer.Write(LocalPosition.x);
                writer.Write(LocalPosition.y);
                writer.Write(LocalPosition.z);
                writer.Write(LocalRotation.x);
                writer.Write(LocalRotation.y);
                writer.Write(LocalRotation.z);
                writer.Write(LocalRotation.w);
                writer.Write(LocalScale.x);
                writer.Write(LocalScale.y);
                writer.Write(LocalScale.z);
            }

            public void Read(BinaryReader reader) {
                GameObjectPath = reader.ReadString();
                Active = reader.ReadBoolean();
                LocalPosition = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                LocalRotation = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                LocalScale = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                if (GameObjectCache == null) {
                    GameObjectCache = new Dictionary<string, GameObject>();
                }
                if (!GameObjectCache.TryGetValue(GameObjectPath, out GameObject)) {
                    GameObject = GameObject.Find(GameObjectPath);
                    if (GameObject != null) {
                        GameObjectCache[GameObjectPath] = GameObject;
                    }
                }
                if (GameObject != null) {
                    GameObject.SetActive(Active);
                    GameObject.transform.localPosition = LocalPosition;
                    GameObject.transform.localRotation = LocalRotation;
                    GameObject.transform.localScale = LocalScale;
                }
            }
        }

        public class SyncParameter {
            public delegate string GetParameterDelegate();

            public delegate void OnParameterChangeDelegate(string newParameter);

            public string Name;
            public string Parameter;
            public GetParameterDelegate GetParameter;
            public float Time;
            public float CurrentTime;

            public SyncParameter(string name, string parameter) {
                Name = name;
                Parameter = parameter;
            }

            public SyncParameter(string name, GetParameterDelegate getParameter, float time) {
                Name = name;
                GetParameter = getParameter;
                Time = time;
                CurrentTime = time;
            }

            public bool UpdateSync(float deltaTime) {
                CurrentTime -= deltaTime;
                if (CurrentTime <= 0.0f) {
                    CurrentTime = Time;
                    string newParameter = GetParameter();
                    if (Parameter == null || newParameter != Parameter) {
                        Parameter = newParameter;
                        return true;
                    }
                }
                return false;
            }

            public void Reset() {
                Parameter = string.Empty;
            }
        }

        public class SyncLog {
            public LogType LogType;
            public string Condition;
            public string StackTrace;

            public SyncLog(LogType logType, string condition, string stackTrace) {
                LogType = logType;
                Condition = condition;
                StackTrace = stackTrace;
            }
        }

        public abstract class Message {
            public abstract MessageType MessageType { get; }

            public abstract void Send(BinaryWriter writer, object data);

            public abstract void Receive(BinaryReader reader);
        };

        public class Command {
            public string Name;

            public delegate void OnInvokeCommandDelegate();

            public OnInvokeCommandDelegate OnInvokeCommand;

            public Command(string name, OnInvokeCommandDelegate onInvokeCommand) {
                Name = name;
                OnInvokeCommand = onInvokeCommand;
            }
        };

        public class ClientParametersMessage : Message {
            public override MessageType MessageType { get { return MessageType.ClientParameters; } }

            public delegate void OnParameterDelegate(string name, string parameter);

            public OnParameterDelegate OnParameter;

            public override void Send(BinaryWriter writer, object data) {
                List<SyncParameter> syncParameters = data as List<SyncParameter>;
                writer.Write(syncParameters.Count);
                for (int i = 0; i < syncParameters.Count; i++) {
                    SyncParameter syncParameter = syncParameters[i];
                    writer.Write(syncParameter.Name);
                    writer.Write(syncParameter.Parameter);
                }
            }

            public override void Receive(BinaryReader reader) {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++) {
                    string name = reader.ReadString();
                    string parameter = reader.ReadString();
                    OnParameter(name, parameter);
                }
            }
        }

        public class ClientLogMessage : Message {
            public override MessageType MessageType { get { return MessageType.ClientLog; } }

            public override void Send(BinaryWriter writer, object data) {
                SyncLog syncLog = data as SyncLog;
                writer.Write((byte) syncLog.LogType);
                writer.Write(syncLog.Condition);
                writer.Write(syncLog.StackTrace);
            }

            public override void Receive(BinaryReader reader) {
                LogType logType = (LogType) reader.ReadByte();
                string condition = reader.ReadString();
                string stackTrace = reader.ReadString();
                switch (logType) {
                    case LogType.Assert:
                    case LogType.Error:
                    case LogType.Exception:
                        Debug.LogErrorFormat("REMOTE: {0}\n{1}", condition, stackTrace);
                        break;
                    case LogType.Log:
                        Debug.LogFormat("REMOTE: {0}", condition);
                        break;
                    case LogType.Warning:
                        Debug.LogWarningFormat("REMOTE: {0}", condition);
                        break;
                }
            }
        }

        public class ClientInitMessage : Message {

            public override MessageType MessageType { get { return MessageType.ClientInit; } }

            public delegate void OnInitDelegate(string[] commands);

            public OnInitDelegate OnInit;

            public override void Send(BinaryWriter writer, object data) {
                Dictionary<string,Command> commands = data as Dictionary<string,Command>;
                writer.Write(commands.Count);
                foreach (KeyValuePair<string,Command> pair in commands) {
                    writer.Write(pair.Key);
                }
            }

            public override void Receive(BinaryReader reader) {
                int count = reader.ReadInt32();
                string[] names = new string[count];
                for (int i = 0; i < count; i++) {
                    names[i] = reader.ReadString();
                }
                OnInit(names);
            }
        }


        public class ServerGameObjectsMessage : Message {
            public override MessageType MessageType { get { return MessageType.ServerGameObjects; } }

            public override void Send(BinaryWriter writer, object data) {
                List<SyncGameObject> syncGameObjects = data as List<SyncGameObject>;
                writer.Write(syncGameObjects.Count);
                for (int i = 0; i < syncGameObjects.Count; i++) {
                    syncGameObjects[i].Write(writer);
                }
            }

            public override void Receive(BinaryReader reader) {
                int count = reader.ReadInt32();
                SyncGameObject syncGameObject = new SyncGameObject();
                for (int i = 0; i < count; i++) {
                    syncGameObject.Read(reader);
                }
            }
        }

        public class ServerCommandMessage : Message {

            public override MessageType MessageType { get { return MessageType.ServerCommand; } }

            public delegate void OnCommandDelegate(string command);

            public OnCommandDelegate OnCommand;

            public override void Send(BinaryWriter writer, object data) {
                string command = data as string;
                writer.Write(command);
            }

            public override void Receive(BinaryReader reader) {
                string command = reader.ReadString();
                OnCommand(command);
            }
        }

        public static void SendMessage(ClientServer clientServer, AppInfoNetworkProtocol.Message message, object data) {
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(memoryStream);
            writer.Write((byte) message.MessageType);
            message.Send(writer, data);
            byte[] sendData = memoryStream.ToArray();
            clientServer.Send(sendData, sendData.Length);
        }
    }
}

#endif