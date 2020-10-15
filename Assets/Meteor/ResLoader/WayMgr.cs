using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayMng : MonoBehaviour {
    public static WayMng Instance { get { return _Instance; } }
    static WayMng _Instance;
    private void Awake()
    {
        _Instance = this;
        wayPoints.Clear();
        WayPoints [] points = GetComponentsInChildren<WayPoints>(true);
        for (int i = 0; i < points.Length; i++)
        {
            WayPoint wp = new WayPoint();
            wp.pos = points[i].transform.position;
            wp.size = 10;
            wp.link = new SortedDictionary<int, WayLength>();
            wp.index = int.Parse(points[i].name);
            for (int j = 0; j < points[i].Link.Length; j++)
            {
                int jIndex = int.Parse(points[i].Link[j].name);
                WayLength wayl = new WayLength();
                wayl.length = Vector3.Distance(points[i].Link[j].transform.position, points[i].transform.position);
                wayl.mode = 0;
                wp.link.Add(jIndex, wayl);
            }
            wayPoints.Add(wp);
        }
    }
    private void OnDestroy()
    {
        _Instance = null;
    }
    public List<WayPoint> wayPoints = new List<WayPoint>();
}
