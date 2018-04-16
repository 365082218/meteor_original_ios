using UnityEngine;
using System.Collections;

public class DebugAttackRange1 : MonoBehaviour {

    public float activeTime = 5;

    public UILabel label1;
    public UILabel label2;
    public UILabel label3;
    public UILabel label4;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowAttackRange(string text, float time)
    {
        if (!this.gameObject.activeSelf)
            this.gameObject.SetActive(true);

        label1.text = text;
        label2.text = text;
        label3.text = text;
        label4.text = text;

        if (time > 0)
            Invoke("SetUnActive", time);
    }

    public void SetUnActive()
    {
        //this.gameObject.SetActive(false);
    }
}
