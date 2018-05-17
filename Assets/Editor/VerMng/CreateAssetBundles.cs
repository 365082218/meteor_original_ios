using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System;

public class CreateAssetBundle
{
    public static List<string> strBuilded = new List<string>();
    public static bool PackageAll(BuildTarget target, string newVersion)
	{
        //很大粒度打包，不太好使用
        string path = "";
        if (!File.Exists(PlatformMap.GetPlatformPath(BuildTarget.Android) + "/" + newVersion))
            System.IO.Directory.CreateDirectory(PlatformMap.GetPlatformPath(BuildTarget.Android) + "/" + newVersion);
        path = PlatformMap.GetPlatformPath(BuildTarget.Android) + "/" + newVersion;
        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, BuildTarget.Android);
        return true;
	}

    static void PackageOnePrefab(UnityEditor.BuildTarget target, UnityEngine.Object SelectObject)
    {
        string file = AssetDatabase.GetAssetPath(SelectObject);
		UnityEngine.Object []depend = EditorUtility.CollectDependencies(new UnityEngine.Object[]{SelectObject});
		string [] strdepend = AssetDatabase.GetDependencies(new string[]{file});
        file = file.Replace('\\', '/');
        string path = file;
        string name = "";
        int nNameBegin = path.LastIndexOf('/');
        int nNameEnd = path.LastIndexOf('.');
        name = path.Substring(nNameBegin + 1, nNameEnd - nNameBegin - 1);
		//SavePath = PlatformMap.GetPlatformPath(target) + "/" + VersionManager.GetCurVersion(target)+ "/";
        path = VerMng.SavePath + path;
        path = path.Substring(0, path.LastIndexOf('.'));
        path += ".assetbundle";
		//Directory.CreateDirectory(SavePath);
        //if (strBuilded.Contains(file))
        //    return;
		//BuildPipeline.BuildAssetBundle(SelectObject, null, SavePath, BuildAssetBundleOptions.CompleteAssets | BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.CollectDependencies, target);
    }
}