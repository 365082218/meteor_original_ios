using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPoints : MonoBehaviour {
    //加载,编辑,保存路点信息
    public List<WayPoints> NextWayPoints;//可通行路点
    public int WayIndex;//序号
    public float Size;//尺寸-决定了离此路点多近认为到达了此路点.
}
