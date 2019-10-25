using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CommonEvent
{
    OpenCamera = 0,
}


public class EventBus : Singleton<EventBus> {
    Dictionary<int, List<System.Action>> events = new Dictionary<int, List<System.Action>>();
    public void SetEvent(CommonEvent evt, System.Action fun)
    {
        if (!events.ContainsKey((int)evt))
            events[(int)evt] = new List<System.Action>();
        events[(int)evt].Add(fun);
    }

    public void RemoveEvent(CommonEvent evt, System.Action fun)
    {
        if (events.ContainsKey((int)evt))
            events[(int)evt].Remove(fun);
    }

    public void Fire(CommonEvent evt)
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
