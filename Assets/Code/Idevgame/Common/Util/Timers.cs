using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timers {

    // Update is called once per frame
    public void Update(float delta)
    {
        if (tick - delta <= 0)
        {
            if (this.func != null)
                this.func.Invoke();
            tick = second;
        }
        else
            tick -= delta;
    }

    public int id;
    public float second;//间隔
    public float tick;//计时
    public TimersMng.TimerFun func;
}

public class TimersMng:Singleton<TimersMng>
{
    public delegate void TimerFun();
    public TimersMng()
    {
        FrameReplay.Instance.OnUpdates += Update;
    }

    public void SetTimer(int id, int second, TimerFun f)
    {
        for (int i = 0; i < timer.Count; i++)
        {
            Timers pT = timer[i];
            if (pT != null && pT.id == id)
            {
                pT.second = second;
                pT.tick = second;
                return;
            }
        }

        Timers t = new Timers();
        
        t.id = id;
        t.func = f;
        t.second = second;
        t.tick = second;
        timer.Add(t);
    }

    public void KillTimer(int id)
    {
        for (int i = 0; i < timer.Count; i++)
        {
            Timers pT = timer[i];
            if (pT != null && pT.id == id)
            {
                timer.RemoveAt(i);
                return;
            }
        }
    }

    void Update()
    {
        timer2.Clear();
        timer2.AddRange(timer);
        for (int i = 0; i < timer2.Count; i++)
        {
            Timers pT = timer2[i];
            if (pT != null)
            {
                pT.Update(Time.deltaTime);
            }
        }
    }

    List<Timers> timer2 = new List<Timers>();
    List<Timers> timer = new List<Timers>();
}
