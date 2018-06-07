using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

//代理OC平台上的调用
public class IosWrapper : MonoBehaviour {
#if UNITY_IOS
    //[DllImport("__Internal")]
    //private static extern void IOS_AXXX();
#endif
    public static IosWrapper IosInstance;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        IosInstance = this;
    }


	public static void Init()
	{
        if (IosInstance == null)
        {
            GameObject obj = new GameObject("Meteor2Ios");
            obj.transform.rotation = Quaternion.identity;
            obj.transform.position = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            IosInstance = obj.AddComponent<IosWrapper>();
        }
    }
}
