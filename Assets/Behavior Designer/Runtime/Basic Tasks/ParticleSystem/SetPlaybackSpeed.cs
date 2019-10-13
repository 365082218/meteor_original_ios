using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Basic.UnityParticleSystem
{
    [TaskCategory("Basic/ParticleSystem")]
    [TaskDescription("Sets the playback speed of the Particle System.")]
    public class SetPlaybackSpeed : Action
    {
        [Tooltip("The GameObject that the task operates on. If null the task GameObject is used.")]
        public SharedGameObject targetGameObject;
        [Tooltip("The playback speed of the ParticleSystem")]
        public SharedFloat playbackSpeed = 1;

        private ParticleSystem particleSystem;
        private GameObject prevGameObject;

        public override void OnStart()
        {
            var currentGameObject = GetDefaultGameObject(targetGameObject.Value);
            if (currentGameObject != prevGameObject) {
                particleSystem = currentGameObject.GetComponent<ParticleSystem>();
                prevGameObject = currentGameObject;
            }
        }

        public override TaskStatus OnUpdate()
        {
            if (particleSystem == null) {
                Debug.LogWarning("ParticleSystem is null");
                return TaskStatus.Failure;
            }

#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4
            particleSystem.playbackSpeed = playbackSpeed.Value;
#else
            ParticleSystem.MainModule mainParticleSystem = particleSystem.main;
            mainParticleSystem.simulationSpeed= playbackSpeed.Value;
#endif

            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            targetGameObject = null;
            playbackSpeed = 1;
        }
    }
}