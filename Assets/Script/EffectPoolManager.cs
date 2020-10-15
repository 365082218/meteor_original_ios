using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//对象池-游戏原因，不好使用池
//特效，每个参数都不一样，都是从文件读取数据恢复
//飞轮/飞镖，受帧同步影响，不好回收再使用，每个的模型也是动态加载
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

    ////该对象是否由复用池管理.
    //bool CanResycle(string poolType, GameObject obj)
    //{

    //}
}
