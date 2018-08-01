//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System.Collections.Generic;
using Outfit7.Util;

namespace Outfit7.Event {

    /// <summary>
    /// Event listener list.
    /// </summary>
    public class EventListenerList {

        private readonly Dictionary<int, HashSet<OnEvent>> listenerMap;

        public EventListenerList() {
            listenerMap = new Dictionary<int, HashSet<OnEvent>>();
        }

        public void Add(int eventId, OnEvent listener) {
            Assert.NotNull(listener, "listener");

            HashSet<OnEvent> listeners = GetAllListeners(eventId);
            if (listeners == null) {
                listeners = new HashSet<OnEvent>();
                listenerMap.Add(eventId, listeners);
            }

            bool added = listeners.Add(listener);
            if (added) {
                O7Log.DebugT(EventBus.Tag, "Added listener for eventId={0}", eventId);
            } else {
                O7Log.WarnT(EventBus.Tag, "Same listener for eventId={0} already exist", eventId);
            }
        }

        public void Remove(int eventId, OnEvent listener) {
            Assert.NotNull(listener, "listener");

            HashSet<OnEvent> listeners = GetAllListeners(eventId);
            if (listeners == null) {
                O7Log.WarnT(EventBus.Tag, "Listener for eventId={0} does not exist", eventId);
                return;
            }

            bool removed = listeners.Remove(listener);
            if (removed) {
                O7Log.DebugT(EventBus.Tag, "Removed listener for eventId={0}", eventId);
            } else {
                O7Log.WarnT(EventBus.Tag, "Listener for eventId={0} does not exist", eventId);
            }
        }

        public HashSet<OnEvent> GetAllListeners(int eventId) {
            HashSet<OnEvent> listeners;
            listenerMap.TryGetValue(eventId, out listeners);
            return listeners;
        }
    }
}
