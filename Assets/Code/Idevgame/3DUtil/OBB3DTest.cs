using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OBB3DTest : MonoBehaviour {
    public OBB3DBehaviour a;
    public OBB3DBehaviour b;
    //把2个组件拖到想要测试的碰撞盒上，然后计算即可

    void Update() {
        var isIntersects = a.Intersects(b);
        if (isIntersects) {
            a.gizmosColor = Color.red;
            b.gizmosColor = Color.red;
        } else {
            a.gizmosColor = Color.white;
            b.gizmosColor = Color.white;
        }
    }
}
