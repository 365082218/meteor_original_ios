using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using LitJson;
using Idevgame.Util;

public static class BuildDefine
{
    public const string StripKeyBoard = "STRIP_KEYBOARD";//剔除键盘操作-导出移动包必选
    // Editor
    public const string DefineEditor = "UNITY_EDITOR";
    public const string DefineDebugBuild = "DEVEL_BUILD";
    public const string DefineProdBuild = "PROD_BUILD";
    public const string DefineStripLogs = "STRIP_LOGS";
    public const string DefineStripDebugSettings = "STRIP_DBG_SETTINGS";
    public const string DefineRavenCompiled = "RAVEN_COMPILED";
    // Testing
    public const string DefineStripTest = "STRIP_TEST";

    // native simulation
    public const string DefineNativeSimulationBuild = "NATIVE_SIM";

    // WP8, WSA
    public const string DefineWindows = "UNITY_WP8";

    // Android
    public const string DefineAndroid = "UNITY_ANDROID";
    public const string DefineEveryplayAndroid = "EVERYPLAY_ANDROID";
    public const string Define360 = "ANDROID_360";
    public const string DefineAndroidBemobi = "UNITY_BEMOBI";
    public const string DefineAmazon = "ANDROID_AMAZON";

    // iOS
    public const string DefineIOS = "UNITY_IPHONE";
    public const string DefineEveryplayIOS = "EVERYPLAY_IPHONE";

    // build script related
    public const string ExportPathDefine = "exportPath";
}

public class BuildWizardAndroid : ScriptableWizard {
    public string NewVersion;
    public string Tip = "版本0.0.0.0为默认初始版本";
	[MenuItem("Meteor/Build/Build Android", false, 0)]
	static void CreateWizard()
	{
		ScriptableWizard.DisplayWizard<BuildWizardAndroid>("打包", "打包");
	}

	void OnWizardCreate()
	{
        if (string.IsNullOrEmpty(NewVersion))
        {
            EditorUtility.DisplayDialog("错误", "需要设置新版本号", "退出");
            CreateWizard();
            return;
        }
        //检查是否需要初始化
        VerMng.Initialize(BuildTarget.Android);
		VerMng.CreateNewVersion(BuildTarget.Android, NewVersion);
        //打包前设置
        //PlayerSettings.bundleIdentifier = "com.Idevgame.Meteor";
        //PlayerSettings.iPhoneBundleIdentifier = "com.Idevgame.Meteor";
        //PlayerSettings.bundleVersion = NewVersion;
        //EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ETC;
        //PlayerSettings.use32BitDisplayBuffer = true;
        //PlayerSettings.Android.useAPKExpansionFiles = true;
        //PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        //PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        //PlayerSettings.allowedAutorotateToLandscapeRight = true;
        //PlayerSettings.allowedAutorotateToPortrait = false;
        //PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

        ////PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "NOLOG");//设置宏
        //string targetPath = "";
        //targetPath = EditorUtility.SaveFolderPanel("选择存储路径", "D:/", "");
        //if (string.IsNullOrEmpty(targetPath))
        //{
        //    WSLog.LogError("user canceled");
        //    return;
        //}
        //Debug.LogError(string.Format("{0} selected", targetPath + "/" + PlayerSettings.bundleIdentifier + "_" + NewVersion + ".apk"));
        //string[] scenes = VerMng.GetBuildScenes().ToArray();
        //BuildPipeline.BuildPlayer(scenes, targetPath + "/" + PlayerSettings.bundleIdentifier + "_" + NewVersion + ".apk", BuildTarget.Android, BuildOptions.None);
        //string arg = string.Format("/select,\"{0}\"", targetPath + "/" + PlayerSettings.bundleIdentifier + "_" + NewVersion + ".apk");
        //arg = arg.Replace("//", "\\");
        //Debug.LogError(arg);
        //System.Diagnostics.Process.Start("explorer.exe", arg);
    }

	void OnWizardOtherButton()
	{

	}

	void OnWizardUpdate()
	{
	}
}

public class BuildWizardIos : ScriptableWizard
{
    public string NewVersion;
    [MenuItem("Meteor/Build/Build Ios", false, 1)]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<BuildWizardAndroid>("打包", "打包");
    }

    void OnWizardCreate()
    {
        if (string.IsNullOrEmpty(NewVersion))
        {
            EditorUtility.DisplayDialog("错误", "需要设置新版本号", "退出");
            CreateWizard();
            return;
        }
        VerMng.Initialize(BuildTarget.iOS);
        VerMng.CreateNewVersion(BuildTarget.iOS, NewVersion);
        //打包前设置
        //PlayerSettings.bundleIdentifier = "com.Idevgame.Meteor";
        //PlayerSettings.iPhoneBundleIdentifier = "com.Idevgame.Meteor";
        //PlayerSettings.bundleVersion = NewVersion;
        //PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, "NOLOG");//设置宏
        //string targetPath = "";
        //targetPath = EditorUtility.SaveFolderPanel("选择存储路径", "D:/", PlayerSettings.bundleIdentifier + "_" + NewVersion);
        //if (string.IsNullOrEmpty(targetPath))
        //{
        //    WSLog.LogError("user canceled");
        //    return;
        //}

        //BuildPipeline.BuildPlayer(VerMng.GetBuildScenes().ToArray(), targetPath, BuildTarget.iOS, BuildOptions.None);
    }

    void OnWizardOtherButton()
    {

    }

    void OnWizardUpdate()
    {
    }
}

public class BuildTool
{
    [MenuItem("Meteor/Build/Clean", false, 2)]
    static void Clear()
    {
        string[] files = Directory.GetFiles(VerMng.GetPlatformPath(BuildTarget.Android), "*.*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
            File.Delete(files[i]);
        Directory.Delete(VerMng.GetPlatformPath(BuildTarget.Android), true);

        string[] files2 = Directory.GetFiles(VerMng.GetPlatformPath(BuildTarget.iOS), "*.*", SearchOption.AllDirectories);
        for (int i = 0; i < files2.Length; i++)
            File.Delete(files2[i]);
        Directory.Delete(VerMng.GetPlatformPath(BuildTarget.iOS), true);
    }

    //[MenuItem("MeteorTool/Build/TestDepend", false, 0)]
    //static void TestDepend()
    //{
    //    //加载依赖表
    //    VerMng.TestDepend(BuildTarget.Android, "0.5.2.0");
    //}

    //[MenuItem("MeteorTool/Build/TestCopy", false, 0)]
    //static void TestCopy()
    //{
    //    //测试复制文件到一个基路径
    //    DirectoryInfo baseDir = Directory.CreateDirectory(System.IO.Path.GetTempPath() + string.Format("{0}_{1}", "0.0.0.0", "0.5.2.0"));
    //    string file = "9.07/characteract.bytes";
    //    string dir = baseDir.FullName + "/" + file;
    //    int dirIndex = file.LastIndexOf('/');
    //    if (dirIndex != -1)
    //    {
    //        dir = baseDir.FullName + "/" + file.Substring(0, dirIndex);
    //        Directory.CreateDirectory(dir);
    //    }
    //    File.Copy(VerMng.GetPlatformPath(BuildTarget.Android) + "/" + "0.5.2.0" + "/" + "9.07/characteract.bytes", baseDir.FullName + "/" + "9.07/characteract.bytes");
    //}

    [MenuItem("Meteor/Build/Delete PlayerPref", false, 3)]
    static void DeletePrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    [MenuItem("Meteor/Build/Delete GameSave", false, 3)]
    static void DeleteSave()
    {
        GameData.Instance.ResetState();
    }
    //[MenuItem("MeteorTool/CollectDependence", false, 0)]
    //static void CollectDependence()
    //{
    //    string[] depend = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(Selection.activeObject));
    //    for (int i = 0; i < depend.Length; i++)
    //    {
    //        UnityEngine.Debug.LogError(depend[i]);
    //    }
    //}
}