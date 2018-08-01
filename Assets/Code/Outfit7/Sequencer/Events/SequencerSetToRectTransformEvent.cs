using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Graphics;
using Outfit7.Logic.StateMachineInternal;

namespace Outfit7.Sequencer {
    public class SequencerSetToRectTransformEvent : SequencerTriggerEvent {
        public ParameterComponentField ComponentField = new ParameterComponentField(null);
        public bool AffectPosition = true;
        public bool AffectRotation = true;
        public bool AffectScale = true;
        private RectTransform ToTransform;

        public override void OnInit() {
            ComponentType = typeof(UnityEngine.RectTransform);
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
            ToTransform = ComponentField.Value as RectTransform;
            RectTransform myTransform = components[0] as RectTransform;
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