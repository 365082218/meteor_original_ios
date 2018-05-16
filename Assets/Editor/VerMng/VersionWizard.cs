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
    }

    void OnWizardOtherButton()
    {

    }

    void OnWizardUpdate()
    {
    }
}