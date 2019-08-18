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

    public class Pair<F, S>
    {

        public F First { get; set; }

        public S Second { get; set; }

        public Pair()
        {
        }

        public Pair(F first, S second)
        {
            this.First = first;
            this.Second = second;
        }

        public override string ToString()
        {
            return string.Format("[Pair: First={0}, Second={1}]", First, Second);
        }
    }
}