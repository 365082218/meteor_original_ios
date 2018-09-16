using ProtoBuf;
using System.Collections.Generic;

[ProtoContract(AsReferenceDefault = true)]
public class ReferenceNode
{
    public static void Reset()
    {
        referenceDict.Clear();
    }

    public static ReferenceNode GetExistNode(string strLocation)
    {
        if (referenceDict.ContainsKey(strLocation))
            return referenceDict[strLocation];
        return null;
    }

    public static ReferenceNode Alloc(string str)
    {
        if (referenceDict.ContainsKey(str))
            return referenceDict[str];
        ReferenceNode ret = new ReferenceNode(str);
        referenceDict.Add(str, ret);
        return ret;
    }

    public ReferenceNode()
    {

    }

    public ReferenceNode(string str)
    {
        strResources = str;
    }

    public static Dictionary<string, ReferenceNode> referenceDict = new Dictionary<string, ReferenceNode>();
    [ProtoMember(1)]
    public string strResources;//客户端：bundle地址，如果有'/'则最后一个文件名作为资源加载名LoadAsset()，编辑器：文件完整路径.
    [ProtoMember(2, AsReference = true)]
    public List<ReferenceNode> child;//依赖列表.
    [ProtoMember(3, AsReference = true)]
    public List<ReferenceNode> parent;//被人依赖列表.
    //增加一个引用节点，我引用了其他资源.
    public void AddDependencyNode(ReferenceNode childnode)
    {
        if (childnode == null)
            Log.WriteError("childnode == null");
        if (child == null)
            child = new List<ReferenceNode>();
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
            Log.WriteError("parentnode == null");
        if (parent == null)
            parent = new List<ReferenceNode>();
        foreach (ReferenceNode node in parent)
        {
            if (node.strResources == parentnode.strResources)
                return;
        }
        parent.Add(parentnode);
    }

    public static ReferenceNode CloneTree(ReferenceNode root)
    {
        ReferenceNode ret = new ReferenceNode(root.strResources);
        if (root.child != null)
        {
            foreach (ReferenceNode son in root.child)
            {
                ReferenceNode sonnode = CloneTree(son);
                if (sonnode != null)
                    ret.AddDependencyNode(sonnode);
            }
        }
        return ret;
    }

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
                catch (System.Exception exp)
                {
                    Log.WriteError(exp.Message + "|" + exp.StackTrace);
                }
            }

        }
        return ret;
    }

    public static bool GetTopNode(ref List<ReferenceNode> lst, ReferenceNode root)
    {
        if (root.child != null && root.child.Count == 0)
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


}