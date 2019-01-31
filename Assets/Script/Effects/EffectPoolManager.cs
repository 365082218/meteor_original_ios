using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//�����
public class EffectPoolManager : MonoBehaviour
{
    public static EffectPoolManager Instance;
    void Awake()
    {
        Instance = this;
    }
    Dictionary<string, ObjectPool> mEffectPools = new Dictionary<string, ObjectPool>();
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

    ////�ö����Ƿ��ɸ��óع���.
    //bool CanResycle(string poolType, GameObject obj)
    //{

    //}
}
