/*
 * FrameMaterials
 * Create by Fengyueyun
 * Date 2013.01.25
 */

using UnityEngine;
using System.Collections;

public class FrameMaterials : MonoBehaviour 
{
    public float StartTime = 0;
    public int TileX = 1;
	public int TileY=1;
	public int FPS=12;
	public int StartFrame=0;
	public bool Loop=true;
	
	float XTiling,YTiling,XOffset,YOffset;
	float UpdateTime;
	float TimeFlg;
	int index=0;
	Material UseMaterial;
	// Use this for initialization
	void Start () 
	{
		index=StartFrame;
		UpdateTime=1.0f/FPS;
		UseMaterial=GetComponent<Renderer>().material;
		XTiling=1.0f/TileX;
		YTiling=1.0f/TileY;
		TimeFlg=Time.time+StartTime;
		
		UseMaterial.SetTextureScale("_MainTex", new Vector2(XTiling, YTiling));
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(Time.time-TimeFlg>UpdateTime)
		{
			if(index<TileX*TileY-1)
			{
				index++;
			}
			else if(Loop)
			{
				index=0;
			}
			XOffset=index%TileX*XTiling;
			YOffset=1-(index/TileX)*YTiling;
			TimeFlg=Time.time;
			UseMaterial.SetTextureOffset("_MainTex",new Vector2(XOffset,YOffset));
		}
	}
	
	
}
