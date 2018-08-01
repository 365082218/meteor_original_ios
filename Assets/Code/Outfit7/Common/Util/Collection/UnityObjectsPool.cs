//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;

namespace Outfit7.Util.Collection {

    public enum UnityObjectsPoolResetStrategy {
        /// Reset instance when created/initialised and returned to pool.
        EAGER,
        /// Reset instance before returning from GetObject() method.
        LAZY,
    }

    /// <summary>
    /// A pool of UnityEngine.Object with default eager reset strategy. Objects must be returned to the pool
    /// (when discarded) in order for the pool to have any effect!
    ///
    /// More info: http://www.gamasutra.com/blogs/WendelinReich/20131127/203843/C_Memory_Management_for_Unity_Developers_part_3_of_3.php
    /// </summary>
    public class UnityObjectsPool<T> where T : UnityEngine.Object {

        private T Original;
        private readonly List<T> Instances = new List<T>();
        private int InstanceCounter;
        private UnityObjectsPoolResetStrategy ResetStrategy = UnityObjectsPoolResetStrategy.EAGER;

        /// <summary>
        /// The object reset action.
        /// First value is an instance to be rest, the second value is if this instance is being initialised for the first time or not.
        /// </summary>
        private readonly Action<T, bool> ObjectResetAction;

        /// <summary>
        /// Initializes a pool with default eager reset strategy.
        /// </summary>
        public UnityObjectsPool(T original)
            : this(original, UnityObjectsPoolResetStrategy.EAGER, 0, null) {
        }

        public UnityObjectsPool(T original, int initialCapacity)
            : this(original, UnityObjectsPoolResetStrategy.EAGER, initialCapacity, null) {
        }

        public UnityObjectsPool(T original, UnityObjectsPoolResetStrategy resetStrategy, int initialCapacity)
            : this(original, resetStrategy, initialCapacity, null) {
        }

        /// <summary>
        /// Initializes a pool.
        /// </summary>
        /// <param name="original">Original - to clone new instances from</param>
        /// <param name="initialCapacity">Initial capacity.</param>
        /// <param name="objectResetAction">Object reset action - used to reset cached instance values when retrieved (implementation recommended)</param>
        /// <param name = "resetStrategy">Reset objects at storage or retrieve time</param>
        public UnityObjectsPool(T original, UnityObjectsPoolResetStrategy resetStrategy, int initialCapacity, Action<T, bool> objectResetAction) {
            Original = original;
            ObjectResetAction = objectResetAction;
            ResetStrategy = resetStrategy;

            CreateInstances(original, initialCapacity);
        }

        private T CreateInstance(T original) {
            T instance = UnityEngine.Object.Instantiate(original) as T;
            instance.name = string.Format("{0} ({1})", original.name, InstanceCounter);
            InstanceCounter++;

            if (ResetStrategy == UnityObjectsPoolResetStrategy.EAGER) {
                ResetObject(instance, true);
            }

            return instance;
        }

        private void CreateInstances(T original, int count) {
            for (int i = 0; i < count; i++) {
                Instances.Add(CreateInstance(original));
            }
        }

        public T GetObject() {
            T instance;
            bool initialize = false;

            if (Instances.Count > 0) {
                instance = Instances[Instances.Count - 1];
                Instances.RemoveAt(Instances.Count - 1);
            } else {
                instance = CreateInstance(Original);
                initialize = true;
            }

            if (ResetStrategy == UnityObjectsPoolResetStrategy.LAZY) {
                ResetObject(instance, initialize);
            }

            return instance;
        }

        public void ReturnObject(T instance) {
            Instances.Add(instance);

            if (ResetStrategy == UnityObjectsPoolResetStrategy.EAGER) {
                ResetObject(instance, false);
            }
        }

        public bool ResetObject(T instance) {
            return ResetObject(instance, false);
        }

        public bool ResetObject(T instance, bool initialize) {
            if (ObjectResetAction == null) {
                return false;
            }

            ObjectResetAction(instance, initialize);
            return true;
        }

        public bool ResetAllObjects() {
            return ResetAllObjects(false);
        }

        public bool ResetAllObjects(bool initialize) {
            if (ObjectResetAction == null) {
                return false;
            }

            for (int i = 0; i < Instances.Count; i++) {
                ObjectResetAction(Instances[i], initialize);
            }

            return true;
        }
    }
}
