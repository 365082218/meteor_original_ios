using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;

public class ldaUIGrid1 : MonoBehaviour {

    public GameObject ldaUIGridPanel1;
    //public List<string> GridItems;
    public GameObject ldaUIGridItem1;
    public GameObject VerticalScrollBar;

    //public delegate void VoidDelegate(GameObject go);
    //public VoidDelegate onClick;

    public bool AddItem(string itemname, string itemtext, UIEventListener.VoidDelegate onClick)
    {

        //克隆预设
        GameObject obj = (GameObject)Instantiate(ldaUIGridItem1) as GameObject;
        obj.name = itemname;

        //UIButton button = obj.GetComponentInChildren<UIButton>();
        //得到文字对象
        UILabel label = obj.GetComponentInChildren<UILabel>();
		//修改文字内容
		label.text = itemtext;
        label.name = itemname;

        //将新预设放在Panel对象下面
        obj.transform.parent = ldaUIGridPanel1.transform;

        //这个不能去除
        //GameObject item = GameObject.Find(obj.name);
        obj.transform.localPosition = new Vector3(0, 0, 0);
        obj.transform.localScale = new Vector3(1, 1, 1);

        UIEventListener.Get(label.gameObject).onClick = onClick;
            
        return true;
    }

    public void repositionNow()
    {
        ldaUIGridPanel1.GetComponent<UIGrid>().repositionNow = true;

        VerticalScrollBar.GetComponent<UIScrollBar>().scrollValue = 1;
    }

    public bool DeleteItem(string itemname)
    {
        return true;
    }

    public bool DeleteAllItems()
    {
        //销毁英雄列表
        for (int k = 0; k < ldaUIGridPanel1.transform.childCount; k++)
        {
            GameObject go = ldaUIGridPanel1.transform.GetChild(k).gameObject;
            DestroyImmediate(go);
            // 这个标记会让元素立即重新排列。
            //ldaUIGridPanel1.GetComponent<UIGrid>().Reposition();
        }

        repositionNow();
        return true;
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
