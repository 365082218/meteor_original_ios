using UnityEngine;
using System.Collections;

public class BossDeadDissolve01 : MonoBehaviour {
	public Shader dissolveShader;
	public Texture2D dissolvePattern;
	public Color dissolveEmissionColor;
	public float dissolveSpeed = 0.1f;
	float sliceAmount;
	bool dissolve = false;

    //拥有者
    MeteorUnit mOwner;

    Shader InitShader;
		
    //bool mouseOver;

    bool bDoDissolve = false;

    void Awake()
    {
        InitShader = transform.GetComponent<Renderer>().material.shader;
        transform.GetComponent<Renderer>().material.shader = InitShader;
        sliceAmount = 0;
    }

    void Start()
    {
        sliceAmount = 0;
        transform.GetComponent<Renderer>().material.shader = InitShader;
    }

    void OnEnable()
    {
        sliceAmount = 0;
        transform.GetComponent<Renderer>().material.shader = InitShader;
    }


	void Update () {		
        //if(mouseOver){
        //    if(Input.GetMouseButtonUp(0)){
                if (bDoDissolve)
                {
                    bDoDissolve = false;
                    transform.GetComponent<Renderer>().material.shader = dissolveShader;
                    transform.GetComponent<Renderer>().material.SetColor("_DissolveEmissionColor", dissolveEmissionColor);
                    transform.GetComponent<Renderer>().material.SetFloat("_DissolveEmissionThickness", -0.05f);
                    transform.GetComponent<Renderer>().material.SetTexture("_DissolveTex", dissolvePattern);
                    transform.GetComponent<Renderer>().material.SetColor("_ColorX", new Color(0.73f, 0.73f, 0.73f, 1));
                    transform.GetComponent<Renderer>().material.SetFloat("_Cutoff", 0.01f);
                    transform.GetComponent<Renderer>().material.SetTextureOffset("_DissolveTex", new Vector2(Random.Range(1.0f, 10.0f), Random.Range(1.0f, 10.0f)));
                    dissolve = true;
                    float dp = transform.GetComponent<Renderer>().material.GetFloat("_DissolvePower");
                }
        //    }
        //}
		
		if(dissolve){
			sliceAmount -= Time.deltaTime * dissolveSpeed;
			transform.GetComponent<Renderer>().material.SetFloat("_DissolvePower", 0.65f + Mathf.Sin(0.9f)*sliceAmount);

            if (GetComponent<Renderer>().material.GetFloat("_DissolvePower") < 0.2f)
            {
                dissolve = false;
                //transform.renderer.material.shader = InitShader;
            }
		}
	}

    public void StartDissolve(float time, MeteorUnit owner){
        Invoke("DoDissolve", time);
        mOwner = owner;
    }

    void DoDissolve()
    {
        bDoDissolve = true;
    }
	
    //void OnMouseEnter(){
    //    mouseOver = true;
    //}
    //void OnMouseExit(){
    //    mouseOver = false;
    //}
}
