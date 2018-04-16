using UnityEngine;
using UnityEngine.UI;

public class GameTime : MonoBehaviour {

    public Text timeText;
    float fTick = 0.0f;
    //public int count = 0;
    //public float startTick;
    // Use this for initialization
    void Start () {
        timeText = GetComponent<Text>();
        //count = 0;
        //startTick = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
        string format = System.String.Format("{0:F6} Time", fTick);
        timeText.text = format;
        //if (Time.time - startTick <= 1.0f)
        //    count++;
        //float s = Time.deltaTime;

        //Debug.Log(s);
    }
}
