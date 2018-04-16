using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System;

public class CreateAssetBundle
{
    public static List<string> strBuilded = new List<string>();
    public static bool PackageAllPrefab(UnityEditor.BuildTarget target, List<string> ignoreFilelist, UnityEngine.Object SelectObject = null)
	{
		string SavePath = "";
		//try
		//{
		//	SavePath = PlatformMap.GetPlatformPath(target) + "/" + VersionManager.GetCurVersion(target)+ "/";
		//}
		//catch(IOException exp)
		//{
		//	EditorUtility.DisplayDialog("Error", exp.Message, "OK");
		//	return false;
		//}

		string SelectPath = "Assets/";
        string []files = null;
        if (SelectObject != null)
        {
            //打包一个预设文件.
            SelectPath = AssetDatabase.GetAssetPath(SelectObject);
            PackageOnePrefab(target, SelectObject);
            AssetDatabase.Refresh();
            return true;
        }
        else
        {
            try
            {
                files = Directory.GetFiles(SelectPath, "*.prefab", SearchOption.AllDirectories);
            }
            catch (Exception exp)
            {
                UnityEngine.Debug.LogError(exp.Message);
            }
            strBuilded.Clear();
        }
        
		int packagefile = 0;
		int unpackagefile = 0;
		foreach (string eachfile in files)
		{
            string file = eachfile.Replace('\\', '/');
			string path = file;
			if (ignoreFilelist != null)
			{
				bool bIgnore = false;
				string name = "";
				int nNameBegin = path.LastIndexOf('/');
				int nNameEnd = path.LastIndexOf('.');
				name = path.Substring(nNameBegin + 1, nNameEnd - nNameBegin - 1);
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
			path = SavePath + path;
			path = path.Substring(0, path.LastIndexOf('.'));
			path += ".assetbundle";
            if (strBuilded.Contains(file))
                continue;
            UnityEngine.Object o = AssetDatabase.LoadMainAssetAtPath(file);
            BuildPipeline.BuildAssetBundle(o, null, path, BuildAssetBundleOptions.CompleteAssets | BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.CollectDependencies, target);
            strBuilded.Add(file);
            UnityEngine.Object[] depend = EditorUtility.CollectDependencies(new UnityEngine.Object[] { o });
            foreach (UnityEngine.Object inner in depend)
            {
                string str = AssetDatabase.GetAssetPath(inner);
                if (str.EndsWith(".cs") || str == "")
                    continue;
                if (str == file)
                    continue;
                if (inner != null)
                    PackageOnePrefab(target, inner);
            }
		}
        EditorUtility.DisplayDialog("Tip", "Package file : " + packagefile.ToString() + "\r\nunPackage file : " + unpackagefile.ToString(), "OK");
        AssetDatabase.Refresh();
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
        string SavePath = "";
		//SavePath = PlatformMap.GetPlatformPath(target) + "/" + VersionManager.GetCurVersion(target)+ "/";
        path = SavePath + path;
        path = path.Substring(0, path.LastIndexOf('.'));
        path += ".assetbundle";
		//Directory.CreateDirectory(SavePath);
        //if (strBuilded.Contains(file))
        //    return;
		//BuildPipeline.BuildAssetBundle(SelectObject, null, SavePath, BuildAssetBundleOptions.CompleteAssets | BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.CollectDependencies, target);
    }
}