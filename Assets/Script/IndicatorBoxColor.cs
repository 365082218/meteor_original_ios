using UnityEngine;
using System.Collections;

public class IndicatorBoxColor : MonoBehaviour {
    public Material material;

	public float time=3f;

	public float alpha=1;

	void Update()
	{
		if (material == null)
		{
			material = gameObject.GetComponent<Renderer>().material;
			return;
		}
		if(alpha<0)return;
		float step = (1f/time/30f);
		alpha-=step;
		Color c = material.color;
		c.a= alpha;
		material.color = c;
//		Debug.Log(alpha);
	}

//    float minAlpha = 0.1f;
//    float maxAlpha = 0.9f;
//    float varifySpeed = -0.3f;
//    public float curAlpha = 0.5f;
//	// Use this for initialization
//	void Awake () {
//
//	     material = gameObject.renderer.material;
//        if (material == null) print("托盘位置提示box颜色控制脚本无法进行，找不到托盘指示box的Material");
//	}
//	
//
//	void Update () 
//    {
//        if (material == null) return;
//        curAlpha+= Time.deltaTime * varifySpeed;
//        if (curAlpha < minAlpha || curAlpha > maxAlpha) varifySpeed *= -1;
//
//        curAlpha = Mathf.Clamp(curAlpha, minAlpha, maxAlpha);
//        Color c = material.color;
//        c.a = curAlpha;
//        material.color= c;
//	}

}
