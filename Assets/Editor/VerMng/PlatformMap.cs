using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class PlatformMap {
    private const string PathBase = "Assets/VerMng/"; 
	public static string GetPlatformPath(UnityEditor.BuildTarget target)
	{
        string SavePath = "";
        if (target == BuildTarget.Android)
            SavePath = PathBase + RuntimePlatform.Android.ToString();
        else if (target == BuildTarget.iOS)
            SavePath = PathBase + RuntimePlatform.IPhonePlayer.ToString();
		else if (target == BuildTarget.StandaloneWindows)
			SavePath = PathBase + RuntimePlatform.WindowsPlayer.ToString();
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
