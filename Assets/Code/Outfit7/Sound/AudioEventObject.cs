using UnityEngine;

namespace Outfit7.Audio {

    [System.Serializable]
    public class AudioEventObject {

        public AbstractAudioEventData AudioEventData = null;
        [Range(0.0f, 1.0f)]
        public float Volume = 1.0f;
        [Range(0.0f, 5.0f)]
        public float Pitch = 1.0f;
        [Range(0.0f, 10.0f)]
        public float Delay = 0.0f;
        //
        public Transform AttachTransform = null;
        public int UserDefinedPlayIndex = 0;

        private void InternalPlay(float v, float p, AudioEventInfo info, Transform t) {
            if (AudioEventData == null || v <= 0.0001f || p <= 0.0001f) {
#if VERBOSE
                Util.O7Log.DebugT("AudioEventObject", "InternalPlay: skipping play: {0} {1} {2}", AudioEventData, v, p);
#endif
                return;
            }
            //
            AudioEvent ae = AudioEventManager.Instance.CreateAudioEvent(AudioEventData.GetAudioEventData(), AttachTransform, EAudioEventAttributes.AutoDelete);
            if (ae != null) {
                ae.Volume = v;
                ae.Pitch = p;
                if (t != null) {
                    ae.transform.position = t.position;
                }
                ae.PlayFullArg(true, UserDefinedPlayIndex, info, Delay);
            }
        }

        public void Play(float volume, float pitch) {
            float v = volume * Volume;
            float p = pitch * Pitch;
            //
            InternalPlay(v, p, null, null);
        }

        public void Play() {
            InternalPlay(Volume, Pitch, null, null);
        }

        public void PlayWithInfo(AudioEventInfo info) {
            InternalPlay(Volume, Pitch, info, null);
        }

        public void PlayOnPosition(Transform t) {
            InternalPlay(Volume, Pitch, null, t);
        }
    }
}