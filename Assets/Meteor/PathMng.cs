using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathMng:Singleton<PathMng>
{
    #region 寻路缓存
    List<int> looked = new List<int>();
    public List<WayPoint> FindPath(Vector3 now, MeteorUnit user, MeteorUnit target, out int freeSlot, out Vector3 end)
    {
        int p0 = GetWayIndex(now);
        Vector3 vec = target.GetFreePos(out freeSlot, user);
        end = vec;
        int p1 = GetWayIndex(vec);
        looked.Clear();

        List<WayPoint> ret = FindPath2(p0, p1);
        return ret;
    }

    public List<WayPoint> FindPath(int start, int end)
    {
        looked.Clear();
        return FindPath2(start, end);
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

    List<KeyValuePair<WayPoint, float>> S = new List<KeyValuePair<WayPoint, float>>();
    List<KeyValuePair<WayPoint, float>> U = new List<KeyValuePair<WayPoint, float>>();

    List<WayPoint> FindPath2(int start, int end)
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
            //深度优先递归.
            Dictionary<int, WayLength> ways = Global.GLevelItem.wayPoint[start].link;
            foreach (var each in ways)
            {
                List<WayPoint> p = FindPath2(each.Key, end);
                if (p != null && p.Count != 0)
                {
                    path.Add(Global.GLevelItem.wayPoint[start]);
                    for (int i = 0; i < p.Count; i++)
                        path.Add(p[i]);
                    return path;
                }
            }
        }
        return path;
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
