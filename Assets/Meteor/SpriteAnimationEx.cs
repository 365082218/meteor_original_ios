using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class SpriteAnimationEx : MonoBehaviour {
    public Texture[] tex;
    Image target;
    int index;
    float delay;
	// Use this for initialization
	void Start () {
        target = GetComponent<Image>();
        if (target == null)
            DestroyImmediate(this);
        delay = 1.0f / (float)tex.Length;
        StartCoroutine(Play());
    }
	
	// Update is called once per frame
	void Update () {
        
    }

    IEnumerator Play()
    {
        while (true)
        {
            index++;
            index %= tex.Length;
            target.material.SetTexture("_MainTex", tex[index]);
            target.SetMaterialDirty();
            yield return new WaitForSeconds(delay);
        }
    }
}
