using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class MeteorMenu {
    [MenuItem("Meteor/重置设置")]
    public static void DeleteState() {
        if (File.Exists(Application.persistentDataPath + "/" + "game_state.dat"))
            File.Delete(Application.persistentDataPath + "/" + "game_state.dat");
    }
    [MenuItem("Meteor/重置资料片设置")]
    public static void DeleteDlcState() {
        if (File.Exists(Application.persistentDataPath + "/" + "dlc_state.dat"))
            File.Delete(Application.persistentDataPath + "/" + "dlc_state.dat");
    }
}


