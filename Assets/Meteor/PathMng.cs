using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    public int parent;//上一层节点.
    public int wayPointIdx;
}

public enum WalkType
{
    Normal = 0, 
    Jump = 1,
}

public class PathMng:Singleton<PathMng>
{
    #region 寻路缓存
    List<int> looked = new List<int>();
    public List<WayPoint> FindPath(Vector3 now, MeteorUnit user, MeteorUnit target)
    {
        int startPathIndex = GetWayIndex(now);
        Vector3 vec = target.mPos - 50 * (user.mPos - target.mPos).normalized;
        int endPathIndex = GetWayIndex(vec);
        looked.Clear();
        List<WayPoint> ret = FindPathCore(startPathIndex, endPathIndex);
        return ret;
    }

    public WalkType GetWalkMethod(int start, int end)
    {
        if (Global.GLevelItem.wayPoint.Count > start && Global.GLevelItem.wayPoint.Count > end)
        {
            if (Global.GLevelItem.wayPoint[start].link.ContainsKey(end))
            {
                return (WalkType)Global.GLevelItem.wayPoint[start].link[end].mode;
            }
            else
                Debug.LogError(string.Format("{0}-{1} can not link", start, end));
        }
        return WalkType.Normal;
    }

    public List<WayPoint> FindPath(int start, int end)
    {
        looked.Clear();
        return FindPathCore(start, end);
    }

    public List<WayPoint> FindShortPath(int start, int end)
    {
        //全路径搜索，得到最短路径
        int kMin = 0;
        List<WayPoint> ret = new List<WayPoint>();
        if (Global.GLevelItem.wayPoint[start].link.ContainsKey(end))
        {
            ret.Add(Global.GLevelItem.wayPoint[start]);
            ret.Add(Global.GLevelItem.wayPoint[end]);
            return ret;
        }

        foreach (var each in Global.GLevelItem.wayPoint[start].link)
        {
            int v = 0;
            List<int> scan = new List<int>();
            List<WayPoint> r = FindPath3(each.Key, end, ref v, ref scan);
            if (v < kMin)
            {
                kMin = v;
                ret = r;
            }
        }
        return ret;
    }

    List<WayPoint> FindPath3(int start, int end, ref int v, ref List<int> scan)
    {
        int kMin = 0;
        if (scan.Contains(start))
        {
            v += 0;
            return null;
        }
        scan.Add(start);
        List<WayPoint> ret = new List<WayPoint>();
        if (Global.GLevelItem.wayPoint[start].link.ContainsKey(end))
        {
            ret.Add(Global.GLevelItem.wayPoint[start]);
            ret.Add(Global.GLevelItem.wayPoint[end]);
            v += 0;
            return ret;
        }
        foreach (var each in Global.GLevelItem.wayPoint[start].link)
        {
            int k = 0;
            List<WayPoint> r = FindPath3(each.Key, end, ref v, ref scan);
            if (k < kMin)
            {
                kMin = k;
                ret = r;
            }
        }
        v += kMin;
        return ret;
    }

    SortedDictionary<int, List<PathNode>> PathInfo = new SortedDictionary<int, List<PathNode>>();
    List<WayPoint> FindPathCore(int start, int end)
    {
        if (looked.Contains(start))
            return null;
        looked.Add(start);
        List<WayPoint> path = new List<WayPoint>();
        if (start == -1 || end == -1)
            return path;
        if (start == end)
        {
            path.Add(Global.GLevelItem.wayPoint[start]);
            return path;
        }
        //从开始点，跑到最终点，最短线路？
        if (Global.GLevelItem.wayPoint[start].link.ContainsKey(end))
        {
            path.Add(Global.GLevelItem.wayPoint[start]);
            path.Add(Global.GLevelItem.wayPoint[end]);
            return path;
        }
        else
        {
            //深度优先递归.并非最短
            //Dictionary<int, WayLength> ways = Global.GLevelItem.wayPoint[start].link;
            //foreach (var each in ways)
            //{
            //    List<WayPoint> p = FindPathCore(each.Key, end);
            //    if (p != null && p.Count != 0)
            //    {
            //        path.Add(Global.GLevelItem.wayPoint[start]);
            //        for (int i = 0; i < p.Count; i++)
            //            path.Add(p[i]);
            //        return path;
            //    }
            //}

            if (true)
            {
                //收集路径信息 层次
                PathInfo.Clear();
                PathNode no = new PathNode();
                no.wayPointIdx = start;
                PathInfo.Add(0, new List<PathNode>() { no });
                CollectPathInfo(start, end);

                foreach (var each in PathInfo)
                {
                    Debug.Log(string.Format("layer:{0}", each.Key));
                    for (int i = 0; i < each.Value.Count; i++)
                        Debug.Log(string.Format("{0}", each.Value[i].wayPointIdx));
                }

                //计算最短路径.从A-B，路径越少，越短，2边之和大于第3边
                int target = end;
                nextfind:
                while (true)
                {
                    foreach (var each in PathInfo)
                    {
                        for (int i = 0; i < each.Value.Count; i++)
                        {
                            if (each.Value[i].wayPointIdx == target)
                            {
                                if (path.Count == 0)
                                    path.Add(Global.GLevelItem.wayPoint[target]);
                                else
                                    path.Insert(0, Global.GLevelItem.wayPoint[target]);
                                while (each.Value[i].parent != start)
                                {
                                    target = each.Value[i].parent;
                                    goto nextfind;
                                }
                                goto exitfind;
                            }
                        }
                    }
                }
                exitfind:path.Insert(0, Global.GLevelItem.wayPoint[start]);
            }
        }
        return path;
    }

    //查看之前层级是否已统计过该节点信息
    bool PathLayerExist(int wayPoint)
    {
        foreach (var each in PathInfo)
        {
            for (int i = 0; i < each.Value.Count; i++)
            {
                if (each.Value[i].wayPointIdx == wayPoint)
                    return true;
            }
        }
        return false;
    }

    void CollectPathInfo(int start, int end, int layer = 1)
    {
        CollectPathLayer(start, end, layer);
        while (PathInfo.ContainsKey(layer))
        {
            int nextLayer = layer + 1;
            for (int i = 0; i < PathInfo[layer].Count; i++)
            {
                CollectPathLayer(PathInfo[layer][i].wayPointIdx, end, nextLayer);
            }
            layer = nextLayer;
        }
    }

    //收集从起点到终点经过的所有层级路点,一旦遇见最近层级的终点就结束，用于计算最短路径.
    void CollectPathLayer(int start, int end, int layer = 1)
    {
        Dictionary<int, WayLength> ways = Global.GLevelItem.wayPoint[start].link;
        foreach (var each in ways)
        {
            //之前的所有层次中并不包含此节点.
            if (!PathLayerExist(each.Key))
            {
                PathNode no = new PathNode();
                no.wayPointIdx = each.Key;
                no.parent = start;
                if (PathInfo.ContainsKey(layer))
                    PathInfo[layer].Add(no);
                else
                    PathInfo.Add(layer, new List<PathNode> { no });
            }
        }
    }

    public int GetWayIndex(Vector3 now)
    {
        int ret = -1;
        float min = 10000.0f;
        for (int i = 0; i < Global.GLevelItem.wayPoint.Count; i++)
        {
            WayPoint way = Global.GLevelItem.wayPoint[i];
            float dis = Vector3.Distance(way.pos, now);
            if (dis <= way.size)
                return i;
            if (dis < min)
            {
                min = dis;
                ret = i;
            }
        }
        return ret;
    }
    #endregion
}
