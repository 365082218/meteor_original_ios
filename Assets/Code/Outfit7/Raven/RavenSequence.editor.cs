using UnityEngine;
using System.Collections.Generic;
using System;
using Starlite.Raven.Internal;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Starlite.Raven {

    // This class holds stuff that editor requires but also needs to be serialized
    public partial class RavenSequence {

        #region EditorOnly

#if UNITY_EDITOR
        private double m_LastEditorUpdateTime = 0;
        private bool m_Recording = false;
        private bool m_EventsDirty = false;

        public static bool s_DebugMode = false;

        public List<RavenBookmarkEvent> SortedBookmarks {
            get {
                return m_SortedBookmarks;
            }
        }

        public List<RavenEvent> SortedEvents {
            get {
                return m_SortedEvents;
            }
        }

        public List<RavenParameter> Parameters {
            get {
                return m_Parameters;
            }
        }

        public List<RavenTrackGroup> TrackGroups {
            get {
                return m_SortedTrackGroups;
            }
        }

        public RavenBookmarkTrack BookmarkTrack {
            get {
                return m_BookmarkTrack;
            }
            set {
                m_BookmarkTrack = value;
            }
        }

        public int Fps {
            get {
                return m_Fps;
            }
            set {
                if (m_Fps == value) {
                    return;
                }

                RecalculateFpsChange(m_Fps, value);
            }
        }

        public int TotalFrameCount {
            get {
                return m_TotalFrameCount;
            }
        }

        public bool Recording {
            get {
                return m_Recording;
            }
            set {
                m_Recording = value;
            }
        }

        private bool InitializedEditor {
            get {
                return ShouldWarmup();
            }
        }

        public void InitializeEditor() {
            if (InitializedEditor) {
                CheckIfEventsDirty();
                return;
            }

            if (m_TriggerPoints == null) {
                Initialize();
            }
            m_LastWarmupFrame = 0;
            m_ForceWarmup = true;
            UpdateWarmup();
        }

        public void SetDuration(double duration, bool reinitialize = true) {
            Undo.RecordObject(this, "SetDuration");
            m_Duration = duration;
            if (reinitialize) {
                var frame = GetFrameForTime(duration);
                m_Duration = GetTimeForFrame((int)frame);
                Reinitialize();
            }
        }

        public void RecalculateFpsChange(int oldFps, int newFps) {
            Undo.RecordObject(this, "RecalculateFpsChange");
            Stop();
            Deinitialize();

            var durationFactor = (double)newFps / oldFps;
            //m_Duration *= durationFactor;
            for (int i = 0; i < m_SortedEvents.Count; ++i) {
                var evnt = m_SortedEvents[i];
                Undo.RecordObject(evnt, "RecalculateFpsChange");
                evnt.RecalculateFpsChange(durationFactor);
            }

            m_Fps = newFps;
            Initialize();

            m_ForceWarmup = true;
            UpdateWarmup();
        }

        public void RemoveEvent(RavenEvent evnt) {
            if (evnt == null) {
                return;
            }

            Undo.RecordObject(this, "Event Destroyed");
            if (evnt.EventType == ERavenEventType.Bookmark && m_SortedBookmarks.Remove(evnt as RavenBookmarkEvent)) {
                EditorUtility.SetDirty(this);
                m_EventsDirty = true;
            }

            if (m_SortedEvents.Remove(evnt)) {
                EditorUtility.SetDirty(this);
                m_EventsDirty = true;
            }

            evnt.DestroyEditor(this);
        }

        /// <summary>
        /// At this point the event should already be destroyed!
        /// </summary>
        public bool CanDestroyAnimationData(RavenAnimationDataComponentBase animationData) {
            for (int i = 0; i < m_SortedEvents.Count; ++i) {
                var evnt = m_SortedEvents[i];
                if (evnt.AnimationDataEditorOnly == animationData) {
                    return false;
                }
            }

            return true;
        }

        public void RebuildSortedLists() {
            Undo.RecordObject(this, "RebuildSortedLists");

            m_SortedEvents.Clear();
            m_SortedBookmarks.Clear();

            for (int i = 0; i < m_BookmarkTrack.Events.Count; ++i) {
                var evnt = m_BookmarkTrack.Events[i];
                AddEventToSortedLists(evnt);
            }

            for (int i = 0; i < m_SortedTrackGroups.Count; ++i) {
                var trackGroup = m_SortedTrackGroups[i];
                if (trackGroup.m_PropertyTrack != null) {
                    for (int j = 0; j < trackGroup.m_PropertyTrack.Events.Count; ++j) {
                        AddEventToSortedLists(trackGroup.m_PropertyTrack.Events[j]);
                    }
                }
                if (trackGroup.m_AudioTrack != null) {
                    for (int j = 0; j < trackGroup.m_AudioTrack.Events.Count; ++j) {
                        AddEventToSortedLists(trackGroup.m_AudioTrack.Events[j]);
                    }
                }
            }
            EditorUtility.SetDirty(this);
            m_EventsDirty = true;
        }

        public void AddEventToSortedLists(RavenEvent evnt) {
            Undo.RecordObject(this, "AddEventToSortedLists");

            if (evnt.EventType == ERavenEventType.Bookmark) {
                m_SortedBookmarks.AddSorted(evnt as RavenBookmarkEvent, RavenBookmarkEvent.BookmarkComparer);
            }

            m_SortedEvents.AddSorted(evnt, RavenEvent.Comparer);
            EditorUtility.SetDirty(this);
            m_EventsDirty = true;
        }

        public void EventChanged(RavenEvent evnt) {
            Undo.RecordObject(this, "EventChanged");

            var bookmark = evnt as RavenBookmarkEvent;
            if (bookmark != null) {
                if (m_SortedBookmarks.Remove(bookmark)) {
                    m_SortedBookmarks.AddSorted(bookmark, RavenBookmarkEvent.BookmarkComparer);
                }
            }

            if (m_SortedEvents.Remove(evnt)) {
                m_SortedEvents.AddSorted(evnt, RavenEvent.Comparer);
            }
            m_EventsDirty = true;
        }

        public void AddTrackGroup(RavenTrackGroup trackGroup) {
            Undo.RecordObject(this, "AddTrackGroup");
            m_SortedTrackGroups.Add(trackGroup);
            TrackGroupsChanged();
        }

        public void RemoveTrackGroup(RavenTrackGroup trackGroup) {
            Undo.RecordObject(this, "RemoveTrackGroup");
            if (m_SortedTrackGroups.Remove(trackGroup)) {
                TrackGroupsChanged();
            }
        }

        public void SwapTrackGroups(int index1, int index2) {
            Undo.RecordObject(this, "SwapTrackGroups");
            var tempTrackGroup = m_SortedTrackGroups[index1];
            m_SortedTrackGroups[index1] = m_SortedTrackGroups[index2];
            m_SortedTrackGroups[index2] = tempTrackGroup;
            TrackGroupsChanged();
        }

        public void TrackGroupsChanged() {
            Undo.RecordObject(this, "TrackGroupChanged");
            for (int i = 0; i < m_SortedTrackGroups.Count; ++i) {
                m_SortedTrackGroups[i].TrackIndex = i * RavenTrackGroup.c_TrackCount;
            }
            m_EventsDirty = true;
        }

        public void FlagDirty() {
            // recalc total duration here anyway for editor
            m_FrameDuration = 1f / m_Fps;
            m_TotalFrameCount = (int)Math.Ceiling(m_Fps * m_Duration);
            m_EventsDirty = true;
        }

        public void ClearOrphans() {
            for (int i = 0; i < m_SortedEvents.Count; ++i) {
                var ev = m_SortedEvents[i];
                if (ev != null && !object.Equals(ev, null)) {
                    var orphan = true;
                    if (ev.EventType == ERavenEventType.Bookmark) {
                        if (m_BookmarkTrack.Events.Contains(ev)) {
                            orphan = false;
                        }
                    } else {
                        for (int j = 0; j < m_SortedTrackGroups.Count; ++j) {
                            var trackGroup = m_SortedTrackGroups[j];
                            if (trackGroup.m_PropertyTrack != null) {
                                if (trackGroup.m_PropertyTrack.Events.Contains(ev)) {
                                    orphan = false;
                                    break;
                                }
                            }
                            if (trackGroup.m_AudioTrack != null) {
                                if (trackGroup.m_AudioTrack.Events.Contains(ev)) {
                                    orphan = false;
                                    break;
                                }
                            }
                        }
                    }
                    if (orphan) {
                        RemoveEvent(ev);
                    }
                } else {
                    Undo.RecordObject(this, "ClearOrphans");
                    m_SortedEvents.RemoveAt(i--);
                    EditorUtility.SetDirty(this);
                    m_EventsDirty = true;

                    Undo.RecordObject(m_BookmarkTrack, "ClearOrphans");
                    if (m_BookmarkTrack.Events.Remove(ev)) {
                        continue;
                    }
                    for (int j = 0; j < m_SortedTrackGroups.Count; ++j) {
                        var trackGroup = m_SortedTrackGroups[j];
                        if (trackGroup.m_PropertyTrack != null) {
                            Undo.RecordObject(trackGroup.m_PropertyTrack, "ClearOrphans");
                            if (trackGroup.m_PropertyTrack.Events.Remove(ev)) {
                                EditorUtility.SetDirty(trackGroup.m_PropertyTrack);
                                break;
                            }
                        }
                        if (trackGroup.m_AudioTrack != null) {
                            Undo.RecordObject(trackGroup.m_AudioTrack, "ClearOrphans");
                            if (trackGroup.m_AudioTrack.Events.Remove(ev)) {
                                EditorUtility.SetDirty(trackGroup.m_AudioTrack);
                                break;
                            }
                        }
                    }
                }
                RebuildSortedLists();
            }
        }

        public void ShowAllComponents() {
            var components = gameObject.GetComponents<Component>();
            for (int i = 0; i < components.Length; ++i) {
                if (components[i] == null) {
                    RavenLog.WarnT(Tag, "Null component on {0}!", this);
                    continue;
                }
                components[i].hideFlags = HideFlags.None;
            }
        }

        private void Reinitialize() {
            Deinitialize();
            Initialize();
        }

        private void EditorUpdate() {
            if (Application.isPlaying) {
                return;
            }

            var timeSinceStartup = EditorApplication.timeSinceStartup;
            CustomUpdate(timeSinceStartup - m_LastEditorUpdateTime);
            m_LastEditorUpdateTime = timeSinceStartup;
        }

        private void CheckIfEventsDirty() {
            if (m_EventsDirty) {
                var wasPlaying = Playing;
                Playing = false;

                m_EventsDirty = false;
                Reinitialize();

                m_ForceWarmup = true;
                UpdateWarmup();

                if (wasPlaying) {
                    Resume();
                }
            }
        }

        private void OnValidate() {
            for (int i = 0; i < m_SortedEvents.Count; ++i) {
                if (m_SortedEvents[i] == null) {
                    RavenLog.ErrorT(Tag, "Event null at index {0} on sequence {1}", i, this);
                }
            }
        }

#endif

        #endregion EditorOnly
    }
}