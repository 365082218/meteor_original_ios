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
        int count = CombatData.Ins.wayPoints.Count;
        Container = new PathNode[count];
        for (int i = 0; i < count; i++)
            Container[i] = new PathNode();
    }

    //存储单次寻路的上下文
    public List<int> looked = new List<int>();//已经查看过的路线
    public SortedDictionary<int, List<PathNode>> PathInfo = new SortedDictionary<int, List<PathNode>>();//每一层的路点，一直到终点那一层
    public PathNode[] Container;
    public void ResetPath()
    {
        foreach (var each in PathInfo)
        {
            each.Value.Clear();
        }
        PathInfo.Clear();
        PathInfo.Add(0, new List<PathNode>());
        for (int i = 0; i < Container.Length; i++)
        {
            Container[i].wayPointIdx = 0;
            Container[i].parent = 0;
        }
    }
}


public class PathPameter
{
    public PathPameter(StateMachine machine) {
        stateMachine = machine;
        state = -1;
    }

    public int state;//查询处于什么阶段，-1，无效，0，查询中，1，有结果
    public StateMachine stateMachine;//使用者 AI/系统
    public int startIndex;
    public int endIndex;
    public List<WayPoint> ways = new List<WayPoint>();//寻路结果路径
    public PathContext context = new PathContext();//寻路上下文.如果多线程，就需要，单线程结构不好.
}

public class PathHelper:Singleton<PathHelper>
{
    PathNode[] nodeContainer;//路點.
    public PathHelper()
    {
        
    }
    bool quit = false;
    AutoResetEvent waitEvent = new AutoResetEvent(false);
    List<PathPameter> pathQueue = new List<PathPameter>();
    public void CalcPath(StateMachine machine, Vector3 start, Vector3 end)
    {
        machine.Path.state = 0;
        machine.Path.startIndex = PathMng.Ins.GetWayIndex(start);
        machine.Path.endIndex = PathMng.Ins.GetWayIndex(end);
        //找不到路点
        if (machine.Path.startIndex == -1 || machine.Path.endIndex == -1) {
            machine.Path.state = 1;
            machine.Path.ways.Clear();
            return;
        }

        //路点本身相连
        if (CombatData.Ins.wayPoints[machine.Path.startIndex].link.ContainsKey(machine.Path.endIndex)) {
            machine.Path.state = 1;
            machine.Path.ways.Clear();
            return;
        }

        //路点间接相连
        lock (pathQueue)
        {
            pathQueue.Add(machine.Path);
            waitEvent.Set();
        }
    }

    public void CalcPath(StateMachine machine, Vector3 start, int end) {
        machine.Path.state = 0;
        machine.Path.startIndex = PathMng.Ins.GetWayIndex(start);
        machine.Path.endIndex = end;

        //找不到路点
        if (machine.Path.startIndex == -1 || machine.Path.endIndex == -1) {
            machine.Path.state = 1;
            machine.Path.ways.Clear();
            return;
        }

        //路点本身相连
        if (CombatData.Ins.wayPoints[machine.Path.startIndex].link.ContainsKey(machine.Path.endIndex)) {
            machine.Path.state = 1;
            machine.Path.ways.Clear();
            return;
        }

        //路点间接相连
        lock (pathQueue) {
            pathQueue.Add(machine.Path);
            waitEvent.Set();
        }
    }

    System.Threading.Thread CalcThread;
    public void StartCalc()
    {
        this.quit = false;
        if (CalcThread != null)
        {
            CalcThread.Abort();
            UnityEngine.Debug.Log("计算线程还未停止");
            return;
        }
        CalcThread = new System.Threading.Thread(new System.Threading.ThreadStart(this.CalcCore));
        CalcThread.Start();
        //UnityEngine.Debug.Log("创建寻路线程");
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

            if (quit) {
                break;
            }

            lock (pathQueue)
            {
                while (pathQueue.Count != 0)
                {
                    if (quit) {
                        break;
                    }

                    PathPameter pameter = pathQueue[0];
                    pathQueue.RemoveAt(0);
                    PathMng.Ins.FindPath(pameter.context, pameter.startIndex, pameter.endIndex, pameter.ways);
                    pameter.state = 1;
                }
            }
        }

        lock (pathQueue) {
            pathQueue.Clear();
        }
        
        CalcThread = null;
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
    //通过起始坐标，结束坐标，得到一条通路.
    //public void FindPath(PathContext context, Vector3 source, Vector3 target, List<WayPoint> waypoint)
    //{
    //    int startPathIndex = GetWayIndex(source);
    //    int endPathIndex = GetWayIndex(target);
    //    if (startPathIndex == -1 || endPathIndex == -1)
    //    {
    //        Debug.LogError("无法得到正确线路");
    //        return;
    //    }
    //    context.looked.Clear();
    //    waypoint.Clear();
    //    FindPathCore(context, startPathIndex, endPathIndex, waypoint);
    //}

    public WalkType GetWalkMethod(int start, int end)
    {
        if (CombatData.Ins.wayPoints.Count > start && CombatData.Ins.wayPoints.Count > end)
        {
            if (CombatData.Ins.wayPoints[start].link.ContainsKey(end))
            {
                return (WalkType)CombatData.Ins.wayPoints[start].link[end].mode;
            }
        }
        return WalkType.Normal;
    }

    //public void FindPath(PathContext context, Vector3 start, int end, List<WayPoint> waypoint) {
    //    int startPathIndex = GetWayIndex(start);
    //    if (startPathIndex == -1 || end == -1) {
    //        Debug.LogError("无法得到正确线路");
    //        return;
    //    }
    //    context.looked.Clear();
    //    waypoint.Clear();
    //    FindPathCore(context, startPathIndex, end, waypoint);
    //}

    //通过起始路点，结束路点，得到一条通路
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
            return;
        }

        context.looked.Add(start);
        if (start == -1 || end == -1)
            return;

        //路点相同,可直接走向终点
        if (start == end)
            return;

        //收集路径信息 层次
        context.ResetPath();
        PathNode no = context.Container[start];
        no.wayPointIdx = start;
        no.parent = -1;
        context.PathInfo[0].Add(no);
        CollectPathInfo(context, start, end);
        int scan = 0;
        foreach (var each in context.PathInfo)
        {
            scan += each.Value.Count;
        }
        //Debug.LogError("层信息:" + context.PathInfo.Count + " 总结点:" + scan);
        //计算最短路径.
        int target = end;
        bool find = false;
        foreach (var each in context.PathInfo)
        {
            for (int i = 0; i < each.Value.Count; i++)
            {
                if (each.Value[i].wayPointIdx == end)
                {
                    find = true;
                    //Debug.LogError("找到目标点:" + end);
                    if (wp.Count == 0)
                        wp.Add(CombatData.Ins.wayPoints[target]);
                    else
                        wp.Insert(0, CombatData.Ins.wayPoints[target]);
                    PathNode p = each.Value[i];
                    while (p.parent != start)
                    {
                        target = p.parent;
                        p = context.Container[target];
                        if (wp.Count == 0)
                            wp.Add(CombatData.Ins.wayPoints[target]);
                        else
                            wp.Insert(0, CombatData.Ins.wayPoints[target]);
                        if (wp.Count >= 100)
                        {
                            Debug.LogError("寻路链路超过100！！！");
                            break;
                        }
                    }
                    break;
                }
            }
            if (find)
                break;
        }

        if (!find)
        {
            Debug.LogError(string.Format("孤立的寻路点:从{0}到{1},没有路径通向目标", start, target));
            if (wp.Count == 0)
                wp.Add(CombatData.Ins.wayPoints[target]);
            else
                wp.Insert(0, CombatData.Ins.wayPoints[target]);
        }
        wp.Insert(0, CombatData.Ins.wayPoints[start]);
    }

    //查看之前层级是否已统计过该节点信息
    bool PathLayerExist(PathContext context, int wayPoint)
    {
        foreach (var each in context.PathInfo)
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
    void CollectPathInfo(PathContext context, int start, int end, int layer = 1)
    {
        CollectPathLayer(context, start, end, layer);
        if (PathLayerExist(context, end))
            return;
        while (context.PathInfo.ContainsKey(layer))
        {
            int nextLayer = layer + 1;
            for (int i = 0; i < context.PathInfo[layer].Count; i++)
            {
                CollectPathLayer(context, context.PathInfo[layer][i].wayPointIdx, end, nextLayer);
                if (PathLayerExist(context, end))
                    return;
            }
            layer = nextLayer;
        }
    }

    //收集从起点到终点经过的所有层级路点,一旦遇见最近层级的终点就结束，用于计算最短路径.
    void CollectPathLayer(PathContext context, int start, int end, int layer = 1)
    {
        SortedDictionary<int, WayLength> ways = CombatData.Ins.wayPoints[start].link;
        foreach (var each in ways)
        {
            if (!PathLayerExist(context, each.Key))
            {
                //之前的所有层次中并不包含此节点.
                PathNode no = context.Container[each.Key];
                no.wayPointIdx = each.Key;
                no.parent = start;
                if (context.PathInfo.ContainsKey(layer))
                    context.PathInfo[layer].Add(no);
                else
                    context.PathInfo.Add(layer, new List<PathNode> { no });
            }
        }
    }

    //得到某个点的相邻路点-视为躲避路点.随机
    public int GetDodgeWayPoint(Vector3 vec) {
        int start = GetWayIndex(vec);
        if (CombatData.Ins.wayPoints.Count > start && start >= 0) {
            if (CombatData.Ins.wayPoints[start].link != null) {
                List<int> ret = CombatData.Ins.wayPoints[start].link.Keys.ToList();
                int k = Utility.Range(0, ret.Count);
                if (ret.Count != 0)
                    return CombatData.Ins.wayPoints[ret[k]].index;
            }
        }
        //要么是一个单向路点，无法找到相邻节点，要么是所处位置已经混乱.
        return -1;
    }

    //得到当前位置所处路点临近的路点其中之一
    public Vector3 GetNearestWayPoint(Vector3 vec)
    {
        int start = GetWayIndex(vec);
        if (CombatData.Ins.wayPoints.Count > start && start >= 0)
        {
            if (CombatData.Ins.wayPoints[start].link != null)
            {
                List<int> ret = CombatData.Ins.wayPoints[start].link.Keys.ToList();
                int k = Utility.Range(0, ret.Count);
                if (ret.Count != 0)
                    return CombatData.Ins.wayPoints[ret[k]].pos;
            }
        }
        return Vector3.zero;
    }

    //一定要在主线程调用
    public int GetWayIndex(Vector3 now)
    {
        int ret = -1;
        float min = int.MaxValue;
        float radis = 250;
        for (int i = 1; i <= 10; i++) {
            float r = i * radis;
            Vector3 v = now;
            if (Physics.CheckSphere(v, r, LayerManager.WayPointMask)) {
                Collider[] waypoints = Physics.OverlapSphere(v, r, LayerManager.WayPointMask);
                for (int j = 0; j < waypoints.Length; j++) {
                    Vector3 vecTarget = waypoints[j].transform.position;
                    Vector3 vecSource = now;
                    RaycastHit hit;
                    float d = 10;
                    if (Physics.SphereCast(v, d, (vecTarget - vecSource).normalized, out hit, Vector3.Distance(vecTarget, vecSource), LayerManager.AllSceneMask))
                        continue;
                    //检查射线之间是否有阻隔，若有，则忽略掉这个.
                    float dis = Vector3.SqrMagnitude(vecTarget - vecSource);
                    if (dis < min) {
                        min = dis;
                        ret = j;
                    }
                }
                if (ret != -1) {
                    WayPoints wp = waypoints[ret].gameObject.GetComponent<WayPoints>();
                    return wp.WayIndex;
                }
            }
        }
        return -1;
    }
}
