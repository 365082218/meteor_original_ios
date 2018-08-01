using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using Starlite.Raven.Internal;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Starlite.Raven {

    [ExecuteInEditMode]
    public partial class RavenSequence {

        public class IgnoredEventEntry {
            public RavenEvent m_Event;
            public int m_IgnoreCount;

            public void Initialize(RavenEvent evnt, int ignoreCount) {
                m_Event = evnt;
                m_IgnoreCount = ignoreCount;
            }

            public void Reset() {
                m_Event = null;
                m_IgnoreCount = 0;
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct DoubleHax {

            [FieldOffset(0)]
            private long m_LongValue;

            [FieldOffset(0)]
            private double m_DoubleValue;

            public DoubleHax(double v) {
                m_LongValue = 0;
                m_DoubleValue = v;
            }

            public DoubleHax(long v) {
                m_DoubleValue = 0;
                m_LongValue = v;
            }

            public long GetLegitLong() {
                var i1 = (long)m_DoubleValue;
                ++m_LongValue;
                var i2 = (long)m_DoubleValue;

                if (i1 != i2) {
                    return i2;
                }

                return i1;
            }
        }

        public const string Tag = "Raven";

        public Action<RavenSequence> e_OnEndDelegate;

        private bool m_Playing;

        private int m_TotalFrameCount = 0;
        private int m_LastProcessedFrame = -1;

        private double m_FrameDuration = 0;
        private double m_CurrentTime = 0;
        private double m_PreviousTime = 0;
        private double m_JumpFrameTime = 0;
        private double m_JumpFrameTimeOverflow = 0f;

        private bool m_JumpedFrame = false;
        private bool m_StopProcessing = false;
        private bool m_PlayingInstantly = false;

        private bool m_PausedThisFrame = false;
        private int m_PausedTriggerPointIndex = -1;

        // this will hold TotalFrameCount + 1 elements (+1 for those that need to be executed after end)
        private List<List<RavenEventTriggerPoint>> m_TriggerPoints;

        private readonly HashSet<RavenEvent> m_IgnoredEvents = new HashSet<RavenEvent>();
        private readonly List<IgnoredEventEntry> m_IgnoredEventsInternal = new List<IgnoredEventEntry>(4);

        // it's a sorted list
        private readonly List<RavenContinuousEvent> m_ActiveContinuousEvents = new List<RavenContinuousEvent>(64);

        private RavenEvent m_CurrentProcessingEvent = null;

        // private delegates
        private Action<RavenSequence> e_InternalOnEndDelegate;

        public bool Playing {
            get {
                return m_Playing;
            }
            private set {
                if (m_Playing == value) {
                    return;
                }

                m_Playing = value;
                if (!m_Playing) {
                    m_PlayingInstantly = false;
                    m_StopProcessing = true;
                    if (e_InternalOnEndDelegate != null) {
                        e_InternalOnEndDelegate(this);
                    }
                    if (e_OnEndDelegate != null) {
                        e_OnEndDelegate(this);
                    }
                    RavenLog.DebugT(Tag, "Stopping sequence {0} at {1}", gameObject.name, m_CurrentTime.ToString("F6"));
                } else {
                    m_ForceWarmup = true;
                    RavenOverseer.RegisterSequencePlay(this, ShouldWarmup());
                    UpdateWarmup();
                    if (m_LastWarmupFrame != m_TotalFrameCount) {
                        RavenAssert.IsTrue(false, "Warmup not finished! {0}/{1}", m_LastWarmupFrame, m_TotalFrameCount);
                    }
                    RavenLog.DebugT(Tag, "Starting sequence {0} at {1}", gameObject.name, m_CurrentTime.ToString("F6"));
                }
            }
        }

        public bool PlayOnAwake {
            get {
                return m_PlayOnAwake;
            }
            set {
                m_PlayOnAwake = value;
            }
        }

        public bool PlayOnEnable {
            get {
                return m_PlayOnEnable;
            }
            set {
                m_PlayOnEnable = value;
            }
        }

        public bool Loop {
            get {
                return m_Loop;
            }
            set {
                m_Loop = value;
            }
        }

        public double CurrentTime {
            get {
                return m_CurrentTime;
            }
        }

        public double FrameDuration {
            get {
                return m_FrameDuration;
            }
        }

        public double Duration {
            get {
                return m_Duration;
            }
        }

        public double TimeScale {
            get {
                return m_TimeScale;
            }
            set {
                m_TimeScale = value;
            }
        }

        public string Name {
            get {
                return m_CustomName;
            }
            set {
                m_CustomName = value;
            }
        }

        public int GetCurrentFrame() {
            return (int)GetFrameForTime(m_CurrentTime);
        }

        public int GetPreviousFrame() {
            return (int)GetFrameForTime(m_PreviousTime);
        }

        public double GetFrameInterpolationTime() {
            return (m_CurrentTime - GetCurrentFrame() * m_FrameDuration) / m_FrameDuration;
        }

        public void JumpToTime(double time) {
            SetTimeInternal(time, true);
        }

        public void JumpToFrame(int frame) {
            JumpToTime(GetTimeForFrame(frame));
        }

        public long GetFrameForTime(double time) {
            // this works for more frames than we'll ever need (> billion)
            // if we ever need more, this can be reworked
            var x = new DoubleHax(time / m_FrameDuration);
            return x.GetLegitLong();

            // test for above comment
            //for (int i = 0; i < int.MaxValue; ++i) {
            //    var t = (i * FrameDuration);
            //    var f = GetFrameForTime(t);
            //    if (i != f) {
            //        var x1 = new DoubleHax(t / FrameDuration);
            //        var x2 = new DoubleHax(x1.m_LongValue + 1);
            //        O7Log.Error("Frame no match for {0}", i);
            //        return;
            //    }
            //}
        }

        public double GetTimeForFrame(int frame) {
            return frame * m_FrameDuration;
        }

        public void Play(bool fromStart = true, bool instant = false) {
            if (fromStart) {
                JumpToTime(0f);
            }

            if (gameObject.activeInHierarchy || instant) {
                Playing = true;
            }

            if (instant) {
                m_PlayingInstantly = true;
                m_CurrentTime = m_Duration;
                Evaluate();
            }
        }

        public void Play(string bookmarkName, bool instant = false) {
            GoToBookmark(bookmarkName);
            Play(false, instant);
        }

        public void GoToBookmark(string bookmarkName) {
            RavenBookmarkEvent selectedBookmark = null;

            for (int i = 0; i < m_SortedBookmarks.Count; ++i) {
                var bookmark = m_SortedBookmarks[i];
                if (bookmark.BookmarkName == bookmarkName) {
                    selectedBookmark = bookmark;
                    break;
                }
            }

            if (selectedBookmark == null) {
                RavenAssert.IsTrue(false, "Bookmark {0} not found in sequence!", bookmarkName);
            }

            JumpToFrame(selectedBookmark.StartFrame);
            AddIgnoredEvent(selectedBookmark, 1);
        }

        public void Stop() {
            if (!Playing) {
                return;
            }

            EndContinuousEvents(m_LastProcessedFrame, m_LastProcessedFrame, true);
            SetTimeInternal(0f, false);
            Clear();
            Playing = false;

#if RAVEN_DEBUG
            RavenAssert.IsTrue(m_ActiveContinuousEvents.Count == 0, "{0} active continuous events still left after Stop!", m_ActiveContinuousEvents.Count);
#endif
        }

        public void Pause() {
            if (!Playing) {
                return;
            }

            var pauseFrame = m_LastProcessedFrame;
            PauseContinuousEvents(pauseFrame);
            m_PausedThisFrame = true;
            Playing = false;
        }

        public void Resume() {
            Play(false, false);
        }

        public string FindFirstBookmarkBefore(int frame) {
            for (int i = m_SortedBookmarks.Count - 1; i >= 0; --i) {
                var bookmark = m_SortedBookmarks[i];
                if (bookmark.StartFrame < frame) {
                    return bookmark.BookmarkName;
                }
            }

            return null;
        }

        public string FindFirstBookmarkAfter(int frame) {
            for (int i = 0; i < m_SortedBookmarks.Count; ++i) {
                var bookmark = m_SortedBookmarks[i];
                if (bookmark.StartFrame > frame) {
                    return bookmark.BookmarkName;
                }
            }

            return null;
        }

        public void AddInternalDelegate(Action<RavenSequence> del) {
            e_InternalOnEndDelegate += del;
        }

        public void RemoveInternalDelegate(Action<RavenSequence> del) {
            e_InternalOnEndDelegate -= del;
        }

        private void Initialize() {
            m_FrameDuration = 1.0 / m_Fps;
            m_LastWarmupFrame = 0;
            m_TotalFrameCount = (int)Math.Ceiling(m_Fps * m_Duration);
            m_TriggerPoints = new List<List<RavenEventTriggerPoint>>(m_TotalFrameCount + 1);
            InitializeParameters();
            Clear();
        }

        private void Deinitialize() {
            m_ActiveContinuousEvents.Clear();
            if (m_TriggerPoints != null) {
                for (int i = 0; i < m_TriggerPoints.Count; ++i) {
                    // push everything back to pool
                    RavenOverseer.PushTriggerPointList(m_TriggerPoints[i]);
                }
            }
            m_TriggerPoints = null;
            DeinitializeParameters();
        }

        protected override void Awake() {
            base.Awake();
            RavenOverseer.RegisterSequence(this);
            Initialize();
#if UNITY_EDITOR
            if (Application.isPlaying) {
#endif
                if (m_PlayOnAwake) {
                    Play();
                }
#if UNITY_EDITOR
            } else {
                m_LastEditorUpdateTime = EditorApplication.timeSinceStartup;
            }
#endif
        }

        protected override void OnEnable() {
            base.OnEnable();
#if UNITY_EDITOR
            if (Application.isPlaying) {
#endif
                if (m_PlayOnEnable) {
                    Play();
                }
#if UNITY_EDITOR
            } else {
                if (EditorApplication.isPlayingOrWillChangePlaymode) {
                    return;
                }
                Reinitialize();
                UpdateWarmup();
                EditorApplication.update += EditorUpdate;
            }
#endif
        }

        public override void OnPreUpdate(float deltaTime) {
#if UNITY_EDITOR
            if (Application.isPlaying) {
#endif
                CustomUpdate(deltaTime);
#if UNITY_EDITOR
            }
#endif
        }

        private void CustomUpdate(double deltaTime) {
            if (!Playing) {
                UpdateWarmup();
                return;
            }

            deltaTime *= m_TimeScale;
#if UNITY_EDITOR
            deltaTime = m_Recording ? 0.0 : deltaTime;
            CheckIfEventsDirty();
#endif
            Evaluate();

            // only increment current time if we didn't forcefully stop
            if (!m_StopProcessing) {
                m_CurrentTime += deltaTime;
                if (m_CurrentTime > m_Duration) {
                    m_CurrentTime = m_Duration;
                }
            }
        }

        private void Evaluate() {
            if (m_CurrentProcessingEvent != null) {
                RavenAssert.IsTrue(false, "Current processing event not null! {0}", m_CurrentProcessingEvent);
            }

            do {
                for (int i = 0; i < m_IgnoredEventsInternal.Count; ++i) {
                    var entry = m_IgnoredEventsInternal[i];
                    --entry.m_IgnoreCount;
                    if (entry.m_IgnoreCount < 0) {
                        RavenAssert.IsTrue(false, "Ignore count < 0!");
                    }
                }

                var previousFrame = GetPreviousFrame();
                var currentFrame = GetCurrentFrame();
                var frameInterpolationTime = GetFrameInterpolationTime();
                var isSameFrame = m_LastProcessedFrame == previousFrame && !m_PausedThisFrame && !m_JumpedFrame;

                if (m_JumpedFrame) {
                    EndContinuousEvents(previousFrame, m_LastProcessedFrame, false);
                }

                m_JumpedFrame = false;
                m_StopProcessing = false;

                for (int frame = previousFrame; frame <= currentFrame && frame < m_TotalFrameCount; ++frame) {
#if RAVEN_DEBUG
                    RavenLog.DebugT(Tag, "Processing frame {0}", frame);
#endif
                    bool isCurrentFrame = frame == currentFrame;
                    m_LastProcessedFrame = frame;
                    // This optimization and the one inside can produce different results when playing instantly vs not playing instantly
                    // It can also produce different results when events depend on results from continuous events (e.g. LookAt) due to frame interpolation time
                    // being different in the previous frame on every run. Perhaps invokes should sync/barrier when they're run.
                    // That means process previous frame but only continuous events on frame interpolation time 1.0
                    // We'll solve that by injecting a barrier event at the beginning of a frame in case there's a barrier requesting event in that frame
                    // However, there will still be cases when a continuous event will be processed AFTER barrier and BEFORE barrier event screwing it up slightly
                    // due to frame interpolation time
                    if (!isSameFrame || previousFrame == currentFrame) {
                        ProcessEvents(frame, isSameFrame, isCurrentFrame, frameInterpolationTime);
                    }

                    if (m_StopProcessing) {
                        if (m_JumpedFrame) {
                            m_PreviousTime = m_JumpFrameTime;
                            // If we're playing instantly, force current time to end time so we don't start processing this
                            // as if it wasn't instant
                            m_CurrentTime = m_PlayingInstantly ? m_Duration : (m_JumpFrameTime + m_JumpFrameTimeOverflow);
                        } else {
                            // Set current time to the frame we're ending on
                            m_CurrentTime = GetTimeForFrame(frame);
                        }
                        break;
                    }

                    isSameFrame = false;
                    ClearPause();
                }

                for (int i = 0; i < m_IgnoredEventsInternal.Count; ++i) {
                    var entry = m_IgnoredEventsInternal[i];
                    if (entry.m_IgnoreCount == 0) {
                        m_IgnoredEventsInternal.RemoveAt(i--);
                        m_IgnoredEvents.Remove(entry.m_Event);
                        RavenOverseer.PushIgnoredEventEntry(entry);
                    }
                }
            } while (m_JumpedFrame && Playing);

            m_PreviousTime = m_CurrentTime;

            if (m_PlayingInstantly) {
                Playing = false;
            }

            if (m_CurrentTime == m_Duration) {
                ProcessEvents(m_TotalFrameCount, false, true, 0f);
                // don't loop again if we forcefully stopped it
                if (m_Playing) {
                    Playing = m_Loop;
                }
                JumpToTime(0f);
            }
        }

        private void ProcessEvents(int frame, bool isSameFrame, bool isCurrentFrame, double frameInterpolationTime) {
            var triggerPoints = m_TriggerPoints[frame];
            if (triggerPoints == null) {
                return;
            }

            for (int i = m_PausedTriggerPointIndex; i < triggerPoints.Count; ++i) {
                var triggerPoint = triggerPoints[i];
                var evnt = triggerPoints[i].RavenEvent;
                if (m_IgnoredEventsInternal.Count > 0 && m_IgnoredEvents.Contains(evnt)) {
                    continue;
                }

                m_CurrentProcessingEvent = evnt;

                switch (triggerPoint.Type) {
                    case ERavenEventTriggerPointType.Barrier:
                        // if we paused then we don't need to execute it since pause executes OnProcess on continuous events which is exactly the same
                        if (!isSameFrame && !m_PausedThisFrame) {
#if RAVEN_DEBUG
                            RavenLog.DebugT(Tag, "Executing barrier at frame {0}, owner: {1}", frame, evnt);
#endif
                            ProcessEvents(frame, true, true, 0f);
#if RAVEN_DEBUG
                            RavenLog.DebugT(Tag, "Leaving barrier at frame {0}, owner: {1}", frame, evnt);
#endif
                        }
                        break;

                    case ERavenEventTriggerPointType.Start:
                    case ERavenEventTriggerPointType.Bookmark:
                        if (!isSameFrame) {
                            if (evnt.EventType == ERavenEventType.Continuous) {
                                var continuousEvent = evnt as RavenContinuousEvent;
                                if (!continuousEvent.Active) {
                                    m_ActiveContinuousEvents.Add(continuousEvent);
                                    evnt.OnEnter(frame);
                                }
                            } else {
                                evnt.OnEnter(frame);
                            }
                        }
                        break;

                    case ERavenEventTriggerPointType.Process:
#if RAVEN_DEBUG
                        RavenAssert.IsTrue(evnt as RavenContinuousEvent != null, "Not a continuous event {0}", evnt);
#endif
                        if (isCurrentFrame) {
                            var continuousEvent = evnt as RavenContinuousEvent;
                            if (!continuousEvent.Active) {
                                m_ActiveContinuousEvents.Add(continuousEvent);
                            }
                            continuousEvent.OnProcess(frame, frameInterpolationTime);
                        }
                        break;

                    case ERavenEventTriggerPointType.End:
#if RAVEN_DEBUG
                        RavenAssert.IsTrue(evnt as RavenContinuousEvent != null, "Not a continuous event {0}", evnt);
#endif
                        if (!isSameFrame) {
                            var continuousEvent = evnt as RavenContinuousEvent;
                            if (continuousEvent.Active) {
                                // remove first in case onend calls stop or something that would cause it to be processed again
                                m_ActiveContinuousEvents.Remove(continuousEvent);
                                continuousEvent.OnEnd(frame);
                            }
                        }
                        break;
                }

                m_CurrentProcessingEvent = null;

                if (m_StopProcessing) {
                    if (m_PausedThisFrame) {
                        m_PausedTriggerPointIndex = i + 1;
                    }
                    break;
                }
            }
        }

        private void EndContinuousEvents(int currentFrame, int jumpFrame, bool forceEnd) {
#if RAVEN_DEBUG
            // case when we call stop right after play
            if (jumpFrame == -1) {
                RavenAssert.IsTrue(m_ActiveContinuousEvents.Count == 0, "EndContinuousEvents count {0} > 0 when ending on {1} frame", m_ActiveContinuousEvents.Count, jumpFrame);
            }
#endif

            for (int i = 0; i < m_ActiveContinuousEvents.Count; ++i) {
                var continuousEvent = m_ActiveContinuousEvents[i];
                if (forceEnd || continuousEvent.ShouldEndEventAfterJump(currentFrame)) {
                    // reuse warmup list...
                    // we have to do this because even tho this is sorted by arrival, it doesn't guarantee order onend
                    // as an even that started later but has a higher track priority would get end executed after the one that
                    // started before
                    m_TempTriggerPointSortListForSearch.AddSorted(RavenOverseer.PopTriggerPoint(jumpFrame, ERavenEventTriggerPointType.End, continuousEvent), RavenEventTriggerPoint.Comparer);
                    m_ActiveContinuousEvents.RemoveAt(i--);
                }
            }

            var evntCount = m_TempTriggerPointSortListForSearch.Count;
            if (evntCount > 0) {
                for (int i = 0; i < evntCount; ++i) {
                    (m_TempTriggerPointSortListForSearch[i].RavenEvent as RavenContinuousEvent).OnEnd(jumpFrame);
                    RavenOverseer.PushTriggerPoint(m_TempTriggerPointSortListForSearch[i]);
                }
                m_TempTriggerPointSortListForSearch.Clear();
            }
        }

        private void PauseContinuousEvents(int pauseFrame) {
            for (int i = 0; i < m_ActiveContinuousEvents.Count; ++i) {
                var continuousEvent = m_ActiveContinuousEvents[i];
                m_TempTriggerPointSortListForSearch.AddSorted(RavenOverseer.PopTriggerPoint(pauseFrame, ERavenEventTriggerPointType.Pause, continuousEvent), RavenEventTriggerPoint.Comparer);
            }

            var evntCount = m_TempTriggerPointSortListForSearch.Count;
            if (evntCount > 0) {
                for (int i = 0; i < evntCount; ++i) {
                    (m_TempTriggerPointSortListForSearch[i].RavenEvent as RavenContinuousEvent).OnPause(pauseFrame);
                    RavenOverseer.PushTriggerPoint(m_TempTriggerPointSortListForSearch[i]);
                }
                m_TempTriggerPointSortListForSearch.Clear();
            }
        }

        private void SetTimeInternal(double time, bool calculateOverflow) {
            m_JumpedFrame = true;
            m_StopProcessing = true;
            ClearPause();

            m_JumpFrameTime = time;
            if (calculateOverflow) {
                m_JumpFrameTimeOverflow = m_CurrentTime - GetTimeForFrame(m_LastProcessedFrame);
            } else {
                m_JumpFrameTimeOverflow = 0f;
            }

            // this will get overriden only if we're in the process of executing an event
            // it will get overriden by the above in the main loop (Evaluate)

            m_CurrentTime = time;
            m_PreviousTime = time;
        }

        private void ClearPause() {
            m_PausedThisFrame = false;
            m_PausedTriggerPointIndex = 0;
        }

        private void AddIgnoredEvent(RavenEvent evnt, int ignoreUpdateCount) {
            if (evnt == null) {
                return;
            }

            if (m_IgnoredEvents.Contains(evnt)) {
                IgnoredEventEntry existingEntry = null;
                for (int i = 0; i < m_IgnoredEventsInternal.Count; ++i) {
                    var ignoredEventEntry = m_IgnoredEventsInternal[i];
                    if (ignoredEventEntry.m_Event == evnt) {
                        existingEntry = ignoredEventEntry;
                        break;
                    }
                }
                if (existingEntry == null) {
                    RavenAssert.IsTrue(false, "Ignored event {0} was in hashset but not in internal list!", evnt);
                }
                existingEntry.m_IgnoreCount = ignoreUpdateCount;
            } else {
                m_IgnoredEventsInternal.Add(RavenOverseer.PopIgnoredEventEntry(evnt, ignoreUpdateCount));
                m_IgnoredEvents.Add(evnt);
            }
        }

        private void Clear() {
            m_LastProcessedFrame = -1;
            ClearPause();
        }

        protected override void OnDisable() {
            base.OnDisable();
            Stop();
#if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate;
#endif
        }

        private void OnDestroy() {
            Deinitialize();
            RavenOverseer.UnregisterSequence(this);
        }

        protected sealed override bool ShouldRegisterToManager() {
            return true;
        }
    }
}