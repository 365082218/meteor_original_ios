using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
public class MakeDependTabel : ScriptableWizard
{
    public static Dictionary<string, ReferenceNode> referenceDict = new Dictionary<string, ReferenceNode>();
    public string strResources;

    public List<ReferenceNode> child = new List<ReferenceNode>();//依赖列表.
    public List<ReferenceNode> parent = new List<ReferenceNode>();//被人依赖列表.
    public static void OnStep(ReferenceNode current)
    {
        //已经计算过此节点的依赖项
        if (current.child != null)
            return;
        //此节点不依赖其他节点
        string[] depend = AssetDatabase.GetDependencies(new string[] { current.strResources });
        if (depend == null || depend.Length == 0)
            return;
        //统计依赖的其他节点
        UnityEngine.Object objCurrent = AssetDatabase.LoadMainAssetAtPath(current.strResources);
        if (objCurrent == null)
        {
            WSLog.LogError("objCurrent is null:" + current.strResources);
            return;
        }
        if (objCurrent.name == "")
        {
            WSLog.LogError("objCurrent.name is empty:" + current.strResources);
            return;
        }
        objCurrent = null;
        foreach (string son in depend)
        {
            string strson = son;
            strson = strson.Replace("\\", "/");
            if (strson == "")
                continue;

            FileInfo fInfo = new FileInfo(strson);
            //代码和动态链接库是不需要
            if (fInfo.Extension.ToLower() == ".cs" || fInfo.Extension.ToLower() == ".js" || fInfo.Extension.ToLower() == ".dll")
                continue;
            //自身
            if (strson == current.strResources)
                continue;
            if (strson == "")
            {
                WSLog.LogError("依赖项为空?:" + current.strResources);
                continue;
            }
            ReferenceNode sonNode = ReferenceNode.Alloc(strson);
            current.AddDependencyNode(sonNode);
        }
    }

    //自己保证依赖.
    public static string GetUnitString(long bytes, int level = 3)
    {
        string[] unit = new string[] { "GB", "MB", "KB", "Byte" };
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
        string strExtPath = strSavePath + "MergeWSLog.txt";//合并记录表.
        FileStream fsExt = File.Create(strExtPath);
        foreach (var node in mergedNode)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(node.strResources);
            fsExt.Write(buffer, 0, buffer.Length);
            fsExt.Write(new byte[] { (byte)'\r', (byte)'\n' }, 0, 2);
        }
        fsExt.Close();
    }
    //public static void PackagePrefab(List<ReferenceNode> referenceTree, List<ReferenceNode> freeNode, BuildTarget target, string strBaseDirectory)
    //{
    //    //生成一个依赖表，用于加载文件的时候去寻找依赖关系
    //    Directory.CreateDirectory(strBaseDirectory);
    //    strBaseDirectory = strBaseDirectory.Replace("\\", "/");
    //    //所有节点数据都要得到信息
    //    GenResourceInfo(referenceDict, strBaseDirectory, "ext.txt");

    //    CleanTree(ref referenceTree);

    //    //从头节点递归子节点看能否合并.
    //    //List<ReferenceNode> mergedNode = MergeNode(ref referenceTree);

    //    //记录合并的节点
    //    //GenMergeLog(mergedNode, strBaseDirectory);

    //    //得到依赖表，通过合并的节点(子节点列表为空，自身独立成为一个数据块),自由节点（无依赖）
    //    GenReferenceTable(referenceTree, freeNode, strBaseDirectory);

    //    //场景是不能被预设依赖的，所有场景需要在最内部.
    //    List<string> sceneList = new List<string>();

    //    //自由资源打包.
    //    if (freeNode != null)
    //    {
    //        List<string> Dellst = new List<string>();
    //        foreach (var node in freeNode)
    //        {
    //            int nLast = 0;
    //            int nExt = 0;
    //            string strDirectory = "";
    //            //string strName = "";
    //            nLast = node.strResources.LastIndexOf('/');
    //            nExt = node.strResources.LastIndexOf('.');
    //            FileInfo f = new FileInfo(node.strResources);
    //            strDirectory = "";
    //            if (nLast != -1)
    //                strDirectory = node.strResources.Substring(0, nLast + 1);
    //            //strName = node.strResources.Substring(nLast == -1 ? 0 : nLast + 1, (nLast == -1) ? nExt : (nExt - nLast) - 1);
    //            if (!Directory.Exists(strBaseDirectory + strDirectory))
    //                Directory.CreateDirectory(strBaseDirectory + strDirectory);
    //            Object objRoot = AssetDatabase.LoadMainAssetAtPath(node.strResources);
    //            if (objRoot == null)
    //            {
    //                Debug.LogError(node.strResources);
    //                continue;
    //            }
    //            FileInfo fInfo = new FileInfo(node.strResources);
    //            //这些一定是场景.
    //            if (fInfo.Extension == ".unity")
    //            {
    //                sceneList.Add(node.strResources);
    //                Dellst.Add(node.strResources);
    //            }
    //            else
    //            if (objRoot.name == "")
    //                Dellst.Add(node.strResources);
    //        }

    //        foreach (string str in Dellst)
    //        {
    //            bool bDel = false;
    //            ReferenceNode delNode = null;
    //            foreach (var node in freeNode)
    //            {
    //                if (node.strResources == str)
    //                {
    //                    bDel = true;
    //                    delNode = node;
    //                    break;
    //                }
    //            }
    //            if (bDel)
    //            {
    //                freeNode.Remove(delNode);
    //            }
    //        }

    //        //这些里面一定是预设.
    //        Debug.Log("<color=red>:打包自由节点 不依赖其他资源也不被其他资源依赖</color>:\r\n");
    //        foreach (var node in freeNode)
    //        {
    //            int nLast = 0;
    //            int nExt = 0;
    //            string strDirectory = "";
    //            //string strName = "";
    //            nLast = node.strResources.LastIndexOf('/');
    //            nExt = node.strResources.LastIndexOf('.');
    //            FileInfo f = new FileInfo(node.strResources);
    //            strDirectory = "";
    //            if (nLast != -1)
    //                strDirectory = node.strResources.Substring(0, nLast + 1);
    //            //strName = node.strResources.Substring(nLast == -1 ? 0 : nLast + 1, (nLast == -1) ? nExt : (nExt - nLast) - 1);
    //            if (!Directory.Exists(strBaseDirectory + strDirectory))
    //                Directory.CreateDirectory(strBaseDirectory + strDirectory);
    //            Object objRoot = AssetDatabase.LoadMainAssetAtPath(node.strResources);
    //            if (objRoot.name == "")
    //            {
    //                Debug.Log("<color=red>模板预设不打包</color>" + node.strResources);
    //                Object[] depend = EditorUtility.CollectDependencies(new Object[] { objRoot });
    //                if (depend == null)
    //                    continue;
    //                if (depend.Length == 0)
    //                    continue;
    //            }
    //            try
    //            {
    //                Debug.Log("<color=red:打包自由节点</color>:\r\n" + node.strResources);
    //                Object[] depend = EditorUtility.CollectDependencies(new Object[] { objRoot });
    //                foreach (Object miss in depend)
    //                {
    //                    if (miss == null)
    //                        continue;
    //                    string strMiss = AssetDatabase.GetAssetPath(miss);
    //                    if (strMiss == "")
    //                    {
    //                        System.Type tp = miss.GetType();
    //                        if (tp == typeof(UnityEngine.Shader) || tp == typeof(UnityEngine.Material) || tp == typeof(UnityEngine.Mesh))
    //                            continue;
    //                        Debug.LogError("miss:" + miss.name);
    //                        GameObject.DestroyImmediate(miss, true);
    //                    }
    //                }
    //                //BuildPipeline.BuildAssetBundle(objRoot, null, strBaseDirectory + strDirectory + f.Name + ".assetbundle", op, target);
    //            }
    //            catch (System.Exception exp)
    //            {
    //                WSLog.LogError(exp.Message);
    //                return;
    //            }
    //        }
    //        Debug.Log("<color=red>:打包自由节点 不依赖其他资源也不被其他资源依赖 完毕</color>:\r\n");
    //    }

    //    List<ReferenceNode> prefabList = new List<ReferenceNode>();
    //    List<ReferenceNode> baseRes = new List<ReferenceNode>();
    //    List<ReferenceNode> fbxRes = new List<ReferenceNode>();
    //    while (true)
    //    {
    //        //每次都得到剩下节点的最外层，依赖关系的最外层.
    //        List<ReferenceNode> ls = ReferenceNode.GetTopLayerNode(ref referenceTree);
    //        if (ls == null)
    //            break;
    //        if (ls.Count == 0)
    //            break;
    //        foreach (var node in ls)
    //        {
    //            if (node.strResources.ToLower().EndsWith(".prefab"))
    //            {
    //                if (!prefabList.Contains(node))
    //                    prefabList.Add(node);
    //                continue;
    //            }
    //            else
    //            {
    //                if (node.strResources.ToLower().EndsWith(".png") ||
    //                    node.strResources.ToLower().EndsWith(".ttf") ||
    //                    node.strResources.ToLower().EndsWith(".tga") ||
    //                    node.strResources.ToLower().EndsWith(".psd") ||
    //                    node.strResources.ToLower().EndsWith(".exr") ||
    //                    node.strResources.ToLower().EndsWith(".wav") ||
    //                    node.strResources.ToLower().EndsWith(".tif") ||
    //                    node.strResources.ToLower().EndsWith(".jpg") ||
    //                    node.strResources.ToLower().EndsWith(".mp3"))
    //                {
    //                    if (!baseRes.Contains(node))
    //                        baseRes.Add(node);
    //                    continue;
    //                }

    //                if (node.strResources.ToLower().EndsWith(".fbx"))
    //                {
    //                    if (!fbxRes.Contains(node))
    //                        fbxRes.Add(node);
    //                    continue;
    //                }
    //            }
    //            Object objRoot = AssetDatabase.LoadMainAssetAtPath(node.strResources);
    //            FileInfo fInfo = new FileInfo(node.strResources);
    //            int nLast = 0;
    //            int nExt = 0;
    //            string strDirectory = "";
    //            //string strName = "";
    //            nLast = node.strResources.LastIndexOf('/');
    //            nExt = node.strResources.LastIndexOf('.');
    //            strDirectory = "";
    //            if (nLast != -1)
    //                strDirectory = node.strResources.Substring(0, nLast + 1);

    //            if (!Directory.Exists(strBaseDirectory + strDirectory))
    //                Directory.CreateDirectory(strBaseDirectory + strDirectory);

    //            if (fInfo.Extension == ".unity" || fInfo.Name.EndsWith(".unity") || fInfo.Extension.ToLower() == ".unity")
    //            {
    //                if (!sceneList.Contains(node.strResources))
    //                {
    //                    sceneList.Add(node.strResources);
    //                }
    //            }
    //            else
    //            {
    //                Debug.LogError("不知类型资源" + node.strResources);
    //            }
    //        }

    //        if (referenceTree.Count == 0)
    //            break;
    //    }

    //    if (baseRes.Count != 0)
    //    {
    //        BuildPipeline.PushAssetDependencies();
    //        foreach (var node in baseRes)
    //        {
    //            Object objRoot = AssetDatabase.LoadMainAssetAtPath(node.strResources);
    //            FileInfo fInfo = new FileInfo(node.strResources);
    //            int nLast = 0;
    //            int nExt = 0;
    //            string strDirectory = "";
    //            nLast = node.strResources.LastIndexOf('/');
    //            nExt = node.strResources.LastIndexOf('.');
    //            strDirectory = "";
    //            if (nLast != -1)
    //                strDirectory = node.strResources.Substring(0, nLast + 1);
    //            if (!Directory.Exists(strBaseDirectory + strDirectory))
    //                Directory.CreateDirectory(strBaseDirectory + strDirectory);
    //            //BuildPipeline.BuildAssetBundle(objRoot, null, strBaseDirectory + strDirectory + fInfo.Name + ".assetbundle", op, target);
    //        }
    //    }

    //    if (fbxRes.Count != 0)
    //    {
    //        //打包每一个fbx.
    //        BuildPipeline.PushAssetDependencies();
    //        foreach (var node in fbxRes)
    //        {
    //            Object objRoot = AssetDatabase.LoadMainAssetAtPath(node.strResources);
    //            FileInfo fInfo = new FileInfo(node.strResources);
    //            int nLast = 0;
    //            int nExt = 0;
    //            string strDirectory = "";
    //            nLast = node.strResources.LastIndexOf('/');
    //            nExt = node.strResources.LastIndexOf('.');
    //            strDirectory = "";
    //            if (nLast != -1)
    //                strDirectory = node.strResources.Substring(0, nLast + 1);
    //            if (!Directory.Exists(strBaseDirectory + strDirectory))
    //                Directory.CreateDirectory(strBaseDirectory + strDirectory);

    //            //扫描这个fbx的上层prefab节点.打包每个预设.这些预设依赖这个fbx，但是互相不依赖, fbx与fbx之间也互相不依赖.
    //            //如果2个fbx共同拥有一个父,且2个fbx公用一部分脚本或者shader, 那么这2个fbx不能在同一级里，否则第一次加载公用资源的fbx含有公用资源,后面的fbx就缺失了这个资源，那么这个父必然拥有
    //            //List<ReferenceNode> parentPrefab = new List<ReferenceNode>();
    //            //foreach (var no in node.parent)
    //            //{
    //            //    if (no.strResources.ToLower().EndsWith(".prefab"))
    //            //        if (!parentPrefab.Contains(no))
    //            //            parentPrefab.Add(no);
    //            //}
    //            //被其他fbx拥有的父可能已经打包过
    //            //List<ReferenceNode> allreadyPackaged = new List<ReferenceNode>();
    //            //foreach (var noprefab in parentPrefab)
    //            //{
    //            //    if (prefabList.Contains(noprefab))
    //            //        prefabList.Remove(noprefab);
    //            //    else
    //            //    {
    //            //2个预设依赖2个fbx，打包第一个fbx会打包2个预设，并让2个预设依赖这个fbx，但是打包后一个fbx时，之前的预设都打包过.会发生问题.
    //            //        allreadyPackaged.Add(noprefab);
    //            //    }
    //            //}

    //            //foreach (var no in allreadyPackaged)
    //            //{
    //            //    parentPrefab.Remove(no);
    //            //}

    //            //收集这些父节点
    //            //BuildPipeline.PushAssetDependencies();
    //            //不收集依赖.
    //            //BuildPipeline.BuildAssetBundle(objRoot, null, strBaseDirectory + strDirectory + fInfo.Name + ".assetbundle", op2, target);

    //            //foreach (var no in parentPrefab)
    //            //{
    //            //    FileInfo fInfoPrefab2 = new FileInfo(no.strResources);
    //            //    Object objPrefab = AssetDatabase.LoadMainAssetAtPath(no.strResources);
    //            //    FileInfo fInfoPrefab = new FileInfo(no.strResources);
    //            //    nLast = 0;
    //            //    nExt = 0;
    //            //    strDirectory = "";
    //            //    nLast = no.strResources.LastIndexOf('/');
    //            //    nExt = no.strResources.LastIndexOf('.');
    //            //    strDirectory = "";
    //            //    if (nLast != -1)
    //            //        strDirectory = no.strResources.Substring(0, nLast + 1);
    //            //    if (!Directory.Exists(strBaseDirectory + strDirectory))
    //            //        Directory.CreateDirectory(strBaseDirectory + strDirectory);
    //            //    BuildPipeline.PushAssetDependencies();
    //            //    BuildPipeline.BuildAssetBundle(objPrefab, null, strBaseDirectory + strDirectory + fInfoPrefab2.Name + ".assetbundle", op, target);
    //            //    BuildPipeline.PopAssetDependencies();
    //            //}
    //            //BuildPipeline.PopAssetDependencies();
    //        }
    //    }
    //    //所有预设依赖前面的资源，但是预设间不互相依赖，否则shader,脚本，都会引用到这个shader或脚本最先被打包到的预设
    //    if (prefabList.Count != 0)
    //    {
    //        foreach (var node in prefabList)
    //        {
    //            Object objRoot = AssetDatabase.LoadMainAssetAtPath(node.strResources);
    //            FileInfo fInfo = new FileInfo(node.strResources);
    //            int nLast = 0;
    //            int nExt = 0;
    //            string strDirectory = "";
    //            nLast = node.strResources.LastIndexOf('/');
    //            nExt = node.strResources.LastIndexOf('.');
    //            strDirectory = "";
    //            if (nLast != -1)
    //                strDirectory = node.strResources.Substring(0, nLast + 1);
    //            if (!Directory.Exists(strBaseDirectory + strDirectory))
    //                Directory.CreateDirectory(strBaseDirectory + strDirectory);

    //            try
    //            {
    //                if (objRoot.name == "")
    //                {
    //                    Debug.Log("<color=red>模板预设不打包</color>" + node.strResources);
    //                    continue;
    //                }

    //                string[] strDepend = AssetDatabase.GetDependencies(new string[] { node.strResources });
    //                Debug.Log("打包:" + node.strResources);
    //                Object[] IncludeItem = EditorUtility.CollectDependencies(new Object[] { objRoot });
    //                foreach (Object miss in IncludeItem)
    //                {
    //                    if (miss == null)
    //                        continue;
    //                    string strMiss = AssetDatabase.GetAssetPath(miss);
    //                    if (strMiss == "")
    //                    {
    //                        System.Type tp = miss.GetType();
    //                        if (tp == typeof(UnityEngine.Shader) || tp == typeof(UnityEngine.Material) || tp == typeof(UnityEngine.Mesh))
    //                            continue;
    //                        Debug.LogError("miss:" + miss.name);
    //                        GameObject.DestroyImmediate(miss, true);
    //                    }
    //                }
    //                //BuildPipeline.PushAssetDependencies();
    //                //BuildPipeline.BuildAssetBundle(objRoot, null, strBaseDirectory + strDirectory + fInfo.Name + ".assetbundle", op, target);
    //                //BuildPipeline.PopAssetDependencies();
    //            }

    //            catch (System.Exception exp)
    //            {
    //                WSLog.Log("<color=green>打包ab出错:</color>" + exp.Message);
    //                return;
    //            }
    //        }
    //    }

    //    foreach (string strScene in sceneList)
    //    {
    //        Object objRoot = AssetDatabase.LoadMainAssetAtPath(strScene);
    //        FileInfo fInfo = new FileInfo(strScene);
    //        int nLast = 0;
    //        int nExt = 0;
    //        string strDirectory = "";
    //        nLast = strScene.LastIndexOf('/');
    //        nExt = strScene.LastIndexOf('.');
    //        strDirectory = "";
    //        if (nLast != -1)
    //            strDirectory = strScene.Substring(0, nLast + 1);
    //        if (!Directory.Exists(strBaseDirectory + strDirectory))
    //            Directory.CreateDirectory(strBaseDirectory + strDirectory);
    //        if (fInfo.Extension == ".unity")
    //        {
    //            Debug.Log("<color=red>场景打包</color>:" + strScene);
    //            try
    //            {
    //                BuildPipeline.PushAssetDependencies();
    //                BuildPipeline.BuildPlayer(new string[] { strScene }, strBaseDirectory + strDirectory + fInfo.Name + ".assetbundle", target, BuildOptions.BuildAdditionalStreamedScenes);
    //                BuildPipeline.PopAssetDependencies();
    //            }
    //            catch (System.Exception exp)
    //            {
    //                WSLog.Log("<color=green>打包场景出错:</color>" + exp.Message);
    //                return;
    //            }
    //        }
    //        else
    //        {
    //            Debug.Log("<color=red>场景打包后缀名不对</color>:" + fInfo.FullName);
    //        }
    //    }

    //    if (fbxRes.Count != 0)
    //        BuildPipeline.PopAssetDependencies();
    //    if (baseRes.Count != 0)
    //        BuildPipeline.PopAssetDependencies();
    //    return;
    //}

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
		strKBMB = GetUnitString(nByte);
		byte[] buff2 = System.Text.Encoding.UTF8.GetBytes("all大小:" + strKBMB);
		fs.Write(buff2, 0, buff2.Length);
		fs.Flush();
		fs.Close();
		
		Debug.Log("GetSelectInfo done !  " + Application.persistentDataPath + "/info.txt");

	}

    //[MenuItem("VersionManager/9.版本更新工具", false, 8)]
    static void CreateWizard()
    {

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
    }

	Vector2 vecPos = new Vector2(0, 0);
	void OnGUI()
	{
		GUILayout.TextArea("平台设置:", GUILayout.Width(150));
		target = (BuildTarget)EditorGUILayout.EnumPopup(target, GUILayout.Width(100));
        //这个功能只有首包需要设置，以后的版本会自动，主要原因是首包没有对比，不知道哪些数据是服务器提供的,一旦设置则首包会生成2个版本的数据.
        OnWizardUpdate();
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
    }
}
