using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
public class UnitTopUITest : MonoBehaviour {
    public Text MonsterName;
    // Use this for initialization
    private void Awake()
    {
    }

    void Start () {
	    
	}
	
	// Update is called once per frame
	void LateUpdate () {
    }

    public void Init(string name, Vector3 position)
    {
        Transform mainCanvas = GameObject.Find("Canvas").transform;
        transform.SetParent(mainCanvas);
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;
        transform.SetAsLastSibling();
        MonsterName.text = name;
     }
}
