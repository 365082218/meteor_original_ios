using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ObjectPool : MonoBehaviour
{
    public Object Prefab;
    public int InitNum;//初始化的对象.
    public int PoolNum;//池子内的对象.<包括初始化对象>

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
        if (mInstances.Count > 0)
        {
            Object instance = mInstances.Pop();
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
