using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class MeteorMenu {
    [MenuItem("Meteor/重置设置")]
    public static void DeleteState() {
        string path = string.Format("{0}/{1}/game_state.dat", Application.persistentDataPath, AppInfo.Ins.AppVersion());
        if (File.Exists(path))
            File.Delete(path);
    }
    [MenuItem("Meteor/重置资料片设置")]
    public static void DeleteDlcState() {
        string path = string.Format("{0}/{1}/dlc_state.dat", Application.persistentDataPath, AppInfo.Ins.AppVersion());
        if (File.Exists(path))
            File.Delete(path);
    }
}


