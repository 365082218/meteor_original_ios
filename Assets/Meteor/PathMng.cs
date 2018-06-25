using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    public PathNode parent;
    public PathNode child;
    public int wayPointIdx;
}

public class PathMng:Singleton<PathMng>
{
    #region 寻路缓存
    List<int> looked = new List<int>();
    public List<WayPoint> FindPath(Vector3 now, MeteorUnit user, MeteorUnit target, out int freeSlot, out Vector3 end)
    {
        int startPathIndex = GetWayIndex(now);
        Vector3 vec = target.GetFreePos(out freeSlot, user);
        end = vec;
        int endPathIndex = GetWayIndex(vec);
        looked.Clear();

        List<WayPoint> ret = FindPathCore(startPathIndex, endPathIndex);
        return ret;
    }

    public List<WayPoint> FindPath(MeteorUnit unit, int end)
    {
        int startPathIndex = GetWayIndex(unit.mPos);
        return FindPath(startPathIndex, end);
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
            //收集路径信息 层次
            PathInfo.Clear();
            CollectPathLayer(start, end);
            //计算最短路径.
            Dictionary<int, WayLength> ways = Global.GLevelItem.wayPoint[start].link;

        }
        return path;
    }

    //收集从起点到终点经过的所有层级路点,一旦遇见最低层级的终点就结束，用于计算最短路径.
    void CollectPathLayer(int start, int end, int layer = 0)
    {
        Dictionary<int, WayLength> ways = Global.GLevelItem.wayPoint[start].link;
        foreach (var each in ways)
        {
            PathNode no = new PathNode();
            no.wayPointIdx = start;
            no.parent = null;
            no.child = null;
        }
    }

    int GetWayIndex(Vector3 now)
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
