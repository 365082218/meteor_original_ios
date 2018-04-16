using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

//代理OC平台上的调用
public class OCAgent : MonoBehaviour {
	[DllImport("__Internal")]
	private static extern void appstart();
	// Use this for initialization
	void Start () {
        if (GameObject.FindObjectsOfType<OCAgent>().Length != 1)
        {
            DestroyImmediate(this);
            return;
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public static void AppStart()
	{
        appstart();//函数定义在plugin/ios/castlestory.m和对应的.a里，导出xcode工程时.会自动添加到工程依赖里，里面反向调用了u3d的函数.
		//这个oc函数定义里面向主相机发送了一个消息，UnitySendMessage("Main Camera", "testResult", "-10862904$_^_$接口调用失败");
		//在U3D.cs里拥有一个testResult函数会执行.在屏幕上显示一个字符串
	}
}
