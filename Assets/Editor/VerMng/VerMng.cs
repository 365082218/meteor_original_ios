using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.IO;
using LitJson;
public class VersionItem
{
    public string strVersion;//版本号
}

public class VerMng : EditorWindow{
    
    public static string SavePath
    {
        get { return PlatformMap.GetPlatformPath(Target) + "/" + Version + "/"; }
    }

    internal static string strVersionRoot = "VersionTotal";
	internal static string strVersionElement = "Version";
	internal static string strVersionAttr = "Ver";
	internal static string strVerFile = "Version.json";
    internal static string Version = "0.5.2.0";
    internal static BuildTarget Target;
    //获取某个平台的全部版本.
    public static List<string> GetAllVersion(UnityEditor.BuildTarget target)
	{
		List<string> strRet = new List<string>();
		string strVersionPath;
		strVersionPath = PlatformMap.GetPlatformPath(target);
		strVersionPath += "/" + strVerFile;
        strRet = ReadVersion(strVersionPath);
		return strRet;
	}

	public static List<string> GetHistoryVersion(UnityEditor.BuildTarget target)
	{
		string strTarget = System.Enum.GetName(typeof(BuildTarget), target);
		if (strTarget == null || strTarget == "")
			return null;

		List<string> strRet = new List<string>();

		string strVersionPath;
		strVersionPath = PlatformMap.GetPlatformPath(target);
		strVersionPath += "/" + strVerFile;
		strRet =  ReadVersion(strVersionPath);
		if (strRet.Count != 0)
		{
			string strVersion = strRet[0];
			for (int i = 1; i <= strRet.Count - 1; ++i)
			{
				if (strVersion.CompareTo(strRet[i]) == -1)
					strVersion = strRet[i];
			}
			strRet.Remove(strVersion);
		}
		return strRet;
	}

	static List<string> ReadVersion(string fileName)
	{
		List<string> strCurver = new List<string>();
		if (System.IO.File.Exists(fileName) == false)
			return strCurver;
        strCurver = JsonMapper.ToObject<List<string>>(File.ReadAllText(fileName), false);
		return strCurver;
	}

    public static string[] GetBuildScenes()
    {
        List<string> names = new List<string>();
        foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
        {
            if (e == null)
                continue;
            if (e.enabled)
                names.Add(e.path);
        }
        return names.ToArray();
    }

    public static void CreateNewVersion(BuildTarget target, string NewVersion)
	{
        Version = NewVersion;
        Target = target;
		string strVer = NewVersion;
		List<string> strVersionLst = GetAllVersion(target);

        if (!File.Exists(PlatformMap.GetPlatformPath(target) + "/" + strVer))
            System.IO.Directory.CreateDirectory(PlatformMap.GetPlatformPath(target) + "/" + strVer);

        //开始打包.=>打包预设=>打包在BuildSetting内的场景=>打包Resources目录下的其他资源=>加密资源=>压缩资源=>放置在版本路径，记录其信息（文件大小，是否加密，是否压缩，文件hash值）=>生成版本资源清单=>编辑版本更新信息
        //=>依次各个版本清单文件对比，生成各个版本的一次性更新差异文件，把该平台下的资源打包后的存储路径放到服务器，就完成了更新配置.
        //生成资源清单文件.
        CreateAssetBundle.PackageAllPrefab(target, strVer);//打包所有资源
        BuildScene(target);//打包所有场景
        CreateMD5List.GenFileListXml(PlatformMap.GetPlatformPath(target), strVer);//生成文件信息列表
        GenAllUpdateVersionXml.Execute(target);//用当前版本对比之前各个版本，生成一次更新文件
        UpdateVersion(target, strVersionLst, strVer);//把最新版本写入到版本列表
		AssetDatabase.Refresh();
        //打包前设置
        PlayerSettings.bundleIdentifier = "com.Idevgame.Meteor";
        PlayerSettings.iPhoneBundleIdentifier = "com.Idevgame.Meteor";
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "NOLOG");//设置宏
        string targetPath = "";
        targetPath = EditorUtility.SaveFolderPanel("选择存储路径", "D:/", PlayerSettings.bundleIdentifier + "_" + NewVersion);
        if (string.IsNullOrEmpty(targetPath))
        {
            WSLog.Log("user canceled");
            return;
        }

        BuildPipeline.BuildPlayer(GetBuildScenes(), targetPath, BuildTarget.Android, BuildOptions.None);
	}

	public static bool UpdateVersion(BuildTarget target, List<string>strOldVersionLst, string strNewVer)
	{
		string strVersionPath;
		strVersionPath = PlatformMap.GetPlatformPath(target);
		strVersionPath += "/" + strVerFile;
		return true;
	}

	void OnGUI()
	{
	}

    public static bool BuildScene(UnityEditor.BuildTarget target)
    {
        string[] files = VerMng.GetBuildScenes();
        int packagefile = 0;
        int unpackagefile = 0;
        foreach (string file in files)
        {
            string path = file.Replace("\\", "/");
            string name = "";
            int nNameBegin = path.LastIndexOf('/');
            int nNameEnd = path.LastIndexOf('.');
            name = path.Substring(nNameBegin + 1, nNameEnd - nNameBegin - 1);
            packagefile++;
            path = VerMng.SavePath + name;
            path += ".unity3d";
            BuildPipeline.BuildPlayer(new string[1] { file }, path, target, BuildOptions.BuildAdditionalStreamedScenes);
        }
        EditorUtility.DisplayDialog("Tip", "Package scene : " + packagefile.ToString() + "\r\nunPackage scene : " + unpackagefile.ToString(), "OK");
        AssetDatabase.Refresh();
        return true;
    }
}
