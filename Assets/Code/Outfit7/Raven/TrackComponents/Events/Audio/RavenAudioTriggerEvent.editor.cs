using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenAudioTriggerEvent {
#if UNITY_EDITOR

        public Outfit7.Audio.AudioEventData AudioEventData {
            get {
                return m_AudioEventData;
            }
            set {
                m_AudioEventData = value;
            }
        }

        public float Volume {
            get {
                return m_Volume;
            }
            set {
                m_Volume = value;
            }
        }

        public float Pitch {
            get {
                return m_Pitch;
            }
            set {
                m_Pitch = value;
            }
        }

        public override RavenAnimationDataComponentBase AnimationDataEditorOnly {
            get {
                return null;
            }
        }

        protected override void OnSetTargetEditor(RavenSequence sequence, GameObject target) {
        }

#endif
    }
}