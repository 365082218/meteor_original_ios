using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Logic.StateMachineInternal;


namespace Outfit7.Sequencer {
    public class SequencerChangeMaterialEvent : SequencerContinuousEvent {
        public bool SelfInstantiate = true;
        public Material NewMaterial;
        public bool ChangeBack = true;
        public Material OldMaterial;
        private bool MaterialChanged = false;

        public override void OnInit() {
            ComponentType = typeof(UnityEngine.Renderer);
        }

        public override void OnEnter(List<Component> components, float absoluteTime, float normalizedTime) {   
            MaterialChanged = false;
            if (components.Count == 0)
                return;
            Renderer renderer = components[0] as Renderer;
            if (renderer == null)
                return;

            if (SelfInstantiate) {
                OldMaterial = renderer.sharedMaterial;
                renderer.material = (Material) Material.Instantiate(OldMaterial);
                MaterialChanged = true;
            } else if (NewMaterial != null) {
                OldMaterial = renderer.sharedMaterial;
                renderer.material = (Material) Material.Instantiate(NewMaterial);
                MaterialChanged = true;
            }
        }

        public override void OnProcess(List<Component> components, float absoluteTime, float normalizedTime) {
        }

        public override void OnExit(List<Component> components) {
            if (components.Count == 0)
                return;
            Renderer renderer = components[0] as Renderer;
            if (renderer == null)
                return;
            if (!MaterialChanged || OldMaterial == null)
                return;

            if (ChangeBack) {
                renderer.material = OldMaterial;
                OldMaterial = null;
            }
        }
    }
}   