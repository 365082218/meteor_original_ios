//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Outfit7.Util.Collection {

    public class PrefabsPool {

#region Class variables

        /// <summary>
        /// Enable the instance on instantiate/when retrieving from pool.
        /// </summary>
        public bool EnableInstanceOnInstantiate = true;
        /// <summary>
        /// Disable the instance when adding it pool.
        /// </summary>
        public bool DisableInstanceWhenAddedToPool = true;
        /// <summary>
        /// Disable the initial prefab when creating a prefab pool.
        /// </summary>
        public bool DisablePrefabWhenAddedToPool = false;

        private readonly Dictionary<GameObject, UnityObjectsPool<GameObject>> PrefabToPool = new Dictionary<GameObject, UnityObjectsPool<GameObject>>();
        // seperate dictionary so that this class can have a reset method implemented (if ever needed)
        private readonly Dictionary<GameObject, UnityObjectsPool<GameObject>> InstanceToPool = new Dictionary<GameObject, UnityObjectsPool<GameObject>>();

#endregion

#region Properties

        /// Attach instantiated/retrieved objects to this GameObject if set (for convenience).
        public GameObject InstantiateToObject { get; set; }

        public bool InstantiateToRoot { get; private set; }

        /// Attach returned/stored objects to this GameObject if set. This way all pooled instances can be stored under
        /// one GO so that the sceene (root) is not littered with disabled GO.
        public GameObject ReturnToObject { get; set; }

        /// <summary>
        /// The default object reset action.
        ///
        /// Action<gameObject, prefab, initialize>
        /// </summary>
        public Action<GameObject, GameObject, bool> DefaultObjectResetAction { get ; private set ; }

        /// <summary>
        /// The extention to the default object reset action.
        ///
        /// Action<gameObject, prefab, initialize>
        /// </summary>
        public Action<GameObject, GameObject, bool> ExtendDefaultObjectResetAction { get; set ; }

#endregion

#region Constructors

        public PrefabsPool()
            : this(null, null) {
        }

        public PrefabsPool(bool instatiateToRoot)
            : this(null, null) {
            InstantiateToRoot = instatiateToRoot;
        }

        public PrefabsPool(bool instatiateToRoot, GameObject returnToObject)
            : this(null, returnToObject) {
            InstantiateToRoot = instatiateToRoot;
        }

        public PrefabsPool(GameObject instantiateToObject, GameObject returnToObject) {
            InstantiateToObject = instantiateToObject;
            InstantiateToRoot = false;
            ReturnToObject = returnToObject;

            DefaultObjectResetAction = (go, prefab, init) => {
                // always deactivate object since this reset is called at object storage time and not retrieve time
                if (DisableInstanceWhenAddedToPool) {
                    go.SetActive(false);
                }

                if (init && ReturnToObject != null) {
                    go.transform.parent = ReturnToObject.transform;
                }

                go.transform.localPosition = prefab.transform.localPosition;
                go.transform.localScale = prefab.transform.localScale;
                go.transform.localRotation = prefab.transform.localRotation;

                if (go.GetComponent<Rigidbody>() != null && !go.GetComponent<Rigidbody>().isKinematic) {
                    go.GetComponent<Rigidbody>().velocity = prefab.GetComponent<Rigidbody>().velocity;
                    go.GetComponent<Rigidbody>().angularVelocity = prefab.GetComponent<Rigidbody>().angularVelocity;
                }

                if (ExtendDefaultObjectResetAction != null) {
                    ExtendDefaultObjectResetAction(go, prefab, init);
                }
            };
        }

#endregion

        public bool AddPrefab(GameObject prefab) {
            return AddPrefab(prefab, 0);
        }

        public bool AddPrefab(GameObject prefab, int initialCapacity) {
            return AddPrefab(prefab, initialCapacity, null);
        }

        public bool AddPrefab(GameObject prefab, int initialCapacity, Action<GameObject, bool> objectResetAction) {
            if (PrefabToPool.ContainsKey(prefab)) {
                throw new NotSupportedException("Adding an already added prefab to pool: " + prefab);
            }

            if (DisablePrefabWhenAddedToPool) {
                prefab.SetActive(false);
            }

            // default ObjectResetAction if none defined
            if (objectResetAction == null) {
                objectResetAction = (go, init) => {
                    DefaultObjectResetAction(go, prefab, init);
                };
            }

            UnityObjectsPool<GameObject> pool = new UnityObjectsPool<GameObject>(prefab, UnityObjectsPoolResetStrategy.EAGER, initialCapacity, objectResetAction);
            PrefabToPool.Add(prefab, pool);
            return true;
        }

        public bool RemovePrefab(GameObject prefab) {
            return PrefabToPool.Remove(prefab);
        }

        public GameObject GetInstanceForPrefab(GameObject prefab) {
            UnityObjectsPool<GameObject> pool;
            PrefabToPool.TryGetValue(prefab, out pool);

            if (pool != null) {
                GameObject instance = pool.GetObject();
                if (InstantiateToObject != null) {
                    instance.transform.parent = InstantiateToObject.transform;
                } else if (InstantiateToRoot) {
                    instance.transform.parent = null;
                }

                InstanceToPool.Add(instance, pool);

                if (EnableInstanceOnInstantiate) {
                    instance.gameObject.SetActive(true);
                }
                return instance;
            }

            throw new KeyNotFoundException("No pool for prefab: " + prefab);
        }

        public void ReturnInstanceToPool(GameObject instance) {
            ReturnInstanceToPool(instance, true);
        }

        private void ReturnInstanceToPool(GameObject instance, bool removeInstanceFromPool) {
            UnityObjectsPool<GameObject> pool;
            if (!InstanceToPool.TryGetValue(instance, out pool)) {
                throw new KeyNotFoundException("No pool found for instance: " + instance);
            }

            // don't remove instance immediately if this is being done through ReturnAllInstances() - it'll be removed later
            if (removeInstanceFromPool) {
                InstanceToPool.Remove(instance);
            }

            if (DisableInstanceWhenAddedToPool) {
                instance.SetActive(false);
            }

            pool.ReturnObject(instance);
            if (ReturnToObject != null) {
                instance.transform.parent = ReturnToObject.transform;
            }
        }

        public void ReturnAllInstances() {
            foreach (var instance in InstanceToPool.Keys) {
                ReturnInstanceToPool(instance, false);
            }

            InstanceToPool.Clear();
        }
    }
}
