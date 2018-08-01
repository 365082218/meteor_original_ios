using System;
using Outfit7.Graphics;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Outfit7.UI.Tween {
    public abstract class BaseTween : UIBehaviour {

        public enum Type {
            Forward,
            Backward,
        }

        [SerializeField]
        private float Duration = 1;

        [SerializeField]
        private Type AnimationType;

        private float StartTime;
        private bool Animate;

        public Action OnFinish;
	
        // Update is called once per frame
        void Update() {
            UpdateAnimation();
        }

        public virtual void StartAnimation(Type type) {
            Animate = true;
            AnimationType = type;
            StartTime = Time.time;
            UpdateAnimation();
        }

        private void UpdateAnimation() {
            if (!Animate) {
                return;
            }

            float progress = (Time.time - StartTime) / Duration;

            if (progress > 1) {
                progress = 1;
                Animate = false;
                if (OnFinish != null) {
                    OnFinish();
                }
            }

            switch (AnimationType) {
                case Type.Forward:
                    progress = Interpolation.Acceleration(0, 1, progress);
                    break;
                case Type.Backward:
                    progress = Interpolation.Deceleration(0, 1, 1 - progress);
                    break;
            }

            UpdateProgress(progress);
        }

        public abstract void UpdateProgress(float progress);
    }
}