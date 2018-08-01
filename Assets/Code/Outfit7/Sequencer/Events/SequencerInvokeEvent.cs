using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Logic.StateMachineInternal;
using Outfit7.Logic;
using System;


namespace Outfit7.Sequencer {
    public class SequencerInvokeEvent : SequencerTriggerEvent {
        public ParameterComponentField ComponentField = new ParameterComponentField(null);
        [SerializeField] public InvokeEvent InvokeEvent = new InvokeEvent();
        [NonSerialized] public List<MonoBehaviour> CallbackMonoBehaviours;


        protected override void Awake() {
            base.Awake();
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
#endif
            //TODO make custom awake function, should be safer
            GameObject target = ComponentField.Value.gameObject;
            CallbackMonoBehaviours = new List<MonoBehaviour>(target.GetComponents<MonoBehaviour>());
        }

        public override bool IgnoreObjects() {
            return true;
        }

        public override void OnTrigger(List<Component> components, float currentTime) {
            if (!Application.isPlaying)
                return;

            InvokeEvent.Initialize(); 

            ComponentField.Init(null);
            InvokeEvent.Invoke(CallbackMonoBehaviours);

        }
    }
}   