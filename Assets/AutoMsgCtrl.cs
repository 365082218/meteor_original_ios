using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//查找亲儿子,给个时间显示,之后渐变Alpha,之后删除
public class AutoMsgCtrl : MonoBehaviour {
    float lastTime = 2.5f;
    float alphaTime = 0.5f;
    // Use this for initialization
    public void SetConfig(float last, float alpha)
    {
        lastTime = last;
        alphaTime = alpha;
        tick = lastTime + alphaTime;
    }

    public void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
            GameObject.Destroy(transform.GetChild(i).gameObject);
        tick = lastTime + alphaTime;
    }

    // Update is called once per frame
    protected void Update() {
        if (transform.childCount != 0)
        {
            tick -= Time.deltaTime;
            if (tick <= 0.0f)
            {
                Transform son = transform.GetChild(0);
                GameObject.Destroy(son.gameObject);
                crossFade = false;
                tick = lastTime + alphaTime;
            }
            else
            if (tick < alphaTime && !crossFade)
            {
                Transform son = transform.GetChild(0);
                Text[] graphs = son.GetComponentsInChildren<Text>();
                for (int i = 0; i < graphs.Length; i++)
                    graphs[i].CrossFadeAlpha(0.0f, alphaTime, true);
                crossFade = true;
            }
        }

        if (Time.time - lastTick > 1f && msg.Count != 0)
        {
            GameObject obj = new GameObject();
            obj.name = (transform.childCount + 1).ToString();
            Text txt = obj.AddComponent<Text>();
            txt.text = msg[0];
            //00AAFFFF
            txt.font = Main.Ins.TextFont;
            txt.fontSize = 32;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.raycastTarget = false;
            txt.color = new Color(1.0f, 1.0f, 1.0f, 1f);
            obj.transform.SetParent(transform);
            obj.transform.localScale = Vector3.one;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            lastTick = Time.time;
            msg.RemoveAt(0);
        }
    }

    float tick;
    bool crossFade;

    float lastTick;
    List<string> msg = new List<string>();
    public void PushMessage(string text)
    {
        if (Time.time - lastTick <= 1f || msg.Count != 0)
        {
            msg.Add(text);
        }
        else
        {
            GameObject obj = new GameObject();
            obj.name = (transform.childCount + 1).ToString();
            Text txt = obj.AddComponent<Text>();
            txt.text = text;
            txt.font = Main.Ins.TextFont;
            txt.fontSize = 32;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.raycastTarget = false;
            txt.color = new Color(1.0f, 1.0f, 1.0f, 1f);
            obj.transform.SetParent(transform);
            obj.transform.localScale = Vector3.one;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            lastTick = Time.time;
        }
    }
}
