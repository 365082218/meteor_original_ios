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

public class FileReference
{
    //类型-标识符-路径-依赖项
    public string strIden;
    public string strPath;
    public List<string> Dependence = new List<string>();
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
		if (!System.IO.File.Exists(fileName))
			return strCurver;
        strCurver = JsonMapper.ToObject<List<string>>(File.ReadAllText(fileName), false);
		return strCurver;
	}

    public static List<string> GetBuildScenes()
    {
        List<string> names = new List<string>();
        foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
        {
            if (e == null)
                continue;
            if (e.enabled)
                names.Add(e.path);
        }
        return names;
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
        Build(target, strVer);//打包所有资源
        return;
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
            WSLog.LogError("user canceled");
            return;
        }

        BuildPipeline.BuildPlayer(GetBuildScenes().ToArray(), targetPath, target, BuildOptions.None);
	}

    
    public static void ClearAllBundleName()
    {
        string[] allBundleNames = AssetDatabase.GetAllAssetBundleNames();
        for (int i = 0; i < allBundleNames.Length; i++)
            AssetDatabase.RemoveAssetBundleName(allBundleNames[i], true);
        //List<string> hasBundleNameAssets = new List<string>();
        //foreach (string n in allBundleNames)
        //{
        //    foreach (string p in AssetDatabase.GetAssetPathsFromAssetBundle(n))
        //    {
        //        hasBundleNameAssets.Add(p);
        //    }
        //}
        ////float idx = 0f;
        //foreach (string asset in hasBundleNameAssets)
        //{
        //    SetBundleName(asset, "", false);
        //    //EditorUtility.DisplayProgressBar("清除所有Bundle名称", "当前处理文件:" + Path.GetFileName(asset), idx++ / hasBundleNameAssets.Count);
        //}
        //EditorUtility.ClearProgressBar();
        AssetDatabase.RemoveUnusedAssetBundleNames();
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }

    public static string SetBundleName(string path, string name, bool isForce = false)
    {
        AssetImporter m_importer = AssetImporter.GetAtPath(path);
        if (m_importer != null)
            m_importer.assetBundleName = name;
        else
        {
            WSLog.LogError("path:" + path + " can not find importer");
            return "";
        }
        AssetDatabase.Refresh();
        return m_importer.assetBundleName;
    }

    public static string GetBundleName(string path, string name, bool isForce = false)
    {
        string retBundleName = null;

        if (!isForce)
        {
            if (Directory.Exists(path))
            {
                return retBundleName;
            }
        }

        string dictName = Path.GetDirectoryName(path);
        string fileName = Path.GetFileNameWithoutExtension(path);
        string extension = Path.GetExtension(path);

        if (!isForce)
        {
            if (extension.Equals(".dll") || extension.Equals(".cs") || extension.Equals(".js") || (name != "" && fileName.Contains("atlas") && !extension.Equals(".prefab")))
            {
                return null;
            }
        }

        if (name != "")
        {
            retBundleName = dictName + "/" + fileName + name;

            //Object tex = AssetDatabase.LoadAssetAtPath(path, typeof(Object));  

            //if (tex is Texture2D)  
            //{  
            //    SetTexture(tex as Texture2D);  
            //}  
        }

        Debug.Log("Asset name: " + fileName);

        AssetDatabase.Refresh();

        return retBundleName;
    }

    static void Build(BuildTarget target, string newVersion)
    {
        List<string> UpdateScene = VerMng.GetBuildScenes();
        
        string[] strDirectory = Directory.GetDirectories("Assets/", "Resources", SearchOption.AllDirectories);
        List<string> strResDirectory = new List<string>();
        for (int i = 0; i < strDirectory.Length; i++)
        {
            strDirectory[i] = strDirectory[i].Replace("\\", "/");
            strResDirectory.Add(strDirectory[i]);
        }

        List<string> strResItem = new List<string>();
        //get resources 
        //get resources used resources
        //get export's unity used resources
        //gen it's reference table
        foreach (var res in strResDirectory)
        {
            string[] strFiles = Directory.GetFiles(res, "*.*", SearchOption.AllDirectories);
            foreach (var eachFile in strFiles)
            {
                string strMenu = eachFile.Replace("\\", "/");
                if (strMenu.ToLower().EndsWith(".cs") || strMenu.ToLower().EndsWith(".js") || strMenu.ToLower().EndsWith(".meta") ||
                    strMenu.ToLower().EndsWith(".h") || strMenu.ToLower().EndsWith(".dll") || strMenu.ToLower().EndsWith(".mm") ||
                    strMenu.ToLower().EndsWith(".cpp") || strMenu.EndsWith(".DS_Store") || strMenu.EndsWith(".svn"))
                    continue;
                if (!strResItem.Contains(strMenu))
                    strResItem.Add(strMenu);
                else
                    WSLog.LogError("already exist:" + strMenu);
            }
        }

        string[] sceneDepend = AssetDatabase.GetDependencies(UpdateScene.ToArray());
        for (int i = 0; i < sceneDepend.Length; i++)
        {
            sceneDepend[i] = sceneDepend[i].Replace("\\", "/");
            if (sceneDepend[i].EndsWith(".cs") || sceneDepend[i].EndsWith(".js"))
                continue;
            if (strResItem.Contains(sceneDepend[i]))
                continue;
            strResItem.Add(sceneDepend[i]);
        }

        //清除系统配置的各个AssetBundle设置
        ClearAllBundleName();

        //准备分析全部依赖表.
        ReferenceNode.Reset();
        //分析依赖，设置AssetBundle名.
        for (int i = 0; i < strResItem.Count; i++)
            ReferenceNode.Alloc(strResItem[i]);
        for (int i = 0; i < strResItem.Count; i++)
            MakeDependTabel.OnStep(ReferenceNode.Alloc(strResItem[i]));
        //Assert.IsTrue(ReferenceNode.referenceDict.Count == strResItem.Count);
        //Dictionary<string, List<string>> jsonData = new Dictionary<string, List<string>>();
        List<FileReference> fDepend = new List<FileReference>();
        int unDepend = 0;
        foreach (var each in ReferenceNode.referenceDict)
        {
            FileReference f = new FileReference();
            f.strPath = each.Key;
            //jsonData.Add(each.Key, new List<string>());
            for (int i = 0; i < each.Value.child.Count; i++)
            {
                f.Dependence.Add(each.Value.child[i].strResources);
                //jsonData[each.Key].Add(each.Value.child[i].strResources);
            }
            fDepend.Add(f);
            if (f.Dependence.Count == 0)
                unDepend++;
        }

        File.WriteAllText(VerMng.SavePath + "_FILE.txt", LitJson.JsonMapper.ToJson(fDepend));
        WSLog.LogError(string.Format("共有{0}个文件 {1}个不依赖任何资源", fDepend.Count, unDepend));

        return;

        //string SavePath = "";
        //SavePath = "";// PlatformMap.GetPlatformPath(target) + "/" + strNextVersion + "/";
        //ReferenceNode.PackagePrefab(mostInnerLevel, freeLevel, target, SavePath);

        ////加密脚本.
        //List<string> strOutputScript = new List<string>();
        //List<string> scriptCompressOut = new List<string>();
        ////if (!EncryptXOR.EncryptResGroup(strScript[".csl"], ref strOutputScript))
        //{
        //    //压缩加密后的脚本.
        //    //CompressForFile.Compress(SavePath, strOutputScript, true, ref scriptCompressOut);
        //}

        //List<string> strTableEncrypted = new List<string>();
        //List<string> strTableCompressOut = new List<string>();
        ////加密表格
        ////if (!EncryptXOR.EncryptResGroup(strTable[".txt"], ref strTableEncrypted))
        //{
        //    //压缩加密后的表格.
        //    //CompressForFile.Compress(SavePath, strTableEncrypted, true, ref strTableCompressOut);
        //}

        ////压缩动作和关卡配置bytes文件，非文本，直接压缩.
        //List<string> strBytesCompressOut = new List<string>();
        ////CompressForFile.Compress(SavePath, strBytes[".bytes"], true, ref strBytesCompressOut);

        ////自动生成文件MD5列表.
        //CreateMD5List.GenFileListXml(SavePath);
        ////对比每个版本自动生成更新列表.并更新v.xml
        ////GenAllUpdateVersionXml.GenUpdateXmlByVersion(target, strNextVersion, strOldVersion);

        ////把新版本号写到version.xml里去.
        ////VersionManager.UpdateVersionXml(target, strOldVersion, strNextVersion);

        ////自动生成第1个包.去除A,B使用的资源.
        //strNextVersion = "0.0.0.0";
        ////把第二个包相关的资源都生成出来
        ////表格，脚本，关卡配置动作配置文件都拷贝过来.
        //string oldSavePath = SavePath;
        //SavePath = "";// PlatformMap.GetPlatformPath(target) + "/" + strNextVersion + "/";
        //ReferenceNode.Reset();

        //foreach (var str in scriptCompressOut)
        //{
        //    if (str.StartsWith(oldSavePath))
        //    {
        //        string strTargetDirectory = SavePath + str.Replace(oldSavePath, "");
        //        int nIndex = strTargetDirectory.LastIndexOf('/');
        //        if (nIndex != -1)
        //            Directory.CreateDirectory(strTargetDirectory.Substring(0, nIndex));
        //        File.Copy(str, strTargetDirectory);
        //    }
        //}

        //foreach (var str in strTableCompressOut)
        //{
        //    if (str.StartsWith(oldSavePath))
        //    {
        //        string strTargetDirectory = SavePath + str.Replace(oldSavePath, "");
        //        int nIndex = strTargetDirectory.LastIndexOf('/');
        //        if (nIndex != -1)
        //            Directory.CreateDirectory(strTargetDirectory.Substring(0, nIndex));
        //        File.Copy(str, strTargetDirectory);
        //    }
        //}

        //foreach (var str in strBytesCompressOut)
        //{
        //    if (str.StartsWith(oldSavePath))
        //    {
        //        string strTargetDirectory = SavePath + str.Replace(oldSavePath, "");
        //        int nIndex = strTargetDirectory.LastIndexOf('/');
        //        if (nIndex != -1)
        //            Directory.CreateDirectory(strTargetDirectory.Substring(0, nIndex));
        //        File.Copy(str, strTargetDirectory);
        //    }
        //}

        ////资源包.减去AB依赖的任何资源
        //List<string> deleted = new List<string>();//被AB依赖的任意资源都不能在首包出现.
        //foreach (var each in UpdateScene)
        //{
        //    if (each.Value && strResItem.Contains(each.Key))
        //    {
        //        strResItem.Remove(each.Key);
        //        string[] depend = AssetDatabase.GetDependencies(new string[] { each.Key });
        //        foreach (var str in depend)
        //        {
        //            string eachdel = str.Replace("\\", "/");
        //            if (eachdel.ToLower().EndsWith(".cs") || eachdel.ToLower().EndsWith(".js") || eachdel.ToLower().EndsWith(".meta") ||
        //            eachdel.ToLower().EndsWith(".h") || eachdel.ToLower().EndsWith(".dll") || eachdel.ToLower().EndsWith(".mm") ||
        //            eachdel.ToLower().EndsWith(".cpp") || eachdel.EndsWith(".DS_Store") || eachdel.EndsWith(".svn"))
        //                continue;
        //            if (!deleted.Contains(eachdel))
        //                deleted.Add(eachdel);
        //        }
        //    }
        //}

        ////把Main Startup用到而AB不用到的复制到0.0.0.0目录.不打包，没有依赖表，首包仅仅给其他版本的文件做对比.
        //List<string> allReference = new List<string>();
        //foreach (var str in strResItem)
        //{
        //    string[] depend = AssetDatabase.GetDependencies(new string[] { str });
        //    foreach (var eachstr in depend)
        //    {
        //        string refer = eachstr.Replace("\\", "/");
        //        if (refer.ToLower().EndsWith(".cs") || refer.ToLower().EndsWith(".js") || refer.ToLower().EndsWith(".meta") ||
        //            refer.ToLower().EndsWith(".h") || refer.ToLower().EndsWith(".dll") || refer.ToLower().EndsWith(".mm") ||
        //            refer.ToLower().EndsWith(".cpp") || refer.EndsWith(".DS_Store") || refer.EndsWith(".svn"))
        //            continue;
        //        if (!deleted.Contains(refer))
        //        {
        //            if (!allReference.Contains(refer))
        //            {
        //                allReference.Add(refer);
        //            }
        //        }
        //    }
        //}

        ////把所有allreference里的文件对应的.assetbundle从0.0.0.1版本复制到对应的0.0.0.0版本的对应目录.
        //foreach (var each in allReference)
        //{
        //    if (File.Exists(oldSavePath + each + ".assetbundle"))
        //    {
        //        string strDir = SavePath + each + ".assetbundle";
        //        int i = strDir.LastIndexOf('/');
        //        if (i != -1)
        //            strDir = strDir.Substring(0, i);
        //        Directory.CreateDirectory(strDir);
        //        File.Copy(oldSavePath + each + ".assetbundle", SavePath + each + ".assetbundle");
        //    }
        //    else
        //        Debug.LogError("not contains res error:" + each);
        //}

        ////记录信息.
        ////自动生成文件MD5列表.
        //CreateMD5List.GenFileListXml(SavePath);
        //对比每个版本自动生成更新列表.并更新v.xml
        //GenAllUpdateVersionXml.GenUpdateXmlByVersion(target, strNextVersion, strOldVersion);

        //把新版本号写到version.xml里去.
        //VersionManager.UpdateVersionXml(target, strOldVersion, strNextVersion);
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
        List<string>files = VerMng.GetBuildScenes();
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
