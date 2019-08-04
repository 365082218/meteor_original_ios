using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public IEnumerator FindPath(MeteorUnit user, Vector3 source, Vector3 target, List<WayPoint> wp)
    {
        int startPathIndex = GetWayIndex(source, user);
        //Debug.LogError("起始点:" + startPathIndex);
        int endPathIndex = GetWayIndex(target, null);
        //Debug.LogError("终点:" + endPathIndex);
        if (startPathIndex == -1 || endPathIndex == -1)
        {
            Debug.LogError("无法得到该位置所属路点");
            yield break;
        }
        looked.Clear();
        wp.Clear();
        yield return FindPathCore(user, startPathIndex, endPathIndex, wp);
    }

    public IEnumerator FindPathEx(MeteorUnit user, MeteorUnit target, List<WayPoint> wp)
    {
        int startPathIndex = GetWayIndex(user.transform.position, user);
        //Debug.LogError("起始点:" + startPathIndex);
        int endPathIndex = GetWayIndex(target.transform.position, target);
        //Debug.LogError("终点:" + endPathIndex);
        if (startPathIndex == -1 || endPathIndex == -1)
        {
            //Debug.LogError("无法得到该位置所属路点");
            yield break;
        }
        looked.Clear();
        wp.Clear();
        yield return FindPathCore(user, startPathIndex, endPathIndex, wp);
    }

    public WalkType GetWalkMethod(int start, int end)
    {
        if (Global.Instance.GLevelItem.wayPoint.Count > start && Global.Instance.GLevelItem.wayPoint.Count > end)
        {
            if (Global.Instance.GLevelItem.wayPoint[start].link.ContainsKey(end))
            {
                return (WalkType)Global.Instance.GLevelItem.wayPoint[start].link[end].mode;
            }
        }
        return WalkType.Normal;
    }

    public IEnumerator FindPath(MeteorUnit user, int start, int end, List<WayPoint> wp)
    {
        looked.Clear();
        wp.Clear();
        yield return FindPathCore(user, start, end, wp);
    }

    IEnumerator FindPathCore(MeteorUnit user, int start, int end, List<WayPoint> wp)
    {
        if (looked.Contains(start))
        {
            user.Robot.FindWayFinished = true;
            yield break;
        }
        //Debug.Log(string.Format("start:{0}:end:{1}", start, end));
        looked.Add(start);
        if (start == -1 || end == -1)
        {
            user.Robot.FindWayFinished = true;
            yield break;
        }
        if (start == end)
        {
            wp.Add(Global.Instance.GLevelItem.wayPoint[start]);
            user.Robot.FindWayFinished = true;
            yield break;
        }

        if (Global.Instance.GScript.DisableFindWay())
        {
            //List<WayPoint> direct = new List<WayPoint>();
            wp.Add(Global.Instance.GLevelItem.wayPoint[start]);
            wp.Add(Global.Instance.GLevelItem.wayPoint[end]);
            user.Robot.FindWayFinished = true;
            yield break;
        }

        //从开始点，跑到最终点，最短线路？
        if (Global.Instance.GLevelItem.wayPoint[start].link.ContainsKey(end))
        {
            wp.Add(Global.Instance.GLevelItem.wayPoint[start]);
            wp.Add(Global.Instance.GLevelItem.wayPoint[end]);
            user.Robot.FindWayFinished = true;
            yield break;
        }
        else
        {
            //收集路径信息 层次
            user.Robot.PathReset();
            PathNode no = user.Robot.nodeContainer[start];
            no.wayPointIdx = start;
            no.parent = -1;
            user.Robot.PathInfo[0].Add(no);
            yield return CollectPathInfo(user, start, end);
            //int scan = 0;
            //foreach (var each in user.Robot.PathInfo)
            //{
            //    scan += each.Value.Count;
            //}
            //Debug.LogError("层信息:" + user.Robot.PathInfo.Count + " 总结点:" + scan);
            //计算最短路径.
            int target = end;
            bool find = false;
            foreach (var each in user.Robot.PathInfo)
            {
                for (int i = 0; i < each.Value.Count; i++)
                {
                    if (each.Value[i].wayPointIdx == end)
                    {
                        find = true;
                        //Debug.LogError("找到目标点:" + end);
                        if (wp.Count == 0)
                            wp.Add(Global.Instance.GLevelItem.wayPoint[target]);
                        else
                            wp.Insert(0, Global.Instance.GLevelItem.wayPoint[target]);
                        PathNode p = each.Value[i];
                        while (p.parent != start)
                        {
                            target = p.parent;
                            p = user.Robot.nodeContainer[target];
                            if (wp.Count == 0)
                                wp.Add(Global.Instance.GLevelItem.wayPoint[target]);
                            else
                                wp.Insert(0, Global.Instance.GLevelItem.wayPoint[target]);
                            //Debug.LogError("找到父节点:" +  target);
                            if (wp.Count >= 100)
                            {
                                Debug.LogError("寻路链路超过100！！！");
                                break;
                            }
                            //if (p.parent == start)
                            //    Debug.LogError("找到起点:" + start);
                        }
                        break;
                    }                       
                }
                //yield return 0;
                if (find)
                    break;
            }

            if (!find)
            {
                Debug.LogError(string.Format("孤立的寻路点:{0},没有点可以走向目标", target));
                if (wp.Count == 0)
                    wp.Add(Global.Instance.GLevelItem.wayPoint[target]);
                else
                    wp.Insert(0, Global.Instance.GLevelItem.wayPoint[target]);
            }
            wp.Insert(0, Global.Instance.GLevelItem.wayPoint[start]);
        }
        user.Robot.FindWayFinished = true;
    }

    //查看之前层级是否已统计过该节点信息
    bool PathLayerExist(MeteorUnit user, int wayPoint)
    {
        foreach (var each in user.Robot.PathInfo)
        {
            for (int i = 0; i < each.Value.Count; i++)
            {
                if (each.Value[i].wayPointIdx == wayPoint)
                    return true;
            }
        }
        return false;
    }

    //从起点开始 构造寻路树.
    IEnumerator CollectPathInfo(MeteorUnit user, int start, int end, int layer = 1)
    {
        yield return CollectPathLayer(user, start, end, layer);
        if (PathLayerExist(user, end))
            yield break;
        while (user.Robot.PathInfo.ContainsKey(layer))
        {
            int nextLayer = layer + 1;
            for (int i = 0; i < user.Robot.PathInfo[layer].Count; i++)
            {
                yield return CollectPathLayer(user, user.Robot.PathInfo[layer][i].wayPointIdx, end, nextLayer);
                if (PathLayerExist(user, end))
                    yield break;
            }
            layer = nextLayer;
        }
    }

    //收集从起点到终点经过的所有层级路点,一旦遇见最近层级的终点就结束，用于计算最短路径.
    IEnumerator CollectPathLayer(MeteorUnit user, int start, int end, int layer = 1)
    {
        Dictionary<int, WayLength> ways = Global.Instance.GLevelItem.wayPoint[start].link;
        foreach (var each in ways)
        {
            if (!PathLayerExist(user, each.Key))
            {
                //之前的所有层次中并不包含此节点.
                PathNode no = user.Robot.nodeContainer[each.Key];
                no.wayPointIdx = each.Key;
                no.parent = start;
                if (user.Robot.PathInfo.ContainsKey(layer))
                    user.Robot.PathInfo[layer].Add(no);
                else
                    user.Robot.PathInfo.Add(layer, new List<PathNode> { no });
            }
        }
        yield return 0;
    }

    //得到当前位置所处路点临近的路点其中之一
    public Vector3 GetNearestWayPoint(Vector3 vec, MeteorUnit target)
    {
        int start = GetWayIndex(vec, target);
        if (Global.Instance.GLevelItem.wayPoint.Count > start && start >= 0)
        {
            if (Global.Instance.GLevelItem.wayPoint[start].link != null)
            {
                List<int> ret = Global.Instance.GLevelItem.wayPoint[start].link.Keys.ToList();
                int k = Random.Range(0, ret.Count);
                if (ret.Count != 0)
                    return Global.Instance.GLevelItem.wayPoint[ret[k]].pos;
            }
        }
        return Vector3.zero;
    }

    //这个不能仅判断距离，还要判断射线是否撞到墙壁.
    List<WayPoint> CandidateList = new List<WayPoint>();
    List<float> CandiateDistance = new List<float>();//距离排序
    public int GetWayIndex(Vector3 now, MeteorUnit user)
    {
        CandidateList.Clear();
        CandiateDistance.Clear();
        int ret = -1;

        Collider[] other = Physics.OverlapSphere(now, 500, 1 << LayerMask.NameToLayer("WayPoint"));
        for (int i = 0; i < other.Length; i++)
        {
            WayPointTrigger wayPointTrigger = other[i].gameObject.GetComponent<WayPointTrigger>();
            wayPointTrigger.OverlapSphereIndex = i;
            WayPoint way = Global.Instance.GLevelItem.wayPoint[wayPointTrigger.WayIndex];
            Vector3 vecTarget = way.pos;
            Vector3 diff = vecTarget - now;
            float dis = Vector3.SqrMagnitude(diff);
            ret = wayPointTrigger.WayIndex;
            int k = 0;
            for (int j = 0; j < CandiateDistance.Count; j++)
            {
                k = j;
                if (CandiateDistance[j] < dis)
                    continue;   
                break;
            }
            CandiateDistance.Insert(k, dis);
            CandidateList.Insert(k, way);
        }

        //找到最近的一个未被场景遮挡的.
        if (user != null)
        {
            //玩家的所处于的位置.要看这个玩家和该路点间有无阻碍，有则不是符合的路点.
            for (int i = 0; i < CandidateList.Count; i++)
            {
                if (user.PassThrough(CandidateList[i].pos))
                {
                    ret = CandidateList[i].index;
                    break;
                }
            }
            return ret;
        }
        else
        {
            if (CandidateList.Count > 0)
                return CandidateList[0].index;
            Debug.LogError("无法找到指定位置所处的路点???");
            return -1;
        }
    }
    #endregion
}
