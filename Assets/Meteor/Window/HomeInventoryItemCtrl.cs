using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class HomeInventoryItemCtrl : MonoBehaviour {
    public Text ItemName;
    public Text ItemCount;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetItem(string name, uint cnt)
    {
        ItemName.text = name;
        ItemCount.text = cnt.ToString();
    }

    public void SetCount(uint cnt)
    {
        ItemCount.text = cnt.ToString();
    }

    public void SetName(string name)
    {
        ItemName.text = name;
    }
}
