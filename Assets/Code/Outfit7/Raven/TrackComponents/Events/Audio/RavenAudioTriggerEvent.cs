using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenAudioTriggerEvent : RavenTriggerEvent {

        [Header("Settings")]
        [SerializeField]
        private Outfit7.Audio.AudioEventData m_AudioEventData;

        [SerializeField]
        private float m_Volume = 1f;

        [SerializeField]
        private float m_Pitch = 1f;
    }
}