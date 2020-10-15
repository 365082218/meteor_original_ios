using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EventId
{
    PingChanged,
    RoomUpdate,
}

public class TEventArgs : EventArgs
{

}

public class EventBus
{
    SortedDictionary<int, EventHandler<TEventArgs>> _events = new SortedDictionary<int, EventHandler<TEventArgs>>();
    public void Register(EventId evt, EventHandler<TEventArgs> function)
    {
        if (!_events.ContainsKey((int)evt))
            _events[(int)evt] = function;
        else
            _events[(int)evt] += function;
    }

    public void UnRegister(EventId evt, EventHandler<TEventArgs> function)
    {
        if (_events.ContainsKey((int)evt))
            _events[(int)evt] -= function;
        if (_events[(int)evt] == null)
            _events.Remove((int)evt); 
    }

    public void Fire(EventId evt, object sender = null, TEventArgs args = null)
    {
        if (_events.ContainsKey((int)evt))
        {
            _events[(int)evt].Invoke(sender, args);
        }
    }
}
