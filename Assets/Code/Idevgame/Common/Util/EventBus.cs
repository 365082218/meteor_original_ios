using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EventId
{
    OpenCamera = 0,
}


public class EventBus
{
    Dictionary<int, List<System.Action>> events = new Dictionary<int, List<System.Action>>();
    public void Register(EventId evt, System.Action fun)
    {
        if (!events.ContainsKey((int)evt))
            events[(int)evt] = new List<System.Action>();
        events[(int)evt].Add(fun);
    }

    public void UnRegister(EventId evt, System.Action fun)
    {
        if (events.ContainsKey((int)evt))
            events[(int)evt].Remove(fun);
    }

    public void Fire(EventId evt)
    {
        if (events.ContainsKey((int)evt))
        {
            for (int i = 0; i < events[(int)evt].Count; i++)
            {
                events[(int)evt][i].Invoke();
            }
        }
    }
}
