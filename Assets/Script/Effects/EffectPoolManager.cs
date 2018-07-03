using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EffectPoolManager : MonoBehaviour
{
    Dictionary<string, ObjectPool> mEffectPools = new Dictionary<string, ObjectPool>();
    public GameObject Spawn(int idx)
    {
        GameObject effect = Spawn(name);
        return effect;
    }

    public GameObject Spawn(string name)
    {
        ObjectPool objectPool;
        if (mEffectPools.TryGetValue(name, out objectPool))
            return Spawn(objectPool);

        GameObject prefab = Resources.Load(name) as GameObject;
        if (prefab != null)
        {
            objectPool = gameObject.AddComponent<ObjectPool>();
            objectPool.Prefab = prefab;
        }

        mEffectPools[name] = objectPool;
        return Spawn(objectPool);
    }

    GameObject Spawn(ObjectPool objectPool)
    {
        if (objectPool == null) 
            return null;
        GameObject effect = objectPool.Spawn(true) as GameObject;
        return effect;
    }
}
