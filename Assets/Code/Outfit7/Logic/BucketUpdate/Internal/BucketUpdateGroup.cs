using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outfit7.Util;
using Outfit7.Logic.Internal;

namespace Outfit7.Logic.BucketUpdateInternal {

    public class BucketUpdateBehaviourComparer : IComparer<BucketUpdateBehaviour> {
        public int Compare(BucketUpdateBehaviour a, BucketUpdateBehaviour b) {
            return a.BucketSortHash.CompareTo(b.BucketSortHash);
        }
    }

    public class BucketUpdateIndexComparer : IComparer<int> {
        public int Compare(int a, int b) {
            return a.CompareTo(b);
        }
    }

    public class BucketUpdateGroup {

        private class Bucket {
            public BinarySortList<BucketUpdateBehaviour> Behaviours = new BinarySortList<BucketUpdateBehaviour>(1024);
        }

        private BucketUpdateBehaviourComparer BucketUpdateBehaviourComparer = new BucketUpdateBehaviourComparer();
        private BucketUpdateIndexComparer BucketUpdateIndexComparer = new BucketUpdateIndexComparer();
        private Bucket[] Buckets = null;
        private BinarySortList<int> ActiveBucketIndices = null;
        private int activeBehaviourCount = 0;

        public BucketUpdateGroup(int maxBucketCount) {
            Buckets = new Bucket[maxBucketCount];
            for (int i = 0; i < Buckets.Length; ++i) {
                Buckets[i] = new Bucket();
            }
            ActiveBucketIndices = new BinarySortList<int>(maxBucketCount);
        }

        public int ActiveBehaviourCount {
            get {
                return activeBehaviourCount;
            }
        }

        public void AddToBucket(BucketUpdateBehaviour bucketUpdateBehaviour) {
            int bucketIndex = bucketUpdateBehaviour.BucketIndex;
            Bucket bucket = Buckets[bucketIndex];
            bucket.Behaviours.AddSorted(bucketUpdateBehaviour, BucketUpdateBehaviourComparer);
            ++activeBehaviourCount;
            if (bucket.Behaviours.Count == 1) {
                ActiveBucketIndices.AddSorted(bucketIndex, BucketUpdateIndexComparer);
            }
        }

        public void RemoveFromBucket(BucketUpdateBehaviour bucketUpdateBehaviour) {
            int bucketIndex = bucketUpdateBehaviour.BucketIndex;
            Bucket bucket = Buckets[bucketIndex];
            // In case of queued buckets it can happen that the object is not in the bucket at all when destroying
            if (bucket.Behaviours.RemoveSorted(bucketUpdateBehaviour, BucketUpdateBehaviourComparer)) {
                --activeBehaviourCount;
                if (bucket.Behaviours.Count == 0) {
                    ActiveBucketIndices.RemoveSorted(bucketIndex);
                }
                Assert.IsTrue(activeBehaviourCount >= 0);
            }
        }

        public void PreUpdate(float deltaTime, ManagerSystem managerSystem) {
            // Update buckets
            for (int i = 0; i < ActiveBucketIndices.Count; ++i) {
                int bucketIndex = ActiveBucketIndices[i];
                Bucket bucket = Buckets[bucketIndex];
                // Update bucket behaviours
                for (int j = 0; j < bucket.Behaviours.Count; ++j) {
                    bucket.Behaviours[j].OnPreUpdate(deltaTime);
                }
                // Update managers
                managerSystem.BucketUpdate(bucketIndex);
            }
        }

        public void PostUpdate(float deltaTime) {
            // Update buckets
            for (int i = 0; i < ActiveBucketIndices.Count; ++i) {
                Bucket bucket = Buckets[ActiveBucketIndices[i]];
                // Update bucket behaviours
                for (int j = 0; j < bucket.Behaviours.Count; ++j) {
                    bucket.Behaviours[j].OnPostUpdate(deltaTime);
                }
            }
        }

        public void Clear() {
            activeBehaviourCount = 0;
            for (int i = 0; i < ActiveBucketIndices.Count; ++i) {
                Bucket bucket = Buckets[ActiveBucketIndices[i]];
                bucket.Behaviours.Clear();
            }
            ActiveBucketIndices.Clear();
        }

        public void OnPause() {
            for (int i = 0; i < ActiveBucketIndices.Count; ++i) {
                Bucket bucket = Buckets[ActiveBucketIndices[i]];
                for (int j = 0; j < bucket.Behaviours.Count; ++j) {
                    bucket.Behaviours[j].OnPause();
                }
            }
        }

        public void OnResume() {
            for (int i = 0; i < ActiveBucketIndices.Count; ++i) {
                Bucket bucket = Buckets[ActiveBucketIndices[i]];
                for (int j = 0; j < bucket.Behaviours.Count; ++j) {
                    bucket.Behaviours[j].OnResume();
                }
            }
        }
    }

}