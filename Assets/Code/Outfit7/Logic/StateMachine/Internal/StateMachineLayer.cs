using System;
using System.Collections.Generic;
using UnityEngine;
using Outfit7.Util;

namespace Outfit7.Logic.StateMachineInternal {

    public class LayerStateData {
    }

    [Serializable]
    public class Layer : Base {
        // Constants
        private const int MaxIterationCount = 10;
        // Serialized
        public bool Enabled = true;
        public string Comment;

        public float Weight = 1.0f;

        public int[] StateIndices;
        public int DefaultStateIndex = -1;

        public float NormalizedTime = 0.0f;
        public float Speed = 1.0f;

        // Internal
        [NonSerialized] public State DefaultState;

        [NonSerialized] public float PreviousNormalizedTime = 0.0f;
        [NonSerialized] public float CurrentNormalizedTime = 0.0f;
        [NonSerialized] public float PreviousNormalizedSpeed = 1.0f;
        [NonSerialized] public float CurrentNormalizedSpeed = 1.0f;
        [NonSerialized] public float TransitionSpeed = 1.0f;
        [NonSerialized] public float CurrentStateWeight = 1.0f;
        [NonSerialized] public float MaxPreviousStateWeight = 0.0f;
        [NonSerialized] public float MaxCurrentStateWeight = 0.0f;

        [NonSerialized] public State CachedPreviousState;
        [NonSerialized] public State PreviousState;
        [NonSerialized] public State CurrentState;
        [NonSerialized] public int PreviousStateIndex;
        [NonSerialized] public int CurrentStateIndex;
        [NonSerialized] public int LoopCount = 0;

        [NonSerialized] public Transition CurrentTransition;
        [NonSerialized] public LayerStateData CurrentStateData = null;
        [NonSerialized] public LayerStateData PreviousStateData = null;

        [NonSerialized] private List<Transition> ActiveTransitions;
        [NonSerialized] private Transition InitialTransition = new Transition();
        [NonSerialized] private bool InReset = false;

        public virtual void Initialize(StateMachine stateMachine) {
            ActiveTransitions = new List<Transition>();
            // Initialize events
            for (int i = 0; i < StateIndices.Length; i++) {
                State state = stateMachine.States[StateIndices[i]];
                state.OnInitialize(stateMachine);
            }
        }

        protected virtual void OnPreReset(StateMachine stateMachine) {
        }

        protected virtual void OnPostReset(StateMachine stateMachine) {
        }

        public virtual void Reset(StateMachine stateMachine) {
            if (InReset) {
                return;
            }
            InReset = true;
            OnPreReset(stateMachine);
            if (CurrentState != null) {
                TriggerStateEvents(stateMachine, CurrentState.ExitEvents);
            }
            NormalizedTime = 0.0f;
            CachedPreviousState = null;
            PreviousState = null;
            CurrentState = null;
            PreviousStateIndex = -1;
            CurrentStateIndex = -1;
            ActiveTransitions.Clear();
            OnPostReset(stateMachine);
            InReset = false;
        }

        private Transition CheckStateTransitions(StateMachine stateMachine, State state) {
            for (int i = 0; i < state.TransitionIndices.Length; i++) {
                Transition transition = stateMachine.Transitions[state.TransitionIndices[i]];
                if (!transition.Enabled) {
                    continue;
                }
                bool conditionsOk = true;
                // Check conditions
                for (int j = 0; j < transition.Conditions.Length; j++) {
                    Condition condition = transition.Conditions[j];
#if UNITY_EDITOR
                    if (condition.Parameter == null) {
                        O7Log.ErrorT(stateMachine.Tag, "Condition {0} in state {1} transition {2} invalid!", condition.ParameterIndex, state.Name, transition.DestinationState.Name);
                        continue;
                    }
#endif
                    if (!condition.IsTrue()) {
                        conditionsOk = false;
                        break;
                    }
                }
                // Set next transition
                if (conditionsOk) {
                    return transition;
                }
            }
            return null;
        }

        private float UpdateWeight(float weight, float weightTarget, float weightSpeed, float deltaTime) {
            if (weightSpeed <= 0.0f)
                return weightTarget;
            if (weight < weightTarget) {
                return Mathf.Min(weight + weightSpeed * deltaTime, weightTarget);
            } else {
                return Mathf.Max(weight - weightSpeed * deltaTime, weightTarget);
            }
        }

        protected virtual bool OnUpdateCurrentStateTime(StateMachine stateMachine, float weight, float normalizedSpeed, Parameter timeParameter, int layerIndex) {
            return false;
        }

        protected virtual bool OnUpdatePreviousStateTime(StateMachine stateMachine, float weight, float normalizedSpeed, Parameter timeParameter, int layerIndex) {
            return false;
        }

        protected virtual void OnUpdateWeights(StateMachine stateMachine, float deltaTime, float realDeltaTime, Parameter timeParameter, int layerIndex) {
            // Blend new state ine
            float weight = 1.0f;
            if (CurrentState != null) {
                float normalizedSpeed = CurrentNormalizedSpeed * CurrentState.Speed * Speed * deltaTime;
                float speed = 1.0f;
                // Update weight
                CurrentStateWeight = Speed > 0.0f ? UpdateWeight(CurrentStateWeight, 1.0f, TransitionSpeed * Speed, realDeltaTime) : CurrentStateWeight;
                weight = CurrentTransition != null && CurrentTransition.AnimationCurve != null ? CurrentTransition.AnimationCurve.Evaluate(CurrentStateWeight) : CurrentStateWeight;
                if (weight >= 1.0f) {
                    CurrentTransition = null;
                }
                MaxCurrentStateWeight = weight;
                // Don't update time
                if (CurrentTransition != null && !CurrentTransition.UpdateTime) {
                    speed = 0.0f;
                }
                // Update states
                if (!OnUpdateCurrentStateTime(stateMachine, weight * CurrentState.Weight, normalizedSpeed * speed, timeParameter, layerIndex)) {
                    // Just update time
                    timeParameter.ValueFloat += deltaTime * speed;
                }
            }
            // Blend previous state out
            if (PreviousState != null) {
                float previousWeight = (1.0f - weight) * MaxPreviousStateWeight;
                // Update speed
                float normalizedSpeed = PreviousNormalizedSpeed * PreviousState.Speed * Speed * deltaTime;
                // Update animation time
                OnUpdatePreviousStateTime(stateMachine, previousWeight * PreviousState.Weight, normalizedSpeed, timeParameter, layerIndex);
                // Completely blend out
                if (previousWeight <= 0.000001f) {
                    // Exit previous state
                    OnTransitionExitState(stateMachine, PreviousState, PreviousStateData);
                    PreviousNormalizedSpeed = 0.0f;
                    PreviousNormalizedTime = 0.0f;
                    MaxPreviousStateWeight = 0.0f;
                    PreviousState = null;
                }
            }
        }


        public virtual void OnUpdate(StateMachine stateMachine, float deltaTime, Parameter timeParameter, Parameter attributeParameter, int index) {
            if (!Enabled) {
                Reset(stateMachine);
                return;
            }
            if (DefaultState == null) {
                throw new System.Exception("Default state is null!");
            }
            float realDeltaTime = deltaTime;
            // Update if there's no active state or not paused
            if (CurrentState == null || stateMachine.UpdateIfPaused || (!stateMachine.UpdateIfPaused && deltaTime > 0.0f)) {
                // Iterate state machine
                int iteration = 0;
                bool canIterate = true;
                while (true) {
                    if (canIterate && (CurrentState == null || CurrentState.Switch || CurrentTransition == null || !CurrentTransition.Atomic)) {
                        // Get next transition
                        Transition nextTransition = null;
                        // Check any state first (but only in first iteration)
                        if (iteration == 0) {
                            nextTransition = CheckStateTransitions(stateMachine, stateMachine.States[StateIndices[0]]);
                        }
                        // Only update if there's no anystate transition
                        if (nextTransition == null) {
                            // On restart create new transition with default state
                            if (CurrentState == null) {
                                nextTransition = InitialTransition;
                                nextTransition.DestinationState = DefaultState;
                                nextTransition.DestinationStateIndex = DefaultStateIndex;
                                nextTransition.Duration = 0.0f;
                                nextTransition.Conditions = new Condition[0];
                            } else {
                                if (nextTransition == null) {
                                    nextTransition = CheckStateTransitions(stateMachine, CurrentState);
                                }
                            }
                        }
                        // State change
                        if (nextTransition != null) {
                            // Call events
                            stateMachine.Callbacks.PreStateChange(stateMachine, index, CurrentStateIndex, nextTransition.DestinationStateIndex);
                            // Reset time
                            timeParameter.ValueFloat = 0.0f;
                            canIterate = OnTransition(stateMachine, timeParameter, nextTransition, index);
                            // Set attribute mask if not switch
                            if (!nextTransition.DestinationState.Switch) {
                                attributeParameter.ValueInt = CurrentState.AttributeMask;
                            }
                            // Clear delta time to prevent first frame being skipped
                            deltaTime = 0.0f;
                            // Call events
                            stateMachine.Callbacks.PostStateChange(stateMachine, index, PreviousStateIndex, CurrentStateIndex);
                            // Debug
                            if (stateMachine.DebugInfo != DebugInfoType.None) {
                                O7Log.DebugT(stateMachine.Tag, "{0} Layer:{1} From:{2} To:{3}", stateMachine.name, Name, PreviousState != null ? PreviousState.Name : "None", CurrentState.Name);
                            }
                        } else {
                            // Break iteration
                            break;
                        }
                    } else {
                        break;
                    }
                    iteration++;
                    if (iteration >= MaxIterationCount) {
                        throw new System.Exception(string.Format("Infinite loop in {0}/{1}!", stateMachine.name, CurrentState.Name));
                    }
                }
            }
            // For switch only update time
            if (CurrentState.Switch) {
                timeParameter.ValueFloat += deltaTime;
            } else {
                // Update blend tree and weights
                if (PreviousState != null) {
                    PreviousState.OnUpdate(stateMachine, this, PreviousStateData, ref PreviousNormalizedSpeed);
                }
                CurrentState.OnUpdate(stateMachine, this, CurrentStateData, ref CurrentNormalizedSpeed);
                OnUpdateWeights(stateMachine, deltaTime, realDeltaTime, timeParameter, index);
            }
        }

        public virtual void OnPostUpdate(int layerIndex) {
            // Reset triggers
            for (int i = 0; i < ActiveTransitions.Count; i++) {
                ActiveTransitions[i].ResetTriggers();
            }
            ActiveTransitions.Clear();
        }

        public static void TriggerStateEvents(StateMachine stateMachine, InvokeEvent[] events) {
            if (events == null) {
                return;
            }
            for (int i = 0; i < events.Length; i++) {
                InvokeEvent e = events[i];
                e.Invoke(stateMachine.CallbackMonoBehaviours);
            }
        }

        protected virtual void OnTransitionState(StateMachine stateMachine, State previousState, State newState) {
        }

        protected virtual void OnTransitionExitState(StateMachine stateMachine, State state, LayerStateData stateData) {
        }

        protected virtual bool OnTransition(StateMachine stateMachine, Parameter timeParameter, Transition transition, int index) {
            // Trigger exit state
            if (CurrentState != null) {
                TriggerStateEvents(stateMachine, CurrentState.ExitEvents);
                CurrentState.OnExit(stateMachine, this, index, CurrentStateData);
            }
            // Remember weight
            float previousStateWeight = CurrentStateWeight;
            // Switch to new state
            if (CurrentState != null && !CurrentState.Switch) {
                OnTransitionState(stateMachine, CurrentState, transition.DestinationState);
                // Switch animation states
                LayerStateData tempStateData = PreviousStateData;
                PreviousStateData = CurrentStateData;
                CurrentStateData = tempStateData;
                PreviousNormalizedTime = CurrentNormalizedTime;
                PreviousNormalizedSpeed = CurrentNormalizedSpeed;
                CurrentNormalizedTime = 0.0f;
                MaxPreviousStateWeight = MaxCurrentStateWeight;
                MaxCurrentStateWeight = 0.0f;
                // Switch states
                CachedPreviousState = CurrentState;
                PreviousState = CurrentState;
                PreviousStateIndex = CurrentStateIndex;
            } else if (CurrentState == null) {
                TransitionSpeed = 0.0f;
            }
            // Set transition duration (Special case for switch states)
            if (CurrentState != null && (!CurrentState.Switch || CurrentTransition == null || transition.Duration > CurrentTransition.Duration)) {
                CurrentTransition = transition;
                TransitionSpeed = (transition.Duration > 0.0f ? 1.0f / transition.Duration : 0.0f);
            }
            LoopCount = 0;
            CurrentState = transition.DestinationState;
            CurrentStateIndex = transition.DestinationStateIndex;
            CurrentNormalizedTime = CurrentState.PlayRange.x;
            // Trigger enter state events
            TriggerStateEvents(stateMachine, CurrentState.EnterEvents);
            // Trigger pre enter transition events
            if (transition != null) {
                TriggerStateEvents(stateMachine, transition.PreEnterEvents);
            }
            // Only enter state in a non-switch state
            if (!CurrentState.Switch) {
                if (TransitionSpeed > 0.0f) {
                    CurrentStateWeight = 1.0f - previousStateWeight;
                } else {
                    CurrentStateWeight = 1.0f;
                }
                // Stop previous state
                if (TransitionSpeed <= 0.0f) {
                    OnTransitionExitState(stateMachine, PreviousState, PreviousStateData);
                }
                // Stop current state
                OnTransitionExitState(stateMachine, CurrentState, CurrentStateData);
            }
            // Enter state
            CurrentState.OnEnter(stateMachine, this, index, CurrentStateData, true);
            // Trigger post enter transition events
            if (transition != null) {
                TriggerStateEvents(stateMachine, transition.PostEnterEvents);
            }
            ActiveTransitions.Add(transition);
            return true;
        }

        public int FindStateIndex(StateMachine stateMachine, string name) {
            return StateMachine.FindBaseIndex(name, StateIndices, stateMachine.States);
        }

        public void UpdateReferences(StateMachine stateMachine) {
            DefaultState = stateMachine.GetStateByIndex(DefaultStateIndex);
            for (int i = 0; i < StateIndices.Length; i++) {
                State state = stateMachine.States[StateIndices[i]];
                state.OnUpdateReferernces(stateMachine, this);
                for (int j = 0; j < state.TransitionIndices.Length; j++) {
                    Transition transition = stateMachine.Transitions[state.TransitionIndices[j]];
                    transition.DestinationState = stateMachine.GetStateByIndex(transition.DestinationStateIndex);
                    for (int k = 0; k < transition.Conditions.Length; k++) {
                        Condition condition = transition.Conditions[k];
                        condition.Parameter = stateMachine.GetParameterByIndex(condition.ParameterIndex);
                        condition.ValueParameter = stateMachine.GetParameterByIndex(condition.ValueIndex);
                    }
                }
            }
        }
    }

}
