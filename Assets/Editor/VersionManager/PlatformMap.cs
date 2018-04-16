using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class PlatformMap {

	public static string GetPlatformPath(UnityEditor.BuildTarget target)
	{
        string SavePath = "";
        if (target == BuildTarget.Android)
            SavePath = "Assets/VersionManager/" + RuntimePlatform.Android.ToString();
        else if (target == BuildTarget.iOS)
            SavePath = "Assets/VersionManager/" + RuntimePlatform.IPhonePlayer.ToString();
		else if (target == BuildTarget.StandaloneWindows)
			SavePath = "Assets/VersionManager/" + RuntimePlatform.WindowsPlayer.ToString();
        else
        {
            Debug.LogError("target invalid");
            return SavePath;
        }

        if (Directory.Exists(SavePath) == false)
        {
            Directory.CreateDirectory(SavePath);
            AssetDatabase.Refresh();
        }
        return SavePath;
	}
}
