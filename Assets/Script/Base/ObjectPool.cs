using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    public GameObject Prefab;
    public int InitNum;
    public int PoolNum;

    Stack<GameObject> mInstances = new Stack<GameObject>();

    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < InitNum; i++)
        {
            GameObject instance = GameObject.Instantiate(Prefab) as GameObject;
            instance.SetActive(false);
            mInstances.Push(instance);
            PoolNum++;
        }
    }

    public GameObject Spawn(bool create)
    {
        //add by Lindean
        //if (create)
        //    return GameObject.Instantiate(Prefab) as GameObject;

        if (mInstances.Count > 0)
        {
            GameObject instance = mInstances.Pop();
            instance.SetActive(true);
            PoolNum--;
            return instance;
        }

        //GOD
        if (create)
            return GameObject.Instantiate(Prefab) as GameObject;

        return null;
    }

    public void Recycle(GameObject instance)
    {
        instance.SetActive(false);
        mInstances.Push(instance);
        PoolNum++;
    }
}
