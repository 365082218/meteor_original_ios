using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class ReferenceNode
{
    public static void Reset()
    {
        referenceDict.Clear();
    }

    public static ReferenceNode Alloc(string str)
    {
        if (referenceDict.ContainsKey(str))
            return referenceDict[str];
        ReferenceNode ret = new ReferenceNode(str);
        referenceDict.Add(str, ret);
        return ret;
    }

    public ReferenceNode(string str)
    {
        strResources = str;
    }

    public static Dictionary<string, ReferenceNode> referenceDict = new Dictionary<string, ReferenceNode>();
    public string strResources;

    public List<ReferenceNode> child = new List<ReferenceNode>();//依赖列表.
    public List<ReferenceNode> parent = new List<ReferenceNode>();//被人依赖列表.
    public ReferenceNode mainParent = null;
    //增加一个引用节点，我引用了其他资源.
    public void AddDependencyNode(ReferenceNode childnode)
    {
        if (childnode == null)
            UnityEngine.Debug.LogError("childnode == null");

        if (!referenceDict.ContainsKey(childnode.strResources))
            referenceDict.Add(childnode.strResources, childnode);

        //修改判断子树是否已经包含要推入的节点，避免没有层级关系的跨越引用
        foreach (ReferenceNode node in child)
        {
            if (node.strResources == childnode.strResources)
                return;
        }
        child.Add(childnode);
        childnode.AddDependencyByNode(this);
    }

    //增加一个被引用节点，其他资源引用了我.
    public void AddDependencyByNode(ReferenceNode parentnode)
    {
        if (parentnode == null)
            UnityEngine.Debug.LogError("parentnode == null");

        if (!referenceDict.ContainsKey(parentnode.strResources))
            referenceDict.Add(parentnode.strResources, parentnode);

        foreach (ReferenceNode node in parent)
        {
            if (node.strResources == parentnode.strResources)
                return;
        }
        parent.Add(parentnode);
    }

    public static void OnStep(ReferenceNode current)
    {
        if (current.child.Count != 0)
            return;
        string[] depend = AssetDatabase.GetDependencies(new string[] { current.strResources });
        if (depend == null)
            return;
        if (depend.Length == 0)
            return;
        UnityEngine.Object objCurrent = AssetDatabase.LoadMainAssetAtPath(current.strResources);
        if (objCurrent == null)
            return;
        if (objCurrent.name == "")
            return;
        objCurrent = null;
        foreach (string son in depend)
        {
            string strson = son;
            strson = strson.Replace("\\", "/");
            if (strson == "")
                continue;

            FileInfo fInfo = new FileInfo(strson);
            if (fInfo.Extension.ToLower() == ".cs"|| fInfo.Extension.ToLower() == ".js" || fInfo.Extension.ToLower() == ".dds")
                continue;

            //这些资源比较小，可以冗余.重复打包到每个预设里.
            if (fInfo.Extension.ToLower() == ".mat" || 
                fInfo.Extension.ToLower() == ".shader" || 
                fInfo.Extension.ToLower() == ".anim" || 
                fInfo.Extension.ToLower() == ".obj" || 
                fInfo.Extension.ToLower() == ".asset" ||
                fInfo.Extension.ToLower() == ".controller")
                continue;

            if (strson == current.strResources || strson == "")
                continue;

            //场景不依赖预设，依赖与各个fbx,png,ttf,tga,psd,exr,wav,tif,jpg,mp3,exr
            if (current.strResources.ToLower().EndsWith(".unity") && (fInfo.Extension.ToLower() == ".prefab"))
            {
                ReferenceNode NewNode = ReferenceNode.Alloc(strson);
                if (NewNode.child.Count == 0)
                    OnStep(NewNode);
                continue;
            }

            if (current.strResources.ToLower().EndsWith(".unity") && fInfo.Extension.ToLower() == ".fbx")
            {
                ReferenceNode NewNode = ReferenceNode.Alloc(strson);
                current.AddDependencyNode(NewNode);
                if (NewNode.child.Count == 0)
                    OnStep(NewNode);
                continue;
            }

            if (current.strResources.ToLower().EndsWith(".prefab") && (fInfo.Extension.ToLower() == ".prefab"))
            {
                //预设依赖预设.断开依赖关系.
                ReferenceNode NewNode = ReferenceNode.Alloc(strson);
                if (NewNode.child.Count == 0)
                    OnStep(NewNode);
                continue;
            }

            if (current.strResources.ToLower().EndsWith(".prefab") && fInfo.Extension.ToLower() == ".fbx")
            {
                ReferenceNode NewNode = ReferenceNode.Alloc(strson);
                current.AddDependencyNode(NewNode);
                if (NewNode.child.Count == 0)
                    OnStep(NewNode);
                continue;
            }

            //各个fbx,png,ttf,tga,psd,exr,wav,tif,jpg,mp3
            ReferenceNode sonNode = ReferenceNode.Alloc(strson);
            bool bBug = false;
            foreach (var node in sonNode.child)
            {
                if (node.strResources == current.strResources)
                {
                    //互相依赖.可能是u3d的bug
                    bBug = true;
                    break;
                }
            }
            //跳过相互依赖的.
            if (bBug)
                continue;
            current.AddDependencyNode(sonNode);
        }
    }

    //自己保证依赖.选用2,用push则一个资源一次.
    static BuildAssetBundleOptions op2 = BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.CompleteAssets;
    static BuildAssetBundleOptions op = BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.CompleteAssets | BuildAssetBundleOptions.CollectDependencies;


    //子树摘叶，每次摘最外层的叶子，然后push一次依赖关系.
    public static List<ReferenceNode> GetTopLayerNode(ref List<ReferenceNode> root)
    {
        if (root == null)
            return null;
        List<ReferenceNode> ret = new List<ReferenceNode>();
        ReferenceNode[] rootClone = new ReferenceNode[root.Count];
        for (int i = 0; i < root.Count; i++)
        {
            rootClone[i] = root[i];
        }

        for (int i = 0; i < rootClone.Length; i++)
        {
            if (GetTopNode(ref ret, rootClone[i]))
            {
                try
                {
                    if (root.Contains(rootClone[i]))
                        root.Remove(rootClone[i]);
                    rootClone[i] = null;
                }
                catch(System.Exception exp)
                {
                    UnityEngine.Debug.LogError(exp.Message + "|" + exp.StackTrace);
                }
            }

        }
        return ret;
    }

    //返回该节点是否是叶子节点，是叶子节点证明其被摘取
    public static bool GetTopNode(ref List<ReferenceNode> lst, ReferenceNode root)
    {
        if (root.child.Count == 0)
        {
            bool bExisted = false;
            foreach (var eachnode in lst)
            {
                if (eachnode.strResources == root.strResources)
                    bExisted = true;
            }
            if (!bExisted)
                lst.Add(root);

            if (root.parent.Count != 0)
            {
                foreach (var node in root.parent)
                    node.child.Remove(root);
            }
            return true;
        }
        else
        {
            ReferenceNode[] childlist = new ReferenceNode[root.child.Count];
            for (int i = 0; i < root.child.Count; i++)
            {
                childlist[i] = root.child[i];
            }

            for (int i = 0; i < childlist.Length; i++)
            {
                GetTopNode(ref lst, childlist[i]);
            }
        }
        return false;
    }

    public static string GetUnitString(long bytes, int level = 3)
    {
        string[] unit = new string[] { "GB","MB","KB","Byte" };
        long kb = bytes / 1024;
        bytes = bytes % 1024;
        if (kb <= 0)
            return bytes.ToString() + unit[level];
        return GetUnitString(kb, level - 1) + ":" + bytes.ToString() + unit[level];
    }

    static void GenResourceInfo(Dictionary<string, ReferenceNode> refDict, string strSavePath, string strFile)
    {
        string strExtPath = strSavePath + strFile;//后缀类型表.
        FileStream fsExt = File.Create(strExtPath);
        Dictionary<string, Dictionary<string, long>> ResInfoDict = new Dictionary<string, Dictionary<string, long>>();
        foreach (KeyValuePair<string, ReferenceNode> node in refDict)
        {
            FileInfo f = new FileInfo(node.Key);
            int nLast = node.Key.LastIndexOf('/');
            int nExt = node.Key.LastIndexOf('.');
            string strDirectory = "";
            if (nLast != -1)
                strDirectory = node.Key.Substring(0, nLast + 1);
            //string strName = node.Key.Substring(nLast == -1 ? 0:nLast + 1, (nLast == -1) ? nExt:(nExt - nLast) - 1);
            //后缀必须加上,否则会重复.
            if (!ResInfoDict.ContainsKey(f.Extension.ToLower()))
            {
                Dictionary<string, long> value = new Dictionary<string, long>();
                value.Add(f.FullName.Replace("\\", "/"), f.Length);
                ResInfoDict.Add(f.Extension.ToLower(), value);
            }
            else
            {
                Dictionary<string, long> value = null;
                ResInfoDict.TryGetValue(f.Extension.ToLower(), out value);
                if (value != null)
                {
                    if (!value.ContainsKey(f.FullName.Replace("\\", "/")))
                        value.Add(f.FullName.Replace("\\", "/"), f.Length);
                }
            }

            foreach (var sonnode in node.Value.child)
            {
                FileInfo res = new FileInfo(sonnode.strResources);
                nLast = sonnode.strResources.LastIndexOf('/');
                nExt = sonnode.strResources.LastIndexOf('.');
                strDirectory = "";
                if (nLast != -1)
                    strDirectory = sonnode.strResources.Substring(0, nLast + 1);
                //strName = sonnode.strResources.Substring(nLast == -1 ? 0 : nLast + 1, (nLast == -1) ? nExt : (nExt - nLast) - 1);
                //同一个目录如果相同文件名不同后缀的打包会互相覆盖,打包出来的assetbundle需要加后缀.
                if (ResInfoDict.ContainsKey(res.Extension.ToLower()))
                {
                    Dictionary<string, long> fileinfo = null;
                    ResInfoDict.TryGetValue(res.Extension.ToLower(), out fileinfo);
                    if (fileinfo != null)
                    {
                        if (!fileinfo.ContainsKey(res.FullName.Replace("\\", "/")))
                            fileinfo.Add(res.FullName.Replace("\\", "/"), res.Length);
                    }
                }
                else
                {
                    Dictionary<string, long> fileinfo = new Dictionary<string, long>();
                    fileinfo.Add(res.FullName.Replace("\\", "/"), res.Length);
                    ResInfoDict.Add(res.Extension.ToLower(), fileinfo);
                }
            }
        }

        long nAllTotalSize = 0;
        foreach (KeyValuePair<string, Dictionary<string, long>> each in ResInfoDict)
        {
            long nTotalSize = 0;
            foreach (KeyValuePair<string, long> eachIn in each.Value)
            {
                nTotalSize += eachIn.Value;
            }
            nAllTotalSize += nTotalSize;
            string strKBMB = GetUnitString(nTotalSize);
            byte[] buff = System.Text.Encoding.UTF8.GetBytes("类型:" + each.Key + "文件总大小:" + strKBMB);
            fsExt.Write(buff, 0, buff.Length);
            fsExt.Write(new byte[] { (byte)'\r', (byte)'\n' }, 0, 2);
            foreach (KeyValuePair<string, long> eachIn in each.Value)
            {
                strKBMB = GetUnitString(eachIn.Value);
                buff = System.Text.Encoding.UTF8.GetBytes("文件:" + eachIn.Key + "大小:" + strKBMB);
                fsExt.Write(buff, 0, buff.Length);
                fsExt.Write(new byte[] { (byte)'\r', (byte)'\n' }, 0, 2);
            }
        }
        fsExt.Write(new byte[] { (byte)'\r', (byte)'\n' }, 0, 2);
        string strTotal = GetUnitString(nAllTotalSize);
        byte[] bufftotal = System.Text.Encoding.UTF8.GetBytes("全部文件总大小:" + strTotal);
        fsExt.Write(bufftotal, 0, bufftotal.Length);
        fsExt.Flush();
        fsExt.Close();
    }


    //bNodeMerge指示的是node节点相合并.非预设节点不能合并.
    static bool CanMerge(ReferenceNode node, bool bNodeMerge = false)
    {
        if (node.strResources.ToLower().EndsWith(".prefab") && bNodeMerge)
        {
            foreach (var son in node.child)
            {
                if (!CanMerge(son))
                    return false;
            }

            if (node.parent.Count == 1)
            {
                return true;
            }
            else
            {
                //如果其父节点都是*.unity的话，那么他也可以算合并节点，打成一个资源，供多个资源依赖
                bool bFindOther = false;
                foreach (var par in node.parent)
                {
                    if (!par.strResources.ToLower().EndsWith(".unity"))
                    {
                        bFindOther = true;
                        break;
                    }
                }
                if (bFindOther)
                    return false;
                else
                {
                    return true;
                }
            }
        }
            //要合并的头节点并非预设.
        else if (bNodeMerge)
        {
            return false;
        }
        else
        {
            //在递归里，可能是预设的子元件.判断一个头节点是否能合并，因此要判断这个头节点的子节点是否能合并.
            foreach (var son in node.child)
            {
                if (!CanMerge(son))
                    return false;
            }

            if (node.parent.Count == 1)
            {
                return true;
            }
            return false;
        }
    }
    //增大粒度，部分资源如果没有被其他资源依赖，那么可以合并到父节点.仅能在父节点为prefab时这样合并.
    static List<ReferenceNode> MergeNode(ref List<ReferenceNode> refDict)
    {
        List<ReferenceNode> mergedNode = new List<ReferenceNode>();
        List<ReferenceNode> childList = new List<ReferenceNode>();
        foreach (var each in refDict)
        {
            if (!childList.Contains(each))
                childList.Add(each);
        }

        //交换层次
        List<ReferenceNode> childList2 = new List<ReferenceNode>();
        bool bUseChild = true;
        while (true)
        {
            List<ReferenceNode> child = null;
            List<ReferenceNode> child2 = null;
            if (bUseChild)
            {
                child = childList;
                child2 = childList2;
            }
            else
            {
                child = childList2;
                child2 = childList;
            }

            foreach (var each in child)
            {
                if (CanMerge(each, true))
                {
                    each.child.Clear();
                    if (!mergedNode.Contains(each))
                        mergedNode.Add(each);
                }
                else
                {
                    foreach (var childchild in each.child)
                        child2.Add(childchild);
                }
            }

            child.Clear();
            if (child2.Count == 0)
                break;
            bUseChild = !bUseChild;
        }

        return mergedNode;
    }

    static void GenReferenceTable(List<ReferenceNode> refList, List<ReferenceNode> freeList, string strSavePath)
    {
        List<ReferenceNode> all = new List<ReferenceNode>();
        foreach (var node in refList)
        {
            if (!all.Contains(node))
                all.Add(node);
        }

        foreach (var node in freeList)
        {
            if (!all.Contains(node))
                all.Add(node);
        }

        string strPath = strSavePath + "referenceTable.txt";
        FileStream fs = File.Create(strPath);
        FileInfo fCsvInfo = new FileInfo(strPath);
        string strBaseDirectory = strSavePath;
        strBaseDirectory = strBaseDirectory.Replace("\\", "/");

        //交换父子层
        List<ReferenceNode> childList = new List<ReferenceNode>();
        List<ReferenceNode> childList2 = new List<ReferenceNode>();
        bool bUseChild = true;
        foreach (var node in all)
        {
            if (!childList.Contains(node))
                childList.Add(node);
        }

        while (true)
        {
            List<ReferenceNode> child = null;
            List<ReferenceNode> child2 = null;
            if (bUseChild)
            {
                child = childList;
                child2 = childList2;
            }
            else
            {
                child = childList2;
                child2 = childList;
            }

            foreach (var node in child)
            {
                if (node.child.Count == 0)
                    continue;
                FileInfo f = new FileInfo(node.strResources);
                int nLast = node.strResources.LastIndexOf('/');
                int nExt = node.strResources.LastIndexOf('.');
                string strDirectory = "";
                if (nLast != -1)
                    strDirectory = node.strResources.Substring(0, nLast + 1);
                //string strName = node.Key.Substring(nLast == -1 ? 0:nLast + 1, (nLast == -1) ? nExt:(nExt - nLast) - 1);
                //后缀必须加上,否则会重复.
                byte[] buff = null;
                buff = System.Text.Encoding.UTF8.GetBytes(strDirectory + f.Name + ".assetbundle");
                fs.Write(buff, 0, buff.Length);
                fs.Write(System.Text.Encoding.UTF8.GetBytes(":"), 0, 1);
                int i = 0;
                foreach (var sonnode in node.child)
                {
                    if (!child2.Contains(sonnode))
                    {
                        child2.Add(sonnode);
                    }

                    FileInfo res = new FileInfo(sonnode.strResources);
                    nLast = sonnode.strResources.LastIndexOf('/');
                    nExt = sonnode.strResources.LastIndexOf('.');
                    strDirectory = "";
                    if (nLast != -1)
                        strDirectory = sonnode.strResources.Substring(0, nLast + 1);

                    byte[] buffinner = System.Text.Encoding.UTF8.GetBytes(strDirectory + res.Name + ".assetbundle");
                    fs.Write(buffinner, 0, buffinner.Length);
                    i++;
                    //先i++
                    if (i != node.child.Count)
                        fs.WriteByte((byte)',');
                }
                fs.Write(System.Text.Encoding.UTF8.GetBytes("\r\n"), 0, 2);
            }

            child.Clear();
            if (child2.Count == 0)
                break;
            bUseChild = !bUseChild;
        }
        fs.Flush();
        fs.Close();
        //CompressForFile.CompressFile(strPath, strBaseDirectory + Path.GetFileNameWithoutExtension(strPath) + ".txt.zip");
    }

    public static List<ReferenceNode> ScanChild(ReferenceNode node)
    {
        List<ReferenceNode> ret = new List<ReferenceNode>();
        //非递归扫描指定节点的子节点.
        foreach (var son in node.child)
        {
            bool bAdd = true;
            foreach (var each in son.parent)
            {
                if (node.child.Contains(each))
                {
                    bAdd = false;
                    break;
                }
            }
            if (bAdd)
            {
                if (!ret.Contains(son))
                    ret.Add(son);
            }
        }
        return ret;
    }
    public static void CleanTree(ref List<ReferenceNode> refTree)
    {
        //每个节点的子节点列表都包含了子树的任意一个节点.
        foreach (var node in refTree)
        {
            List<ReferenceNode> directParent = new List<ReferenceNode>();
            //得到直系孩子
            directParent = ScanChild(node);
            //令非直系子节点删除指向该父节点的引用.
            foreach (var each in node.child)
            {
                if (!directParent.Contains(each))
                    each.parent.Remove(node);
            }
            //重新创建子节点.
            node.child.Clear();
            foreach (var each in directParent)
            {
                node.child.Add(each);
            }
            CleanTree(ref directParent);
        }
    }

    static void GenMergeLog(List<ReferenceNode> mergedNode, string strSavePath)
    {
        string strExtPath = strSavePath + "MergeLog.txt";//合并记录表.
        FileStream fsExt = File.Create(strExtPath);
        foreach (var node in mergedNode)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(node.strResources);
            fsExt.Write(buffer, 0, buffer.Length);
            fsExt.Write(new byte[] { (byte)'\r', (byte)'\n' }, 0, 2);
        }
        fsExt.Close();
    }
    public static void PackagePrefab(List<ReferenceNode> referenceTree, List<ReferenceNode> freeNode, BuildTarget target, string strBaseDirectory)
    {
        //生成一个依赖表，用于加载文件的时候去寻找依赖关系
        Directory.CreateDirectory(strBaseDirectory);
        strBaseDirectory = strBaseDirectory.Replace("\\", "/");
        //所有节点数据都要得到信息
        GenResourceInfo(referenceDict, strBaseDirectory, "ext.txt");

        CleanTree(ref referenceTree);

        //从头节点递归子节点看能否合并.
        //List<ReferenceNode> mergedNode = MergeNode(ref referenceTree);

        //记录合并的节点
        //GenMergeLog(mergedNode, strBaseDirectory);

        //得到依赖表，通过合并的节点(子节点列表为空，自身独立成为一个数据块),自由节点（无依赖）
        GenReferenceTable(referenceTree, freeNode, strBaseDirectory);

        //场景是不能被预设依赖的，所有场景需要在最内部.
        List<string> sceneList = new List<string>();

        //自由资源打包.
        if (freeNode != null)
        {
            List<string> Dellst = new List<string>();
            foreach (var node in freeNode)
            {
                int nLast = 0;
                int nExt = 0;
                string strDirectory = "";
                //string strName = "";
                nLast = node.strResources.LastIndexOf('/');
                nExt = node.strResources.LastIndexOf('.');
                FileInfo f = new FileInfo(node.strResources);
                strDirectory = "";
                if (nLast != -1)
                    strDirectory = node.strResources.Substring(0, nLast + 1);
                //strName = node.strResources.Substring(nLast == -1 ? 0 : nLast + 1, (nLast == -1) ? nExt : (nExt - nLast) - 1);
                if (!Directory.Exists(strBaseDirectory + strDirectory))
                    Directory.CreateDirectory(strBaseDirectory + strDirectory);
                Object objRoot = AssetDatabase.LoadMainAssetAtPath(node.strResources);
                if (objRoot == null)
                {
                    Debug.LogError(node.strResources);
                    continue;
                }
                FileInfo fInfo = new FileInfo(node.strResources);
                //这些一定是场景.
                if (fInfo.Extension == ".unity")
                {
                    sceneList.Add(node.strResources);
                    Dellst.Add(node.strResources);
                }
                else
                if (objRoot.name == "")
                    Dellst.Add(node.strResources);
            }

            foreach (string str in Dellst)
            {
                bool bDel = false;
                ReferenceNode delNode = null;
                foreach (var node in freeNode)
                {
                    if (node.strResources == str)
                    {
                        bDel = true;
                        delNode = node;
                        break;
                    }
                }
                if (bDel)
                {
                    freeNode.Remove(delNode);
                }
            }

            //这些里面一定是预设.
            Debug.Log("<color=red>:打包自由节点 不依赖其他资源也不被其他资源依赖</color>:\r\n");
            foreach (var node in freeNode)
            {
                int nLast = 0;
                int nExt = 0;
                string strDirectory = "";
                //string strName = "";
                nLast = node.strResources.LastIndexOf('/');
                nExt = node.strResources.LastIndexOf('.');
                FileInfo f = new FileInfo(node.strResources);
                strDirectory = "";
                if (nLast != -1)
                    strDirectory = node.strResources.Substring(0, nLast + 1);
                //strName = node.strResources.Substring(nLast == -1 ? 0 : nLast + 1, (nLast == -1) ? nExt : (nExt - nLast) - 1);
                if (!Directory.Exists(strBaseDirectory + strDirectory))
                    Directory.CreateDirectory(strBaseDirectory + strDirectory);
                Object objRoot = AssetDatabase.LoadMainAssetAtPath(node.strResources);
                if (objRoot.name == "")
                {
                    Debug.Log("<color=red>模板预设不打包</color>" + node.strResources);
                    Object[] depend = EditorUtility.CollectDependencies(new Object[] {objRoot });
                    if (depend == null)
                        continue;
                    if (depend.Length == 0)
                        continue;
                }
                try
                {
                    Debug.Log("<color=red:打包自由节点</color>:\r\n" + node.strResources);
                    Object[] depend = EditorUtility.CollectDependencies(new Object[] { objRoot });
                    foreach (Object miss in depend)
                    {
                        if (miss == null)
                            continue;
                        string strMiss = AssetDatabase.GetAssetPath(miss);
                        if (strMiss == "")
                        {
                            System.Type tp = miss.GetType();
                            if (tp == typeof(UnityEngine.Shader) || tp == typeof(UnityEngine.Material) || tp == typeof(UnityEngine.Mesh))
                                continue;    
                            Debug.LogError("miss:" + miss.name);
                            GameObject.DestroyImmediate(miss, true);
                        }
                    }
                    BuildPipeline.BuildAssetBundle(objRoot, null, strBaseDirectory + strDirectory + f.Name + ".assetbundle", op, target);
                }
                catch (System.Exception exp)
                {
                    UnityEngine.Debug.LogError(exp.Message);
                    return;
                }
            }
            Debug.Log("<color=red>:打包自由节点 不依赖其他资源也不被其他资源依赖 完毕</color>:\r\n");
        }

        List<ReferenceNode> prefabList = new List<ReferenceNode>();
        List<ReferenceNode> baseRes = new List<ReferenceNode>();
        List<ReferenceNode> fbxRes = new List<ReferenceNode>();
        while (true)
        {
            //每次都得到剩下节点的最外层，依赖关系的最外层.
            List<ReferenceNode> ls = GetTopLayerNode(ref referenceTree);
            if (ls == null)
                break;
            if (ls.Count == 0)
                break;
            foreach (var node in ls)
            {
                if (node.strResources.ToLower().EndsWith(".prefab"))
                {
                    if (!prefabList.Contains(node))
                        prefabList.Add(node);
                    continue;
                }
                else
                {
                    if (node.strResources.ToLower().EndsWith(".png") || 
                        node.strResources.ToLower().EndsWith(".ttf") || 
                        node.strResources.ToLower().EndsWith(".tga") ||
                        node.strResources.ToLower().EndsWith(".psd") ||
                        node.strResources.ToLower().EndsWith(".exr") ||
                        node.strResources.ToLower().EndsWith(".wav") ||
                        node.strResources.ToLower().EndsWith(".tif") ||
                        node.strResources.ToLower().EndsWith(".jpg") ||
                        node.strResources.ToLower().EndsWith(".mp3"))
                    {
                        if (!baseRes.Contains(node))
                            baseRes.Add(node);
                        continue;
                    }

                    if (node.strResources.ToLower().EndsWith(".fbx"))
                    {
                        if (!fbxRes.Contains(node))
                            fbxRes.Add(node);
                        continue;
                    }
                }
                Object objRoot = AssetDatabase.LoadMainAssetAtPath(node.strResources);
                FileInfo fInfo = new FileInfo(node.strResources);
                int nLast = 0;
                int nExt = 0;
                string strDirectory = "";
                //string strName = "";
                nLast = node.strResources.LastIndexOf('/');
                nExt = node.strResources.LastIndexOf('.');
                strDirectory = "";
                if (nLast != -1)
                    strDirectory = node.strResources.Substring(0, nLast + 1);

                if (!Directory.Exists(strBaseDirectory + strDirectory))
                    Directory.CreateDirectory(strBaseDirectory + strDirectory);

                if (fInfo.Extension == ".unity" || fInfo.Name.EndsWith(".unity") || fInfo.Extension.ToLower() == ".unity")
                {
                    if (!sceneList.Contains(node.strResources))
                    {
                        sceneList.Add(node.strResources);
                    }
                }
                else
                {
					Debug.LogError("不知类型资源" + node.strResources);
                }
            }

            if (referenceTree.Count == 0)
                break;
        }

        if (baseRes.Count != 0)
        {
            BuildPipeline.PushAssetDependencies();
            foreach (var node in baseRes)
            {
                Object objRoot = AssetDatabase.LoadMainAssetAtPath(node.strResources);
                FileInfo fInfo = new FileInfo(node.strResources);
                int nLast = 0;
                int nExt = 0;
                string strDirectory = "";
                nLast = node.strResources.LastIndexOf('/');
                nExt = node.strResources.LastIndexOf('.');
                strDirectory = "";
                if (nLast != -1)
                    strDirectory = node.strResources.Substring(0, nLast + 1);
                if (!Directory.Exists(strBaseDirectory + strDirectory))
                    Directory.CreateDirectory(strBaseDirectory + strDirectory);
                BuildPipeline.BuildAssetBundle(objRoot, null, strBaseDirectory + strDirectory + fInfo.Name + ".assetbundle", op, target);
            }
        }

        if (fbxRes.Count != 0)
        {
            //打包每一个fbx.
            BuildPipeline.PushAssetDependencies();
            foreach (var node in fbxRes)
            {
                Object objRoot = AssetDatabase.LoadMainAssetAtPath(node.strResources);
                FileInfo fInfo = new FileInfo(node.strResources);
                int nLast = 0;
                int nExt = 0;
                string strDirectory = "";
                nLast = node.strResources.LastIndexOf('/');
                nExt = node.strResources.LastIndexOf('.');
                strDirectory = "";
                if (nLast != -1)
                    strDirectory = node.strResources.Substring(0, nLast + 1);
                if (!Directory.Exists(strBaseDirectory + strDirectory))
                    Directory.CreateDirectory(strBaseDirectory + strDirectory);

                //扫描这个fbx的上层prefab节点.打包每个预设.这些预设依赖这个fbx，但是互相不依赖, fbx与fbx之间也互相不依赖.
                //如果2个fbx共同拥有一个父,且2个fbx公用一部分脚本或者shader, 那么这2个fbx不能在同一级里，否则第一次加载公用资源的fbx含有公用资源,后面的fbx就缺失了这个资源，那么这个父必然拥有
                //List<ReferenceNode> parentPrefab = new List<ReferenceNode>();
                //foreach (var no in node.parent)
                //{
                //    if (no.strResources.ToLower().EndsWith(".prefab"))
                //        if (!parentPrefab.Contains(no))
                //            parentPrefab.Add(no);
                //}
                //被其他fbx拥有的父可能已经打包过
                //List<ReferenceNode> allreadyPackaged = new List<ReferenceNode>();
                //foreach (var noprefab in parentPrefab)
                //{
                //    if (prefabList.Contains(noprefab))
                //        prefabList.Remove(noprefab);
                //    else
                //    {
                        //2个预设依赖2个fbx，打包第一个fbx会打包2个预设，并让2个预设依赖这个fbx，但是打包后一个fbx时，之前的预设都打包过.会发生问题.
                //        allreadyPackaged.Add(noprefab);
                //    }
                //}

                //foreach (var no in allreadyPackaged)
                //{
                //    parentPrefab.Remove(no);
                //}

                //收集这些父节点
                //BuildPipeline.PushAssetDependencies();
                //不收集依赖.
                BuildPipeline.BuildAssetBundle(objRoot, null, strBaseDirectory + strDirectory + fInfo.Name + ".assetbundle", op2, target);

                //foreach (var no in parentPrefab)
                //{
                //    FileInfo fInfoPrefab2 = new FileInfo(no.strResources);
                //    Object objPrefab = AssetDatabase.LoadMainAssetAtPath(no.strResources);
                //    FileInfo fInfoPrefab = new FileInfo(no.strResources);
                //    nLast = 0;
                //    nExt = 0;
                //    strDirectory = "";
                //    nLast = no.strResources.LastIndexOf('/');
                //    nExt = no.strResources.LastIndexOf('.');
                //    strDirectory = "";
                //    if (nLast != -1)
                //        strDirectory = no.strResources.Substring(0, nLast + 1);
                //    if (!Directory.Exists(strBaseDirectory + strDirectory))
                //        Directory.CreateDirectory(strBaseDirectory + strDirectory);
                //    BuildPipeline.PushAssetDependencies();
                //    BuildPipeline.BuildAssetBundle(objPrefab, null, strBaseDirectory + strDirectory + fInfoPrefab2.Name + ".assetbundle", op, target);
                //    BuildPipeline.PopAssetDependencies();
                //}
                //BuildPipeline.PopAssetDependencies();
            }
        }
        //所有预设依赖前面的资源，但是预设间不互相依赖，否则shader,脚本，都会引用到这个shader或脚本最先被打包到的预设
        if (prefabList.Count != 0)
        {
            foreach (var node in prefabList)
            {
                Object objRoot = AssetDatabase.LoadMainAssetAtPath(node.strResources);
                FileInfo fInfo = new FileInfo(node.strResources);
                int nLast = 0;
                int nExt = 0;
                string strDirectory = "";
                nLast = node.strResources.LastIndexOf('/');
                nExt = node.strResources.LastIndexOf('.');
                strDirectory = "";
                if (nLast != -1)
                    strDirectory = node.strResources.Substring(0, nLast + 1);
                if (!Directory.Exists(strBaseDirectory + strDirectory))
                    Directory.CreateDirectory(strBaseDirectory + strDirectory);
            
                try
                {
                    if (objRoot.name == "")
                    {
                        Debug.Log("<color=red>模板预设不打包</color>" + node.strResources);
                        continue;
                    }

                    string[] strDepend = AssetDatabase.GetDependencies(new string[] { node.strResources });
                    Debug.Log("打包:" + node.strResources);
                    Object[] IncludeItem = EditorUtility.CollectDependencies(new Object[] { objRoot });
                    foreach (Object miss in IncludeItem)
                    {
                        if (miss == null)
                            continue;
                        string strMiss = AssetDatabase.GetAssetPath(miss);
                        if (strMiss == "")
                        {
                            System.Type tp = miss.GetType();
                            if (tp == typeof(UnityEngine.Shader) || tp == typeof(UnityEngine.Material) || tp == typeof(UnityEngine.Mesh))
                                continue;
                            Debug.LogError("miss:" + miss.name);
                            GameObject.DestroyImmediate(miss, true);
                        }
                    }
                    BuildPipeline.PushAssetDependencies();
                    BuildPipeline.BuildAssetBundle(objRoot, null, strBaseDirectory + strDirectory + fInfo.Name + ".assetbundle", op, target);
                    BuildPipeline.PopAssetDependencies();
                }

                catch (System.Exception exp)
                {
                    UnityEngine.Debug.Log("<color=green>打包ab出错:</color>" + exp.Message);
                    return;
                }
            }
        }

        foreach (string strScene in sceneList)
        {
            Object objRoot = AssetDatabase.LoadMainAssetAtPath(strScene);
            FileInfo fInfo = new FileInfo(strScene);
            int nLast = 0;
            int nExt = 0;
            string strDirectory = "";
            nLast = strScene.LastIndexOf('/');
            nExt = strScene.LastIndexOf('.');
            strDirectory = "";
            if (nLast != -1)
                strDirectory = strScene.Substring(0, nLast + 1);
            if (!Directory.Exists(strBaseDirectory + strDirectory))
                Directory.CreateDirectory(strBaseDirectory + strDirectory);
            if (fInfo.Extension == ".unity")
            {
                Debug.Log("<color=red>场景打包</color>:" + strScene);
                try
                {
                    BuildPipeline.PushAssetDependencies();
					BuildPipeline.BuildPlayer(new string[] { strScene }, strBaseDirectory + strDirectory + fInfo.Name + ".assetbundle", target, BuildOptions.BuildAdditionalStreamedScenes);
                    BuildPipeline.PopAssetDependencies();
                }
                catch (System.Exception exp)
                {
                    UnityEngine.Debug.Log("<color=green>打包场景出错:</color>" + exp.Message);
                    return;
                }
            }
            else
            {
                Debug.Log("<color=red>场景打包后缀名不对</color>:" + fInfo.FullName);
            }
        }

        if (fbxRes.Count != 0)
            BuildPipeline.PopAssetDependencies();
        if (baseRes.Count != 0)
            BuildPipeline.PopAssetDependencies();
        return;
    }
}

public class MakeDependTabel : ScriptableWizard
{
    public static BuildTarget target = BuildTarget.iOS;
    //首包如果要使AB场景能够下载使用，则必须勾选首包生成2个版本的信息.这样才能使首包（不含AB的包）和第一个更新包(包含AB以及首包内容)产生对比.
    public static bool useUpdate = false;
    //值如果是真，则打包，否则不打包.
	public static Dictionary<string, bool> PackageScene = new Dictionary<string, bool>();
    //首包要使用AB场景从服务器下载，则必须自动生成下个版本.并且下个版本包含首包选择的打包的其他场景.
    public static Dictionary<string, bool> UpdateScene = new Dictionary<string, bool>();

    //要打包的表格数据<后缀,文件列表>
    public static Dictionary<string, List<string>> strTable = new Dictionary<string, List<string>>();
    //要打包的脚本数据<后缀,文件列表>
    public static Dictionary<string, List<string>> strScript = new Dictionary<string, List<string>>();
    //要打包的动作文件和关卡配置文件
    public static Dictionary<string, List<string>> strBytes = new Dictionary<string, List<string>>();

    //以前存在的版本的.
    public List<string> strOldVersion = new List<string>();
    //下个版本.
    public string strNextVersion = "0.0.0.1";
    [MenuItem("Assets/Tool/FixUILabelEncoding")]
    static void FixUILabelEncoding()
    {
        UILabel[] labels = GameObject.FindObjectsOfType<UILabel>();
        for (int i = 0; i < labels.Length; i++)
        {
            labels[i].supportEncoding = false;
        } 
    }
    [MenuItem("Assets/Tool/ViewReference")]
    static void GetWhichPrefabDependThisAsset()
    {
        string target = "";
        if (Selection.activeObject != null)
            target = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(target))
            return;
        string[] files = Directory.GetFiles(Application.dataPath, "*.prefab", SearchOption.AllDirectories);
        string[] scene = Directory.GetFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories);
        
        List<Object> filelst = new List<Object>();
        for (int i = 0; i < files.Length; i++)
        {
            string[] source = AssetDatabase.GetDependencies(new string[] { files[i].Replace(Application.dataPath, "Assets") });
            for (int j = 0; j < source.Length; j++)
            {
                if (source[j] == target)
                    filelst.Add(AssetDatabase.LoadMainAssetAtPath(files[i].Replace(Application.dataPath, "Assets")));
            }
        }
        for (int i = 0; i < scene.Length; i++)
        {
            string[] source = AssetDatabase.GetDependencies(new string[] { scene[i].Replace(Application.dataPath, "Assets") });
            for (int j = 0; j < source.Length; j++)
            {
                if (source[j] == target)
                    filelst.Add(AssetDatabase.LoadMainAssetAtPath(scene[i].Replace(Application.dataPath, "Assets")));
            }
        }
        Selection.objects = filelst.ToArray();
    }

    //统计Prefab引用资源大小
    [MenuItem("Assets/Tool/查看预设依赖资源大小", false, 9)]
	static void GetSelectInfo()
	{
		FileStream fs = File.Open(Application.persistentDataPath + "/info.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
		List<string> filelst = new List<string>();
		if (fs != null){
			foreach (Object o in EditorUtility.CollectDependencies(Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets))){
				string str = AssetDatabase.GetAssetPath(o);
				if (filelst.Contains(str))
					continue;
				filelst.Add(str);

			}
		}
		long nByte = 0;
		foreach (var each in filelst)
		{
			FileInfo fiinfo = new FileInfo(each);
			string str = fiinfo.FullName + ":" + fiinfo.Length + " byte\r\n";
			byte[] buff = System.Text.Encoding.UTF8.GetBytes(str);
			fs.Write(buff, 0, buff.Length);
			nByte += fiinfo.Length;
		}

		string strKBMB = "";
		strKBMB = ReferenceNode.GetUnitString(nByte);
		byte[] buff2 = System.Text.Encoding.UTF8.GetBytes("all大小:" + strKBMB);
		fs.Write(buff2, 0, buff2.Length);
		fs.Flush();
		fs.Close();
		
		Debug.Log("GetSelectInfo done !  " + Application.persistentDataPath + "/info.txt");

	}

    //[MenuItem("VersionManager/9.版本更新工具", false, 8)]
    static void CreateWizard()
    {
		PackageScene.Clear();
        UpdateScene.Clear();
        useUpdate = false;
        target = BuildTarget.iOS;
        strTable.Clear();
        strScript.Clear();
        strBytes.Clear();
        strTable.Add(".txt", new List<string>());
        strScript.Add(".csl", new List<string>());
        strBytes.Add(".bytes", new List<string>());
		string []all = Directory.GetFiles("Assets/", "*.unity", SearchOption.AllDirectories);
		foreach (var s in all)
		{
			PackageScene.Add(s.Replace("\\", "/"), false);
            UpdateScene.Add(s.Replace("\\", "/"), false);
		}
        ScriptableWizard.DisplayWizard<MakeDependTabel>("生成预设的依赖表", "生成");
    }


    private static FileStream fs_thread;
    static void LogFile(string strWarning, string strStackTrace, LogType type)
    {
        byte[] line = new byte[2] { (byte)'\r', (byte)'\n' };
        if (fs_thread != null)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strWarning);
            fs_thread.Write(buffer, 0, buffer.Length);
            fs_thread.Write(line, 0, 2);
            buffer = System.Text.Encoding.UTF8.GetBytes(strStackTrace);
            fs_thread.Write(buffer, 0, buffer.Length);
            fs_thread.Write(line, 0, 2);
            fs_thread.Flush();
        }
    }

//    public static void Init()
//    {
//        if (fs_thread == null)
//            fs_thread = File.Open("C:/AssetBundle" + "/" + "error.log", FileMode.OpenOrCreate, FileAccess.Write);
//        Application.RegisterLogCallbackThreaded(LogFile);
//    }

    BuildTarget lastTarget;
    void OnWizardUpdate()
    {
        //改变了平台/改变了是否首包，读取对应平台的版本配置.
        if (lastTarget != null && lastTarget == target)
            return;
        string strVersionFile = "";
        try
        {
            strVersionFile = "";// PlatformMap.GetPlatformPath(target);
        }
        catch
        {

        }
        lastTarget = target;
        if (string.IsNullOrEmpty(strVersionFile))
        {
            strNextVersion = "0.0.0.0";
        }
        else
        {
            //strOldVersion = VersionManager.GetAllVersion(target);
            if (strOldVersion.Count == 0)
            {
                strNextVersion = "0.0.0.0";
            }
            else
            {
                strNextVersion = strOldVersion[strOldVersion.Count - 1];
                string[] str = strNextVersion.Split('.');
                if (str.Length == 4)
                {
                    int n = System.Convert.ToInt32(str[0]);
                    n++;
                    string tar = n.ToString();
                    for (int i = 1; i < 4; i++)
                    {
                        tar += "." + str[i];
                    }
                    strNextVersion = tar;
                }
            }
        }
    }

	Vector2 vecPos = new Vector2(0, 0);
	void OnGUI()
	{
		GUILayout.TextArea("平台设置:", GUILayout.Width(150));
		target = (BuildTarget)EditorGUILayout.EnumPopup(target, GUILayout.Width(100));
        //这个功能只有首包需要设置，以后的版本会自动，主要原因是首包没有对比，不知道哪些数据是服务器提供的,一旦设置则首包会生成2个版本的数据.
        OnWizardUpdate();
        if (strNextVersion == "0.0.0.0")
            useUpdate = GUILayout.Toggle(useUpdate, "部分场景从服务器更新（启动场景无法更新.）!", GUILayout.Width(300));
		Dictionary<string, bool> packgetmp = new Dictionary<string, bool>();
        Dictionary<string, bool> updatetmp = new Dictionary<string,bool>();
		GUILayout.BeginArea(new Rect(20, 50, 800, 800));
		vecPos = GUILayout.BeginScrollView(vecPos, GUILayout.Width(800), GUILayout.Height(500));
		foreach (var each in PackageScene)
		{
			packgetmp.Add(each.Key, each.Value);
            updatetmp.Add(each.Key, UpdateScene[each.Key]);
            //首包 打包的场景，无法从服务器下载.
            if (useUpdate && each.Value && updatetmp[each.Key])
                updatetmp[each.Key] = false;
			GUILayout.BeginHorizontal(GUILayout.Width(800), GUILayout.Height(20));
			packgetmp[each.Key] = GUILayout.Toggle(packgetmp[each.Key], each.Key, GUILayout.Width(500));
            //首包没有打包的场景,可以选择是否打到下个版本，是则首包可以从服务器读取下个版本的场景.
            if (!packgetmp[each.Key] && useUpdate)
            {
                updatetmp[each.Key] = GUILayout.Toggle(updatetmp[each.Key], "首包从服务器更新它");
            }

			GUILayout.EndHorizontal();
		}
		GUILayout.EndScrollView();
        foreach (var each in packgetmp)
        {
            PackageScene[each.Key] = each.Value;
            UpdateScene[each.Key] = updatetmp[each.Key];
            if (each.Value && UpdateScene[each.Key] && useUpdate)
            {
                UpdateScene[each.Key] = false;
            }
        }

		if (GUILayout.Button("Gen Reference Assetbundle", GUILayout.Width(250)))
			OnWizardCreate();
		GUILayout.EndArea();
	}

    void OnWizardCreate()
    {
        //更新无论如何放在第一个启动的场景是无法更新的.之后的任何场景都有可能更新.
        bool bGenTwoVersion = false;
        if (strNextVersion == "0.0.0.0")
        {
            if (useUpdate)
            {
                int nUpdateCount = 0;
                int nPackageCount = 0;
                foreach (var each in UpdateScene)
                {
                    if (each.Value)
                        nUpdateCount++;
                }

                foreach (var each in PackageScene)
                {
                    if (each.Value)
                        nPackageCount++;
                }

                if (nUpdateCount != 0 && nPackageCount != 0)
                {
                    //自动生成2个版本的更新包.首包记录Main,Startup的资源信息（不包含AB使用的.）
                    //自动生成版本记录Main.Startup,A,B的资源信息，然后对比，那么AB是新增的，必然更新.
                    if (EditorUtility.DisplayDialog("提示", "确认打包吗", "确定", "取消"))
                    {
                        ReferenceNode.Reset();
                        strOldVersion.Clear();
                        strNextVersion = "0.0.0.1";

                        string[] strResDirectory = Directory.GetDirectories("Assets/", "Resources", SearchOption.AllDirectories);
                        List<string> strRes = new List<string>();
                        for (int i = 0; i < strResDirectory.Length; i++)
                        {
                            strResDirectory[i] = strResDirectory[i].Replace("\\", "/");
                            if (strResDirectory[i].EndsWith(".DS_Store"))
                                continue;
                            string[] strMenu = strResDirectory[i].Split('/');
                            bool bAdd = true;
                            foreach (string eachMenu in strMenu)
                            {
                                if (eachMenu == "VersionManager" || eachMenu.ToLower() == ".svn" || eachMenu.ToLower() == ".ds_store")
                                {
                                    bAdd = false;
                                    break;
                                }
                            }
                            if (bAdd)
                                strRes.Add(strResDirectory[i]);
                        }

                        List<string> strResItem = new List<string>();
                        //get resources 
                        //get resources used resources
                        //get export's unity used resources
                        //gen it's reference table
                        foreach (var res in strRes)
                        {
                            string[] strFiles = Directory.GetFiles(res, "*.*", SearchOption.AllDirectories);
                            foreach (var eachFile in strFiles)
                            {
                                string strMenu = eachFile.Replace("\\", "/");
                                string[] dir = strMenu.Split('/');
                                bool bContinue = false;
                                foreach (var eachdir in dir)
                                {
                                    if (eachdir == ".svn")
                                    {
                                        bContinue = true;
                                        break;
                                    }
                                }
                                if (bContinue)
                                    continue;

                                //这些数据用不着打包，直接压缩为zip对比出更新列表就可以提供与下载.
                                if (strMenu.ToLower().EndsWith(".csl"))
                                {
                                    strScript[".csl"].Add(strMenu);
                                    continue;
                                }
                                else if (strMenu.ToLower().EndsWith(".bytes"))
                                {
                                    strBytes[".bytes"].Add(strMenu);
                                    continue;
                                }
                                else if (strMenu.ToLower().EndsWith(".txt"))
                                {
                                    strTable[".txt"].Add(strMenu);
                                    continue;
                                }

                                if (strMenu.ToLower().EndsWith(".cs") || strMenu.ToLower().EndsWith(".js") || strMenu.ToLower().EndsWith(".meta") ||
                                    strMenu.ToLower().EndsWith(".h") || strMenu.ToLower().EndsWith(".dll") || strMenu.ToLower().EndsWith(".mm") ||
                                    strMenu.ToLower().EndsWith(".cpp") || strMenu.EndsWith(".DS_Store") || strMenu.EndsWith(".svn"))
                                    continue;
                                if (strMenu.ToLower().EndsWith(".xor"))
                                {
                                   if (EditorUtility.DisplayDialog("Clean Tmp File", "delete file: " + strMenu + "[this file may be a compressed file]", "Ok", "Cancel"))
                                   {
                                       File.Delete(strMenu);
                                       continue;
                                   }
                                }
                                if (!strResItem.Contains(strMenu))
                                    strResItem.Add(strMenu);
                            }
                        }

                        //Main,Startup
                        foreach (var each in PackageScene)
                        {
                            if (each.Value)
                            {
                                if (!strResItem.Contains(each.Key))
                                    strResItem.Add(each.Key);
                            }
                        }

                        //A,B
                        foreach (var each in UpdateScene)
                        {
                            if (each.Value)
                                strResItem.Add(each.Key);
                        }

                        foreach (string str in strResItem)
                        {
                            ReferenceNode root = ReferenceNode.Alloc(str);
                            //只有unity,prefab,obj有依赖，其他资源互相不依赖.
                            if (str.ToLower().EndsWith(".unity") || str.ToLower().EndsWith(".prefab"))
                                ReferenceNode.OnStep(root);
                        }

                        List<ReferenceNode> mostInnerLevel = new List<ReferenceNode>();//最内层节点，即不被其他资源依赖的.
                        List<ReferenceNode> freeLevel = new List<ReferenceNode>();

                        foreach (KeyValuePair<string, ReferenceNode> each in ReferenceNode.referenceDict)
                        {
                            if (each.Value.parent.Count == 0 && each.Value.child.Count == 0)
                            {
                                //既不依赖其他，也不被其他依赖.
                                //可以独立打包的.
                                if (!freeLevel.Contains(each.Value))
                                    freeLevel.Add(each.Value);
                                else
                                    Debug.LogError("same free node" + each.Value);
                            }
                            else if (each.Value.parent.Count == 0)
                            {
                                if (!mostInnerLevel.Contains(each.Value))
                                    mostInnerLevel.Add(each.Value);
                                else
                                    Debug.LogError("same inner node" + each.Value);
                            }
                            else if (each.Value.child.Count == 0)
                            {
                                //无法独立打包，因为被其他人依赖，在其他资源的依赖链里
                            }
                        }

                        string SavePath = "";
                        SavePath = "";// PlatformMap.GetPlatformPath(target) + "/" + strNextVersion + "/";
                        ReferenceNode.PackagePrefab(mostInnerLevel, freeLevel, target, SavePath);

                        //加密脚本.
                        List<string> strOutputScript = new List<string>();
                        List<string> scriptCompressOut = new List<string>();
                        //if (!EncryptXOR.EncryptResGroup(strScript[".csl"], ref strOutputScript))
                        {
                            //压缩加密后的脚本.
                            //CompressForFile.Compress(SavePath, strOutputScript, true, ref scriptCompressOut);
                        }

                        List<string> strTableEncrypted = new List<string>();
                        List<string> strTableCompressOut = new List<string>();
                        //加密表格
                        //if (!EncryptXOR.EncryptResGroup(strTable[".txt"], ref strTableEncrypted))
                        {
                            //压缩加密后的表格.
                            //CompressForFile.Compress(SavePath, strTableEncrypted, true, ref strTableCompressOut);
                        }

                        //压缩动作和关卡配置bytes文件，非文本，直接压缩.
                        List<string> strBytesCompressOut = new List<string>();
                        //CompressForFile.Compress(SavePath, strBytes[".bytes"], true, ref strBytesCompressOut);

                        //自动生成文件MD5列表.
                        CreateMD5List.GenFileListXml(SavePath);
                        //对比每个版本自动生成更新列表.并更新v.xml
                        //GenAllUpdateVersionXml.GenUpdateXmlByVersion(target, strNextVersion, strOldVersion);

                        //把新版本号写到version.xml里去.
                        //VersionManager.UpdateVersionXml(target, strOldVersion, strNextVersion);

                        //自动生成第1个包.去除A,B使用的资源.
                        strNextVersion = "0.0.0.0";
                        //把第二个包相关的资源都生成出来
                        //表格，脚本，关卡配置动作配置文件都拷贝过来.
                        string oldSavePath = SavePath;
                        SavePath = "";// PlatformMap.GetPlatformPath(target) + "/" + strNextVersion + "/";
                        ReferenceNode.Reset();

                        foreach (var str in scriptCompressOut)
                        {
                            if (str.StartsWith(oldSavePath))
                            {
                                string strTargetDirectory = SavePath + str.Replace(oldSavePath, "");
                                int nIndex = strTargetDirectory.LastIndexOf('/');
                                if (nIndex != -1)
                                    Directory.CreateDirectory(strTargetDirectory.Substring(0, nIndex));
                                File.Copy(str, strTargetDirectory);
                            }
                        }

                        foreach (var str in strTableCompressOut)
                        {
                            if (str.StartsWith(oldSavePath))
                            {
                                string strTargetDirectory = SavePath + str.Replace(oldSavePath, "");
                                int nIndex = strTargetDirectory.LastIndexOf('/');
                                if (nIndex != -1)
                                    Directory.CreateDirectory(strTargetDirectory.Substring(0, nIndex));
                                File.Copy(str, strTargetDirectory);
                            }
                        }

                        foreach (var str in strBytesCompressOut)
                        {
                            if (str.StartsWith(oldSavePath))
                            {
                                string strTargetDirectory = SavePath + str.Replace(oldSavePath, "");
                                int nIndex = strTargetDirectory.LastIndexOf('/');
                                if (nIndex != -1)
                                    Directory.CreateDirectory(strTargetDirectory.Substring(0, nIndex));
                                File.Copy(str, strTargetDirectory);
                            }
                        }

                        //资源包.减去AB依赖的任何资源
                        List<string> deleted = new List<string>();//被AB依赖的任意资源都不能在首包出现.
                        foreach (var each in UpdateScene)
                        {
                            if (each.Value && strResItem.Contains(each.Key))
                            {
                                strResItem.Remove(each.Key);
                                string[] depend = AssetDatabase.GetDependencies(new string[]{each.Key});
                                foreach (var str in depend)
                                {
                                    string eachdel = str.Replace("\\", "/");
                                    if (eachdel.ToLower().EndsWith(".cs") || eachdel.ToLower().EndsWith(".js") || eachdel.ToLower().EndsWith(".meta") ||
                                    eachdel.ToLower().EndsWith(".h") || eachdel.ToLower().EndsWith(".dll") || eachdel.ToLower().EndsWith(".mm") ||
                                    eachdel.ToLower().EndsWith(".cpp") || eachdel.EndsWith(".DS_Store") || eachdel.EndsWith(".svn"))
                                        continue;
                                    if (!deleted.Contains(eachdel))
                                        deleted.Add(eachdel);
                                }
                            }
                        }

                        //把Main Startup用到而AB不用到的复制到0.0.0.0目录.不打包，没有依赖表，首包仅仅给其他版本的文件做对比.
                        List<string> allReference = new List<string>();
                        foreach (var str in strResItem)
                        {
                            string[] depend = AssetDatabase.GetDependencies(new string[] { str });
                            foreach (var eachstr in depend)
                            {
                                string refer = eachstr.Replace("\\", "/");
                                if (refer.ToLower().EndsWith(".cs") || refer.ToLower().EndsWith(".js") || refer.ToLower().EndsWith(".meta") ||
                                    refer.ToLower().EndsWith(".h") || refer.ToLower().EndsWith(".dll") || refer.ToLower().EndsWith(".mm") ||
                                    refer.ToLower().EndsWith(".cpp") || refer.EndsWith(".DS_Store") || refer.EndsWith(".svn"))
                                    continue;
                                if (!deleted.Contains(refer))
                                {
                                    if (!allReference.Contains(refer))
                                    {
                                        allReference.Add(refer);
                                    }
                                }
                            }
                        }
                        
                        //把所有allreference里的文件对应的.assetbundle从0.0.0.1版本复制到对应的0.0.0.0版本的对应目录.
                        foreach (var each in allReference)
                        {
                            if (File.Exists(oldSavePath + each + ".assetbundle"))
                            {
                                string strDir = SavePath + each + ".assetbundle";
                                int i = strDir.LastIndexOf('/');
                                if (i != -1)
                                    strDir = strDir.Substring(0, i);
                                Directory.CreateDirectory(strDir);
                                File.Copy(oldSavePath + each + ".assetbundle", SavePath + each + ".assetbundle");
                            }
                            else
                                Debug.LogError("not contains res error:" + each);
                        }

                        //记录信息.
                        //自动生成文件MD5列表.
                        CreateMD5List.GenFileListXml(SavePath);
                        strOldVersion.Clear();
                        strOldVersion.Add("0.0.0.0");
                        strNextVersion = "0.0.0.1";
                        //对比每个版本自动生成更新列表.并更新v.xml
                        //GenAllUpdateVersionXml.GenUpdateXmlByVersion(target, strNextVersion, strOldVersion);

                        //把新版本号写到version.xml里去.
                        //VersionManager.UpdateVersionXml(target, strOldVersion, strNextVersion);
                    }
                    return;
                }
                else
                {
                    //首包如果任何场景都不选择，那么下个更新包会把Main Startup等包都更新，所以首包要生成Main,Startup的资源包，生成文件列表
                    //第二个包.将生成Main,Startup,A,B等场景的资源,生成文件列表，与首包对比则AB,更新.
                    Debug.LogError("设置出错，选择首包使用更新，则一定要设置1-N个从服务器更新的场景，首包仅选择要导出的场景");
                }
            }
        }

        if (!bGenTwoVersion)
        {
            //生成更新包
            //依据设置的用到的包的所有资源.
            //表格，脚本，动作配置，关卡配置.
            //选择的场景的相关资源，
            if (EditorUtility.DisplayDialog("提示", "确认打包吗", "确定", "取消"))
            {
                ReferenceNode.Reset();
                string[] strResDirectory = Directory.GetDirectories("Assets/", "Resources", SearchOption.AllDirectories);
                List<string> strRes = new List<string>();
                for (int i = 0; i < strResDirectory.Length; i++)
                {
                    strResDirectory[i] = strResDirectory[i].Replace("\\", "/");
                    if (strResDirectory[i].EndsWith(".DS_Store"))
                        continue;
                    string[] strMenu = strResDirectory[i].Split('/');
                    bool bAdd = true;
                    foreach (string eachMenu in strMenu)
                    {
                        if (eachMenu == "VersionManager" || eachMenu.ToLower() == ".svn" || eachMenu.ToLower() == ".ds_store")
                        {
                            bAdd = false;
                            break;
                        }
                    }
                    if (bAdd)
                        strRes.Add(strResDirectory[i]);
                }

                List<string> strResItem = new List<string>();
                //get resources 
                //get resources used resources
                //get export's unity used resources
                //gen it's reference table
                foreach (var res in strRes)
                {
                    string[] strFiles = Directory.GetFiles(res, "*.*", SearchOption.AllDirectories);
                    foreach (var eachFile in strFiles)
                    {
                        string strMenu = eachFile.Replace("\\", "/");
                        string[] dir = strMenu.Split('/');
                        bool bContinue = false;
                        foreach (var eachdir in dir)
                        {
                            if (eachdir == ".svn")
                            {
                                bContinue = true;
                                break;
                            }
                        }
                        if (bContinue)
                            continue;

                        //这些数据用不着打包，直接压缩为zip对比出更新列表就可以提供与下载.
                        if (strMenu.ToLower().EndsWith(".csl"))
                        {
                            strScript[".csl"].Add(strMenu);
                            continue;
                        }
                        else if (strMenu.ToLower().EndsWith(".bytes"))
                        {
                            strBytes[".bytes"].Add(strMenu);
                            continue;
                        }
                        else if (strMenu.ToLower().EndsWith(".txt"))
                        {
                            strTable[".txt"].Add(strMenu);
                            continue;
                        }

                        if (strMenu.ToLower().EndsWith(".cs") || strMenu.ToLower().EndsWith(".js") || strMenu.ToLower().EndsWith(".meta") ||
                            strMenu.ToLower().EndsWith(".h") || strMenu.ToLower().EndsWith(".dll") || strMenu.ToLower().EndsWith(".mm") ||
                            strMenu.ToLower().EndsWith(".cpp") || strMenu.EndsWith(".DS_Store") || strMenu.EndsWith(".svn"))
                            continue;
                        if (!strResItem.Contains(strMenu))
                            strResItem.Add(strMenu);
                    }
                }

                foreach (var each in PackageScene)
                {
                    if (each.Value)
                    {
                        if (!strResItem.Contains(each.Key))
                            strResItem.Add(each.Key);
                    }
                }

                foreach (string str in strResItem)
                {
                    ReferenceNode root = ReferenceNode.Alloc(str);
                    //只有unity,prefab,obj有依赖，其他资源互相不依赖.
                    if (str.ToLower().EndsWith(".unity") || str.ToLower().EndsWith(".prefab"))
                        ReferenceNode.OnStep(root);
                }

                List<ReferenceNode> mostInnerLevel = new List<ReferenceNode>();//最内层节点，即不被其他资源依赖的.
                List<ReferenceNode> freeLevel = new List<ReferenceNode>();

                foreach (KeyValuePair<string, ReferenceNode> each in ReferenceNode.referenceDict)
                {
                    if (each.Value.parent.Count == 0 && each.Value.child.Count == 0)
                    {
                        //既不依赖其他，也不被其他依赖.
                        //可以独立打包的.
                        if (!freeLevel.Contains(each.Value))
                            freeLevel.Add(each.Value);
                        else
                            Debug.LogError("same free node" + each.Value);
                    }
                    else if (each.Value.parent.Count == 0)
                    {
                        if (!mostInnerLevel.Contains(each.Value))
                            mostInnerLevel.Add(each.Value);
                        else
                            Debug.LogError("same inner node" + each.Value);
                    }
                    else if (each.Value.child.Count == 0)
                    {
                        //无法独立打包，因为被其他人依赖，在其他资源的依赖链里
                    }
                }

                string SavePath = "";
                SavePath = "";// PlatformMap.GetPlatformPath(target) + "/" + strNextVersion + "/";
                ReferenceNode.PackagePrefab(mostInnerLevel, freeLevel, target, SavePath);

                //加密脚本.
                List<string> strOutputScript = new List<string>();
                List<string> scriptCompressOut = new List<string>();
                //if (!EncryptXOR.EncryptResGroup(strScript[".csl"], ref strOutputScript))
                {
                    //压缩加密后的脚本.
                    //CompressForFile.Compress(SavePath, strOutputScript, true, ref scriptCompressOut);
                }

                List<string> strTableEncrypted = new List<string>();
                List<string> strTableCompressOut = new List<string>();
                //加密表格
                //if (!EncryptXOR.EncryptResGroup(strTable[".txt"], ref strTableEncrypted))
                {
                    //压缩加密后的表格.
                    //CompressForFile.Compress(SavePath, strTableEncrypted, true, ref strTableCompressOut);
                }

                //压缩动作和关卡配置bytes文件，非文本，直接压缩.
                List<string> strBytesCompressOut = new List<string>();
                //CompressForFile.Compress(SavePath, strBytes[".bytes"], true, ref strBytesCompressOut);

                //自动生成文件MD5列表.
                CreateMD5List.GenFileListXml(SavePath);
                //对比每个版本自动生成更新列表.并更新v.xml
                //GenAllUpdateVersionXml.GenUpdateXmlByVersion(target, strNextVersion, strOldVersion);

                //把新版本号写到version.xml里去.
                //VersionManager.UpdateVersionXml(target, strOldVersion, strNextVersion);
            }

            if (fs_thread != null)
            {
                fs_thread.Flush();
                fs_thread.Close();
                fs_thread = null;
            }
            return;
        }
    }
}
