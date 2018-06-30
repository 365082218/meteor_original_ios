using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    public Object Prefab;
    public int InitNum;
    public int PoolNum;

    Stack<Object> mInstances = new Stack<Object>();

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

    public Object Spawn(bool create)
    {
        //add by Lindean
        //if (create)
        //    return GameObject.Instantiate(Prefab) as GameObject;

        if (mInstances.Count > 0)
        {
            Object instance = mInstances.Pop();
            //instance.SetActive(true);
            PoolNum--;
            return instance;
        }

        //GOD
        if (create)
            return GameObject.Instantiate(Prefab);

        return null;
    }

    public void Recycle(Object instance)
    {
        mInstances.Push(instance);
        PoolNum++;
    }
}
