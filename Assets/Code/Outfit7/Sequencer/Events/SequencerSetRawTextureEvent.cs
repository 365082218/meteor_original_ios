using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Graphics;
using Outfit7.Logic.StateMachineInternal;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Outfit7.Sequencer {
    public class SequencerSetRawTextureEvent : SequencerTriggerEvent {
        public Texture NewTexture;

        public override void OnInit() {
            ComponentType = typeof(RawImage);
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
            
            RawImage rawImage = components[0] as RawImage;
            if (rawImage != null) {
                rawImage.texture = NewTexture;
            }

            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                EditorUtility.SetDirty(rawImage);
            }
            #endif
        }
    }
}