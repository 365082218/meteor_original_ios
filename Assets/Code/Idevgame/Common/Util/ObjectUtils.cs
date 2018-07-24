using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Idevgame.Util
{
    public static class ObjectUtils 
    {
        public static GameObject Identity(this GameObject obj, Transform parent)
        {
            obj.transform.SetParent(parent);
            obj.transform.position = Vector3.zero;
            obj.transform.rotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            return obj;
        }
    }
}