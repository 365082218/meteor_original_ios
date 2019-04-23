using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.IO;
using LitJson;
using System.Security.Cryptography;
using Idevgame.Util;

public class VerMng : EditorWindow{
    private const string PathBase = "Assets/.VerMng/";
    public static string GetPlatformPath(UnityEditor.BuildTarget target)
    {
        string SavePath = "";
        if (target == BuildTarget.Android)
            SavePath = PathBase + RuntimePlatform.Android.ToString();
        else if (target == BuildTarget.iOS)
            SavePath = PathBase + RuntimePlatform.IPhonePlayer.ToString();
        else if (target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64)
            SavePath = PathBase + RuntimePlatform.WindowsPlayer.ToString();
        else
        {
            throw new ArgumentException(string.Format("Invalid Build Target:{0}", target));
        }

        if (Directory.Exists(SavePath) == false)
        {
            Directory.CreateDirectory(SavePath);
            AssetDatabase.Refresh();
        }
        return SavePath;
    }
    public static string SavePath
    {
        get { return GetPlatformPath(Target) + "/" + Version + "/"; }
    }

    //对指定平台做最初版本的文件列表（空) 版本号0.0.0.0-首版本号
    public static void Initialize(BuildTarget target)
    {
        if (!Directory.Exists(GetPlatformPath(target) + "/" + "0.0.0.0"))
        {
            Directory.CreateDirectory(GetPlatformPath(target) + "/" + "0.0.0.0");
            //写文件列表
            List<PackageItem> package = new List<PackageItem>();
            string manifest = "0.0.0.0.json";
            File.WriteAllText(GetPlatformPath(target) + "/" + manifest, JsonMapper.ToJson(package));

            //写版本号
            VersionItem curItem = new VersionItem();
            curItem.strFilelist = manifest;
            curItem.strVersion = "0.0.0.0";
            curItem.strVersionMax = "0.0.0.0";
            curItem.zip = null;
            List<VersionItem> VersionList = new List<VersionItem> { curItem};
            File.WriteAllText(VerMng.GetPlatformPath(target) + "/" + Main.strVerFile, JsonMapper.ToJson(VersionList));
        }
    }

    internal static string Version = "0.5.2.0";
    internal static BuildTarget Target;
    //获取某个平台的全部版本.
    public static List<VersionItem> GetAllVersion(UnityEditor.BuildTarget target)
	{
        List<VersionItem> strRet = new List<VersionItem>();
		string strVersionPath = GetPlatformPath(target) + "/" + Main.strVerFile;
        if (!File.Exists(strVersionPath))
            return strRet;
        strRet = Main.ReadVersionJson(File.ReadAllText(strVersionPath));
		return strRet;
	}

	public static List<VersionItem> GetHistoryVersion(UnityEditor.BuildTarget target)
	{
		List<VersionItem> strRet = new List<VersionItem>();
		string strVersionPath = GetPlatformPath(target) + "/" + Main.strVerFile;
		strRet =  ReadVersion(strVersionPath);
		return strRet;
	}

	static List<VersionItem> ReadVersion(string fileName)
	{
        List<VersionItem> strCurver = new List<VersionItem>();
		if (!System.IO.File.Exists(fileName))
			return strCurver;
        strCurver = Main.ReadVersionJson(File.ReadAllText(fileName));
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
        if (Directory.Exists(GetPlatformPath(target) + "/" + strVer))
        {
            EditorUtility.DisplayDialog("错误", string.Format("已经存在版本号:{0}文件夹，请先清理", strVer), "确定");
            return;
        }

        Directory.CreateDirectory(GetPlatformPath(target) + "/" + strVer);
        //开始打包.=>打包预设=>打包在BuildSetting内的场景=>打包Resources目录下的其他资源=>加密资源=>压缩资源=>放置在版本路径，记录其信息（文件大小，是否加密，是否压缩，文件hash值）=>生成版本资源清单=>编辑版本更新信息
        //=>依次各个版本清单文件对比，生成各个版本的一次性更新差异文件，把该平台下的资源打包后的存储路径放到服务器，就完成了更新配置.
        //生成资源清单文件.
        Build(target, strVer);//打包所有资源
	}

    
    public static void ClearAllBundleName()
    {
        string[] allBundleNames = AssetDatabase.GetAllAssetBundleNames();
        for (int i = 0; i < allBundleNames.Length; i++)
            AssetDatabase.RemoveAssetBundleName(allBundleNames[i], true);
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
            Debug.LogError("path:" + path + " can not find importer");
            return "";
        }
        return m_importer.assetBundleName;
    }

    static void Build(BuildTarget target, string newVersion)
    {
        //检查此版本是否已经存在.
        List<VersionItem> VersionList = GetAllVersion(target);
        for (int i = 0; i < VersionList.Count; i++)
        {
            if (VersionList[i].strVersion == newVersion)
            {
                EditorUtility.DisplayDialog("错误", string.Format("版本库已存在{0}此版本号，请先修改", newVersion), "确定");
                return;
            }
        }

        string path = GetPlatformPath(target) + "/" + newVersion;
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
                    Log.WriteError("already exist:" + strMenu);
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
        //准备对每个资源设置bundle名称
        Dictionary<string, string> bundle = new Dictionary<string, string>();//path=>id+ext
        //Dictionary<string, string> bundleExt = new Dictionary<string, string>();//path=>ext
        foreach (var each in ReferenceNode.referenceDict)
        {
            int resFirstIndex = each.Key.IndexOf("/Resources/");
            int resLastIndex = each.Key.LastIndexOf("/Resources/");
            if (resLastIndex != resFirstIndex)
                Log.WriteError(string.Format("嵌套的Resources路径是不允许的,{0}", each.Key));
            else
            {
                if (resFirstIndex != -1)
                {
                    //在Resources目录下的路径+文件名不能相同.
                    string res = each.Key.Substring(resFirstIndex + 11);
                    //文件存在后缀
                    if (bundle.ContainsValue(res))
                        Log.WriteError(string.Format("file:{0} allready exist at {1}", res, each.Key));
                    else
                        bundle.Add(each.Key, res);
                }
                else
                {
                    //不在Resources目录下文件名不能相同.
                    int index = each.Key.LastIndexOf('/');
                    if (index != -1)
                    {
                        string file = each.Key.Substring(index + 1);
                        //文件存在后缀.
                        if (bundle.ContainsValue(file))
                            Log.WriteError(string.Format("file:{0} allready exist at {1}", file, each.Key));
                        else
                            bundle.Add(each.Key, file);
                    }
                }
            }
        }
        Log.Write(string.Format("file {0}", bundle.Count));
        //当有重复产生时，不允许打包，文件重复意味着某个文件名+后缀在不同路径出现多次
        if (bundle.Count != ReferenceNode.referenceDict.Count)
        {
            Debug.LogError("bundle length not equal");
            return;
        }

        //开始设置bundle名称
        List<ReferenceNode> referenceTable = new List<ReferenceNode>();
        foreach (var each in bundle)
        {
            SetBundleName(each.Key, each.Value, true);
            //修改依赖项为标识符。
            ReferenceNode.referenceDict[each.Key].strResources = each.Value;
            if (!referenceTable.Contains(ReferenceNode.referenceDict[each.Key]))
                referenceTable.Add(ReferenceNode.referenceDict[each.Key]);
            else
                Log.WriteError(string.Format("node all ready exist:{1}", each.Key));
        }

        FileStream fs = File.Open(SavePath + ResMng.RefTable, FileMode.OpenOrCreate);
        ProtoBuf.Serializer.Serialize<List<ReferenceNode>>(fs, referenceTable);
        fs.Close();
        //给bundle添加RefTable.dat
        bundle.Add(SavePath + ResMng.RefTable, ResMng.RefTable);

        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, target);
        AssetDatabase.Refresh();

        //清除所有manifest,自己保存依赖
        string[] deleted = Directory.GetFiles(SavePath, "*.manifest", SearchOption.AllDirectories);
        for (int i = 0; i < deleted.Length; i++)
            File.Delete(deleted[i]);

        AssetDatabase.Refresh();
        //遍历生成的所有bundle，生成资源清单列表，以便与其他版本清单对比，生成更新压缩包。
        List<PackageItem> package = new List<PackageItem>();
        foreach (var each in bundle)
        {
            PackageItem pkg = new PackageItem();
            pkg.Iden = each.Value;
            pkg.Path = each.Key;
            pkg.Md5 = GetFileMd5(SavePath + "/" + pkg.Iden);
            package.Add(pkg);
        }

        string manifest = newVersion + ".json";
        File.WriteAllText(GetPlatformPath(Target) + "/" + manifest, JsonMapper.ToJson(package));
        VersionItem curItem = new VersionItem();
        curItem.strFilelist = manifest;
        curItem.strVersion = newVersion;
        curItem.strVersionMax = newVersion;
        curItem.zip = null;//最新的包，是没有更新可用的.
        //与之前的各个版本列表对比.
        UpdateVersionJson(target, curItem, VersionList);
        VersionList.Add(curItem);
        File.WriteAllText(VerMng.GetPlatformPath(target) + "/" + Main.strVerFile, JsonMapper.ToJson(VersionList));
        LZMAHelper.CompressFileLZMA(VerMng.GetPlatformPath(target) + "/" + Main.strVerFile, VerMng.GetPlatformPath(target) + "/" + Main.strVerFile + ".zip");
    }

	void OnGUI()
	{
	}

    /// <summary>
    /// 对比最新版本与所有之前版本的文件列表，生成更新包，更新版本信息
    /// </summary>
    /// <param name="target"></param>
    /// <param name="strNewVersion"></param>
    /// <param name="strOldVersion"></param>
    public static void UpdateVersionJson(BuildTarget target, VersionItem NewVersion, List<VersionItem> OldVersion)
    {
        for (int i = 0; i < OldVersion.Count; i++)
        {
            string str = MakeDiffZip(OldVersion[i], NewVersion);
            OldVersion[i].strVersionMax = NewVersion.strVersion;
            UpdateZip zip = new UpdateZip();
            zip.fileName = str;
            zip.Md5 = GetFileMd5(GetPlatformPath(target) + "/" + str);
            zip.size = new FileInfo(GetPlatformPath(target) + "/" + str).Length;
            if (OldVersion[i].zip != null)
            {
                if (File.Exists(GetPlatformPath(target) + "/" + OldVersion[i].zip.fileName))
                {
                    try
                    {
                        File.Delete(OldVersion[i].zip.fileName);
                    }
                    catch
                    {
                        Log.WriteError(string.Format("file:{0} cannot deleted", OldVersion[i].zip.fileName));
                    }
                }
            }
            OldVersion[i].zip = zip;
        }
    }

    //对比2个包的文件列表，进行差异文件分析，把修改的，更新的，全部放到压缩包内，最后返回压缩包的文件子路径.
    static string MakeDiffZip(VersionItem old, VersionItem update)
    {
        List<PackageItem> files_update = JsonMapper.ToObject<List<PackageItem>>(File.ReadAllText(GetPlatformPath(Target) + "/" + update.strFilelist), false);
        List<PackageItem> files_old = JsonMapper.ToObject<List<PackageItem>>(File.ReadAllText(GetPlatformPath(Target) + "/" + old.strFilelist), false);
        List<PackageItem> update_files = new List<PackageItem>();
        for (int i = 0; i < files_update.Count; i++)
        {
            if (Updated(files_update[i], files_old))
                update_files.Add(files_update[i]);
        }
        //把更新的文件复制到一个指定路径，把该路径作为
        DirectoryInfo baseDir = null;
        if (update_files.Count != 0)
        {
            string dir = System.IO.Path.GetTempPath() + string.Format("{0}_{1}", old.strVersion, update.strVersion);
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
            //取得一个临时目录，把全部更新文件按结构放到这个临时目录
            baseDir = Directory.CreateDirectory(dir);
        }

        for (int i = 0; i < update_files.Count; i++)
        {
            string file = update_files[i].Iden;
            string dir = baseDir.FullName + "/" + file;
            int dirIndex = file.LastIndexOf('/');
            if (dirIndex != -1)
            {
                dir = baseDir.FullName + "/" + file.Substring(0, dirIndex);
                Directory.CreateDirectory(dir);
            }
            File.Copy(GetPlatformPath(Target) + "/" + update.strVersion + "/" + update_files[i].Iden, baseDir.FullName + "/" + update_files[i].Iden);
        }

        //打包UPK,LZMA压缩比例比ZIP高很多，所以这里选择用7Z
        string upk = System.IO.Path.GetTempPath() + string.Format("{0}_{1}.upk", old.strVersion, update.strVersion);
        string zip = string.Format("{0}_{1}.zip", old.strVersion, update.strVersion);
        UPKPacker.PackFolder(baseDir.FullName, upk);
        LZMAHelper.CompressFileLZMA(upk, GetPlatformPath(Target) + "/" + zip);
        return zip;
    }

    //返回高版本某个资源是否需要更新
    static bool Updated(PackageItem updateItem, List<PackageItem> pkg)
    {
        bool needUpdate = true;
        for (int i = 0; i < pkg.Count; i++)
        {
            if (pkg[i].Path == updateItem.Path && pkg[i].Iden == updateItem.Iden)
            {
                if (pkg[i].Md5 == updateItem.Md5)
                    needUpdate = false;
                break;
            }
        }

        return needUpdate;
    }

    public static string GetFileMd5(string path)
    {
        MD5CryptoServiceProvider md5Generator = new MD5CryptoServiceProvider();
        FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        byte[] hash = md5Generator.ComputeHash(file);
        string strMD5 = System.BitConverter.ToString(hash);
        return strMD5;
    }

    public static void TestDepend(BuildTarget target, string NewVersion)
    {
        Target = target;
        Version = NewVersion;
        FileStream fs = File.Open(SavePath + ResMng.RefTable, FileMode.Open);
        List<ReferenceNode> refTable = ProtoBuf.Serializer.Deserialize<List<ReferenceNode>>(fs);
        for (int i = 0; i < refTable.Count; i++)
            if (refTable[i].strResources.IndexOf(".dll") != -1)
            {
                Debug.DebugBreak();
            }
        fs.Close();
    }
}
