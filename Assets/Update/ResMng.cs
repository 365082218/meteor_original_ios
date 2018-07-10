using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Security.Cryptography;
using UnityEngine.SceneManagement;
using System;

public class UpdateZip
{
    public string fileName;//v0-v1.zip
    public string Md5;//hash
    public long size;
}

public class VersionItem
{
    public string strVersion;//版本号.
    public string strVersionMax;//最新版本号.
    public string strFilelist;//文件清单名.GetPlatForm(Target)目录下
    public UpdateZip zip;//该版本与最高版本的更新包.
}

public class PackageItem
{
    public string Iden;//唯一标识+拓展名
    public string Md5;//hash.
    public string Path;//本地路径名.
}

public class ResMng {
    public const string ExtScene = ".unity";
    public const string ExtPrefab = ".prefab";
    public const string ExtScript = ".txt";
    public class LoadCallBackParam
	{
		public bool bLoadDone;
	}

	public delegate void LoadCallback(object param);

    public static void Reload()
    {
        Clean();
        Load();
    }

    static void Clean()
    {
        foreach (var bundle in Bundle)
        {
            if (bundle.Value != null)
                bundle.Value.Unload(false);
        }
        Bundle.Clear();
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }

    public const string RefTable = "RefTable.dat";
    static void ReadReferenceTable()
	{
        if (!File.Exists(GetResPath() + "/" + RefTable))
        {
            Debug.LogError(string.Format("Can not Load RefTable", GetResPath() + "/" + RefTable));
            return;
        }
		ReferenceNode.Reset();
        try
        {
            //A文件异常（被锁定、其他），B序列化异常(内存不足?).
            FileStream fs = File.Open(GetResPath() + "/" + RefTable, FileMode.Open);
            List<ReferenceNode> refTable = ProtoBuf.Serializer.Deserialize<List<ReferenceNode>>(fs);
            for (int i = 0; i < refTable.Count; i++)
            {
                RegisterRes(refTable[i]);
            }
        }
        catch
        {

        }
    }

    public static Dictionary<string, List<string>> Res = new Dictionary<string, List<string>>();
    public static Dictionary<string, AssetBundle> Bundle = new Dictionary<string, AssetBundle>();

	static void RegisterRes(ReferenceNode node)
	{
        //单一文件名对应各种类型全名，比如
        string fullPath = node.strResources;
        string name = "";

        string subName = node.strResources;
        int extIndex = subName.LastIndexOf('.');
        name = subName;
        if (extIndex != -1)
            name = subName.Substring(0, extIndex);

        if (Res.ContainsKey(name))
        {
            if (!Res[name].Contains(fullPath))
                Res[name].Add(fullPath);
        }
        else
            Res.Add(name, new List<string> { node.strResources });
        if (!ReferenceNode.referenceDict.ContainsKey(node.strResources))
            ReferenceNode.referenceDict.Add(node.strResources, node);
        else
            Debug.LogError(string.Format("already contains:{0}", node.strResources));
    }

	static void Load()
	{
        WSLog.LogError("ResInit");
		Res.Clear();
        Bundle.Clear();
        ReferenceNode.referenceDict.Clear();
        ReadReferenceTable();
	}


    //得到资源拓展名
    static string GetExt(string name)
    {
        int idx = name.LastIndexOf('.');
        if (idx != -1)
            return name.Substring(idx);
        return "";
    }

    public static ReferenceNode GetResNodeByIden(string iden, string ext = "")
    {
        if (Res.ContainsKey(iden) && Res[iden].Count >= 1)
        {
            if (string.IsNullOrEmpty(ext))
            {
                if (ReferenceNode.referenceDict.ContainsKey(Res[iden][0]))
                    return ReferenceNode.referenceDict[Res[iden][0]];
            }
            else
            {
                for (int i = 0; i < Res[iden].Count; i++)
                {
                    if (GetExt(Res[iden][i]) == ext)
                    {
                        if (ReferenceNode.referenceDict.ContainsKey(Res[iden][i]))
                            return ReferenceNode.referenceDict[Res[iden][i]];
                    }
                }
            }
        }
        return null;
    }

    //通过标识符和后缀名得到唯一名称.
    public static string GetResourceByIden(string iden, string ext)
	{
		return iden + ext;
	}

	static List<ReferenceNode> CollectDependencies(ReferenceNode root)
	{
		List<ReferenceNode> ret = new List<ReferenceNode>();
		bool bUseList = true;
		List<ReferenceNode> childList = new List<ReferenceNode>();
		List<ReferenceNode> childList2 = new List<ReferenceNode>();
		childList.Add(root);
		while (true)
		{
			List<ReferenceNode> child = null;
			List<ReferenceNode> child2 = null;

			if (bUseList)
			{
				child = childList;
				child2 = childList2;
			}
			else
			{
				child = childList2;
				child2 = childList;
			}

            if (child != null)
            {
                foreach (ReferenceNode node in child)
                {
                    if (node.child != null)
                    {
                        foreach (ReferenceNode sonnode in node.child)
                        {
                            child2.Add(sonnode);
                        }
                    }
                }
            }

            if (child2 != null)
            {
                foreach (ReferenceNode node in child2)
                {
                    if (!ret.Contains(node))
                        ret.Add(node);
                }
            }

			child.Clear();
			bUseList = !bUseList;

			if (child.Count == 0 && child2.Count == 0)
				break;
		}
		return ret;
	}

	public static UnityEngine.Object LoadPrefab(string strResource)
	{
        ReferenceNode no = GetResNodeByIden(strResource, ExtPrefab);
        AssetBundle ab = LoadResNode(no);
        if (ab != null)
        {
            string shortName = GetShortName(strResource);
            return ab.LoadAsset(shortName);
        }
        else
            return Resources.Load(strResource, typeof(GameObject));
	}

    //单个节点
    static AssetBundle LoadResNodeInternal(ReferenceNode node)
    {
        if (node == null)
            return null;
        if (Bundle.ContainsKey(node.strResources))
            return Bundle[node.strResources];
        if (File.Exists(ResMng.GetResPath() + "/" + node.strResources))
        {
            FileStream fs = new FileStream(ResMng.GetResPath() + "/" + node.strResources, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);
            fs.Close();
            AssetBundle ab = AssetBundle.LoadFromMemory(buffer);
            Bundle.Add(node.strResources, ab);
            return ab;
        }
        return null;
    }

    static AssetBundle LoadResNode(ReferenceNode node)
    {
        if (node == null)
            return null;
        List<ReferenceNode> depend = CollectDependencies(node);
        for (int i = depend.Count - 1; i >= 0; i--)
            LoadResNodeInternal(depend[i]);
        AssetBundle ab = LoadResNodeInternal(node);
        return ab;
    }

    static string GetShortName(string identity)
    {
        string shortName = identity;
        int folderIndex = identity.LastIndexOf('/');
        if (folderIndex != -1)
            shortName = identity.Substring(folderIndex + 1);
        return shortName;
    }

    //找第一个名字匹配的.不论类型
	public static UnityEngine.Object Load(string str)
	{
        ReferenceNode node = GetResNodeByIden(str);
        if (node == null)
            return Resources.Load(str);
        AssetBundle ab = LoadResNode(node);
        if (ab != null)
        {
            string shortName = GetShortName(str);
            UnityEngine.Object ResObject = ab.LoadAsset(shortName);
            return ResObject;
        }
        else
        {
            string shortName = GetShortName(str);
            return Resources.Load(shortName);
        }
	}

    //找一个文本类型的
    public static TextAsset LoadTextAsset(string str)
    {
        ReferenceNode node = GetResNodeByIden(str, ExtScript);
        if (node == null)
            return Resources.Load<TextAsset>(str);
        AssetBundle ab = LoadResNode(node);
        if (ab != null)
        {
            string shortName = GetShortName(str);
            TextAsset ResObject = (TextAsset)ab.LoadAsset(shortName);
            return ResObject;
        }
        else
        {
            string shortName = GetShortName(str);
            return Resources.Load<TextAsset>(shortName);
        }
    }

    //同步加载关卡
    static void LoadLevelSync(string strScene)
	{
        ReferenceNode no = GetResNodeByIden(strScene, ExtScene);
		AssetBundle ab = LoadResNode(no);
		try
		{
			SceneManager.LoadScene(strScene);
		}
		catch (System.Exception exp)
		{
			WSLog.LogError(exp.Message + "|" + exp.StackTrace);
			return;
		}
	}

    //加载场景资源节点-异步加载关卡第一步.
    public static void LoadScene(string strScene)
    {
        ReferenceNode no = GetResNodeByIden(strScene, ExtScene);
        AssetBundle ab = LoadResNode(no);
    }

    //加载关卡
	public static void LoadLevel(string strScene, LoadCallback cb, object param)
	{
		LoadLevelSync(strScene);
		if (cb != null)
		{
            ResMng.LoadCallBackParam pa = param as ResMng.LoadCallBackParam;
            if (pa != null)
				pa.bLoadDone = true;
            cb(param as object);
		}
	}

    public static string ResPath;
    public static string GetResPath()
    {
        if (!string.IsNullOrEmpty(ResPath))
            return ResPath;
        ResPath = Application.persistentDataPath + "/Resource";
        if (!Directory.Exists(ResPath))
            Directory.CreateDirectory(ResPath);
        return ResPath;
    }

    public static string TmpPath;
    public static string GetUpdateTmpPath()
    {
        if (!string.IsNullOrEmpty(TmpPath))
            return TmpPath;
        TmpPath = Application.persistentDataPath + "/Update";
        if (!Directory.Exists(TmpPath))
            Directory.CreateDirectory(TmpPath);
        return TmpPath;
    }
}
