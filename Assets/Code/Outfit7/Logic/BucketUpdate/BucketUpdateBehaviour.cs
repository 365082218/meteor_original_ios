using UnityEngine;
using Outfit7.Util;
using System;

namespace Outfit7.Logic {

    public abstract class BucketUpdateBehaviour : MonoBehaviour {

        private const int DefaultBucketPriorityIndex = 1000000;

        [HideInInspector, SerializeField] private int bucketIndex = 0;
        [HideInInspector, SerializeField] BucketUpdateBehaviour updateAfterBehaviour;
        private int bucketPriorityIndex = -1;
        private bool inBucketPriority = false;
        private long bucketSortHash = 0;

        public int BucketIndex {
            get {
                return bucketIndex;
            }
            set {
                Assert.IsTrue(!Application.isPlaying || enabled == false);
                bucketIndex = value;
            }
        }

        public long BucketSortHash {
            get {
                return bucketSortHash;
            }
        }

        private int CreateBucketPriorityIndex() {
            if (bucketPriorityIndex != -1) {
                return bucketPriorityIndex;
            }
            // Lock!
            if (inBucketPriority) {
                throw new Exception(string.Format("Bucket priority index cyclic dependency found {0}!", name));
            }
            inBucketPriority = true;
            if (updateAfterBehaviour != null) {
                bucketPriorityIndex = updateAfterBehaviour.CreateBucketPriorityIndex() + 1;
                bucketIndex = updateAfterBehaviour.bucketIndex;
            } else {
                bucketPriorityIndex = DefaultBucketPriorityIndex;
            }
            inBucketPriority = false;
            return bucketPriorityIndex;
        }

        private void InternalOnAwake() {
            CreateBucketPriorityIndex();
        }

        private void InternalOnEnable() {
            // Compute hash
            bucketSortHash = (long) GetHashCode() + ((long) bucketPriorityIndex << 32);
            // Add to bucket update sysytem
#if UNITY_EDITOR
            if (BucketUpdateSystem.Instance != null)
#endif
                BucketUpdateSystem.Instance.AddBehaviour(this);
        }

        private void InternalOnDisable() {
            // Remove from bucket update system
#if UNITY_EDITOR
            if (BucketUpdateSystem.Instance != null)
#endif
                BucketUpdateSystem.Instance.RemoveBehaviour(this);
        }

        // MonoBehaviour
        protected virtual void Awake() {
            InternalOnAwake();
        }

        protected virtual void OnEnable() {
            InternalOnEnable();
        }

        protected virtual void OnDisable() {
            InternalOnDisable();
        }

        // Public methods
        public virtual void OnPreUpdate(float deltaTime) {
        }

        public virtual void OnPostUpdate(float deltaTime) {
        }

        public virtual void OnPause() {
        }

        public virtual void OnResume() {
        }
    }

}