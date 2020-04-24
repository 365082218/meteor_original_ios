using Idevgame.Meteor.AI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class PathContext
{
    public PathContext()
    {
    }

    //存储单次寻路的上下文
    public List<int> looked = new List<int>();//已经查看过的路线
}

public class PathPameter
{
    public StateMachine stateMachine;//使用者 AI/系统
    public Vector3 start;//寻路起始点
    public Vector3 end;//结束点
    public List<WayPoint> ways = new List<WayPoint>();//寻路结果路径
    public PathContext context = new PathContext();//寻路上下文.如果多线程，就需要，单线程结构不好.
}

public class PathHelper:Singleton<PathHelper>
{
    PathNode[] nodeContainer;//路點.
    PathHelper()
    {
        
    }
    bool quit = false;
    AutoResetEvent waitEvent = new AutoResetEvent(false);
    List<PathPameter> pathQueue = new List<PathPameter>();
    public void CalcPath(StateMachine machine, Vector3 start, Vector3 end)
    {
        PathPameter pameter = new PathPameter();
        pameter.stateMachine = machine;
        pameter.start = start;
        pameter.end = end;
        lock (pathQueue)
        {
            pathQueue.Add(pameter);
            waitEvent.Set();
        }
    }

    //取消某个请求
    public void CancelCalc(StateMachine machine)
    {

    }

    //只有在开启寻路的场景，即有寻路点的，才创建这些.
    public void OnBattleStart()
    {
        //int count = Main.Ins.CombatData.wayPoints.Count;
        //nodeContainer = new PathNode[count];
        //for (int i = 0; i < count; i++)
        //    nodeContainer[i] = new PathNode();
        this.quit = false;
        this.StartCalc();
    }

    System.Threading.Thread CalcThread;
    public void StartCalc()
    {
        if (CalcThread != null)
        {
            UnityEngine.Debug.Log("计算线程还未停止");
            return;
        }
        CalcThread = new System.Threading.Thread(new System.Threading.ThreadStart(this.CalcCore));
        CalcThread.Start();
        UnityEngine.Debug.Log("创建寻路线程");
    }

    public void StopCalc()
    {
        this.quit = true;
        if (CalcThread != null)
        {
            waitEvent.Set();
        }
    }

    void CalcCore()
    {
        while (true)
        {
            waitEvent.WaitOne();
            lock (pathQueue)
            {
                while (pathQueue.Count != 0)
                {
                    PathPameter pameter = pathQueue[0];
                    pathQueue.RemoveAt(0);
                    PathMng.Ins.FindPath(pameter.context, pameter.start, pameter.end, pameter.ways);
                    LocalMsg msg = new LocalMsg();
                    msg.Message = (int)LocalMsgType.PathCalcFinished;
                    msg.context = pameter;
                    ProtoHandler.PostMessage(msg);
                }
            }
        }
    }

}

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
    public void FindPath(PathContext context, Vector3 source, Vector3 target, List<WayPoint> waypoint)
    {
        int startPathIndex = GetWayIndex(source);
        //Debug.LogError("起始点:" + startPathIndex);
        int endPathIndex = GetWayIndex(target);
        //Debug.LogError("终点:" + endPathIndex);
        if (startPathIndex == -1 || endPathIndex == -1)
        {
            Debug.LogError("无法得到该位置所属路点");
            return;
        }
        context.looked.Clear();
        waypoint.Clear();
        FindPathCore(context, startPathIndex, endPathIndex, waypoint);
    }

    public WalkType GetWalkMethod(int start, int end)
    {
        if (Main.Ins.CombatData.wayPoints.Count > start && Main.Ins.CombatData.wayPoints.Count > end)
        {
            if (Main.Ins.CombatData.wayPoints[start].link.ContainsKey(end))
            {
                return (WalkType)Main.Ins.CombatData.wayPoints[start].link[end].mode;
            }
        }
        return WalkType.Normal;
    }

    public void FindPath(PathContext context, int start, int end, List<WayPoint> wp)
    {
        context.looked.Clear();
        wp.Clear();
        FindPathCore(context, start, end, wp);
    }

    public void FindPathCore(PathContext context, int start, int end, List<WayPoint> wp)
    {
        if (context.looked.Contains(start))
        {
            //user.Robot.FindWayFinished = true;
            return;
        }
        //Debug.Log(string.Format("start:{0}:end:{1}", start, end));
        context.looked.Add(start);
        if (start == -1 || end == -1)
        {
            //user.Robot.FindWayFinished = true;
            return;
        }
        if (start == end)
        {
            //wp.Add(Global.Instance.GLevelItem.wayPoint[start]);
            //user.Robot.FindWayFinished = true;
            return;
        }

        if (Main.Ins.CombatData.GScript.DisableFindWay())
        {
            //List<WayPoint> direct = new List<WayPoint>();
            //wp.Add(Global.Instance.GLevelItem.wayPoint[start]);
            //wp.Add(Global.Instance.GLevelItem.wayPoint[end]);
            //user.Robot.FindWayFinished = true;
            return;
        }

        //从开始点，跑到最终点，最短线路？
        if (Main.Ins.CombatData.wayPoints[start].link.ContainsKey(end))
        {
            //wp.Add(Global.Instance.GLevelItem.wayPoint[start]);
            //wp.Add(Global.Instance.GLevelItem.wayPoint[end]);
            //user.Robot.FindWayFinished = true;
            return;
        }
        else
        {
            //收集路径信息 层次
            //user.Robot.PathReset();
            //PathNode no = user.Robot.nodeContainer[start];
            //no.wayPointIdx = start;
            //no.parent = -1;
            //user.Robot.PathInfo[0].Add(no);
            //CollectPathInfo(user, start, end);
            //int scan = 0;
            //foreach (var each in user.Robot.PathInfo)
            //{
            //    scan += each.Value.Count;
            //}
            //Debug.LogError("层信息:" + user.Robot.PathInfo.Count + " 总结点:" + scan);
            //计算最短路径.
            //int target = end;
            //bool find = false;
            //foreach (var each in user.Robot.PathInfo)
            //{
            //    for (int i = 0; i < each.Value.Count; i++)
            //    {
            //        if (each.Value[i].wayPointIdx == end)
            //        {
            //            find = true;
            //            //Debug.LogError("找到目标点:" + end);
            //            if (wp.Count == 0)
            //                wp.Add(Global.Instance.GLevelItem.wayPoint[target]);
            //            else
            //                wp.Insert(0, Global.Instance.GLevelItem.wayPoint[target]);
            //            PathNode p = each.Value[i];
            //            while (p.parent != start)
            //            {
            //                target = p.parent;
            //                p = user.Robot.nodeContainer[target];
            //                if (wp.Count == 0)
            //                    wp.Add(Global.Instance.GLevelItem.wayPoint[target]);
            //                else
            //                    wp.Insert(0, Global.Instance.GLevelItem.wayPoint[target]);
            //                //Debug.LogError("找到父节点:" +  target);
            //                if (wp.Count >= 100)
            //                {
            //                    Debug.LogError("寻路链路超过100！！！");
            //                    break;
            //                }
            //                //if (p.parent == start)
            //                //    Debug.LogError("找到起点:" + start);
            //            }
            //            break;
            //        }                       
            //    }
            //    //yield return 0;
            //    if (find)
            //        break;
            //}

            //if (!find)
            //{
            //    Debug.LogError(string.Format("孤立的寻路点:{0},没有点可以走向目标", target));
            //    if (wp.Count == 0)
            //        wp.Add(Global.Instance.GLevelItem.wayPoint[target]);
            //    else
            //        wp.Insert(0, Global.Instance.GLevelItem.wayPoint[target]);
            //}
            //wp.Insert(0, Global.Instance.GLevelItem.wayPoint[start]);
        }
        //user.Robot.FindWayFinished = true;
    }

    //查看之前层级是否已统计过该节点信息
    bool PathLayerExist(MeteorUnit user, int wayPoint)
    {
        //foreach (var each in user.Robot.PathInfo)
        //{
        //    for (int i = 0; i < each.Value.Count; i++)
        //    {
        //        if (each.Value[i].wayPointIdx == wayPoint)
        //            return true;
        //    }
        //}
        return false;
    }

    //从起点开始 构造寻路树.
    void CollectPathInfo(MeteorUnit user, int start, int end, int layer = 1)
    {
        CollectPathLayer(user, start, end, layer);
        if (PathLayerExist(user, end))
            return;
        //while (user.Robot.PathInfo.ContainsKey(layer))
        //{
        //    int nextLayer = layer + 1;
        //    for (int i = 0; i < user.Robot.PathInfo[layer].Count; i++)
        //    {
        //        CollectPathLayer(user, user.Robot.PathInfo[layer][i].wayPointIdx, end, nextLayer);
        //        if (PathLayerExist(user, end))
        //            return;
        //    }
        //    layer = nextLayer;
        //}
    }

    //收集从起点到终点经过的所有层级路点,一旦遇见最近层级的终点就结束，用于计算最短路径.
    void CollectPathLayer(MeteorUnit user, int start, int end, int layer = 1)
    {
        Dictionary<int, WayLength> ways = Main.Ins.CombatData.wayPoints[start].link;
        //foreach (var each in ways)
        //{
        //    if (!PathLayerExist(user, each.Key))
        //    {
        //        //之前的所有层次中并不包含此节点.
        //        PathNode no = user.Robot.nodeContainer[each.Key];
        //        no.wayPointIdx = each.Key;
        //        no.parent = start;
        //        if (user.Robot.PathInfo.ContainsKey(layer))
        //            user.Robot.PathInfo[layer].Add(no);
        //        else
        //            user.Robot.PathInfo.Add(layer, new List<PathNode> { no });
        //    }
        //}
    }

    //得到当前位置所处路点临近的路点其中之一
    public Vector3 GetNearestWayPoint(Vector3 vec)
    {
        int start = GetWayIndex(vec);
        if (Main.Ins.CombatData.wayPoints.Count > start && start >= 0)
        {
            if (Main.Ins.CombatData.wayPoints[start].link != null)
            {
                List<int> ret = Main.Ins.CombatData.wayPoints[start].link.Keys.ToList();
                int k = Random.Range(0, ret.Count);
                if (ret.Count != 0)
                    return Main.Ins.CombatData.wayPoints[ret[k]].pos;
            }
        }
        return Vector3.zero;
    }

    //这个不能仅判断距离，还要判断射线是否撞到墙壁.
    List<WayPoint> CandidateList = new List<WayPoint>();
    List<float> CandiateDistance = new List<float>();//距离排序
    public int GetWayIndex(Vector3 now)
    {
        CandidateList.Clear();
        CandiateDistance.Clear();
        int ret = -1;

        Collider[] other = Physics.OverlapSphere(now, 500, 1 << LayerMask.NameToLayer("WayPoint"));
        for (int i = 0; i < other.Length; i++)
        {
            WayPointTrigger wayPointTrigger = other[i].gameObject.GetComponent<WayPointTrigger>();
            wayPointTrigger.OverlapSphereIndex = i;
            WayPoint way = Main.Ins.CombatData.wayPoints[wayPointTrigger.WayIndex];
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

        if (CandidateList.Count != 0)
        {
            ret = CandidateList[0].index;
            return ret;
        }
        else
        {
            UnityEngine.Debug.LogError("找不到对应的路点");
            return -1;
        }
    }
}
