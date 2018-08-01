using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Outfit7.Logic.StateMachineInternal;
using Outfit7.Logic.Util;

namespace Outfit7.Logic {

    public partial class StateMachineEditor {

        public void AddUndo(string action) {
            Undo.RecordObject(TargetStateMachine, string.Format("{0}: {1}", TargetStateMachine.name, action));
        }

        // Delegates
        private void OnAddState() {
            AddUndo("AddState");
            AddState(ActiveLayer, GetUniqueName(ActiveLayer.StateIndices, TargetStateMachine.States, NewStateName), GetGlobalPosition(Button1MousePosition.GlobalPosition - (StateSize * 0.5f)));
        }

        private void OnRemoveState() {
            AddUndo("RemoveState");
            if (OverState == ActiveState) {
                ActiveState = null;
            }
            ActiveDragState = null;
            RemoveState(ActiveLayer, OverState);
            EditorUtility.SetDirty(TargetStateMachine);
        }

        private  void OnDefaultState() {
            AddUndo("SetDefaultState");
            SetDefaultState(ActiveLayer, OverState);
            EditorUtility.SetDirty(TargetStateMachine);
        }

        private void OnAddTransition() {
            AddUndo("AddTransition");
            TransitionFromState = OverState;
            ActiveState = null;
            ActiveTransition = null;
            EditorUtility.SetDirty(TargetStateMachine);
        }

        private void OnRemoveTransition() {
            AddUndo("RemoveTransition");
            if (OverTransition == ActiveTransition) {
                ActiveTransition = null;
                ActiveTransitionState = null;
            }
            RemoveTransition(OverTransitionState, OverTransition);
            EditorUtility.SetDirty(TargetStateMachine);
        }

        private void OnMakeTransition() {            
            if (OverState != null && OverState != TransitionFromState && !IsAnyState(ActiveLayer, OverState)) {
                AddUndo("MakeTransition");
                AddTransition(ActiveLayer, TransitionFromState, OverState, TargetStateMachine.Parameters[ActiveLayerIndex * BuildInParameterNames.Length]);
                EditorUtility.SetDirty(TargetStateMachine);
            }
        }

        protected virtual void OnSelectState() {
            AddUndo("SelectState");
            ActiveTransition = null;
            ActiveState = OverState;
            SelectionOffset = ActiveState.EditorPosition - Button0MousePosition.GlobalPosition;
            ActiveDragState = OverState;
            ActiveTransitionInGUI = null;
        }

        protected virtual void OnSelectTransition() {
            AddUndo("SelectTransition");
            ActiveTransition = OverTransition;
            ActiveTransitionState = OverTransitionState;
            ActiveState = null;
            ActiveDragState = null;
            ActiveTransitionInGUI = null;
        }

        protected virtual void OnDeselect() {
            AddUndo("Deselect");
            ActiveTransition = null;
            ActiveTransitionState = null;
            ActiveState = null;
            ActiveDragState = null;
            ActiveTransitionInGUI = null;
        }

        // Input
        private void SelectBeginAction() {
            // Create transition
            if (TransitionFromState != null) {
                OnMakeTransition();
            } else if (OverState != null) {
                OnSelectState();
            } else if (OverTransition != null) {
                OnSelectTransition();
            } else {
                OnDeselect();
            }
        }

        private void SelectDragAction() {
            if (ActiveState != null) {
                Vector2 newPosition = GetGlobalPosition(CurrentMousePosition.GlobalPosition + SelectionOffset);
                ActiveState.EditorPosition = newPosition;
                EditorUtility.SetDirty(TargetStateMachine);
            }
        }

        private void SelectEndAction() {
            TransitionFromState = null;
            ActiveDragState = null;
        }

        private void ContextAction() {
            GenericMenu menu = new GenericMenu();
            if (OverState == null) {
                menu.AddItem(new GUIContent("Add state"), false, OnAddState);
            }
            if (OverState != null) {
                if (!IsAnyState(ActiveLayer, OverState)) {
                    menu.AddItem(new GUIContent("Remove state"), false, OnRemoveState);
                    menu.AddItem(new GUIContent("Set as Default State"), false, OnDefaultState);
                }
                menu.AddItem(new GUIContent("Add transition"), false, OnAddTransition);
            }
            if (OverTransition != null) {
                menu.AddItem(new GUIContent("Remove transition"), false, OnRemoveTransition);
            }
            menu.ShowAsContext();
        }

        private void ViewAction() {
            PositionOffset -= UnityEngine.Event.current.delta / Zoom;
        }

        private bool HandleInput() {
            if (!WindowInFocus) {
                return false;
            }
            bool repaint = false;
            CurrentMousePosition.SetPosition(UnityEngine.Event.current.mousePosition, PositionOffset, HalfWindowSize, Zoom);
            OverState = null;
            OverTransition = null;
            float mouseToTransition = 10000.0f;
            // Find over state
            for (int i = 0; i < ActiveLayer.StateIndices.Length; i++) {
                State state = TargetStateMachine.States[ActiveLayer.StateIndices[i]];
                if (GetStateRect(state).Contains(CurrentMousePosition.Position)) {
                    OverState = state;
                }
                // Find over transition
                for (int j = 0; j < state.TransitionIndices.Length; j++) {
                    Transition transition = TargetStateMachine.Transitions[state.TransitionIndices[j]];
                    Vector2 from = GetStateRect(state).center;
                    Vector2 to = GetStateRect(transition.DestinationState).center;
                    Vector2 position = Geometry.GetClosestPointOnLineSegment(CurrentMousePosition.Position, GetFromStatePosition(from, to, true), GetFromStatePosition(to, from, false));
                    float distance = Vector2.Distance(position, CurrentMousePosition.Position);
                    if (distance < MaxTransitionDistance * Zoom && distance < mouseToTransition) {
                        mouseToTransition = distance;
                        OverTransition = transition;
                        OverTransitionState = state;
                    }
                }
            }
            // Handle mouse press
            if (UnityEngine.Event.current.type == EventType.MouseDown) {
                repaint = true;
                if (IsSelectMouseAction()) {
                    Button0MousePosition.SetPosition(UnityEngine.Event.current.mousePosition, PositionOffset, HalfWindowSize, Zoom);
                    SelectBeginAction();
                } else if (IsRightClickMouseAction()) {
                    Button1MousePosition.SetPosition(UnityEngine.Event.current.mousePosition, PositionOffset, HalfWindowSize, Zoom);
                    ContextAction();
                } else {
                    Button2MousePosition.SetPosition(UnityEngine.Event.current.mousePosition, PositionOffset, HalfWindowSize, Zoom);
                }
            } else if (UnityEngine.Event.current.type == EventType.MouseUp) {
                repaint = true;
                if (IsSelectMouseAction()) {
                    SelectEndAction();
                }
            }
            // Handle mouse drag
            if (UnityEngine.Event.current.type == EventType.MouseDrag) {
                if (IsMoveViewMouseAction()) {
                    ViewAction();
                } else if (IsSelectMouseAction() && ActiveState == ActiveDragState) {
                    SelectDragAction();
                }
            }
            // Handle zoom
            if (UnityEngine.Event.current.type == EventType.ScrollWheel) {
                Zoom -= UnityEngine.Event.current.delta.y * 0.01f;
                Zoom = Mathf.Clamp(Zoom, ZoomLimit.x, ZoomLimit.y);
            }
            // Handle focus
            if (UnityEngine.Event.current.keyCode == KeyCode.F) {
                if (ActiveState != null) {
                    PositionOffset = ActiveState.EditorPosition + StateSize * 0.5f;
                } else if (ActiveTransition != null) {
                    Vector2 position = ActiveTransitionState.EditorPosition;
                    if (ActiveTransition.DestinationState != null) {  
                        position = (position + ActiveTransition.DestinationState.EditorPosition) * 0.5f;
                    }
                    PositionOffset = position;
                } else if (ActiveLayer.CurrentState != null) {
                    PositionOffset = ActiveLayer.CurrentState.EditorPosition + StateSize * 0.5f;
                } else {
                    Bounds bounds = GetLayerEditorPositionBounds(ActiveLayer);
                    PositionOffset = bounds.center;
                }
            }
            // Handle keyboard
            SnapPosition = UnityEngine.Event.current.shift == false;
            return repaint;
        }

        private bool IsSelectMouseAction() {
            return UnityEngine.Event.current.button == 0 && !UnityEngine.Event.current.alt;
        }

        private bool IsMoveViewMouseAction() {
            return UnityEngine.Event.current.button == 2 || (UnityEngine.Event.current.button == 0 && UnityEngine.Event.current.alt);
        }

        private bool IsRightClickMouseAction() {
            return UnityEngine.Event.current.button == 1;
        }

    }

}
