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
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "NOLOG");//设置宏
        string targetPath = "";
        targetPath = EditorUtility.SaveFolderPanel("选择存储路径", "D:/", PlayerSettings.bundleIdentifier + "_" + NewVersion);
        if (string.IsNullOrEmpty(targetPath))
        {
            WSLog.LogError("user canceled");
            return;
        }

        BuildPipeline.BuildPlayer(VerMng.GetBuildScenes().ToArray(), targetPath, BuildTarget.Android, BuildOptions.None);
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
}