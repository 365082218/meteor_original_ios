using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EffectPoolManager : MonoBehaviour
{
    static EffectPoolManager msInstance;
    public static EffectPoolManager Instance 
    {
        get 
        {
            if (msInstance == null)
            {
                GameObject effectPool = new GameObject("EffectPool");
                msInstance = effectPool.AddComponent<EffectPoolManager>();
            }
            return msInstance;
        }
    }

    Dictionary<string, ObjectPool> mEffectPools = new Dictionary<string, ObjectPool>();

    void Start()
    {
        msInstance = this;
    }

    void OnDestroy()
    {
        msInstance = null;
    }

    public GameObject Spawn(string name, Vector3 position, Quaternion rotation)
    {
        GameObject effect = Spawn(name);
        if (effect)
        {
            effect.transform.position = position;
            effect.transform.rotation = rotation;
        }
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
        GameObject effect = objectPool.Spawn(true);
        effect.GetComponent<EffectTime>().Pool = objectPool;
        return effect;
    }
}
