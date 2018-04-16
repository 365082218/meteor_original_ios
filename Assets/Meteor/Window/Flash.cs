using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum FlashType
{
    YShake,
    ColorCycle,
}

public class Flash : MonoBehaviour {
    public FlashType type;
    public float yDiff;
    public float fDelay;

    float count;
	// Use this for initialization
	void Start () {
        if (!played)
        {
            count = fDelay;
            gameObject.SetActive(false);
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (played)
        {
            count -= Time.deltaTime;
            if (count <= 0.0f)
                Shake();
        }
	}

    void Shake()
    {
        yDiff = -yDiff;
        transform.Translate(0.0f, yDiff, 0.0f);
        count = fDelay;
    }

    bool played = false;
    public void Play()
    {
        played = true;
        gameObject.SetActive(true);
    }

    public void Stop()
    {
        played = false;
        if (yDiff <= 0)
            Shake();
        gameObject.SetActive(false);
    }
}
