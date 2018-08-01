#if !STARLITE_EDITOR
using Outfit7.Logic.Util;
#else
using Starlite;
#endif

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public sealed class RavenTrackGroupView {
        private const float c_HotzoneWidth = 6f;

        private List<RavenTrackView> m_TrackViews = new List<RavenTrackView>();
        private List<RavenEventView> m_EventViews = new List<RavenEventView>();
        private RavenSequenceView m_ParentSequenceView;
        private RavenTrackGroup m_TrackGroup = null;
        private Rect SidebarTrackRect = new Rect();
        private Rect m_AddTrackButton = new Rect();
        private GUIStyle Button = (GUIStyle)"ButtonMid";
        private Color LineColor = new Color(0.33f, 0.33f, 0.33f);
        private double m_LastComponentCheckTime = float.MinValue;
        private Component[] m_CachedTargetComponents = new Component[0];

        //custom foldout
        private Rect m_FoldoutButtonRect = new Rect();

        private static Texture2D m_CollapsedTexture = EditorGUIUtility.FindTexture("d_Animation.Play");
        private static Texture2D m_OptimizedTexture = EditorGUIUtility.FindTexture("d_UnityEditor.SceneHierarchyWindow");
        private static Texture2D m_ExtendedTexture = EditorGUIUtility.FindTexture("d_AnimationWrapModeMenu");

        private const float c_OverrideParameterIndexHeight = 16f;

        public RavenTrackGroup TrackGroup {
            get {
                return m_TrackGroup;
            }
        }

        public List<RavenTrackView> TrackViews {
            get {
                return m_TrackViews;
            }
        }

        public GameObject Target {
            get {
                return m_TrackGroup.m_Target;
            }
        }

        public List<RavenEventView> EventViews {
            get {
                return m_EventViews;
            }
        }

        public void Initialize(RavenTrackGroup trackGroup, RavenSequenceView parent) {
            m_TrackGroup = trackGroup;
            m_ParentSequenceView = parent;
            CreateAndInitializeTracks();
        }

        public void DestroyTrackGroup() {
            for (int i = 0; i < m_TrackViews.Count; ++i) {
                m_TrackViews[i].DestroyTrack();
            }
            Undo.DestroyObjectImmediate(m_TrackGroup);
        }

        public void CreateAndInitializeTracks() {
            AddTrack<RavenPropertyTrack>(m_TrackGroup.m_PropertyTrack);
            if (m_TrackGroup.m_AudioTrack != null) {
                AddTrack<RavenAudioTrack>(m_TrackGroup.m_AudioTrack);
            }
        }

        public float DrawTimelineGui(Rect windowSize, float startHeight, float splitViewWidth, TimelineData timelineData, List<RavenParameter> parameters) {
            RefreshAllTracks();
            float height = 0;
            height += 20f;

            if (m_TrackGroup.m_TrackGroupMode == RavenTrackGroup.ERavenTrackGroupMode.Extended) {
                height += Target != null ? c_OverrideParameterIndexHeight : 0f;
                for (int i = 0; i < m_TrackViews.Count; i++) {
                    height += m_TrackViews[i].DrawTimelineGui(windowSize, startHeight + height, splitViewWidth, timelineData, parameters);
                }
            } else if (m_TrackGroup.m_TrackGroupMode == RavenTrackGroup.ERavenTrackGroupMode.Optimized) {
            } else if (m_TrackGroup.m_TrackGroupMode == RavenTrackGroup.ERavenTrackGroupMode.Collapsed) {
            }

            DrawTimelineLines(windowSize, startHeight, splitViewWidth, height, timelineData);

            return height;
        }

        public void RemoveEventView(RavenEventView eventView) {
            m_EventViews.Remove(eventView);
        }

        public void AddEventView(RavenEventView eventView) {
            m_EventViews.Add(eventView);
        }

        public float DrawSidebarGui(Rect windowSize, float startHeight, float splitViewWidth, TimelineData timelineData, List<RavenParameter> parameters) {
            m_FoldoutButtonRect = new Rect(0, startHeight, 20, 20);
            Texture2D ViewModeTex = m_ExtendedTexture;
            switch (m_TrackGroup.m_TrackGroupMode) {
                case RavenTrackGroup.ERavenTrackGroupMode.Collapsed:
                    ViewModeTex = m_CollapsedTexture;
                    break;

                case RavenTrackGroup.ERavenTrackGroupMode.Optimized:
                    ViewModeTex = m_OptimizedTexture;
                    break;
            }
            GUI.Label(m_FoldoutButtonRect, ViewModeTex);
            SidebarTrackRect = new Rect(0, startHeight, splitViewWidth, 20);
            float height = 0;
            height += 20f;
            m_AddTrackButton = new Rect(splitViewWidth - 30, startHeight, 30, 20);

            if (GUI.Button(m_AddTrackButton, "+", Button)) {
                DrawEventsContextMenu();
            }
            DrawTargetGameObject(startHeight, splitViewWidth);

            if (m_TrackGroup.m_TrackGroupMode == RavenTrackGroup.ERavenTrackGroupMode.Extended) {
                height += DrawOverrideTargetsParameterField(startHeight + height, splitViewWidth, parameters);
                for (int i = 0; i < m_TrackViews.Count; i++) {
                    height += m_TrackViews[i].DrawSidebarGui(windowSize, startHeight + height, splitViewWidth, timelineData, parameters);
                }
            } else if (m_TrackGroup.m_TrackGroupMode == RavenTrackGroup.ERavenTrackGroupMode.Optimized) {
            }
            return height;
        }

        private void DrawTimelineLines(Rect windowSize, float startHeight, float splitViewWidth, float height, TimelineData timelineData) {
            GUIUtil.DrawFatLine(new Vector2(splitViewWidth + 1, startHeight), new Vector2(windowSize.width - 1, startHeight), 4, LineColor);
            GUIUtil.DrawLine(new Vector2(splitViewWidth + 1, startHeight + height), new Vector2(windowSize.width - 1, startHeight + height), LineColor);
        }

        private void DrawEventsContextMenu(float location = 0f) {
            GenericMenu menu = new GenericMenu();
            if (m_TrackGroup.m_PropertyTrack == null) {
                menu.AddItem(new GUIContent("Property Track"), false, AddTrack<RavenPropertyTrack>, null);
            }
            if (m_TrackGroup.m_AudioTrack == null) {
                menu.AddItem(new GUIContent("Audio Track"), false, AddTrack<RavenAudioTrack>, null);
            }
            menu.ShowAsContext();
        }

        public void SelectEvents(Rect selectionRect) {
            for (int i = 0; i < m_TrackViews.Count; i++) {
                m_TrackViews[i].SelectEvents(selectionRect);
            }
        }

        public bool HandleInput(TimelineData timelineData) {
            var mousePosInScrollRect = m_ParentSequenceView.GetCurrentMousePositionInScrollRect();

            if (m_FoldoutButtonRect.Contains(mousePosInScrollRect) &&
                UnityEngine.Event.current.type == EventType.mouseDown &&
                UnityEngine.Event.current.button == 0) {
                switch (m_TrackGroup.m_TrackGroupMode) {
                    case RavenTrackGroup.ERavenTrackGroupMode.Extended:
                        if (UnityEngine.Event.current.alt) {
                            m_ParentSequenceView.SetTrackGroupsViewMode(RavenTrackGroup.ERavenTrackGroupMode.Collapsed);
                        } else {
                            m_TrackGroup.m_TrackGroupMode = RavenTrackGroup.ERavenTrackGroupMode.Collapsed;
                        }
                        break;

                    case RavenTrackGroup.ERavenTrackGroupMode.Collapsed:
                        if (UnityEngine.Event.current.alt) {
                            m_ParentSequenceView.SetTrackGroupsViewMode(RavenTrackGroup.ERavenTrackGroupMode.Optimized);
                        } else {
                            m_TrackGroup.m_TrackGroupMode = RavenTrackGroup.ERavenTrackGroupMode.Optimized;
                        }
                        break;

                    case RavenTrackGroup.ERavenTrackGroupMode.Optimized:
                        if (UnityEngine.Event.current.alt) {
                            m_ParentSequenceView.SetTrackGroupsViewMode(RavenTrackGroup.ERavenTrackGroupMode.Extended);
                        } else {
                            m_TrackGroup.m_TrackGroupMode = RavenTrackGroup.ERavenTrackGroupMode.Extended;
                        }
                        break;
                }
                return true;
            }
            if (SidebarTrackRect.Contains(mousePosInScrollRect) &&
                UnityEngine.Event.current.type == EventType.mouseUp &&
                UnityEngine.Event.current.button == 1) {
                DrawTrackGroupContextMenu();
                return true;
            }
            if (SidebarTrackRect.Contains(mousePosInScrollRect) &&
                UnityEngine.Event.current.type == EventType.KeyDown) {
                if (UnityEngine.Event.current.keyCode == KeyCode.DownArrow) {
                    m_ParentSequenceView.MoveTrackGroupDown(this);
                } else if (UnityEngine.Event.current.keyCode == KeyCode.UpArrow) {
                    m_ParentSequenceView.MoveTrackGroupUp(this);
                }
                return true;
            }
            if (m_TrackGroup.m_TrackGroupMode == RavenTrackGroup.ERavenTrackGroupMode.Extended || m_TrackGroup.m_TrackGroupMode == RavenTrackGroup.ERavenTrackGroupMode.Optimized) {
                var mousePosition = mousePosInScrollRect;
                for (int i = 0; i < m_TrackViews.Count; i++) {
                    var handled = m_TrackViews[i].HandleInput(mousePosition, timelineData, m_ParentSequenceView, m_TrackGroup.m_TrackGroupMode == RavenTrackGroup.ERavenTrackGroupMode.Optimized);
                    if (handled) {
                        return true;
                    }
                }
            }
            return false;
        }

        public void RecordingStart() {
            for (int i = 0; i < m_TrackViews.Count; ++i) {
                m_TrackViews[i].RecordingStart();
            }
        }

        public void RecordingStop() {
            for (int i = 0; i < m_TrackViews.Count; ++i) {
                m_TrackViews[i].RecordingStop();
            }
        }

        public void UpdateWhileRecording(double currentTime) {
            for (int i = 0; i < m_TrackViews.Count; ++i) {
                m_TrackViews[i].UpdateWhileRecording(currentTime);
            }
        }

        private void DrawTrackGroupContextMenu() {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Remove Track Group/Are you sure?"), false, m_ParentSequenceView.RemoveTrackGroup, this);
            menu.AddItem(new GUIContent("Move Up"), false, m_ParentSequenceView.MoveTrackGroupUp, this);
            menu.AddItem(new GUIContent("Move Down"), false, m_ParentSequenceView.MoveTrackGroupDown, this);
            menu.ShowAsContext();
        }

        public void AddTrack<T>(object ctrl) where T : RavenTrack {
            // if controller comes in at init, it just casts it and creates View for it
            // if function is called to add new, it creates controller first, and then view afterwards
            Undo.RecordObject(m_TrackGroup, "AddTrack");
            T controller = ctrl as T;
            if (controller == null) {
                controller = (T)Undo.AddComponent<T>(m_TrackGroup.gameObject);
                controller.InitializeEditor(RavenSequenceEditor.Instance.Sequence);
                if (controller is RavenPropertyTrack) {
                    m_TrackGroup.m_PropertyTrack = controller as RavenPropertyTrack;
                } else if (controller is RavenAudioTrack) {
                    m_TrackGroup.m_AudioTrack = controller as RavenAudioTrack;
                }
            }

            controller.hideFlags = HideFlags.HideInInspector;
            var viewType = Type.GetType(controller.GetType().ToString() + "View", true);
            var instance = Activator.CreateInstance(viewType) as RavenTrackView;
            instance.Initialize(controller, this);
            m_TrackViews.Add(instance);
        }

        public void SetTargetGameObject(GameObject newTarget) {
            var sameComponents = true;
            var timeNow = EditorApplication.timeSinceStartup;
            if (newTarget != null && timeNow - m_LastComponentCheckTime >= 1f) {
                var components = newTarget.GetComponents<Component>();
                sameComponents = components.Length == m_CachedTargetComponents.Length;
                m_CachedTargetComponents = components;
                m_LastComponentCheckTime = timeNow;
            }
            if (m_TrackGroup.m_Target != newTarget || !sameComponents) {
                Undo.RecordObject(m_TrackGroup, "SetTarget");
                m_TrackGroup.m_Target = newTarget;
                for (int i = 0; i < m_TrackViews.Count; ++i) {
                    m_TrackViews[i].OnTargetChanged(m_TrackGroup.m_Target);
                }
            }
        }

        public void RefreshAllTracks() {
            for (int i = 0; i < m_TrackViews.Count; ++i) {
                m_TrackViews[i].RefreshAllEvents(new List<GameObject>());
            }
        }

        public void RemoveTrack(object obj) {
            var trackView = obj as RavenTrackView;
            if (trackView.Track is RavenPropertyTrack) {
                m_TrackGroup.m_PropertyTrack = null;
            } else {
                m_TrackGroup.m_AudioTrack = null;
            }
            trackView.DestroyTrack();
            m_TrackViews.Remove(trackView);
        }

        private float DrawOverrideTargetsParameterField(float startHeight, float splitViewWidth, List<RavenParameter> parameters) {
            const float indent = 20f;

            if (Target != null) {
                Undo.RecordObject(m_TrackGroup, "DrawOverrideTargetsParameterField");
                var selectedIndex = m_TrackGroup.m_OverrideTargetsParameterIndex;
                RavenParameterEditor.DrawParameterField(new Rect(indent, startHeight, splitViewWidth - indent, c_OverrideParameterIndexHeight), x => x.m_ParameterType == ERavenParameterType.ActorList, null, ref m_TrackGroup.m_OverrideTargetsParameterIndex, ref selectedIndex);
                return c_OverrideParameterIndexHeight;
            } else {
                if (m_TrackGroup.m_OverrideTargetsParameterIndex != -1) {
                    Undo.RecordObject(m_TrackGroup, "DrawOverrideTargetsParameterField");
                    m_TrackGroup.m_OverrideTargetsParameterIndex = -1;
                }
            }

            return 0f;
        }

        private void DrawTargetGameObject(float startHeight, float splitViewWidth) {
            var newTarget = EditorGUI.ObjectField(new Rect(30, startHeight, splitViewWidth - 60, 15), "", m_TrackGroup.m_Target, typeof(GameObject), true) as GameObject;
            SetTargetGameObject(newTarget);
            RavenFunctionCallEditor.Refresh();
        }
    }
}