using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FrameSnapShot:Singleton<FrameSnapShot> {

    //新的一次复盘
    BinaryReader reader;
    BinaryWriter writer;
    string path;
    int trackerId;
    //使用新的目录存储逻辑帧快照
    public void NewTracker(int id) {
        trackerId = id;
        string baseDir = string.Format("{0}/SnapShot", Application.persistentDataPath);
        if (!Directory.Exists(baseDir))
            Directory.CreateDirectory(baseDir);
        string file = string.Format("{0}/SnapShot/{1}.snapshot", Application.persistentDataPath, trackerId);
        if (File.Exists(file))
            File.Delete(file);
        writer = new BinaryWriter(new FileStream(file, FileMode.CreateNew));
    }

    public void SnapShot(int logicframe, List<NetBehaviour> netActors) {
        if (writer != null) {
            for (int i = 0; i < netActors.Count; i++) {
                netActors[i].Write(writer);
            }
        }
    }

    public void Close() {
        if (writer != null) {
            writer.Flush();
            writer.Close();
            writer = null;
        }
    }

    public void LoadSnapShot(int logicframe, List<NetBehaviour> netActors) {
        for (int i = 0; i < netActors.Count; i++) {
            netActors[i].Read(reader);
        }
    }
}

public static class TransformExtension {
    public static void Write(this Transform transform, BinaryWriter writer){
        writer.Write(transform.position.x);
        writer.Write(transform.position.y);
        writer.Write(transform.position.z);
    }
}

public static class ActionMgrExtension {
    public static void Write(this ActionManager actionmgr, BinaryWriter writer) {
    }
}