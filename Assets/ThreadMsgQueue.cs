using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//启动后常驻游戏对象，解决跨线程的调用
//以及网络线程接收的消息
public class ThreadMsgQueue : MonoBehaviour {
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        ProtoHandler.Update();
	}
}
