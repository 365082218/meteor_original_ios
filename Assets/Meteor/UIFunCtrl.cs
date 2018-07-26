using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIFunCtrl : MonoBehaviour {
    public Image NewButton;
    public Button btn;
    private void Awake()
    {
        btn = GetComponent<Button>();
    }
    // Use this for initialization
    void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetEvent(System.Action<int> act, int param)
    {
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(()=>{ act(param); });
    }
    public void SetText(string text)
    {
        btn.GetComponentInChildren<Text>().text = text;
    }
    public void SetClicked(bool act)
    {
        NewButton.gameObject.SetActive(!act);
    }
}
