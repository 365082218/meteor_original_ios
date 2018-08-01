using System.Collections.Generic;
using UnityEngine;
using Outfit7.Util;
using Outfit7.Logic.BucketUpdateInternal;

namespace Outfit7.Logic {

    [RequireComponent(typeof(ManagerSystem))]
    public class BucketUpdateSystem : MonoBehaviour {

        public const int MaxBucketCount = 32;

        public static BucketUpdateSystem Instance = null;

        private BucketUpdateGroup MainBucketGroup = null;
        private BucketUpdateGroup ActiveQueuedBucketGroup = null;
        private BucketUpdateGroup[] QueuedBucketGroups = new BucketUpdateGroup[2];
        private Queue<BucketQueueElement> BucketQueue = new Queue<BucketQueueElement>();

        public ManagerSystem ManagerSystem;

        internal void Initialize() {
            Instance = this;
            MainBucketGroup = new BucketUpdateGroup(MaxBucketCount);
            QueuedBucketGroups[0] = new BucketUpdateGroup(MaxBucketCount);
            QueuedBucketGroups[1] = new BucketUpdateGroup(MaxBucketCount);
            ActiveQueuedBucketGroup = QueuedBucketGroups[0];
        }

        private void UpdateQueueLoop(float deltaTime) {
            // Update queue
            while (true) {
                if (!UpdateQueue(deltaTime)) {
                    break;
                }
            }
        }

        private bool UpdateQueue(float deltaTime) {
            if (BucketQueue.Count == 0) {
                return false;
            }
            // Update queue
            while (BucketQueue.Count > 0) {
                BucketQueueElement bucketQueueElement = BucketQueue.Dequeue();
                switch (bucketQueueElement.BucketUpdateQueueType) {
                    case BucketUpdateQueueType.Add:
                        MainBucketGroup.AddToBucket(bucketQueueElement.BucketUpdateBehaviour);
                        break;
                    case BucketUpdateQueueType.Remove:
                        MainBucketGroup.RemoveFromBucket(bucketQueueElement.BucketUpdateBehaviour);
                        break;
                }
            }
            // Remember current group
            BucketUpdateGroup bucketGroup = ActiveQueuedBucketGroup;
            // Check if there's anything to update
            if (bucketGroup.ActiveBehaviourCount > 0) {
                // And switch to the next one if we start instantiating/enabling stuff again
                ActiveQueuedBucketGroup = ActiveQueuedBucketGroup == QueuedBucketGroups[0] ? QueuedBucketGroups[1] : QueuedBucketGroups[0];
                bucketGroup.PreUpdate(deltaTime, ManagerSystem);
                bucketGroup.Clear();
            }
            return BucketQueue.Count > 0;
        }

        // Updated by ManagerSystem
        internal void PreUpdateSystem(float deltaTime) {
            if (MainBucketGroup == null) {
                return;
            }
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
#endif
            MainBucketGroup.PreUpdate(deltaTime, ManagerSystem);
            UpdateQueueLoop(deltaTime);
        }

        internal void PostUpdateSystem(float deltaTime) {
            if (MainBucketGroup == null) {
                return;
            }
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
#endif
            MainBucketGroup.PostUpdate(deltaTime);
        }

        public void OnPauseSystem(float deltaTime) {
            MainBucketGroup.OnPause();
            UpdateQueueLoop(deltaTime);
        }

        public void OnResumeSystem(float deltaTime) {
            MainBucketGroup.OnResume();
            UpdateQueueLoop(deltaTime);
        }


        // Behaviour
        public void AddBehaviour(BucketUpdateBehaviour bucketUpdateBehaviour) {
            if (bucketUpdateBehaviour.BucketIndex == -1) {
                return;
            }
            Assert.IsTrue(ManagerSystem.ActiveExecuteOrderType != ManagerSystem.ExecuteOrderType.PostUpdate);
            BucketUpdateGroup bucketGroup = null;
            if (ManagerSystem.ActiveExecuteOrderType != ManagerSystem.ExecuteOrderType.Last) {
                BucketQueue.Enqueue(new BucketQueueElement(BucketUpdateQueueType.Add, bucketUpdateBehaviour));
                bucketGroup = ActiveQueuedBucketGroup;
            } else {
                bucketGroup = MainBucketGroup;
            }
            bucketGroup.AddToBucket(bucketUpdateBehaviour);
        }

        public void RemoveBehaviour(BucketUpdateBehaviour bucketUpdateBehaviour) {
            if (bucketUpdateBehaviour.BucketIndex == -1) {
                return;
            }
            Assert.IsTrue(ManagerSystem.ActiveExecuteOrderType != ManagerSystem.ExecuteOrderType.PostUpdate);
            BucketUpdateGroup bucketGroup = null;
            if (ManagerSystem.ActiveExecuteOrderType != ManagerSystem.ExecuteOrderType.Last) {
                BucketQueue.Enqueue(new BucketQueueElement(BucketUpdateQueueType.Remove, bucketUpdateBehaviour));
                bucketGroup = ActiveQueuedBucketGroup;
            } else {
                bucketGroup = MainBucketGroup;
            }
            bucketGroup.RemoveFromBucket(bucketUpdateBehaviour);
        }
    }
}