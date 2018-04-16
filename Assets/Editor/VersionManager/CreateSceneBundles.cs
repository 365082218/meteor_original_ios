using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;


public class CreateSceneBundles
{
	//[MenuItem("Assets/Tool/Single Scene PackageWizard", false, 10)]
	//public static void CreateSingleSceneBundle()
	//{
	//	var path = EditorUtility.SaveFilePanel ("Build Bundle", "", "*", "unity3d");
 //       if (path == "")
 //           return;
	//	string SelectPath = AssetDatabase.GetAssetPath(Selection.activeObject);
	//	string [] scene = new string[]{SelectPath};
	//	BuildPipeline.BuildStreamedSceneAssetBundle (scene, path, BuildTarget.iOS);
	//}

	public static bool Execute(UnityEditor.BuildTarget target, List<string>ignoreFilelist, Object SelectObject)
	{
		string SavePath = "";
		try
		{
			SavePath = PlatformMap.GetPlatformPath(target) + "/" + VersionManager.GetCurVersion(target) + "/";
		}
		catch(IOException exp)
		{
			EditorUtility.DisplayDialog("Error", exp.Message, "OK");
			return false;
		}

		string SelectPath = AssetDatabase.GetAssetPath(SelectObject);
		string []files = Directory.GetFiles(SelectPath, "*.unity", SearchOption.AllDirectories);
		int packagefile = 0;
		int unpackagefile = 0;
		foreach (string file in files)
		{
			string path = file.Replace("\\", "/");
            string name = "";
            int nNameBegin = path.LastIndexOf('/');
            int nNameEnd = path.LastIndexOf('.');
            name = path.Substring(nNameBegin + 1, nNameEnd - nNameBegin - 1);
            if (ignoreFilelist != null)
			{
				bool bIgnore = false;
				foreach (string strIgnore in ignoreFilelist)
				{
					if (name == strIgnore)
					{
						bIgnore = true;
						break;
					}
				}
				
				if (bIgnore)
				{
					unpackagefile++;
					continue;
				}
			}
			packagefile++;
			path = SavePath + name;
			//path =  path.Substring(0, path.LastIndexOf('.'));
			path += ".unity3d";
			BuildPipeline.BuildPlayer(new string[1]{file}, path, target, BuildOptions.BuildAdditionalStreamedScenes);
            //break;
		}

		//int nRet = 0;
		// 当前选中的资源列表
//		foreach (Object o in Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets))
//		{
//			string path = AssetDatabase.GetAssetPath(o);
//			string []strLevel = new string[1];
//			strLevel[0] = path;
//			// 过滤掉meta文件和文件夹
//			if(path.Contains(".meta") || path.Contains(".") == false || path.Contains(".DS_Store"))
//				continue;
//			
//			// 过滤掉UIAtlas目录下的贴图和材质(UI/Common目录下的所有资源都是UIAtlas)
//			if (path.Contains("UI/Common"))
//			{
//				if ((o is Texture) || (o is Material))
//					continue;
//			}
//
//			path = SavePath + ConvertToAssetBundleName(path);
//			path = path.Substring(0, path.LastIndexOf('.'));
//			path += ".unity3d";
//			BuildPipeline.BuildPlayer(strLevel, path, target, BuildOptions.BuildAdditionalStreamedScenes);
//			nRet++;
//		}
		EditorUtility.DisplayDialog("Tip", "Package scene : " + packagefile.ToString() + "\r\nunPackage scene : " + unpackagefile.ToString(), "OK");
		AssetDatabase.Refresh();
		return true;
	}

	static string ConvertToAssetBundleName(string ResName)
	{
		return ResName.Replace('/', '.');
	}
}
