//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System.Collections.Generic;
using Outfit7.Util;

namespace Outfit7.Event {

    /// <summary>
    /// Event listener.
    /// <param name="eventData">
    /// </summary>
    public delegate void OnEvent(object eventData);

    /// <summary>
    /// Event bus.
    /// </summary>
    public class EventBus {

        internal const string Tag = "EventBus";
        private readonly EventListenerList eventListenerList;

        public EventBus() {
            eventListenerList = new EventListenerList();
        }

        public void FireEvent(int eventId) {
            FireEvent(eventId, null);
        }

        public void FireEvent(int eventId, object eventData) {
            HashSet<OnEvent> listeners = eventListenerList.GetAllListeners(eventId);
            if (listeners == null || listeners.Count == 0) {
                O7Log.DebugT(Tag, "No listeners to fire eventId={0} on", eventId);
                return;
            }

            O7Log.InfoT(Tag, "Firing eventId={0} on {1} listeners", eventId, listeners.Count);

            // Create a copy of listeners, so that listener can remove itself from listeners in onEvent()
            List<OnEvent> copy = new List<OnEvent>(listeners);
            foreach (OnEvent listener in copy) {
                listener(eventData);
            }
        }

        public void AddListener(int eventId, OnEvent listener) {
            eventListenerList.Add(eventId, listener);
        }

        public void RemoveListener(int eventId, OnEvent listener) {
            eventListenerList.Remove(eventId, listener);
        }
    }
}
