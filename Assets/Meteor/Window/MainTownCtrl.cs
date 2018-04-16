using UnityEngine;
using System.Collections;
using DG.Tweening;

public class MainTownCtrl : MonoBehaviour {

    public static MainTownCtrl Ins;
    void Awake() { Ins = this; }
    void OnDestroy() { Ins = null; }
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Open()
    {
        transform.SetAsLastSibling();
        transform.DOLocalMoveX(0, 0.5f);
    }

    public void Close()
    {
        transform.DOLocalMoveX(1080.0f, 0.5f);
    }

    public void ChangeLang()
    {
        
    }
}
