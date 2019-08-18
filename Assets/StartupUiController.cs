using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartupUiController : MonoBehaviour {
    GameObject mGoProgress;
    Text mLabel;
    private static int mCount = 0;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Progress(string Progress)
    {
        if (string.IsNullOrEmpty(Progress))
        {
            if (mGoProgress.activeSelf)
                mGoProgress.SetActive(false);
        }

        if (!mGoProgress.activeSelf)
            mGoProgress.SetActive(true);

        mLabel.text = Progress;
    }

    private void Awake()
    {
        mGoProgress.SetActive(false);
        mCount++;
    }
}
