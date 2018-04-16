using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class CountDownLabel : MonoBehaviour
{
    float mCountDownSeconds = 0;
    public float CountDownSeconds { get { return mCountDownSeconds; } set { mCountDownSeconds = value; } }

    public void SetCountDownValue(float seconds)
    {
        CountDownSeconds = seconds;
    }

    void Update()
    {
        if (CountDownSeconds <= 0) CountDownSeconds = 0;
        CountDownSeconds -= Time.deltaTime;
        string str = "";
        int hour = (int)CountDownSeconds / 3600;
        if (hour >= 10) str += hour.ToString() + ":";
        else if (hour > 0 && hour < 10) str += "0" + hour.ToString() + ":";

        int minute = ((int)CountDownSeconds - (hour * 3600)) / 60;
        if (hour >= 10) str += minute.ToString();
        else str +=minute.ToString();
        str += ":";

        int second = (int)CountDownSeconds - (hour * 3600) - (minute * 60);
        if (second >= 10) str += second.ToString();
        else str += "0" + second.ToString();

        gameObject.GetComponent<UILabel>().text = str;
    }
}
