using UnityEngine;
using System.Collections;

public class EffectDestroyThisTimed : MonoBehaviour {

    public float destroyTime = 5;

    //特效是否可叠加显示,false加入新特效把原来同名的特效去除
    public bool CanRepeat = false;

	// Use this for initialization
	void Start () {

        if (!CanRepeat && this.transform.parent!=null)
        {
            string effect_name = this.name;
            for (int i = 0; i < this.transform.parent.transform.childCount; i++)
            {
                GameObject childObj = this.transform.parent.transform.GetChild(i).gameObject;
                if (effect_name == childObj.name && childObj != this.gameObject)
                {
                    DestroyImmediate(childObj);
                }
            }
        }

        Destroy(gameObject, destroyTime);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
