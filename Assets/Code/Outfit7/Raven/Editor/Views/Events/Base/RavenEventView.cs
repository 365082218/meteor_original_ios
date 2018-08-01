using System;
using System.Collections.Generic;
using Starlite.Raven.Compiler;
using UnityEditor;
using UnityEngine;
using Starlite.Raven.Internal;
using Outfit7.Logic.Util;

namespace Starlite.Raven {

    public abstract class RavenEventView {

        public enum EResizeMode {
            Left,
            Right
        }

        private const float m_SelectionRectMargins = 3f;

        public bool m_Selected = false;

        protected RavenTrackView m_ParentTrackView = null;
        protected Rect m_SelectedRect = new Rect();
        protected Rect m_EventRect = new Rect();
        protected Vector2 m_StartDragOffset = new Vector2();
        protected bool m_UsingOptimizedGui = false;
        protected RavenEvent m_Event = null;
        protected Rect m_HandleTouchRect = new Rect();

        protected Vector2 m_StartDragMousePosition = Vector2.zero;
        protected float m_StartDragMouseDeltaFromStartFrame;
        protected float m_StartDragMouseDeltaFromLastFrame;
        protected int m_InitialResizeDelta;
        protected int m_InitialResizeDurationInFrames;
        protected double m_StartFrameRelativePositionOnResize;
        protected double m_EndFrameRelativePositionOnResize;

        protected static GUIStyle s_DefaultNodeBackground = (GUIStyle)"sv_iconselector_labelselection";
        protected static GUIStyle s_SelectedRectStyle = (GUIStyle)"U2D.createRect";

        private static GUIStyle s_NodeDisabled = (GUIStyle)"flow node 0";
        private static GUIStyle s_NodeDisabledSelected = (GUIStyle)"flow node 0 on";
        private static GUIStyle s_NodeCondition = (GUIStyle)"flow node 5";
        private static GUIStyle s_NodeConditionSelected = (GUIStyle)"flow node 5 on";

        public abstract string Name {
            get;
        }

        public abstract int QueuedStartFrameReposition {
            get;
        }

        public abstract int QueuedLastFrameReposition {
            get;
        }

        public RavenEvent Event {
            get {
                return m_Event;
            }
        }

        public Rect EventRect {
            get {
                return m_EventRect;
            }
        }

        public int SubTrackIndex {
            get {
                return m_Event.SubTrackIndex;
            }
        }

        public virtual bool AlwaysVisible {
            get {
                return false;
            }
        }

        protected virtual bool AllowSubTrackChange {
            get {
                return true;
            }
        }

        protected virtual GUIStyle NodeStyle {
            get {
                return s_NodeDisabled;
            }
        }

        protected virtual GUIStyle NodeSelectedStyle {
            get {
                return s_NodeDisabledSelected;
            }
        }

        protected virtual GUIStyle NodeConditionStyle {
            get {
                return s_NodeCondition;
            }
        }

        protected virtual GUIStyle NodeConditionSelectedStyle {
            get {
                return s_NodeConditionSelected;
            }
        }

        protected virtual GUIStyle NodeDisabledStyle {
            get {
                return s_NodeDisabled;
            }
        }

        protected virtual GUIStyle NodeDisabledSelectedStyle {
            get {
                return s_NodeDisabledSelected;
            }
        }

        public virtual void Initialize(RavenEvent evnt, RavenTrackView parent) {
            m_Event = evnt;
            m_ParentTrackView = parent;
            RavenSequenceEditor.Instance.SequenceView.AddEventView(this);
        }

        public virtual void Refresh(object actor) {
            Undo.RecordObject(m_Event, "EventRefresh");
        }

        public void UpdateWhileRecording(double currentTime) {
            OnUpdateWhileRecording(currentTime);
        }

        public void SelectEvent(Rect? selectionRect) {
            m_Selected = selectionRect == null ? true : selectionRect.Value.Contains(m_EventRect.center);
            if (m_Selected) {
                if (Selection.activeGameObject == null) {
                    Selection.activeGameObject = RavenSequenceEditor.Instance.Sequence.gameObject;
                }
                m_Event.SetHideFlags(HideFlags.None);
            } else {
                m_Event.SetHideFlags(HideFlags.HideInInspector);
            }
            OnSelectionChanged(m_Selected);
        }

        public void DeselectEvent() {
            m_Selected = false;
            m_Event.SetHideFlags(HideFlags.HideInInspector);
            OnSelectionChanged(m_Selected);
        }

        public void SetStartFrame(int frame) {
            if (m_Event.StartFrame == frame) {
                return;
            }

            m_Event.SetStartFrame(frame);
            RavenSequenceEditor.Instance.Sequence.EventChanged(m_Event);
        }

        public void SetLastFrame(int frame) {
            if (m_Event.LastFrame == frame) {
                return;
            }

            m_Event.SetLastFrame(frame);
            RavenSequenceEditor.Instance.Sequence.EventChanged(m_Event);
        }

        public void SetEndFrame(int frame) {
            if (m_Event.EndFrame == frame) {
                return;
            }

            m_Event.SetEndFrame(frame);
            RavenSequenceEditor.Instance.Sequence.EventChanged(m_Event);
        }

        public void OffsetEvent(int nFrames) {
            if (nFrames == 0) {
                return;
            }

            m_Event.OffsetEvent(nFrames);
            RavenSequenceEditor.Instance.Sequence.EventChanged(m_Event);
        }

        public virtual void OnSelectionChanged(bool selected) {
        }

        public void DrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, bool foldoutEnabled, float foldoutHeight, List<RavenParameter> parameters, bool optimizedView = false) {
            Undo.RecordObject(m_Event, "DrawGui");
            if (m_Selected) {
                m_Event.SetHideFlags(HideFlags.None);
            }

            OnDefineEventRect(windowSize, startHeight, splitViewLeft, timelineData, foldoutEnabled, foldoutHeight, optimizedView);
            if (foldoutEnabled) {
                OnDrawExtendedGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutEnabled, foldoutHeight, parameters);
            }

            m_UsingOptimizedGui = optimizedView;
            if (optimizedView) {
                OnDrawOptimizedGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutEnabled, foldoutHeight, parameters);
            } else {
                OnDrawGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutEnabled, foldoutHeight, parameters);
            }

            if (m_Selected && RavenPreferences.ShowSelectedEventRect) {
                EditorGUI.LabelField(m_SelectedRect, "", s_SelectedRectStyle);
                DrawFrameLabels(timelineData);
            }
        }

        virtual protected void DrawFrameLabels(TimelineData timelineData) {
            GUIStyle centerStyle = new GUIStyle();
            centerStyle.alignment = TextAnchor.MiddleCenter;

            if (m_Event.StartFrame != m_Event.EndFrame) {
                // middle label
                Vector2 middleLeft = new Vector2(m_EventRect.x, m_SelectedRect.y - 7);
                Vector2 middleRight = new Vector2(m_EventRect.x + m_EventRect.width, m_SelectedRect.y - 7);
                GUIUtil.DrawLine(middleLeft, middleRight, Color.gray);
                centerStyle.normal.textColor = Color.gray;
                EditorGUI.LabelField(new Rect(middleLeft.x + m_EventRect.width * 0.5f - 15, middleRight.y - 23, 30, 30), (m_Event.EndFrame - m_Event.StartFrame).ToString(), centerStyle);

                // end frame label
                Vector2 bottomRight = new Vector2(m_EventRect.x + m_EventRect.width, m_SelectedRect.y);
                Vector2 topRight = new Vector2(m_EventRect.x + m_EventRect.width, m_SelectedRect.y - 15);
                GUIUtil.DrawLine(bottomRight, topRight, Color.green);
                centerStyle.normal.textColor = Color.green;
                EditorGUI.LabelField(new Rect(topRight.x - 15, topRight.y - 25, 30, 30), m_Event.EndFrame.ToString(), centerStyle);
            }

            // start frame label
            Vector2 bottomLeft = new Vector2(m_EventRect.x, m_SelectedRect.y);
            Vector2 topLeft = new Vector2(m_EventRect.x, m_SelectedRect.y - 15);
            GUIUtil.DrawLine(bottomLeft, topLeft, Color.green);
            centerStyle.normal.textColor = Color.green;
            EditorGUI.LabelField(new Rect(topLeft.x - 15, topLeft.y - 25, 30, 30), m_Event.StartFrame.ToString(), centerStyle);
        }

        public void LayoutChanged(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, bool foldoutEnabled, float foldoutHeight, List<RavenParameter> parameters, bool optimizedView = false) {
            OnDefineEventRect(windowSize, startHeight, splitViewLeft, timelineData, foldoutEnabled, foldoutHeight, optimizedView);
        }

        public void DestroyEvent() {
            OnDestroy();
            m_ParentTrackView.OnEventViewDestroyed(this);
            RavenSequenceEditor.Instance.SequenceView.OnEventViewDestroyed(this);
        }

        public void RecordingStart() {
            OnRecordingStart();
        }

        public void RecordingStop() {
            OnRecordingStop();
        }

        public bool HandleInput(Vector2 mousePosition, RavenTrackView parent, TimelineData timelineData, RavenSequenceView sequenceView, Rect timelineTrackRect, int HighiestEventTrackIndex) {
            Undo.RecordObject(m_Event, "HandleInputEventChange");

            if (m_EventRect.Contains(mousePosition) &&
                UnityEngine.Event.current.type == EventType.mouseUp &&
                UnityEngine.Event.current.button == 1) {
                DrawEventsContextMenu(parent);
                return true;
            }

            bool result = OnHandleInput(mousePosition, timelineData, sequenceView, timelineTrackRect, HighiestEventTrackIndex);
            if (result &&
                m_EventRect.Contains(mousePosition) &&
                UnityEngine.Event.current.type == EventType.mouseUp &&
                UnityEngine.Event.current.button == 0) {
                SelectEvent(null);
                return true;
            }
            return result;
        }

        public RavenTriggerPropertyComponentBase GenerateFunctionCallProperty(Type type, GameObject target, UnityEngine.Object targetComponent, string functionName) {
            Undo.RecordObject(m_Event, "AddProperty");
            var sequence = RavenSequenceEditor.Instance.Sequence;
            var property = Undo.AddComponent(m_Event.gameObject, type) as RavenTriggerPropertyComponentBase;

            Undo.RecordObject(property, "AddProperty");
            property.TargetComponent = targetComponent;
            property.Target = target;
            property.FunctionName = functionName;
            property.ComponentType = targetComponent.GetType().ToString();
            var componentBaseType = property.ComponentBaseType;

            ulong hash;
            if (PropertyReflectionCompiledOutput.HasInDatabase(componentBaseType.ToString(), property.FunctionName, out hash)) {
                property.TargetHash = hash;
            } else {
                Compiler.RavenCompiler.CompileProperty(property);
            }

            if (m_Event is RavenCallFunctionEvent) {
                var callFunctionEvent = m_Event as RavenCallFunctionEvent;
                if (callFunctionEvent.Property != null) {
                    callFunctionEvent.Property.DestroyEditor(sequence);
                }
                callFunctionEvent.Property = property;
            } else if (m_Event is RavenCallFunctionContinuousEvent) {
                var callFunctionEvent = m_Event as RavenCallFunctionContinuousEvent;
                if (callFunctionEvent.Property != null) {
                    callFunctionEvent.Property.DestroyEditor(sequence);
                }
                callFunctionEvent.Property = property;
            } else if (m_Event is RavenPropertyEvent) {
                var propertyEvent = m_Event as RavenPropertyEvent;
                if (propertyEvent.Property.TriggerProperty != null) {
                    propertyEvent.Property.TriggerProperty.DestroyEditor(sequence);
                }
                propertyEvent.Property.TriggerProperty = property;
            } else {
                RavenLog.Error("Generate function call property not handled!");
            }

            property.SetHideFlags(m_Event.hideFlags);
            sequence.FlagDirty();

            OnGenerateFunctionCallProperty(property);
            return property;
        }

        public virtual void StartResizeMode(Vector2 mousePosition, int firstEventStart, int lastEventEnd, EResizeMode resizeMode) {
            m_StartDragMousePosition = mousePosition;
            m_InitialResizeDelta = lastEventEnd - firstEventStart;
            m_InitialResizeDurationInFrames = m_Event.DurationInFrames;
            var duration = (double)m_InitialResizeDelta;
            m_StartFrameRelativePositionOnResize = (m_Event.StartFrame - firstEventStart) / duration;
            m_EndFrameRelativePositionOnResize = (m_Event.EndFrame - lastEventEnd) / duration;
        }

        public virtual void EndResizeMode() {
            m_InitialResizeDelta = 0;
            m_StartFrameRelativePositionOnResize = 0;
            m_EndFrameRelativePositionOnResize = 0;
        }

        public virtual void StartDraggingMode(Vector2 mousePosition, TimelineData timelineData) {
            m_StartDragMousePosition = mousePosition;
            m_StartDragMouseDeltaFromStartFrame = mousePosition.x - timelineData.GetPositionAtFrame(m_Event.StartFrame);
            m_StartDragMouseDeltaFromLastFrame = mousePosition.x - timelineData.GetPositionAtFrame(m_Event.LastFrame);
            m_StartDragOffset = new Vector2(timelineData.GetPositionAtFrame(timelineData.GetFrameAtMousePosition(mousePosition)), mousePosition.y) - m_EventRect.min;
        }

        public virtual void EndDraggingMode() {
        }

        public abstract void ResetInput();

        public abstract bool HasInput();

        public abstract bool TryEvaluateDragging(Vector2 mousePosition, TimelineData timelineData, RavenSequenceView sequenceView);

        public abstract void ApplyDragging();

        public abstract bool TryNormalizeRepositioning(RavenSequenceView sequenceView, int newFirstEventStart, int newLastEventEnd);

        public abstract void ApplyNormalizedRepositioning();

        protected abstract void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, bool foldoutEnabled, float foldoutHeight, List<RavenParameter> parameters);

        protected abstract void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, bool foldoutEnabled, float foldoutHeight, List<RavenParameter> parameters);

        protected abstract void OnDrawEventsContextMenu(GenericMenu menu, RavenTrackView parent);

        protected abstract void OnUpdateWhileRecording(double currentTime);

        protected abstract void OnRecordingStart();

        protected abstract void OnRecordingStop();

        protected virtual void OnDefineEventRect(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, bool foldoutEnabled, float foldoutHeight, bool optimizedView = false) {
            float elementHeight = (foldoutEnabled ? foldoutHeight : 20f);
            startHeight += (optimizedView ? 0 : m_Event.SubTrackIndex) * elementHeight;
            m_EventRect = new Rect(timelineData.GetPositionAtFrame(m_Event.StartFrame),
                startHeight,
                Math.Max(timelineData.GetWidthForFrames(m_Event.WidthInFrames), 6f),
                elementHeight);
            m_HandleTouchRect = m_EventRect;
            m_SelectedRect = new Rect(m_EventRect.xMin - m_SelectionRectMargins, m_EventRect.yMin - m_SelectionRectMargins, m_EventRect.width + m_SelectionRectMargins * 2f, m_EventRect.height + m_SelectionRectMargins * 2f);
        }

        protected virtual void OnDrawOptimizedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, bool foldoutEnabled, float foldoutHeight, List<RavenParameter> parameters) {
            OnDrawGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutEnabled, foldoutHeight, parameters);
        }

        protected virtual bool OnHandleInput(Vector2 mousePosition, TimelineData timelineData, RavenSequenceView sequenceView, Rect timelineTrackRect, int HighiestEventTrackIndex) {
            return false;
        }

        protected virtual void OnGenerateFunctionCallProperty(RavenTriggerPropertyComponentBase property) {

        }

        protected virtual void OnGeneratePropertyEventProperty(RavenAnimationPropertyComponentBase property) {

        }

        protected virtual void OnDestroy() {
        }

        protected void DrawEventsContextMenu(RavenTrackView parent) {
            GenericMenu menu = new GenericMenu();
            OnDrawEventsContextMenu(menu, parent);
            menu.AddItem(new GUIContent("Remove Event"), false, parent.RemoveEvent, this);
            menu.ShowAsContext();
        }

        protected void EvaluateDraggingForTrackIndex(Vector2 mousePosition, Rect trackRect, int highestSubTrackIndex) {
            if (!AllowSubTrackChange) {
                return;
            }

            if (m_UsingOptimizedGui) {
                return;
            }

            if (mousePosition.y < trackRect.y) {
                return;
            }

            if (mousePosition.y > trackRect.yMax) {
                if (RavenSequenceEditor.Instance.SequenceView.CheckContinuousFrameOverlap(m_Event, m_Event.StartFrame, m_Event.LastFrame, m_Event.TrackIndex, m_Event.SubTrackIndex + 1)) {
                    return;
                }
                m_Event.SetSubTrackIndex(m_Event.SubTrackIndex + 1);
                RavenSequenceEditor.Instance.Sequence.EventChanged(m_Event);
                return;
            }

            var newSubTrackIndex = (int)(((mousePosition.y - trackRect.y) / trackRect.height) * (highestSubTrackIndex + 1));
            if (newSubTrackIndex != m_Event.SubTrackIndex) {
                if (RavenSequenceEditor.Instance.SequenceView.CheckContinuousFrameOverlap(m_Event, m_Event.StartFrame, m_Event.LastFrame, m_Event.TrackIndex, newSubTrackIndex)) {
                    return;
                }
                m_Event.SetSubTrackIndex(newSubTrackIndex);
                RavenSequenceEditor.Instance.Sequence.EventChanged(m_Event);
            }
        }

        protected void DragSelectedEvents(RavenSequenceView sequenceView, Vector2 mousePosition, TimelineData timelineData) {
            bool dragValid = true;
            for (int i = 0; i < sequenceView.EventViews.Count; ++i) {
                var view = sequenceView.EventViews[i];
                if (view.m_Selected) {
                    if (view.TryEvaluateDragging(mousePosition, timelineData, sequenceView) == false) {
                        dragValid = false;
                        break;
                    }
                }
            }

            if (dragValid) {
                for (int i = 0; i < sequenceView.EventViews.Count; ++i) {
                    var view = sequenceView.EventViews[i];
                    if (view.m_Selected) {
                        view.ApplyDragging();
                    }
                }
            }
        }

        protected void DrawEnabledBox(Rect rect) {
            var oldEnabled = m_Event.IsEnabled;
            m_Event.IsEnabled = EditorGUI.Toggle(rect, m_Event.IsEnabled);
            if (oldEnabled != m_Event.IsEnabled) {
                RavenSequenceEditor.Instance.Sequence.FlagDirty();
            }
        }

        protected RavenAnimationPropertyComponentBase GeneratePropertyEventProperty(GameObject target, string componentType, string memberName, Type propertyType, Type animationDataType, Type argumentType, out bool restoreToPrevious) {
            restoreToPrevious = false;

            Undo.RecordObject(m_Event, "AddProperty");
            var sequence = RavenSequenceEditor.Instance.Sequence;
            var propertyEvent = m_Event as RavenPropertyEvent;
            var oldProperty = propertyEvent.Property;

            var property = Undo.AddComponent(m_Event.gameObject, propertyType) as RavenAnimationPropertyComponentBase;
            var componentTypeT = RavenUtility.GetTypeFromLoadedAssemblies(componentType);
            var isComponentDerivedFromComponent = componentTypeT != null && (componentTypeT.IsSubclassOf(typeof(Component)) || componentTypeT == typeof(Component));

            if (!property.CheckForDependencies()) {
                RavenLog.Error("Dependencies check for {0} failed!", property);
                property.DestroyEditor(sequence);
                propertyEvent.Property = oldProperty;
                restoreToPrevious = true;
                return null;
            }

            Undo.RecordObject(property, "AddProperty");
            property.TargetComponent = isComponentDerivedFromComponent ? target.GetComponent(componentTypeT) : (UnityEngine.Object)target;
            property.Target = target;
            property.MemberName = memberName;
            property.ComponentType = componentType;

            if (!property.IsCustom) {
                var componentBaseType = property.ComponentBaseType;
                ulong hash;
                if (PropertyReflectionCompiledOutput.HasInDatabase(componentBaseType.ToString(), property.MemberName, out hash)) {
                    property.TargetHash = hash;
                } else {
                    Compiler.RavenCompiler.CompileProperty(property);
                }
            } else {
                property.TargetHash = RavenUtility.s_InvalidHash;
            }

            propertyEvent.Property = property;

            var animationData = GenerateAnimationData(target, animationDataType, property, true, sequence);
            if (animationData == null) {
                property.DestroyEditor(sequence);
                propertyEvent.Property = oldProperty;
                restoreToPrevious = true;
                return null;
            } else {
                property.AnimationData = animationData;
            }

            if (oldProperty != null) {
                oldProperty.DestroyEditor(sequence);
            }

            PostprocessAnimationDataChange(propertyEvent, animationDataType);

            property.SetHideFlags(propertyEvent.hideFlags);
            sequence.FlagDirty();

            OnGeneratePropertyEventProperty(property);
            return property;
        }

        protected RavenAnimationDataComponentBase GenerateAnimationData(GameObject target, Type animationDataType, RavenAnimationPropertyComponentBase property, bool setStartingValues, RavenSequence sequence) {
            var type = property.TargetComponent != null ? property.TargetComponent.GetType() : null;
            var specializedType = RavenSequenceEditor.Instance.SequenceView.GetSpecializedAnimationDataTypeForComponentType(type, property.MemberName);
            var legitAnimationDataType = RavenUtility.GetTheMostSpecializedTypeAmongTwoSimilarTypes(specializedType, animationDataType);
            var animationData = Undo.AddComponent(m_Event.gameObject, legitAnimationDataType) as RavenAnimationDataComponentBase;

            if (!animationData.CheckForDependencies()) {
                RavenLog.Error("Dependencies check for {0} failed!", animationData);
                animationData.DestroyEditor(sequence);
                return null;
            }

            if (setStartingValues) {
                try {
                    animationData.SetStartingValuesEditor(property.GetValueEditor(sequence));
                } catch (Exception e) {
                    RavenLog.Error(e, "Failed getting value from {0}", target);
                    animationData.SetStartingValuesEditor(null);
                }
            }

            return animationData;
        }

        protected RavenPropertyBaseNonGenericView CreateAnimationPropertyView<T>(T property) where T : RavenAnimationPropertyComponentBase {
            if (property == null) {
                return null;
            }

            var viewType = RavenEditorUtility.GetGenericViewType(property.GetType(), property.GetPropertyType());
            var instance = Activator.CreateInstance(viewType) as RavenPropertyBaseNonGenericView;
            instance.Initialize(this, property);
            return instance;
        }

        protected RavenPropertyBaseNonGenericView CreateTriggerPropertyView<T>(T property) where T : RavenTriggerPropertyComponentBase {
            if (property == null) {
                return null;
            }

            var viewType = RavenEditorUtility.GetGenericViewType(property.GetType(), null);
            var instance = Activator.CreateInstance(viewType) as RavenPropertyBaseNonGenericView;
            instance.Initialize(this, property);
            return instance;
        }

        protected void PostprocessAnimationDataChange(RavenPropertyEvent propertyEvent, Type animationDataType) {
            if (animationDataType.IsDefined(typeof(TriggerAnimationDataAttribute), true)) {
                propertyEvent.IsSetProperty = true;
                propertyEvent.SetLastFrame(propertyEvent.StartFrame);
            } else {
                propertyEvent.IsSetProperty = false;
            }

            try {
                propertyEvent.Initialize(RavenSequenceEditor.Instance.Sequence);
            } catch (Exception e) {
                RavenLog.Debug(e, "Could not initialize property event {0}", propertyEvent);
            }
        }

        protected Rect GetHorizontalRect(Rect boundingRect, float xOffsetPercentage, float yOffsetPercentage, float width, float height, float overflowSize = -1f) {
            var maxWidth = m_EventRect.width * Mathf.PingPong(xOffsetPercentage, 0.5f);
            var halfWidth = width * 0.5f;
            if (halfWidth > maxWidth) {
                if (overflowSize != -1f) {
                    halfWidth = Mathf.Max(overflowSize * 0.5f, maxWidth);
                } else {
                    halfWidth = maxWidth;
                }
            }

            return new Rect(Mathf.Max(boundingRect.xMin + boundingRect.width * xOffsetPercentage - halfWidth, m_EventRect.xMin), boundingRect.yMin + boundingRect.height * yOffsetPercentage, halfWidth * 2f, height);
        }

        protected void JustifyRectsLeft(Rect[] rects) {
            var minX = float.MaxValue;
            for (int i = 0; i < rects.Length; ++i) {
                if (rects[i].xMin < minX) {
                    minX = rects[i].xMin;
                }
            }

            for (int i = 0; i < rects.Length; ++i) {
                var rect = rects[i];
                var width = rect.width;
                rect.xMin = minX;
                rect.width = width;
                rects[i] = rect;
            }
        }

        protected void JustiftyRectsRight(Rect[] rects) {
            var maxX = float.MinValue;
            for (int i = 0; i < rects.Length; ++i) {
                if (rects[i].xMin > maxX) {
                    maxX = rects[i].xMax;
                }
            }

            for (int i = 0; i < rects.Length; ++i) {
                var rect = rects[i];
                var width = rect.width;
                rect.xMax = maxX;
                rect.width = width;
                rects[i] = rect;
            }
        }

        // Shittiest code ever but whatever................
        protected Vector2 SnapPosition(RavenSequenceView sequenceView, Vector2 mousePosition, TimelineData timelineData) {
            const double kMaxTimeDelta = 0.5;

            if (!UnityEngine.Event.current.alt) {
                return mousePosition;
            }

            var sequence = sequenceView.Sequence;
            var indexOfCurrentEvent = sequence.SortedEvents.IndexOf(m_Event);
            if (indexOfCurrentEvent < 0) {
                return mousePosition;
            }

            var maxDeltaFrames = sequence.GetFrameForTime(kMaxTimeDelta);
            if (maxDeltaFrames <= 0) {
                return mousePosition;
            }

            var leftEventFrame = indexOfCurrentEvent == 0 ? 0 : FindClosestFrameToFrame(m_Event.StartFrame, true, indexOfCurrentEvent, sequence, sequenceView);
            var rightEventFrame = indexOfCurrentEvent == sequence.SortedEvents.Count - 1 ? sequence.TotalFrameCount - 1 : FindClosestFrameToFrame(m_Event.LastFrame, false, indexOfCurrentEvent, sequence, sequenceView);

            var leftDeltaFrames = m_Event.StartFrame - leftEventFrame;
            var rightDeltaFrames = rightEventFrame - m_Event.LastFrame;
            var minDeltaFrames = Math.Min(leftDeltaFrames, rightDeltaFrames);
            if (minDeltaFrames > maxDeltaFrames) {
                return mousePosition;
            }

            if (leftDeltaFrames == minDeltaFrames) {
                var p = timelineData.GetPositionAtFrame(leftEventFrame);
                return new Vector2(p + m_StartDragMouseDeltaFromStartFrame, mousePosition.y);
            } else {
                var p = timelineData.GetPositionAtFrame(rightEventFrame);
                return new Vector2(p + m_StartDragMouseDeltaFromLastFrame, mousePosition.y);
            }
        }

        protected GUIStyle GetNodeStyle() {
            bool hasConditions = m_Event.Conditions.Count > 0;
            GUIStyle style = null;
            if (m_Event.IsValid()) {
                style = m_Selected ? (hasConditions ? NodeConditionSelectedStyle : NodeSelectedStyle) : (hasConditions ? NodeConditionStyle : NodeStyle);
            } else {
                style = m_Selected ? NodeDisabledSelectedStyle : NodeDisabledStyle;
            }

            return style;
        }

        private int FindClosestFrameToFrame(int frame, bool left, int indexOfIgnoredEvent, RavenSequence sequence, RavenSequenceView sequenceView) {
            var closestDelta = int.MaxValue;
            var closestFrame = int.MaxValue;

            if (left) {
                for (int i = 0; i < sequence.SortedEvents.Count; ++i) {
                    var evnt = sequence.SortedEvents[i];
                    RavenEventView eventView;
                    sequenceView.GetEventView(evnt, out eventView);
                    if (i == indexOfIgnoredEvent || eventView.m_Selected) {
                        continue;
                    }

                    if (i > indexOfIgnoredEvent && evnt.StartFrame > frame) {
                        break;
                    }

                    var isBookmark = evnt.EventType == ERavenEventType.Bookmark;
                    if (evnt.LastFrame <= frame) {
                        var deltaLast = frame - evnt.LastFrame - (isBookmark ? 0 : 1);
                        if (deltaLast >= 0 && deltaLast < closestDelta) {
                            closestDelta = deltaLast;
                            closestFrame = evnt.LastFrame + (isBookmark ? 0 : 1);
                        }
                    } else if (evnt.StartFrame <= frame) {
                        var deltaStart = frame - evnt.StartFrame;
                        if (deltaStart >= 0 && deltaStart < closestDelta) {
                            closestDelta = deltaStart;
                            closestFrame = evnt.StartFrame;
                        }
                    }
                }
            } else {
                for (int i = 0; i < sequence.SortedEvents.Count; ++i) {
                    var evnt = sequence.SortedEvents[i];
                    RavenEventView eventView;
                    sequenceView.GetEventView(evnt, out eventView);
                    if (i == indexOfIgnoredEvent || eventView.m_Selected) {
                        continue;
                    }

                    if (i > indexOfIgnoredEvent && evnt.StartFrame >= closestFrame) {
                        break;
                    }

                    var deltaLast = evnt.LastFrame - frame;
                    if (deltaLast >= 0 && deltaLast < closestDelta) {
                        closestDelta = deltaLast;
                        closestFrame = evnt.LastFrame;
                    }
                    var deltaStart = evnt.StartFrame - frame - 1;
                    if (deltaStart >= 0 && deltaStart < closestDelta) {
                        closestDelta = deltaStart;
                        closestFrame = evnt.StartFrame - 1;
                    }
                }
            }

            return closestFrame;
        }
    }
}