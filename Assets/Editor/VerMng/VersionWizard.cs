using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public class BuildWizardAndroid : ScriptableWizard {
    public string NewVersion;

	[MenuItem("MeteorTool/Build/Build Android", false, 0)]
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

		VerMng.CreateNewVersion(BuildTarget.Android, NewVersion);
        //打包前设置
        PlayerSettings.bundleIdentifier = "com.Idevgame.Meteor";
        PlayerSettings.iPhoneBundleIdentifier = "com.Idevgame.Meteor";
        PlayerSettings.bundleVersion = NewVersion;
        EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ETC;
        PlayerSettings.use32BitDisplayBuffer = true;
        PlayerSettings.Android.useAPKExpansionFiles = true;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeRight;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "NOLOG");//设置宏
        string targetPath = "";
        targetPath = EditorUtility.SaveFolderPanel("选择存储路径", "D:/", "");
        if (string.IsNullOrEmpty(targetPath))
        {
            WSLog.LogError("user canceled");
            return;
        }
        string[] scenes = VerMng.GetBuildScenes().ToArray();
        BuildPipeline.BuildPlayer(scenes, targetPath + PlayerSettings.bundleIdentifier + "_" + NewVersion + ".apk", BuildTarget.Android, BuildOptions.None);
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
    [MenuItem("MeteorTool/Build/Build Ios", false, 0)]
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

        VerMng.CreateNewVersion(BuildTarget.iOS, NewVersion);
        //打包前设置
        PlayerSettings.bundleIdentifier = "com.Idevgame.Meteor";
        PlayerSettings.iPhoneBundleIdentifier = "com.Idevgame.Meteor";
        PlayerSettings.bundleVersion = NewVersion;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, "NOLOG");//设置宏
        string targetPath = "";
        targetPath = EditorUtility.SaveFolderPanel("选择存储路径", "D:/", PlayerSettings.bundleIdentifier + "_" + NewVersion);
        if (string.IsNullOrEmpty(targetPath))
        {
            WSLog.LogError("user canceled");
            return;
        }

        BuildPipeline.BuildPlayer(VerMng.GetBuildScenes().ToArray(), targetPath, BuildTarget.iOS, BuildOptions.None);
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
    [MenuItem("MeteorTool/Build/Clean", false, 0)]
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

    [MenuItem("MeteorTool/Build/TestDepend", false, 0)]
    static void TestDepend()
    {
        //加载依赖表
        VerMng.TestDepend(BuildTarget.Android, "0.5.2.0");
    }

    [MenuItem("MeteorTool/Build/TestCopy", false, 0)]
    static void TestCopy()
    {
        //测试复制文件到一个基路径
        DirectoryInfo baseDir = Directory.CreateDirectory(System.IO.Path.GetTempPath() + string.Format("{0}_{1}", "0.0.0.0", "0.5.2.0"));
        string file = "9.07/characteract.bytes";
        string dir = baseDir.FullName + "/" + file;
        int dirIndex = file.LastIndexOf('/');
        if (dirIndex != -1)
        {
            dir = baseDir.FullName + "/" + file.Substring(0, dirIndex);
            Directory.CreateDirectory(dir);
        }
        File.Copy(VerMng.GetPlatformPath(BuildTarget.Android) + "/" + "0.5.2.0" + "/" + "9.07/characteract.bytes", baseDir.FullName + "/" + "9.07/characteract.bytes");
    }

    
}