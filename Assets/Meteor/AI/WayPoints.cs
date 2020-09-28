using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum TransitionMode {
    Run = 0,
    Jump,
}
public class WayPoints : MonoBehaviour {
    //加载,编辑,保存路点信息
    public int WayIndex;//序号
    public Transform[] Link;//可以行走的对象
    public TransitionMode[]TransitionMode;//移动方式
}
