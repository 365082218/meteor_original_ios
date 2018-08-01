using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Graphics;
using Outfit7.Logic.StateMachineInternal;

namespace Outfit7.Sequencer {
    public class SequencerSetToTransformEvent : SequencerTriggerEvent {
        public ParameterComponentField ComponentField = new ParameterComponentField(null);
        public bool AffectPosition = true;
        public bool AffectRotation = true;
        public bool AffectScale = true;
        private Transform ToTransform;

        public override void OnInit() {
            ComponentType = typeof(UnityEngine.Transform);
        }

        public override void OnPreplay() {
            for (int i = 0; i < Objects.Count; i++) {
                OnTrigger(Objects[i].Components, 0);
            }
        }

        protected override void OnLiveInit(SequencerSequence sequence) {
            ComponentField.LiveInit(sequence.Parameters);
        }

        public override void OnTrigger(List<Component> components, float currentTime) {
            ComponentField.Init(null);
            if (components.Count == 0)
                return;
            ToTransform = ComponentField.Value as Transform;
            Transform myTransform = components[0] as Transform;
            if (myTransform != null && ToTransform != null) {
                if (AffectPosition)
                    myTransform.position = ToTransform.position;
                if (AffectRotation)
                    myTransform.rotation = ToTransform.rotation;
                if (AffectScale)
                    myTransform.localScale = ToTransform.localScale;
            }
        }
    }
}