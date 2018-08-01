using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Outfit7.Logic.StateMachineInternal;
using Outfit7.Logic.Util;

namespace Outfit7.Logic {

    public partial class StateMachineEditor {
     
        // GUI
        protected Vector2 GetStatePosition(State state) {
            return (state.EditorPosition - PositionOffset) * Zoom + HalfWindowSize;
        }

        protected Rect GetStateRect(State state) {
            Vector2 position = GetStatePosition(state);
            return new Rect(position.x, position.y, ScaledStateSize.x, ScaledStateSize.y);
        }

        protected Vector2 GetFromStatePosition(Vector2 from, Vector2 to, bool side) {
            Vector2 dir = Vector3.Cross((to - from).normalized, Vector3.forward);
            return from + dir * (ScaledStateSize.y * (side ? TransitionOffset : -TransitionOffset));
        }

        protected void DrawTransitionArrows(Vector2 from, Vector2 to, Color color, int count) {
            Vector2 dir = to - from;
            float dirLength = dir.magnitude;
            if (dirLength < 0.0001f) {
                return;
            }
            dir /= dirLength;
            Vector2 right = Vector3.Cross(dir, Vector3.forward);
            Vector2 center = Vector2.Lerp(from, to, 0.5f);
            Vector2[] points = new Vector2[3];
            float distance = 0.0f;
            float offset = ArrowSize * 0.5f;
            if (count == 0) {
                distance = dirLength * 0.5f;
                offset = 0.0f;
            }
            for (int i = 0; i < Mathf.Max(1, count); i++) {
                Vector2 tempCenter = center + dir * (distance + offset);
                points[0] = tempCenter;
                points[1] = tempCenter + right * ArrowWidth - dir * ArrowSize;
                points[2] = tempCenter - right * ArrowWidth - dir * ArrowSize;
                GUIUtil.DrawTriangle(points, color);
                if ((i % 2) == 0) {
                    distance = -distance + ArrowSize;
                } else {
                    distance = -distance;
                }
            }
        }

        private void DrawActiveTransition(State state, State destinationState) {
            Vector2 from = GetStateRect(state).center;
            Vector2 to = GetStateRect(destinationState).center;
            Vector2 realFrom = GetFromStatePosition(from, to, true);
            Vector2 realTo = GetFromStatePosition(to, from, false);
            GUIUtil.DrawFatLine(realFrom, Vector2.Lerp(realFrom, realTo, ActiveLayer.CurrentStateWeight), ActiveTransitionThickness, Color.cyan);
        }

        private void DrawTransition(State state, State destinationState, bool selected, int count, Vector2 customToPosition) {
            Rect fromRect = GetStateRect(state);
            Rect toRect = destinationState != null ? GetStateRect(destinationState) : fromRect;
            Vector2 from = fromRect.center;
            Vector2 to = destinationState != null ? toRect.center : customToPosition;
            Vector2 realFrom = GetFromStatePosition(from, to, true);
            Vector2 realTo = GetFromStatePosition(to, from, false);
            Vector2 intersection;
            if (Geometry.IntersectLineRectangle(realTo, realFrom, fromRect, out intersection)) {
                realFrom = intersection;
            }
            if (destinationState != null && Geometry.IntersectLineRectangle(realFrom, realTo, toRect, out intersection)) {
                realTo = intersection;
            }
            Color color = selected ? Color.yellow : Color.white;
            if (ActiveState == state) {
                color = Color.red;
            } else if (ActiveState == destinationState) {
                color = Color.green;
            }
            GUIUtil.DrawFatLine(realFrom, realTo, TransitionThickness, color);
            DrawTransitionArrows(realFrom, realTo, color, count);
        }

        private Dictionary<State,int> GetTransitionCounts(State state) {
            Dictionary<State,int> counts = new Dictionary<State,int>();
            for (int j = 0; j < state.TransitionIndices.Length; j++) {
                Transition transition = TargetStateMachine.Transitions[state.TransitionIndices[j]];
                int value;
                if (counts.TryGetValue(transition.DestinationState, out value)) {
                    counts[transition.DestinationState]++;
                } else {
                    counts.Add(transition.DestinationState, 1);
                }
            }
            return counts;
        }

        protected virtual void OnDrawState(State state, Rect rect) {
        }

        protected void DrawProgress(Rect rect, float progress) {
            EditorGUI.LabelField(rect, "", StateActiveBackgroundStyle);
            rect.width *= progress;
            EditorGUI.LabelField(rect, "", StateActiveProgressStyle);
        }

        private void DrawState(State state, bool isAnyState) {
            GUIStyle style = null;
            if (isAnyState) {
                style = state == ActiveState ? StateAnySelectedStyle : StateAnyStyle;
            } else if (state == ActiveLayer.DefaultState) {
                style = state == ActiveState ? StateDefaultSelectedStyle : StateDefaultStyle;
            } else if (state.Switch) {
                style = state == ActiveState ? StateSwitchSelectedStyle : StateSwitchStyle;
            } else {
                style = state == ActiveState ? StateSelectedStyle : StateStyle;
            }
            Rect rect = GetStateRect(state);
            EditorGUI.LabelField(rect, "", style);
            EditorGUI.LabelField(rect, state.Name, CenteredLabelStyle);
            OnDrawState(state, rect);
            if (ActiveLayer.CurrentState == state) {
                rect.y += StateActiveYScale * ScaledStateSize.y;
                DrawProgress(rect, TargetStateMachine.Parameters[ActiveLayerIndex].ValueFloat % 1.0f);
            }
        }

        private void DrawStateMachine() {
            if (UnityEngine.Event.current.type == EventType.Repaint) {
                // Found number of transitions to the same state and draw transitions
                for (int i = 0; i < ActiveLayer.StateIndices.Length; i++) {
                    State state = TargetStateMachine.States[ActiveLayer.StateIndices[i]];
                    Dictionary<State,int> transitionCounts = GetTransitionCounts(state);
                    for (int j = 0; j < state.TransitionIndices.Length; j++) {
                        Transition transition = TargetStateMachine.Transitions[state.TransitionIndices[j]];
                        if (ActiveLayer.CurrentTransition == transition) {
                            DrawActiveTransition(state, transition.DestinationState);
                        }
                        DrawTransition(state, transition.DestinationState, false, transitionCounts[transition.DestinationState], Vector2.zero);
                    }
                }
                // Draw transition connect
                if (TransitionFromState != null) {
                    int count = 0;
                    Vector2 to = CurrentMousePosition.Position;
                    State destinationState = null;
                    if (OverState != null && OverState != TransitionFromState && !IsAnyState(ActiveLayer, OverState)) {
                        destinationState = OverState;
                        Dictionary<State,int> transitionCounts = GetTransitionCounts(TransitionFromState);
                        transitionCounts.TryGetValue(destinationState, out count);
                        count++;
                    }
                    DrawTransition(TransitionFromState, destinationState, true, count, to);
                } else {
                    // Transition selection
                    if (ActiveTransition != null) {
                        DrawTransition(ActiveTransitionState, ActiveTransition.DestinationState, true, GetTransitionCounts(ActiveTransitionState)[ActiveTransition.DestinationState], Vector2.zero);
                    } else if (ActiveTransitionInGUI != null) {
                        DrawTransition(ActiveState, ActiveTransitionInGUI.DestinationState, true, GetTransitionCounts(ActiveState)[ActiveTransitionInGUI.DestinationState], Vector2.zero);
                    }
                }
            }
            // Draw states
            for (int i = 0; i < ActiveLayer.StateIndices.Length; i++) {
                State state = TargetStateMachine.States[ActiveLayer.StateIndices[i]];
                DrawState(state, i == 0);
            }
        }

        protected virtual void OnLayerCustomGlobalWindowGUI() {
        }

        protected virtual bool OnLayerGlobalWindowGUI() {
            EditorGUILayout.BeginVertical(WindowUIStyle, GUILayout.Width(WindowUIWidth));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(string.Format("{0}:", TitleName));
            Color oldColor = GUI.color;
            GUI.color = Color.cyan;
            EditorGUILayout.LabelField(TargetStateMachine.name);
            GUI.color = oldColor;
            EditorGUILayout.EndHorizontal();
            TargetStateMachine.DebugInfo = (DebugInfoType) EditorGUILayout.EnumPopup("Debug Info", TargetStateMachine.DebugInfo);
            TargetStateMachine.Speed = EditorGUILayout.Slider("Global Speed", TargetStateMachine.Speed, 0.0f, 1.0f);
            PushBackgroundColor(CustomColor);
            OnLayerCustomGlobalWindowGUI();
            PopBackgroundColor();
            EditorGUILayout.EndVertical();
            return false;
        }

        protected virtual bool OnLayerSelectionWindowGUI() {
            // Layer selection
            string[] layerNames = new string[TargetStateMachine.Layers.Length];
            for (int i = 0; i < layerNames.Length; i++) {
                layerNames[i] = TargetStateMachine.Layers[i].Name;
            }
            EditorGUILayout.BeginVertical(WindowUIStyle, GUILayout.Width(WindowUIWidth));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Active Layer", GUILayout.Width(100));
            int newLayerIndex = EditorGUILayout.Popup(ActiveLayerIndex, layerNames, GUILayout.Width(200));
            if (newLayerIndex != ActiveLayerIndex) {
                AddUndo("SetActiveLayer");
                SetActiveLayer(newLayerIndex);
            }
            AddRemoveMoveAction addRemoveAndMove = LogicEditorCommon.AddRemoveAndMoveGUI(true, ActiveLayerIndex > 0, ActiveLayerIndex > 1, ActiveLayerIndex > 0 && ActiveLayerIndex < TargetStateMachine.Layers.Length - 1);
            if (addRemoveAndMove == AddRemoveMoveAction.Add) {
                AddUndo("AddLayer");
                AddLayer(GetUniqueName(TargetStateMachine.Layers, NewLayerName));
                SetActiveLayer(TargetStateMachine.Layers.Length - 1);
            } else if (addRemoveAndMove == AddRemoveMoveAction.Remove) {
                AddUndo("RemoveLayer");
                RemoveLayer(ActiveLayerIndex);
                SetActiveLayer(ActiveLayerIndex - 1);
            } else if (addRemoveAndMove == AddRemoveMoveAction.MoveUp) {
                AddUndo("MoveUpLayer");
                SwapLayer(ActiveLayerIndex, ActiveLayerIndex - 1);
                SetActiveLayer(ActiveLayerIndex - 1);
            } else if (addRemoveAndMove == AddRemoveMoveAction.MoveDown) {
                AddUndo("MoveDownLayer");
                SwapLayer(ActiveLayerIndex, ActiveLayerIndex + 1);
                SetActiveLayer(ActiveLayerIndex + 1);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return false;
        }

        protected virtual void OnLayerCustomWindowGUI() {
        }

        private bool OnLayerWindowGUI() {
            EditorGUILayout.BeginVertical(WindowUIStyle, GUILayout.Width(WindowUIWidth));
            bool repaint = false;
            // Show just normalized time on base layer
            if (ActiveLayerIndex > 0) {
                string name = EditorGUILayout.TextField("Layer Name", ActiveLayer.Name);
                if (name != ActiveLayer.Name) {
                    RenameLayer(name);
                    repaint = true;
                }
                ActiveLayer.Enabled = EditorGUILayout.Toggle("Enabled", ActiveLayer.Enabled);
                ActiveLayer.Weight = EditorGUILayout.Slider("Weight", ActiveLayer.Weight, 0.0f, 1.0f);
            }
            ActiveLayer.Speed = EditorGUILayout.Slider("Speed", ActiveLayer.Speed, 0.0f, 1.0f);
            ActiveLayer.NormalizedTime = EditorGUILayout.Slider("Normalized Time", ActiveLayer.NormalizedTime, 0.0f, 1.0f);
            PushBackgroundColor(CustomColor);
            OnLayerCustomWindowGUI();
            PopBackgroundColor();
            EditorGUILayout.EndVertical();
            return repaint;
        }

        protected virtual bool OnWindowCustomGUI() {
            return false;
        }

        public bool OnWindowGUI(Rect windowRect) {
            bool repaint = false;
            BeginEdit();
            WindowSize = windowRect.size;
            HalfWindowSize = WindowSize * 0.5f;
            // Styles
            InitializeStyles();
            // Draw state machine
            DrawStateMachine();
            // Global layer info
            if (OnLayerGlobalWindowGUI()) {
                repaint = true;
            }
            // Layer selection
            if (OnLayerSelectionWindowGUI()) {
                repaint = true;
            }
            // Layer info
            if (OnLayerWindowGUI()) {
                repaint = true;
            }
            // Custom
            if (OnWindowCustomGUI()) {
                repaint = true;
            }
            // Handle input
            if (HandleInput()) {
                repaint = true;
            }
            if (EndEdit(repaint)) {
                Repaint();
            }
            return true;
        }

    }

}
