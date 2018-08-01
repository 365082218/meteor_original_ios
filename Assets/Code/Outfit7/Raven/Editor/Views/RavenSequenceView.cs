#if !STARLITE_EDITOR
using Outfit7.Logic.Util;
#else
using Starlite;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public class RavenSequenceView {

        protected enum ESequenceInputState {
            Inactive,
            DraggingTimeline,
            DraggingCurrentTime,
            DraggingSelection,
            DraggingTimelineScale,
            DraggingDuration,
        }

        public struct SnapInfo {
            public float Value;
            public bool SnappedFromEnd;
            public RavenEvent Event;
            public bool SnappedToEnd;
        }

        private const float c_TopHeight = 15f;
        private const float c_MinScale = 0.05f;
        private const float c_MaxScale = 200f;

        private RavenSequence m_Sequence = null;
        private List<RavenTrackGroupView> m_TrackGroupViews = new List<RavenTrackGroupView>();
        private List<RavenEventView> m_EventViews = new List<RavenEventView>();
        private Dictionary<RavenEvent, RavenEventView> m_EventViewLookup = new Dictionary<RavenEvent, RavenEventView>();
        private RavenBookmarkTrackView m_BookmarkTrackView = null;

        private Rect m_SeqNameButton = new Rect(3, 0, 65, 20);
        private Rect m_PlayButton = new Rect(70, 0, 35, 20);
        private Texture2D m_PlayTex = EditorGUIUtility.FindTexture("d_Animation.Play");
        private Rect m_RecordButton = new Rect(105, 0, 35, 20);
        private Texture2D m_RecordTex = EditorGUIUtility.FindTexture("d_Animation.Record");
        private Rect m_AddTrackButton = new Rect(125, 0, 75, 15);
        private Rect m_CurrentTimeRect = new Rect(20, 0, 40, 30);
        private Rect m_DurationTimeRect = new Rect(20, 0, 40, 30);
        private Rect m_TimelineRect = new Rect();
        private Rect m_ScrollRect = new Rect(0, 0, 500, 100);
        private Vector2 m_ScrollPosition;
        private TimelineData m_TimeLineData = null;
        private Vector2 m_MousePositionStart = new Vector2();
        private Vector2 m_ScaleMousePositionStart = new Vector2();
        private double m_ScaleMouseValueStart = -1f;
        private ESequenceInputState m_InputState = ESequenceInputState.Inactive;
        private Color m_TickColor = new Color(0.33f, 0.33f, 0.33f);
        private Color m_TransparentTickColor = new Color(0.156f, 0.156f, 0.156f);
        private double m_UpdateTime = 0f;
        private double m_LastUpdateTime = 0f;
        private float m_TimelineLenght;
        private GUIStyle m_OffTimelineBG = (GUIStyle)"AnimationKeyFrameBackground";
        private GUIStyle m_LeftPartBG = (GUIStyle)"AnimationKeyframeBackground";

        private int m_AdjustTimeFrame;
        private bool m_ShowAdjustTime = false;
        private Rect m_AdjustTimeRect;
        private AdjustTimePopup m_AdjustTimeWindow = null;

        private int m_CurrentVisibleStartFrame;
        private int m_CurrentVisibleEndFrame;

        private RavenQuickSearch m_QuickSearch = null;

        private static float s_AlwaysOnTopBarHeight = 0f;
        private static GameObject s_AnimationDatasGameObject;
        private static Dictionary<Type, List<RavenAnimationDataComponentBase>> s_AnimationDataInstances;

        public RavenSequence Sequence {
            get {
                return m_Sequence;
            }
        }

        public List<RavenEventView> EventViews {
            get {
                return m_EventViews;
            }
        }

        public TimelineData TimelineData {
            get {
                return m_TimeLineData;
            }
        }

        public void Initialize(RavenSequence sequence, TimelineData timelineData) {
            m_Sequence = sequence;
            m_TimeLineData = timelineData;
            m_Sequence.InitializeEditor();

            AddBookmarkTrack(m_Sequence.BookmarkTrack);

            if (m_Sequence.TrackGroups.Count > 0) {
                for (int i = 0; i < m_Sequence.TrackGroups.Count; i++) {
                    AddTrackGroup(m_Sequence.TrackGroups[i]);
                }
            } else {
                AddTrackGroup(null);
            }

            s_AnimationDataInstances = GetSpecializedAnimationDataTypes();
        }

        private Dictionary<Type, List<RavenAnimationDataComponentBase>> GetSpecializedAnimationDataTypes() {
            var dict = new Dictionary<Type, List<RavenAnimationDataComponentBase>>();
            // create hidden GO for components
            if (s_AnimationDatasGameObject != null) {
                GameObject.DestroyImmediate(s_AnimationDatasGameObject);
            }
            s_AnimationDatasGameObject = new GameObject("AnimationDatas");
            s_AnimationDatasGameObject.hideFlags = HideFlags.HideAndDontSave;

            var animationDataTypes = RavenUtility.GetFinalTypesForGenericType(typeof(RavenAnimationDataBase<>), false);
            for (int i = 0; i < animationDataTypes.Count; ++i) {
                var adType = animationDataTypes[i];

                // ignore the base types which are handled by default
                if (adType.BaseType.IsGenericType) {
                    continue;
                }

                // create ze instance!
                var adInstance = s_AnimationDatasGameObject.AddComponent(adType) as RavenAnimationDataComponentBase;
                List<RavenAnimationDataComponentBase> typeList;
                if (!dict.TryGetValue(adInstance.TargetType, out typeList)) {
                    typeList = new List<RavenAnimationDataComponentBase>();
                    dict[adInstance.TargetType] = typeList;
                }

                typeList.Add(adInstance);
            }

            return dict;
        }

        public Type GetSpecializedAnimationDataTypeForComponentType(Type componentType, string memberName) {
            if (componentType == null) {
                return null;
            }

            List<RavenAnimationDataComponentBase> typeList;
            if (s_AnimationDataInstances.TryGetValue(componentType, out typeList)) {
                RavenAnimationDataComponentBase specializedAnimationData = null;
                for (int i = 0; i < typeList.Count; ++i) {
                    var animationData = typeList[i];
                    var targetMemberNames = animationData.TargetMemberNames;
                    if (specializedAnimationData == null && targetMemberNames == null) {
                        specializedAnimationData = animationData;
                    }

                    if (targetMemberNames != null && Array.IndexOf(targetMemberNames, memberName) >= 0) {
                        specializedAnimationData = animationData;
                        break;
                    }
                }

                if (specializedAnimationData != null) {
                    return specializedAnimationData.GetType();
                }
            }

            return null;
        }

        public void DrawGUI(Rect windowSize, ref Vector2 scrollPosition, float startHeight, float splitViewPosition) {
            startHeight += c_TopHeight;
            m_ScrollPosition = scrollPosition;

            EditorGUI.BeginDisabledGroup(m_ShowAdjustTime);

            GUIStyle SeqButton = (GUIStyle)"toolbarbutton";
            GUIStyle Button = (GUIStyle)"OL Titlemid";
            if (GUI.Button(m_AddTrackButton, "Group+", Button)) {
                DrawEventsContextMenu();
            }
            bool wasPlayingBefore = m_Sequence.Playing;
            string buttonName = m_Sequence.name;
            if (buttonName.StartsWith("seq_")) {
                buttonName.Replace("seq_", "");
            }
            buttonName = buttonName.Substring(0, Math.Min(8, buttonName.Length));
            if (GUI.Button(m_SeqNameButton, buttonName, SeqButton)) {
                Selection.activeGameObject = m_Sequence.gameObject;
            }

            m_Sequence.Recording = GUI.Toggle(m_RecordButton, m_Sequence.Recording, m_RecordTex);

            var playing = GUI.Toggle(m_PlayButton, m_Sequence.Playing, m_PlayTex);
            if (wasPlayingBefore && !playing) {
                m_Sequence.Recording = false;
            }
            playing |= m_Sequence.Recording;
            if (playing || (playing && m_Sequence.Recording)) {
                if (!wasPlayingBefore) {
                    for (int i = 0; i < m_Sequence.TrackGroups.Count; i++) {
                        m_Sequence.TrackGroups[i].Initialize(m_Sequence);
                    }

                    if (m_Sequence.CurrentTime >= m_Sequence.Duration) {
                        m_Sequence.JumpToTime(0f);
                    }
                    m_Sequence.Resume();
                }
            } else if (wasPlayingBefore) {
                m_Sequence.Stop();
                m_Sequence.Recording = false;
            }

            EditorGUI.LabelField(new Rect(splitViewPosition, 0f, windowSize.width, c_TopHeight), "", "", m_OffTimelineBG);
            m_TimeLineData.m_Scale = EditorGUI.Slider(new Rect(windowSize.width - 100f, 0, 50f, 18f), "Scale", m_TimeLineData.m_Scale, c_MinScale, c_MaxScale);

            DrawCurrentTime(windowSize, startHeight, splitViewPosition);
            CalculateDurationRect(windowSize, startHeight, splitViewPosition);
            DrawTimeline(windowSize, startHeight, splitViewPosition);

            float currentHeight = 0f;
            DrawAlwaysOnTopBar(windowSize, startHeight, splitViewPosition, ref currentHeight);

            if (m_InputState == ESequenceInputState.DraggingSelection) {
                EditorGUI.DrawRect(new Rect(m_MousePositionStart, UnityEngine.Event.current.mousePosition - m_MousePositionStart), new Color(1, 1, 1, 0.2f));
            }

            scrollPosition = GUI.BeginScrollView(new Rect(0, startHeight + currentHeight, windowSize.width, windowSize.height - currentHeight - startHeight), scrollPosition, m_ScrollRect, false, false);

            currentHeight = 0f;
            for (int i = 0; i < m_TrackGroupViews.Count; i++) {
                currentHeight += m_TrackGroupViews[i].DrawTimelineGui(windowSize, currentHeight, splitViewPosition, m_TimeLineData, m_Sequence.Parameters);
            }

            EditorGUI.LabelField(new Rect(0, 0, splitViewPosition, m_ScrollRect.height), "", m_LeftPartBG);

            currentHeight = 0f;
            for (int i = 0; i < m_TrackGroupViews.Count; i++) {
                currentHeight += m_TrackGroupViews[i].DrawSidebarGui(windowSize, currentHeight, splitViewPosition, m_TimeLineData, m_Sequence.Parameters);
            }

            GUI.EndScrollView();
            m_ScrollRect.width = windowSize.width - 15f;
            m_ScrollRect.height = currentHeight;

            if (m_QuickSearch != null && m_QuickSearch.OnGui()) {
                m_QuickSearch = null;
            }

            EditorGUI.EndDisabledGroup();

            if (m_ShowAdjustTime && m_AdjustTimeWindow == null) {
                m_AdjustTimeWindow = new AdjustTimePopup(AdjustTimeImpl, AdjustTimeWindowOnClose);
                PopupWindow.Show(m_AdjustTimeRect, m_AdjustTimeWindow);
            }
        }

        private void DrawAlwaysOnTopBar(Rect windowSize, float startHeight, float splitViewPosition, ref float height) {
            height = c_TopHeight;
            height += m_BookmarkTrackView.DrawTimelineGui(windowSize, startHeight + height, splitViewPosition, m_TimeLineData, m_Sequence.Parameters);
            s_AlwaysOnTopBarHeight = height;
            EditorGUI.LabelField(new Rect(0, startHeight, splitViewPosition, height), "", m_LeftPartBG);
            m_BookmarkTrackView.DrawSidebarGui(windowSize, startHeight + c_TopHeight, splitViewPosition, m_TimeLineData, m_Sequence.Parameters);
        }

        public void SetTrackGroupsViewMode(RavenTrackGroup.ERavenTrackGroupMode viewMode) {
            for (int i = 0; i < m_TrackGroupViews.Count; ++i) {
                m_TrackGroupViews[i].TrackGroup.m_TrackGroupMode = viewMode;
            }
        }

        protected void DrawEventsContextMenu() {
            // We only add one type
            AddTrackGroup(null);
        }

        public bool IsEventVisible(RavenEventView eventView) {
            if (eventView.AlwaysVisible) {
                return true;
            }
            var evnt = eventView.Event;
            var startFrameInRect = evnt.StartFrame >= m_CurrentVisibleStartFrame && evnt.StartFrame <= m_CurrentVisibleEndFrame;
            var lastFrameInRect = evnt.LastFrame >= m_CurrentVisibleStartFrame && evnt.LastFrame <= m_CurrentVisibleEndFrame;
            var framesBeyondEnds = evnt.StartFrame < m_CurrentVisibleStartFrame && evnt.LastFrame > m_CurrentVisibleEndFrame;
            return (startFrameInRect || lastFrameInRect || framesBeyondEnds) && eventView.EventRect.yMax >= m_ScrollPosition.y && eventView.EventRect.yMin <= m_ScrollRect.yMax;
        }

        private void DrawTimeline(Rect windowSize, float startHeight, float splitViewPosition) {
            m_TimelineLenght = windowSize.width - splitViewPosition;
            m_TimelineRect = new Rect(splitViewPosition, startHeight, m_TimelineLenght, m_ScrollRect.height + s_AlwaysOnTopBarHeight);
            m_TimeLineData.m_LenghtOfASecond = m_TimeLineData.m_Scale * TimelineData.c_PixelsPerSecond;
            if (m_Sequence.Duration <= 0) {
                return;
            }
            m_TimeLineData.m_Rect = new Rect(splitViewPosition, startHeight + c_TopHeight, windowSize.width - splitViewPosition, m_ScrollRect.height + s_AlwaysOnTopBarHeight - c_TopHeight);

            var timeAtStart = m_TimeLineData.GetTimeAtMousePosition(m_TimeLineData.m_Rect.min);
            if (m_TimeLineData.GetTimeAtMousePosition(m_TimeLineData.m_Rect.min) < 0) {
                EditorGUI.LabelField(new Rect(m_TimeLineData.m_Rect.min, new Vector2((float)(m_TimeLineData.m_LenghtOfASecond * m_TimeLineData.m_Offset), m_TimeLineData.m_Rect.height)), "", m_OffTimelineBG);
            }

            var timeAtEnd = m_TimeLineData.GetTimeAtMousePosition(m_TimeLineData.m_Rect.max);
            var start = (m_Sequence.Duration + m_TimeLineData.m_Offset) * m_TimeLineData.m_LenghtOfASecond;
            if (timeAtEnd > m_Sequence.Duration) {
                EditorGUI.LabelField(new Rect(m_TimeLineData.m_Rect.min.x + (float)start, m_TimeLineData.m_Rect.min.y, m_TimeLineData.m_Rect.width - (float)start, m_TimeLineData.m_Rect.height), "", m_OffTimelineBG);
            }

            // Only draw what we see, and not everything if too small
            // Apply offset (since beginning of current frame) to prevent dragging the view along

            m_CurrentVisibleStartFrame = (int)m_Sequence.GetFrameForTime(timeAtStart);
            var offset = (timeAtStart - m_Sequence.GetTimeForFrame((int)m_CurrentVisibleStartFrame)) * m_TimeLineData.m_LenghtOfASecond;
            var timelineOffsetInSeconds = Math.Max(m_TimeLineData.m_Offset, 0);
            var currentPosition = m_TimeLineData.m_Rect.xMin + (float)(m_TimeLineData.m_LenghtOfASecond * timelineOffsetInSeconds);
            if (timelineOffsetInSeconds == 0) {
                currentPosition -= (float)offset;
            }

            m_CurrentVisibleEndFrame = (int)m_Sequence.GetFrameForTime(timeAtEnd) + 1;
            var maxPosition = currentPosition + m_TimeLineData.GetWidthForFrames(Math.Min(m_CurrentVisibleEndFrame, m_Sequence.TotalFrameCount) - Math.Max(m_CurrentVisibleStartFrame, 0));
            int frameIndex = Math.Max(m_CurrentVisibleStartFrame, 0);

            var frameIncrement = 1;
            var width = (float)(m_TimeLineData.m_LenghtOfASecond / m_Sequence.Fps); //every frame tick
            while (width < 10f) {
                width *= 2f;
                frameIncrement *= 2;
            }

            // Offset currentPosition so we match major frame lines no matter where the view starts (else it "drags" the view along when we scroll)
            var widthSinceFullSecond = m_TimeLineData.GetWidthForFrames(frameIndex % m_Sequence.Fps);
            var nWidthsInWidthSinceFullSecond = widthSinceFullSecond / width;
            var widthRemainder = nWidthsInWidthSinceFullSecond - (int)nWidthsInWidthSinceFullSecond;
            var frameOffset = (int)(widthRemainder * frameIncrement);

            frameIndex -= frameOffset;
            currentPosition -= m_TimeLineData.GetWidthForFrames(frameOffset);

            while (currentPosition <= maxPosition) {
                var frameMod = frameIndex % m_Sequence.Fps;
                bool isMajorLine = frameMod == 0;
                if (currentPosition > splitViewPosition) {
                    GUIUtil.DrawFatLine(new Vector2(currentPosition, startHeight), new Vector2(currentPosition, m_ScrollRect.height + startHeight + s_AlwaysOnTopBarHeight), isMajorLine ? 6 : 2, isMajorLine ? m_TickColor : Color.Lerp(m_TransparentTickColor, m_TickColor, Mathf.InverseLerp(2, 20, width)));
                    if (isMajorLine) {
                        GUIUtil.DrawLine(new Vector2(currentPosition, startHeight), new Vector2(currentPosition, startHeight + c_TopHeight), Color.white);
                        var durationAtLine = m_TimeLineData.GetTimeAtMousePosition(new Vector2(currentPosition, startHeight)).ToString("F0");
                        EditorGUI.LabelField(new Rect(currentPosition + 5f, startHeight, EditorStyles.label.CalcSize(new GUIContent(durationAtLine)).x, c_TopHeight), durationAtLine);
                    } else {
                        GUIUtil.DrawLine(new Vector2(currentPosition, startHeight + c_TopHeight / 2f), new Vector2(currentPosition, startHeight + c_TopHeight), Color.grey);
                    }
                }

                // check if we would skip major line, if so, scale increment down

                var newFrameMod = (frameIndex + frameIncrement) % Sequence.Fps;
                if (newFrameMod < frameMod) {
                    var wantedFrameIncrement = m_Sequence.Fps - frameMod;
                    var newFrameIncrement = frameIncrement;
                    var newWidth = width;
                    while (newFrameIncrement > 1 && newFrameIncrement > wantedFrameIncrement) {
                        newFrameIncrement /= 2;
                        newWidth /= 2;
                    }
                    currentPosition += newWidth;
                    frameIndex += newFrameIncrement;
                } else {
                    currentPosition += width;
                    frameIndex += frameIncrement;
                }
            }
        }

        private void DrawCurrentTime(Rect windowSize, float startHeight, float splitViewPosition) {
            float timePosition = (float)(splitViewPosition + m_TimeLineData.m_LenghtOfASecond * (m_TimeLineData.m_Offset + m_Sequence.CurrentTime));
            if (timePosition > splitViewPosition) {
                GUIUtil.DrawFatLine(new Vector2(timePosition, 0), new Vector2(timePosition, windowSize.height), 2, Color.red);
                m_CurrentTimeRect = new Rect(timePosition - 5f, 0, 10, c_TopHeight * 2f);
                EditorGUIUtility.AddCursorRect(m_CurrentTimeRect, MouseCursor.ResizeHorizontal);
            } else {
                m_CurrentTimeRect = new Rect(0, 0, 0, 0);
            }
        }

        private void CalculateDurationRect(Rect windowSize, float startHeight, float splitViewPosition) {
            float timePosition = (float)(splitViewPosition + m_TimeLineData.m_LenghtOfASecond * (m_TimeLineData.m_Offset + m_Sequence.Duration));
            if (timePosition > splitViewPosition) {
                m_DurationTimeRect = new Rect(timePosition - 5f, startHeight, 10, c_TopHeight);
                EditorGUIUtility.AddCursorRect(m_DurationTimeRect, MouseCursor.ResizeHorizontal);
            } else {
                m_DurationTimeRect = new Rect(0, 0, 0, 0);
            }
        }

        public void ToggleAllTracks(bool enabled, params RavenTrackView[] exclusions) {
            for (int i = 0; i < m_TrackGroupViews.Count; ++i) {
                var trackGrp = m_TrackGroupViews[i];
                for (int j = 0; j < trackGrp.TrackViews.Count; ++j) {
                    var track = trackGrp.TrackViews[j];
                    if (ArrayUtility.Contains(exclusions, track)) {
                        continue;
                    }
                    var oldEnabled = track.Track.IsEnabled;
                    track.Track.IsEnabled = enabled;
                    if (track.Track.IsEnabled != oldEnabled) {
                        m_Sequence.FlagDirty();
                    }
                }
            }
        }

        public void SelectEvents() {
            Rect nonNormalized = new Rect(m_MousePositionStart, UnityEngine.Event.current.mousePosition - m_MousePositionStart);
            Rect normalized = new Rect();
            normalized.x = Mathf.Min(nonNormalized.xMin, nonNormalized.xMax);
            normalized.y = Mathf.Min(nonNormalized.yMin, nonNormalized.yMax);
            normalized.width = Mathf.Abs(nonNormalized.width);
            normalized.height = Mathf.Abs(nonNormalized.height);

            var mousePosInScrollRectStart = ConvertMousePositionToMousePositionInScrollRect(m_MousePositionStart);
            var mousePosInScrollRectNow = GetCurrentMousePositionInScrollRect();
            Rect nonNormalizedInScrollRect = new Rect(mousePosInScrollRectStart, mousePosInScrollRectNow - mousePosInScrollRectStart);
            Rect normalizedInScrollRect = new Rect();
            normalizedInScrollRect.x = Mathf.Min(nonNormalizedInScrollRect.xMin, nonNormalizedInScrollRect.xMax);
            normalizedInScrollRect.y = Mathf.Min(nonNormalizedInScrollRect.yMin, nonNormalizedInScrollRect.yMax);
            normalizedInScrollRect.width = Mathf.Abs(nonNormalizedInScrollRect.width);
            normalizedInScrollRect.height = Mathf.Abs(nonNormalizedInScrollRect.height);

            m_BookmarkTrackView.SelectEvents(normalized);
            for (int i = 0; i < m_TrackGroupViews.Count; i++) {
                m_TrackGroupViews[i].SelectEvents(normalizedInScrollRect);
            }
        }

        public bool CheckFrameOverlap(RavenEvent srcEvnt, int frame, int trackIndex, int subTrackIndex, bool nonSelectedOnly = false) {
            if (nonSelectedOnly) {
                for (int i = 0; i < m_EventViews.Count; ++i) {
                    var view = m_EventViews[i];
                    if (view.m_Selected) {
                        continue;
                    }
                    if (CheckFrameOverlapInternal(srcEvnt, view.Event, frame, trackIndex, subTrackIndex)) {
                        return true;
                    }
                }
            } else {
                for (int i = 0; i < m_Sequence.SortedEvents.Count; ++i) {
                    if (CheckFrameOverlapInternal(srcEvnt, m_Sequence.SortedEvents[i], frame, trackIndex, subTrackIndex)) {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CheckContinuousFrameOverlap(RavenEvent srcEvnt, int startFrame, int lastFrame, int trackIndex, int subTrackIndex, bool nonSelectedOnly = false) {
            if (nonSelectedOnly) {
                for (int i = 0; i < m_EventViews.Count; ++i) {
                    var view = m_EventViews[i];
                    if (view.m_Selected) {
                        continue;
                    }
                    if (CheckContinuousFrameOverlapInternal(srcEvnt, view.Event, startFrame, lastFrame, trackIndex, subTrackIndex)) {
                        return true;
                    }
                }
            } else {
                for (int i = 0; i < m_Sequence.SortedEvents.Count; ++i) {
                    if (CheckContinuousFrameOverlapInternal(srcEvnt, m_Sequence.SortedEvents[i], startFrame, lastFrame, trackIndex, subTrackIndex)) {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CheckFrameOverlapOfSelectedEvents() {
            for (int i = 0; i < m_EventViews.Count; ++i) {
                var view = m_EventViews[i];
                if (!view.m_Selected) {
                    continue;
                }

                var evnt = m_EventViews[i].Event;
                var triggerEvnt = evnt.EventType == ERavenEventType.Trigger || evnt.EventType == ERavenEventType.Bookmark;
                for (int j = i + 1; j < m_EventViews.Count; ++j) {
                    var view2 = m_EventViews[j];
                    var evnt2 = view2.Event;
                    if (!view2.m_Selected) {
                        continue;
                    }

                    if (triggerEvnt) {
                        if (evnt.TrackIndex == evnt2.TrackIndex &&
                            evnt.SubTrackIndex == evnt2.SubTrackIndex &&
                            view.QueuedStartFrameReposition >= view2.QueuedStartFrameReposition &&
                            view.QueuedStartFrameReposition <= view2.QueuedLastFrameReposition) {
                            return true;
                        }
                    } else {
                        if (evnt.TrackIndex == evnt2.TrackIndex &&
                            evnt.SubTrackIndex == evnt2.SubTrackIndex &&
                            (view.QueuedStartFrameReposition >= view2.QueuedStartFrameReposition && view.QueuedStartFrameReposition <= view2.QueuedLastFrameReposition ||
                            view.QueuedLastFrameReposition >= view2.QueuedStartFrameReposition && view.QueuedLastFrameReposition <= view2.QueuedLastFrameReposition ||
                            view.QueuedStartFrameReposition < view2.QueuedStartFrameReposition && view.QueuedLastFrameReposition > view2.QueuedLastFrameReposition)) {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool CheckFrameOverlapInternal(RavenEvent srcEvnt, RavenEvent evnt, int frame, int trackIndex, int subTrackIndex) {
            if (srcEvnt == evnt) {
                return false;
            }

            if (evnt.TrackIndex == trackIndex &&
                evnt.SubTrackIndex == subTrackIndex &&
                frame >= evnt.StartFrame &&
                frame <= evnt.LastFrame) {
                return true;
            }

            return false;
        }

        private bool CheckContinuousFrameOverlapInternal(RavenEvent srcEvnt, RavenEvent evnt, int startFrame, int lastFrame, int trackIndex, int subTrackIndex) {
            if (srcEvnt == evnt) {
                return false;
            }
            if (evnt.TrackIndex == trackIndex &&
                evnt.SubTrackIndex == subTrackIndex &&
                (startFrame >= evnt.StartFrame && startFrame <= evnt.LastFrame ||
                lastFrame >= evnt.StartFrame && lastFrame <= evnt.LastFrame ||
                startFrame < evnt.StartFrame && lastFrame > evnt.LastFrame)) {
                return true;
            }

            return false;
        }

        public bool HandleInput() {
            bool closeQuickSearch;
            if (m_QuickSearch != null && m_QuickSearch.HandleInput(out closeQuickSearch)) {
                if (closeQuickSearch) {
                    m_QuickSearch = null;
                }
                return true;
            }

            var mousePosition = UnityEngine.Event.current.mousePosition;
            if (!Application.isPlaying) {
                if ((m_Sequence.Recording || m_Sequence.Playing) && !AnimationMode.InAnimationMode()) {
                    AnimationMode.StartAnimationMode();
                    StartRecording();
                } else if (!m_Sequence.Recording && !m_Sequence.Playing && AnimationMode.InAnimationMode()) {
                    AnimationMode.StopAnimationMode();
                    EndRecording();
                }
            }

            if (EventType.KeyDown == UnityEngine.Event.current.type && KeyCode.Space == UnityEngine.Event.current.keyCode) {
                if (m_Sequence.Playing) {
                    m_Sequence.Stop();
                } else {
                    m_Sequence.Resume();
                }
                return true;
            }

            //timeline scaling
            if (UnityEngine.Event.current.type == EventType.ScrollWheel) {
                m_MousePositionStart = mousePosition;
                m_TimeLineData.m_Scale += UnityEngine.Event.current.delta.y / (20f);
                m_TimeLineData.m_Scale = Mathf.Clamp(m_TimeLineData.m_Scale, c_MinScale, c_MaxScale);
                m_TimeLineData.m_LenghtOfASecond = m_TimeLineData.m_Scale * TimelineData.c_PixelsPerSecond;

                m_TimeLineData.m_Offset += m_TimeLineData.GetTimeAtMousePosition(m_ScaleMousePositionStart) - m_ScaleMouseValueStart;
                return true;
            }
            if (m_InputState == ESequenceInputState.DraggingTimeline && (UnityEngine.Event.current.type == EventType.mouseDrag)) {
                float mouseDelta = mousePosition.x - m_MousePositionStart.x;
                m_MousePositionStart = mousePosition;
                m_TimeLineData.m_Offset += mouseDelta / m_TimeLineData.m_LenghtOfASecond;
                return true;
            } else if (m_InputState == ESequenceInputState.DraggingTimelineScale && (UnityEngine.Event.current.type == EventType.mouseDrag)) {
                float mouseDelta = mousePosition.x - m_MousePositionStart.x;
                m_MousePositionStart = mousePosition;

                m_TimeLineData.m_Scale += mouseDelta / (250f) * (float)Math.Pow(Math.Max(m_TimeLineData.m_Scale * (c_MaxScale - c_MinScale) - (c_MaxScale - c_MinScale), 1), 0.5); // multiply it by sqrt distance off base to scale it faster when we're farther from baseline
                m_TimeLineData.m_Scale = Mathf.Clamp(m_TimeLineData.m_Scale, c_MinScale, c_MaxScale);
                m_TimeLineData.m_LenghtOfASecond = m_TimeLineData.m_Scale * TimelineData.c_PixelsPerSecond;

                m_TimeLineData.m_Offset += m_TimeLineData.GetTimeAtMousePosition(m_ScaleMousePositionStart) - m_ScaleMouseValueStart;

                return true;
            } else if (m_InputState == ESequenceInputState.DraggingCurrentTime && (UnityEngine.Event.current.type == EventType.mouseDrag)) {
                //var position = Snap(TimeLineData.GetTimeAtMousePosition(mousePosition), null).Value;
                var position = m_TimeLineData.GetTimeAtMousePosition(mousePosition);
                m_Sequence.JumpToTime(Math.Min(Math.Max(position, 0), m_Sequence.Duration));
                return true;
            } else if (m_InputState == ESequenceInputState.DraggingDuration && (UnityEngine.Event.current.type == EventType.mouseDrag)) {
                //var position = Snap(TimeLineData.GetTimeAtMousePosition(mousePosition), null).Value;
                var position = m_TimeLineData.GetTimeAtMousePosition(mousePosition);
                m_Sequence.SetDuration(Math.Min(Math.Max(position, 0), 100), false);
                return true;
            }

            if (m_BookmarkTrackView.HandleInput(mousePosition, m_TimeLineData, this)) {
                return true;
            }

            if (!m_BookmarkTrackView.ContainsMousePosition(mousePosition)) {
                for (int i = 0; i < m_TrackGroupViews.Count; i++) {
                    if (m_TrackGroupViews[i].HandleInput(m_TimeLineData)) {
                        return true;
                    }
                }
            }

            if (UnityEngine.Event.current.type == EventType.mouseUp || UnityEngine.Event.current.type == EventType.Ignore) {
                SetInputState(ESequenceInputState.Inactive);
                return true;
            }

            if (m_TimeLineData.m_Rect.Contains(mousePosition)) {
                if (UnityEngine.Event.current.type == EventType.mouseDown && UnityEngine.Event.current.button == 2 && UnityEngine.Event.current.alt) {
                    SetInputState(ESequenceInputState.DraggingTimelineScale);
                    m_MousePositionStart = mousePosition;
                    m_ScaleMousePositionStart = mousePosition;
                    m_ScaleMouseValueStart = m_TimeLineData.GetTimeAtMousePosition(mousePosition);
                    return true;
                } else if (UnityEngine.Event.current.type == EventType.mouseDown &&
                    (UnityEngine.Event.current.button == 2 || (UnityEngine.Event.current.alt && UnityEngine.Event.current.button == 0))) {
                    SetInputState(ESequenceInputState.DraggingTimeline);
                    m_MousePositionStart = mousePosition;
                    return true;
                }
            } else if (m_CurrentTimeRect.Contains(mousePosition)) {
                if (UnityEngine.Event.current.type == EventType.mouseDown && UnityEngine.Event.current.button == 0) {
                    SetInputState(ESequenceInputState.DraggingCurrentTime);
                    m_MousePositionStart = mousePosition;
                    return true;
                }
            } else if (m_DurationTimeRect.Contains(mousePosition)) {
                if (UnityEngine.Event.current.type == EventType.mouseDown && UnityEngine.Event.current.button == 0) {
                    SetInputState(ESequenceInputState.DraggingDuration);
                    m_MousePositionStart = mousePosition;
                    return true;
                }
            }

            //Click on Timeline
            if (m_InputState == ESequenceInputState.Inactive && UnityEngine.Event.current.type == EventType.mouseDown) {
                Rect TopTimelineRect = new Rect(m_TimelineRect.x, 0, m_TimelineRect.width, 2 * c_TopHeight);
                if (TopTimelineRect.Contains(mousePosition)) {
                    var position = m_TimeLineData.GetTimeAtMousePosition(mousePosition);
                    m_Sequence.JumpToTime(Math.Min(Math.Max(position, 0), m_Sequence.Duration));
                    SetInputState(ESequenceInputState.DraggingCurrentTime);
                    GUIUtility.keyboardControl = -1;
                    return true;
                }
            }
            if (m_TimelineRect.Contains(mousePosition)) {
                if (UnityEngine.Event.current.type == EventType.mouseDown && UnityEngine.Event.current.button == 0) {
                    SetInputState(ESequenceInputState.DraggingSelection);
                    m_MousePositionStart = mousePosition;

                    GUIUtility.keyboardControl = -1;
                    return true;
                }
            }
            if (m_Sequence.Recording && !HasTimeChanged() && m_InputState == ESequenceInputState.Inactive) {
                for (int i = 0; i < m_TrackGroupViews.Count; ++i) {
                    m_TrackGroupViews[i].UpdateWhileRecording(m_Sequence.CurrentTime);
                }
            }

            if (UnityEngine.Event.current.type == EventType.KeyDown) {
                if (UnityEngine.Event.current.keyCode == KeyCode.F) {
                    if (UnityEngine.Event.current.modifiers == EventModifiers.Shift) {
                        FocusSelectedEvent();
                    } else if (UnityEngine.Event.current.modifiers == EventModifiers.Control) {
                        FocusTimeline(m_Sequence.CurrentTime);
                    }
                } else if (UnityEngine.Event.current.keyCode == KeyCode.R &&
                    UnityEngine.Event.current.modifiers == EventModifiers.Shift) {
                    ResetTimelineZoom();
                } else if (UnityEngine.Event.current.keyCode == KeyCode.Delete || UnityEngine.Event.current.keyCode == KeyCode.Backspace) {
                    for (int i = 0; i < m_EventViews.Count; i++) {
                        var eventView = m_EventViews[i];
                        if (eventView.m_Selected) {
                            eventView.DeselectEvent();
                            eventView.DestroyEvent();
                            --i;
                        }
                    }
                }
            }

            if (UnityEngine.Event.current.type == EventType.mouseDown && UnityEngine.Event.current.button == 0) {
                GUIUtility.keyboardControl = -1;
            }

            return false;
        }

        private void SetInputState(ESequenceInputState newState) {
            if (newState == m_InputState) {
                return;
            }

            switch (m_InputState) {
                case ESequenceInputState.DraggingCurrentTime:
                case ESequenceInputState.DraggingTimeline:
                case ESequenceInputState.DraggingTimelineScale:
                    break;

                case ESequenceInputState.DraggingDuration:
                    m_Sequence.SetDuration(m_Sequence.Duration);
                    break;

                case ESequenceInputState.DraggingSelection:
                    SelectEvents();
                    break;
            }

            m_InputState = newState;
        }

        public bool HasTimeChanged() {
            return m_UpdateTime != m_LastUpdateTime;
        }

        private void StartRecording() {
            for (int i = 0; i < m_TrackGroupViews.Count; ++i) {
                m_TrackGroupViews[i].RecordingStart();
            }
        }

        private void EndRecording() {
            for (int i = 0; i < m_TrackGroupViews.Count; ++i) {
                m_TrackGroupViews[i].RecordingStop();
            }
        }

        public bool IsRecording() {
            return m_Sequence.Recording;
        }

        public void PrePaint() {
            m_UpdateTime = m_Sequence.CurrentTime;
        }

        public void PostPaint() {
            m_LastUpdateTime = m_UpdateTime;
        }

        public bool AddTrackGroup(RavenTrackGroup trackGroup) {
            Undo.RecordObject(m_Sequence, "AddTrackGroup");
            if (trackGroup == null) {
                trackGroup = Undo.AddComponent<RavenTrackGroup>(m_Sequence.gameObject);
                if (Selection.gameObjects.Length == 1 && Selection.gameObjects[0] != m_Sequence.gameObject) {
                    trackGroup.m_Target = Selection.gameObjects[0];
                }
                m_Sequence.AddTrackGroup(trackGroup);
                EditorUtility.SetDirty(m_Sequence);
            }

            trackGroup.hideFlags = HideFlags.HideInInspector;
            RavenTrackGroupView instance = new RavenTrackGroupView();
            instance.Initialize(trackGroup, this);
            m_TrackGroupViews.Add(instance);
            return true;
        }

        private void AddBookmarkTrack(RavenBookmarkTrack bookmarkTrack) {
            Undo.RecordObject(m_Sequence, "AddTrackGroup");
            if (bookmarkTrack == null) {
                bookmarkTrack = Undo.AddComponent<RavenBookmarkTrack>(m_Sequence.gameObject);
                m_Sequence.BookmarkTrack = bookmarkTrack;
            }

            bookmarkTrack.hideFlags = HideFlags.HideInInspector;
            var instance = new RavenBookmarkTrackView();
            instance.Initialize(bookmarkTrack, null);
            m_BookmarkTrackView = instance;
        }

        public void OnEventViewDestroyed(RavenEventView eventView) {
            m_Sequence.RemoveEvent(eventView.Event);
            RemoveEventView(eventView);
        }

        public void DeselectAll() {
            for (int i = 0; i < m_BookmarkTrackView.EventViews.Count; ++i) {
                m_BookmarkTrackView.EventViews[i].DeselectEvent();
            }
            for (int i = 0; i < m_TrackGroupViews.Count; ++i) {
                var groupView = m_TrackGroupViews[i];
                for (int j = 0; j < groupView.TrackViews.Count; ++j) {
                    var trackView = groupView.TrackViews[j];
                    for (int k = 0; k < trackView.EventViews.Count; ++k) {
                        trackView.EventViews[k].DeselectEvent();
                    }
                }
            }
        }

        public void AdjustTime(object obj) {
            m_AdjustTimeFrame = (int)obj;
            m_ShowAdjustTime = true;
            m_AdjustTimeRect = new Rect(Vector2.Scale(new Vector2(m_AdjustTimeFrame / (float)m_Sequence.TotalFrameCount, 0.5f), m_DurationTimeRect.max), new Vector2(Screen.width / 6f, Screen.height / 6f));
        }

        private void AdjustTimeImpl(double time) {
            if (time > 0) {
                InsertTimeImpl(time);
            } else if (time < 0) {
                RemoveTimeImpl(m_AdjustTimeFrame, m_AdjustTimeFrame + (int)m_Sequence.GetFrameForTime(-time), null);
            }
        }

        private void InsertTimeImpl(double time) {
            Undo.RecordObject(m_Sequence, "InsertTime");
            m_Sequence.SetDuration(m_Sequence.Duration + time);
            for (int i = 0; i < m_Sequence.SortedEvents.Count; ++i) {
                var evnt = m_Sequence.SortedEvents[i];
                if (evnt.StartFrame >= m_AdjustTimeFrame) {
                    Undo.RecordObject(evnt, "InsertTime");
                    evnt.OffsetEvent((int)m_Sequence.GetFrameForTime(time));
                }
            }
        }

        public void RemoveTimeImpl(int startFrame, int endFrame, RavenEvent eventToDestroy) {
            var delta = endFrame - startFrame;
            RavenEventView viewToDestroy = null;
            if (eventToDestroy != null) {
                GetEventView(eventToDestroy, out viewToDestroy);
            }

            Undo.RecordObject(m_Sequence, "RemoveTime");
            for (int i = 0; i < m_Sequence.SortedEvents.Count; ++i) {
                var evnt = m_Sequence.SortedEvents[i];
                RavenEventView evntView;
                if (!m_EventViewLookup.TryGetValue(evnt, out evntView)) {
                    continue;
                }
                bool timeCheck = evnt.StartFrame >= startFrame && evnt.StartFrame < endFrame;
                bool notBookmarkCheck = !(evnt is RavenBookmarkEvent);

                if ((timeCheck && notBookmarkCheck) || evntView == viewToDestroy) {
                    evntView.DestroyEvent();
                    --i;
                } else if (evnt.StartFrame >= endFrame) {
                    Undo.RecordObject(evnt, "RemoveTime");
                    // try to avoid fpu errors
                    evnt.OffsetEvent(-delta);
                }
            }
            m_Sequence.SetDuration(m_Sequence.Duration - m_Sequence.GetTimeForFrame(delta));
            RavenSequenceEditor.Instance.RemakeView();
        }

        private void AdjustTimeWindowOnClose() {
            m_AdjustTimeWindow = null;
            m_ShowAdjustTime = false;
        }

        public void RemoveTrackGroup(object obj) {
            RavenTrackGroupView trackView = (RavenTrackGroupView)obj;
            m_Sequence.RemoveTrackGroup(trackView.TrackGroup);
            trackView.DestroyTrackGroup();
            m_TrackGroupViews.Remove(trackView);
        }

        public void MoveTrackGroupDown(object obj) {
            RavenTrackGroupView trackView = (RavenTrackGroupView)obj;

            int index = m_Sequence.TrackGroups.IndexOf(trackView.TrackGroup);
            if (index == m_TrackGroupViews.Count - 1) {
                return;
            }

            var newIndex = index + 1;

            RavenTrackGroupView bottomTrackGroupView = m_TrackGroupViews[newIndex];

            m_Sequence.SwapTrackGroups(index, newIndex);

            m_TrackGroupViews[index] = bottomTrackGroupView;
            m_TrackGroupViews[newIndex] = trackView;
        }

        public void MoveTrackGroupUp(object obj) {
            RavenTrackGroupView trackView = (RavenTrackGroupView)obj;

            int index = m_Sequence.TrackGroups.IndexOf(trackView.TrackGroup);
            if (index == 0) {
                return;
            }

            var newIndex = index - 1;

            RavenTrackGroupView bottomTrackGroupView = m_TrackGroupViews[newIndex];

            m_Sequence.SwapTrackGroups(index, newIndex);

            m_TrackGroupViews[index] = bottomTrackGroupView;
            m_TrackGroupViews[newIndex] = trackView;
        }

        public void FocusTimeline(int frame) {
            FocusTimeline(m_Sequence.FrameDuration * frame);
        }

        public void FocusTimeline(double startTime) {
            m_TimeLineData.m_Offset = -startTime + 0.1 / m_TimeLineData.m_Scale;
        }

        public void ResetTimelineZoom() {
            m_TimeLineData.ResetScale();
        }

        public void FocusSelectedEvent() {
            RavenEvent evnt = null;
            for (int i = 0; i < m_EventViews.Count; ++i) {
                var eventView = m_EventViews[i];
                if (eventView.m_Selected) {
                    evnt = eventView.Event;
                    break;
                }
            }

            if (evnt != null) {
                // have to do it manually here because of trigger events
                var eventDuration = evnt.LastFrame - evnt.StartFrame + 1;
                m_TimeLineData.SetScaleToFitFrames(eventDuration);
                m_TimeLineData.m_Offset = -m_Sequence.GetTimeForFrame(evnt.StartFrame) + 0.1 / m_TimeLineData.m_Scale;
            }
        }

        public void AddEventView(RavenEventView eventView) {
            m_EventViewLookup[eventView.Event] = eventView;
            RavenAssert.IsTrue(m_EventViews.Contains(eventView) == false, "Dupe event view");
            m_EventViews.Add(eventView);
        }

        public void RemoveEventView(RavenEventView eventView) {
            m_EventViewLookup.Remove(eventView.Event);
            m_EventViews.Remove(eventView);
        }

        public bool GetEventView(RavenEvent evnt, out RavenEventView eventView) {
            return m_EventViewLookup.TryGetValue(evnt, out eventView);
        }

        public bool GenerateQuickSearchForAnimationProperty(ERavenAnimationDataFilter animationDataFilter, List<RavenPropertyTrackView.PropertyEventMenuData> menuData, Action<object> callback) {
            Type animationDataTypeBase = null;
            switch (animationDataFilter) {
                case ERavenAnimationDataFilter.Tween:
                    animationDataTypeBase = typeof(RavenAnimationDataTween<>);
                    break;

                case ERavenAnimationDataFilter.Curve:
                    animationDataTypeBase = typeof(RavenAnimationDataCurve<>);
                    break;

                case ERavenAnimationDataFilter.Set:
                    animationDataTypeBase = typeof(RavenAnimationDataSet<>);
                    break;
            }

            var wrapperData = menuData.Where(x => RavenUtility.IsSubclassOfGeneric(x.m_AnimationDataType, animationDataTypeBase)).Select(x => RavenQuickSearchRemap.DoRemap(x)).Where(x => x != null);
            if (!wrapperData.Any()) {
                return true;
            }

            m_QuickSearch = new RavenQuickSearch(UnityEngine.Event.current.mousePosition, wrapperData, callback);
            return true;
        }

        public bool GenerateQuickSearchForFunctionProperty(List<RavenPropertyTrackView.PropertyEventMenuData> menuData, Action<object> callback, params Type[] typeConstraints) {
            if (menuData == null || menuData.Count == 0) {
                return true;
            }

            var wrapperData = menuData.Select((x) => {
                return new RavenQuickSearchWrapper() {
                    m_Data = x,
                    m_PrettyName = x.m_Entry,
                };
            });

            m_QuickSearch = new RavenQuickSearch(UnityEngine.Event.current.mousePosition, wrapperData, callback);
            return true;
        }

        public void DrawQuickSearch(RavenQuickSearch quickSearch) {
            m_QuickSearch = quickSearch;
        }

        public Vector2 GetCurrentMousePositionInScrollRect() {
            return UnityEngine.Event.current.mousePosition + new Vector2(0, m_ScrollPosition.y - c_TopHeight - s_AlwaysOnTopBarHeight);
        }

        public Vector2 ConvertMousePositionToMousePositionInScrollRect(Vector2 mousePosition) {
            return mousePosition + new Vector2(0, m_ScrollPosition.y - c_TopHeight - s_AlwaysOnTopBarHeight);
        }

        public Vector2 ConvertMousePositionInScrollRectToMousePosition(Vector2 mousePosition) {
            return mousePosition - new Vector2(0, m_ScrollPosition.y - c_TopHeight - s_AlwaysOnTopBarHeight);
        }

        private class AdjustTimePopup : PopupWindowContent {
            private Action<double> ConfirmAction;
            private Action OnCloseAction;
            private double TimeInsert;

            public AdjustTimePopup(Action<double> onOk, Action onClose) {
                ConfirmAction = onOk;
                OnCloseAction = onClose;
                TimeInsert = 0f;
            }

            public override Vector2 GetWindowSize() {
                return new Vector2(200, 150);
            }

            public override void OnGUI(Rect rect) {
                var position = rect;
                position.height = EditorGUIUtility.singleLineHeight * 1.3f;

                EditorGUI.LabelField(position, "Adjust Time", EditorStyles.boldLabel);
                position.y += position.height;

                GUI.SetNextControlName("AdjustTime");
                TimeInsert = EditorGUI.DoubleField(position, "Time", TimeInsert);
                EditorGUI.FocusTextInControl("AdjustTime");
                position.y += position.height;

                position.y = rect.height - 30f;
                position.height = rect.height - position.y;
                if (GUI.Button(position, "Confirm")) {
                    if (ConfirmAction != null) {
                        ConfirmAction(TimeInsert);
                    }
                    editorWindow.Close();
                }
            }

            public override void OnClose() {
                if (OnCloseAction != null) {
                    OnCloseAction();
                }
            }
        }
    }
}