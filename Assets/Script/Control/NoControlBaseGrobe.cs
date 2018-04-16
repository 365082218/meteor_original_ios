using UnityEngine;
using System.Collections;

public class NoControlBaseGrobe : MonoBehaviour {
    public static bool isPressed = false;
	
	void Start () {

        UIEventListener.Get(gameObject).onPress = BtnPressFun;
	}

    public void BtnPressFun(GameObject go, bool isPress)
    {
        isPressed = isPress;
    }
}
