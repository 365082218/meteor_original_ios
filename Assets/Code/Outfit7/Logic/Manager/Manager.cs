using UnityEngine;

namespace Outfit7.Logic {

    public abstract class Manager<T> : MonoBehaviour, IManager where T : MonoBehaviour {

        public static T Instance { get; private set; }

        public void SetInstance(MonoBehaviour manager) {
            Instance = manager as T;
        }

        public virtual void OnCreate() {
        }

        public virtual bool OnInitialize() {
            return true;
        }

        public virtual void OnTerminate() {
        }

        public virtual void OnPreUpdate(float deltaTime) {
        }

        public virtual void OnBucketUpdate(int index, float deltaTime) {
        }

        public virtual void OnPostUpdate(float deltaTime) {
        }

        public virtual void OnPause() {
        }

        public virtual void OnResume() {
        }

    }

}