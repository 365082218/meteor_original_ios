using System;
using UnityEngine;
using System.IO;
using System.Text;

public class AndroidWrapper:MonoBehaviour
{
	static AndroidWrapper _ins;
    public static AndroidWrapper Instance { get { return _ins; } }
#if UNITY_ANDROID && !UNITY_EDITOR
    AndroidJavaClass Meteor;
    AndroidJavaObject ActivityInstance;
#endif
    private void Awake()
    {
        _ins = this;
#if UNITY_ANDROID && !UNITY_EDITOR
        //Meteor = new AndroidJavaClass("com.meteorsdk.MainActivity");
        //if (Meteor != null)
            //ActivityInstance = Meteor.GetStatic<AndroidJavaObject>("Instance");
#endif
    }
    public static void Init()
	{
        if (Instance == null)
        {
            GameObject cssdkObj = new GameObject("Meteor2Android");
            cssdkObj.transform.position = Vector3.zero;
            cssdkObj.transform.rotation = Quaternion.identity;
            cssdkObj.transform.localScale = Vector3.one;
			_ins = cssdkObj.AddComponent<AndroidWrapper>();
			GameObject.DontDestroyOnLoad (cssdkObj);
        }
    }

	//platform dependence
	public void CallMethod(string function, params object[] args)
	{
		#if UNITY_ANDROID && !UNITY_EDITOR
        if (ActivityInstance != null)
            ActivityInstance.Call(function, args);
        #endif
    }

    //Android端调用这边
    public void FunctionName(string param)
    {

    }
}