using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Graphics;
using Outfit7.Logic.StateMachineInternal;

namespace Outfit7.Sequencer {
    public class SequencerSetMaterialTextureEvent : SequencerTriggerEvent {
        public string TextureSamplerName = "_MainTex";
        public Texture NewTexture;

        public override void OnInit() {
            ComponentType = typeof(UnityEngine.Renderer);
        }

        public override void OnPreplay() {
            for (int i = 0; i < Objects.Count; i++) {
                OnTrigger(Objects[i].Components, 0);
            }
        }

        protected override void OnLiveInit(SequencerSequence sequence) {
        }

        public override void OnTrigger(List<Component> components, float currentTime) {
            if (components.Count == 0)
                return;
            
            Renderer renderer = components[0] as Renderer;
            if (renderer != null) {
                renderer.sharedMaterial.SetTexture(TextureSamplerName, NewTexture);
            }
        }
    }
}