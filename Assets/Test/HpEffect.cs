using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class HpEffect : MonoBehaviour {

    public float delay = 1.0f;
	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        delay -= Time.deltaTime;
        if (delay <= 0.0f)
            DestroyImmediate(gameObject);
	}

    public void ShowDamageText(string str)
    {
        Text txt = gameObject.GetComponent<Text>();
        txt.text = "-" + str;
        //等声音0.1秒后飘字.
        Invoke("DelayFly", 0.1f);
    }

    public void DelayFly()
    {
        gameObject.GetComponent<Text>().FlyTo();
    }
}
