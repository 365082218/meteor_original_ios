using UnityEngine;

namespace Outfit7.Logic.Input {

    public struct TouchWrapper {

        public Vector3 Position;
        public float DeltaTime;
        public int FingerId;
        public TouchPhase Phase;

        public TouchWrapper(Touch touch) {
            Position = touch.position;
            DeltaTime = touch.deltaTime;
            Phase = touch.phase;
            FingerId = touch.fingerId;
        }

        public TouchWrapper(Vector3 position, int fingerId) {
            Position = position;
            Phase = TouchPhase.Began;
            FingerId = fingerId;
            DeltaTime = 0;
        }

        public override string ToString() {
            return string.Format("[TouchWrapper: DeltaTime={0}, FingerId={1}, Phase={2}, Position={3}]", DeltaTime, FingerId, Phase, Position);
        }
    }
}