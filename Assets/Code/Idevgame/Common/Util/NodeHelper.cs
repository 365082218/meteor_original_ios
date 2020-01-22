using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NodeHelper {
    //必须不区分大小写字母.一些关卡原件大小写未明确.
    public static GameObject Find(string name, GameObject parent)
    {
        if (parent.name == name)
            return parent;
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            GameObject childObj = parent.transform.GetChild(i).gameObject;
            if (name.ToLower() == childObj.name.ToLower())
            {
                return childObj;
            }
            GameObject childchildObj = Find(name, childObj);
            if (childchildObj != null)
                return childchildObj;
        }
        return null;
    }

    public static GameObject Control(string name, GameObject parent)
    {
        Transform[] children = parent.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name == name)
                return child.gameObject;
        }
        return null;
    }
}
