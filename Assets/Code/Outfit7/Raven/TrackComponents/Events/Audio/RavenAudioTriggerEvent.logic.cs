namespace Starlite.Raven {

    public sealed partial class RavenAudioTriggerEvent {

        public override bool IsBarrier {
            get {
                return false;
            }
        }

        public override RavenPropertyComponent PropertyComponent {
            get {
                return null;
            }
        }

        protected override void OnEnterCallback(int frame) {
            if (m_AudioEventData == null) {
                return;
            }

#if !STARLITE_EDITOR
#if UNITY_EDITOR
            if (UnityEngine.Application.isPlaying) {
                if (Outfit7.Audio.AudioEventManager.Instance != null) {
#endif
                    var audioEvent = Outfit7.Audio.AudioEventManager.Instance.CreateAudioEvent(m_AudioEventData);
                    if (audioEvent != null) {
                        audioEvent.Volume = m_Volume;
                        audioEvent.Pitch = m_Pitch;
                        audioEvent.Play();
                        audioEvent.EventAttributes |= Outfit7.Audio.EAudioEventAttributes.AutoDelete;
                    }
#if UNITY_EDITOR
                }
            }
#endif
#endif
        }
    }
}