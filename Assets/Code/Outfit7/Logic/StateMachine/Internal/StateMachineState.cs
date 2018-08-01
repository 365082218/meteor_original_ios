using System;
using System.Collections.Generic;
using UnityEngine;

namespace Outfit7.Logic.StateMachineInternal {

    [Serializable]
    public class State : Base {
        // Serialized
        public string Comment;
        public int[] TransitionIndices;
#if UNITY_EDITOR
        public AnimationCurve TimeAnimationCurve = AnimationCurve.Linear(0, 0, 1, 1);
#else
        public AnimationCurve TimeAnimationCurve = null;
#endif
        public bool UseNormalizedTime;
        public bool Loop = false;
        public Vector2 PlayRange = new Vector2(0.0f, 1.0f);
        public bool Switch = false;
        public bool TimeInSeconds = false;
        public int AttributeMask = 0;
        public InvokeEvent[] EnterEvents;
        public InvokeEvent[] ExitEvents;
        public float Speed = 1.0f;
        public float Weight = 1.0f;
        // Editor only
        public Vector2 EditorPosition;

        protected void InitializeEvents(InvokeEvent[] events) {
            for (int i = 0; i < events.Length; i++) {
                InvokeEvent e = events[i];
                e.Initialize();
            }
        }

        public virtual void OnInitialize(StateMachine stateMachine) {
            InitializeEvents(EnterEvents);
            InitializeEvents(ExitEvents);
            for (int j = 0; j < TransitionIndices.Length; j++) {
                Transition transition = stateMachine.Transitions[TransitionIndices[j]];
                InitializeEvents(transition.PreEnterEvents);
                InitializeEvents(transition.PostEnterEvents);
            }
        }

        public virtual bool OnEnter(StateMachine stateMachine, Layer layer, int layerIndex, LayerStateData stateData, bool callOnEnter) {
            return true;
        }

        public virtual void OnExit(StateMachine stateMachine, Layer layer, int layerIndex, LayerStateData stateData) {
        }

        public virtual void OnUpdateReferernces(StateMachine stateMachine, Layer layer) {
        }

        public virtual void OnUpdate(StateMachine stateMachine, Layer layer, LayerStateData stateData, ref float normalizedSpeed) {
        }

    }
}