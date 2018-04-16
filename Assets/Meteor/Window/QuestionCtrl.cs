using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class QuestionCtrl : MonoBehaviour {
    public Text Name;
    public Text Description;
    public Text BattleText;
    public Button OnYes;
    public Button OnCancel;
    public static QuestionCtrl Ins;
    void Awake()
    {
        Ins = this;
    }

    void OnDestory()
    {
        Ins = null;
    }
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
