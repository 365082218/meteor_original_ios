using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Logic.StateMachineInternal;
using System;
using Outfit7.Util;
using Outfit7.Logic;
using Outfit7.Logic.Internal;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Outfit7.Sequencer {
    [ExecuteInEditMode]
    public class SequencerSequence : BucketUpdateBehaviour {
        private class IgnoredEventEntry {
            public SequencerEvent Event;
            public int IgnoreCount;
        }

        public const string Tag = "Sequencer";

        public Action<SequencerSequence> OnEndDelegate;

        public List<SequencerTrackGroup> TrackGroups = new List<SequencerTrackGroup>();
        public List<Parameter> Parameters = new List<Parameter>();
        public List<SequencerBookmarkEvent> Bookmarks = new List<SequencerBookmarkEvent>();
        public readonly HashSet<SequencerEvent> IgnoredEvents = new HashSet<SequencerEvent>();
        public string Name = "Sequence";
        public float TimeScale = 1f;
        public float Duration = 3f;
        public bool Recording = false;
        public bool Loop = false;
        public bool PlayOnAwake = false;
        //
        public bool ForceSplitUpdate = false;
        public bool StopProcessing = false;
        public float ForceSplitTime = 0f;
        public float ForceSplitOverflow = 0f;
        private bool playing = false;
        private float PreviousTime = 0f;
        private float CurrentTime = 0f;

        private BinarySortList<ActionPoint> ActionPoints = new BinarySortList<ActionPoint>();
        private readonly List<IgnoredEventEntry> IgnoredEventsInternal = new List<IgnoredEventEntry>();

        public bool Playing {
            get {
                return playing;
            }
            set {
                if (playing == value) {
                    return;
                }

                playing = value;
                if (!playing) {
                    if (OnEndDelegate != null) {
                        OnEndDelegate(this);
                    }
                    O7Log.VerboseT(Tag, "Stopping sequence {0} at {1}.", gameObject.name, CurrentTime);
                }
            }
        }

#if UNITY_EDITOR
        private double LastTime = 0;
        private float LastUpdateTime = 0;
        public static bool DebugMode = false;
        public SequencerContinuousEvent PreviewEvent = null;

        public static Type LastEventCreated = null;
        public static Type LastPropertyCreated = null;

        protected override void OnEnable() {
            if (Application.isPlaying)
                base.OnEnable();
            if (!Application.isPlaying) {
                EditorApplication.update += EditorUpdate;
                LastTime = EditorApplication.timeSinceStartup;
            }
        }

        protected override void OnDisable() {
            if (Application.isPlaying)
                base.OnDisable();
            if (!Application.isPlaying)
                EditorApplication.update -= EditorUpdate;
        }

        public void UnHideAll() {
            Component[] components = gameObject.GetComponents<Component>();
            foreach (Component c in components) {
                c.hideFlags = HideFlags.None;
            }
        }

        public void EditorUpdate() {
            if (Application.isPlaying)
                return;
            if (!Playing && Mathf.Abs(CurrentTime - LastUpdateTime) < 0.001f) {
                LastTime = EditorApplication.timeSinceStartup;
                return;
            }
            float editorDeltaTime = 0f;
            if (Playing)
                editorDeltaTime = (float) (EditorApplication.timeSinceStartup - LastTime);
            if (PreviewEvent == null)
                CustomUpdate(editorDeltaTime, Playing || Recording, 0, Duration, Loop);
            else {
                //Debug.LogError("Preview mode:" + PreviewEvent.StartTime + " " + PreviewEvent.GetEndPoint());
                CustomUpdate(editorDeltaTime, Playing || Recording, PreviewEvent.StartTime, PreviewEvent.GetEndPoint(), true);
            }
            LastTime = EditorApplication.timeSinceStartup;
            LastUpdateTime = CurrentTime;
        }
#endif

        protected override void Awake() {
            if (Application.isPlaying) {
                base.Awake();
                if (PlayOnAwake) {
                    Playing = true;
                }
                for (int i = 0; i < TrackGroups.Count; i++) {
                    TrackGroups[i].LiveInit(this);
                }
            }
        }

        void OnDestroy() {
            Stop();
        }

        public override void OnPreUpdate(float deltaTime) {
            if (Application.isPlaying)
                CustomUpdate(deltaTime, Playing, 0, Duration, Loop);
        }

        public void Play(bool playFromStart = false, bool instant = false) {
            if (!gameObject.activeInHierarchy) {
                return;
            }
            O7Log.VerboseT(Tag, "Playing sequence {0} from start {1} ({3}) instant {2}.", gameObject.name, playFromStart, instant, CurrentTime);

            if (!instant) {
                if (Playing) {
                    if (playFromStart) {
                        SetCurrentTime(0f);
                        MoveToTime(0f, 0f);
                    }
                } else {
                    if (playFromStart) {
                        SetCurrentTime(0f);
                        Preplay();
                    }
                    Playing = true;
                }
            } else {
                //if instant always from start (maybe add later other possibility)
                PreviousTime = 0f;
                CurrentTime = Duration;
                for (int i = 0; i < Bookmarks.Count; i++) {
                    SequencerBookmarkEvent bookmark = Bookmarks[i];
                    if (bookmark.AffectingSequence == this &&
                        bookmark.Type == SequencerBookmarkEvent.BookmarkType.STOP &&
                        bookmark.StartTime < CurrentTime) {
                        CurrentTime = bookmark.StartTime;
                    }
                }
                Evaluate();
                if (OnEndDelegate != null) {
                    OnEndDelegate(this);
                }
            }
        }

        public void SetTimeAndForceUpdate(float time) {
            SetCurrentTime(time);
            O7Log.VerboseT(Tag, "Forcing update of sequence {0} at {1}.", gameObject.name, time);
            Evaluate(false);
        }

        public void Simulate(float time, bool fromStart = false) {
            O7Log.VerboseT(Tag, "Simulating sequence {0} to {1} from start {2}.", gameObject.name, time, fromStart);
            if (fromStart)
                PreviousTime = 0;
            CurrentTime = time;
            Evaluate(false);
        }


        public void Play(string bookmarkName, bool instant = false, bool ignoreBookmark = true) {
            if (!gameObject.activeInHierarchy) {
                return;
            }

            O7Log.VerboseT(Tag, "Playing sequence {0} from bookmark {1} instant {2}.", gameObject.name, bookmarkName, instant);

            if (!instant) {
                for (int i = 0; i < Bookmarks.Count; i++) {
                    if (bookmarkName == Bookmarks[i].BookmarkName) {
                        if (ignoreBookmark) {
                            AddIgnoredEvent(Bookmarks[i], 1);
                        }
                        var startTime = Bookmarks[i].StartTime;

                        if (Playing) {
                            SetCurrentTime(startTime);
                            MoveToTime(startTime, 0f);
                        } else {
                            SetCurrentTime(startTime);
                            Preplay();
                            Playing = true;
                        }
                        return;
                    }
                }
                Assert.IsTrue(false, "Bookmark name in sequence doesn't exists: " + bookmarkName);
            } else {
                //if instant always from start (maybe add later other possibility)
                float startTime = -1;
                for (int i = 0; i < Bookmarks.Count; i++) {
                    if (bookmarkName == Bookmarks[i].BookmarkName) {
                        if (ignoreBookmark) {
                            AddIgnoredEvent(Bookmarks[i], 1);
                        }
                        startTime = Bookmarks[i].StartTime;
                    }
                }
                if (startTime < 0) {
                    Assert.IsTrue(false, "Bookmark name in sequence doesn't exists" + bookmarkName);
                    return;
                }
                float EndTime = Duration;
                for (int i = 0; i < Bookmarks.Count; i++) {
                    SequencerBookmarkEvent bookmark = Bookmarks[i];
                    if (bookmark.AffectingSequence == this &&
                        bookmark.Type == SequencerBookmarkEvent.BookmarkType.STOP &&
                        bookmark.StartTime < EndTime &&
                        bookmark.StartTime > startTime) {
                        EndTime = bookmark.StartTime;
                    }
                }
                CurrentTime = EndTime;
                PreviousTime = startTime;
                Evaluate();
                if (OnEndDelegate != null) {
                    OnEndDelegate(this);
                }
            }
        }

        public void MoveToTime(float time, float overflow) {
            ForceSplitTime = time;
            ForceSplitOverflow = overflow;
            StopProcessing = true;
            ForceSplitUpdate = true;
        }

        public void Stop() {
            Playing = false;
        }

        public void Stop(float time, SequencerEvent srcEvent = null) {
            CurrentTime = time;
            Playing = false;
            StopProcessing = true;
            AddIgnoredEvent(srcEvent, 1);
        }

        private void AddIgnoredEvent(SequencerEvent evnt, int ignoreUpdateCount) {
            if (evnt == null) {
                return;
            }

            if (IgnoredEvents.Contains(evnt)) {
                var existingEntry = IgnoredEventsInternal.Find((x) => x.Event == evnt);
                Assert.IsTrue(existingEntry != null, "Ignored event was in hashset but not in internal list!");
                existingEntry.IgnoreCount = ignoreUpdateCount;
            } else {
                IgnoredEventsInternal.Add(new IgnoredEventEntry() {
                    Event = evnt,
                    IgnoreCount = ignoreUpdateCount
                });
                IgnoredEvents.Add(evnt);
            }
        }

        public void CustomUpdate(float deltaTime, bool playing, float start, float duration, bool loop) {
            if (deltaTime < 0f)
                deltaTime = Time.deltaTime * TimeScale;
            else
                deltaTime = deltaTime * TimeScale;

            if (playing) {
                float oldTime = CurrentTime;
                CurrentTime += deltaTime;

                if (CurrentTime >= duration) {
                    if (loop) {
                        //evaluate end part
                        CurrentTime = duration;
                        float lastPartDelta = duration - oldTime;
                        Evaluate();
                        //evaluate start part
                        float newPartDelta = deltaTime - lastPartDelta;
                        PreviousTime = start;
                        CurrentTime = start + newPartDelta;
                        Evaluate();
                    } else {
                        CurrentTime = Duration;
                        PreviousTime = oldTime;
                        Evaluate();
                        Playing = false;
                    }
                } else if (CurrentTime < 0f) {
                    Playing = false;
                    CurrentTime = 0f;
                    Evaluate();
                } else {
                    Evaluate();
                }
            }
        }

        public void Preplay() {
            for (int i = 0; i < TrackGroups.Count; i++) {
                TrackGroups[i].Preplay();
            }
        }

        public void Evaluate(bool updateIgnoredEvents = true) {
            if (updateIgnoredEvents) {
                for (int i = 0; i < IgnoredEventsInternal.Count; ++i) {
                    var entry = IgnoredEventsInternal[i];
                    --entry.IgnoreCount;
                    Assert.IsTrue(entry.IgnoreCount >= 0, "Ignore count < 0!");
                }
            }

            do {
                ActionPoints.Clear();
                ForceSplitUpdate = false;
                StopProcessing = false;

                for (int i = 0; i < TrackGroups.Count; i++) {
                    TrackGroups[i].Evaluate(this, ActionPoints, PreviousTime, CurrentTime);
                }

                for (int j = 0; j < ActionPoints.Count; ++j) {
                    ActionPoints[j].Act(PreviousTime, CurrentTime);
                    if (StopProcessing) {
                        if (ForceSplitUpdate) {
                            PreviousTime = ForceSplitTime;
                            CurrentTime = ForceSplitTime + ForceSplitOverflow;
                        }
                        break;
                    }
                }
            } while (ForceSplitUpdate);

            if (updateIgnoredEvents) {
                for (int i = 0; i < IgnoredEventsInternal.Count; ++i) {
                    var entry = IgnoredEventsInternal[i];
                    if (entry.IgnoreCount == 0) {
                        IgnoredEventsInternal.RemoveAt(i--);
                        IgnoredEvents.Remove(entry.Event);
                    }
                }

            }
            PreviousTime = CurrentTime;
        }

        public float GetCurrentTime() {
            return CurrentTime;
        }

        public float GetPreviousTime() {
            return PreviousTime;
        }

        public void SetCurrentTime(float time, bool setPrevious = true) {
            CurrentTime = time;
            if (setPrevious) {
                PreviousTime = time;
            }
        }

        public void SetIntParameter(int index, int parameter) {
            Parameters[index].SetInt(parameter);
        }

        public int GetIntParameter(int index) {
            return Parameters[index].ValueInt;
        }

        public void SetFloatParameter(int index, float parameter) {
            Parameters[index].SetFloat(parameter);
        }

        public float GetFloatParameter(int index) {
            return Parameters[index].ValueFloat;
        }

        public void SetVectorParameter(int index, Vector4 parameter) {
            Parameters[index].SetVector(parameter);
        }

        public Vector4 GetVectorParameter(int index) {
            return Parameters[index].ValueVector;
        }

        public void SetBoolParameter(int index, bool parameter) {
            Parameters[index].SetBool(parameter);
        }

        public void SetIntTriggerParameter(int index, int parameter, object userData = null) {
            Parameters[index].SetIntTrigger(parameter, userData);
        }

        public void SetBoolTriggerParameter(int index, object userData = null) {
            Parameters[index].SetBoolTrigger(userData);
        }

        public bool GetBoolParameter(int index) {
            return Parameters[index].ValueInt != 0;
        }

        public void ResetTriggerParameter(int index) {
            Parameters[index].ResetTrigger();
        }

        public Parameter GetParameterByIndex(int parameterIndex) {
            return parameterIndex != -1 ? Parameters[parameterIndex] : null;
        }

        public void AddComponentToParameter(int index, Component c) {
            Parameters[index].AddComponent(c);
        }

        public void SetComponentParameter(int index, Component c) {
            Parameters[index].SetComponent(c);
        }

        public void ClearListComponents(int index) {
            Parameters[index].ClearComponentList();
        }

        public int FindParameterIndex(string name) {
            for (int i = 0; i < Parameters.Count; i++) {
                if (name == Parameters[i].Name)
                    return Parameters[i].ParameterIndex;
            }
            return -1;
        }

        public void DisableAllContinousEvents() {
            for (int i = 0; i < TrackGroups.Count; i++) {
                for (int j = 0; j < TrackGroups[i].Tracks.Count; j++) {
                    for (int k = 0; k < TrackGroups[i].Tracks[j].Events.Count; k++) {
                        SequencerContinuousEvent continousEvent = TrackGroups[i].Tracks[j].Events[k] as SequencerContinuousEvent;
                        if (continousEvent != null)
                            continousEvent.Active = false;
                    }
                }
            }
        }

        public float NormalizedTime {
            get {
                return CurrentTime / Duration;
            }
            set {
                CurrentTime = Mathf.Clamp(value * Duration, 0f, Duration);
            }
        }
    }
}
