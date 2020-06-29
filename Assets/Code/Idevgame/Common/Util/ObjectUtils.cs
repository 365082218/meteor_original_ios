using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Idevgame.Util
{
    //关卡
    public static class LevelUtils
    {
        public static void AddGridItem(LevelDatas.LevelDatas lev, Transform parent, System.Action<LevelDatas.LevelDatas> OnSelect)
        {
            GameObject objPrefab = Resources.Load("LevelSelectItem", typeof(GameObject)) as GameObject;
            GameObject obj = GameObject.Instantiate(objPrefab) as GameObject;
            obj.transform.SetParent(parent);
            obj.name = lev.Name;
            obj.GetComponent<Button>().onClick.AddListener(() => { if (OnSelect != null) OnSelect(lev); });
            obj.GetComponentInChildren<Text>().text = lev.Name;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
        }
    }

    public static class ObjectUtils 
    {
        public static GameObject Identity(this GameObject obj, Transform parent = null)
        {
            obj.transform.SetParent(parent);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
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