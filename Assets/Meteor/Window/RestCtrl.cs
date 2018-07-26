using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;
public class RestCtrl : MonoBehaviour {
    public Image background;
	// Use this for initialization
	void Start () {
        GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
        GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
        Sequence mySequence = DOTween.Sequence();
        Tweener tw = background.DOColor(Color.black, 1.0f);
        Tweener tw2 = background.DOColor(new Color(0, 0, 0, 0), 1.0f);
        mySequence.Append(tw);
        mySequence.AppendInterval(1.0f);
        mySequence.Append(tw2);
        mySequence.OnComplete(() => { OnClose(); });

	}
	
	// Update is called once per frame
	void Update () {
	    
	}

    void OnClose()
    {
        U3D.TextAppend("\n睡了一觉，状态全满了");
        WsWindow.Close(WsWindow.RestCtrl);
    }
}
