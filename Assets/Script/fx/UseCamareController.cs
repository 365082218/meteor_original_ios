using UnityEngine;
using System.Collections;

public class UseCamareController : MonoBehaviour 
{
	public float StartDistance=3;
	
	Transform Parent1,Parent2,ThisCamera;
	float BaseX,BaseY,BasePosZ;
	float UpDown;
	Vector3 BasePos,BaseMousePos,BaseMousePos2,MousePosLast,MousePosNow;
	// Use this for initialization
	void Start () 
	{
		ThisCamera=Camera.main.transform;
			
		Parent1=GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
		Parent2=GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
		Parent1.position=Vector3.zero;
		Parent2.position=Vector3.zero;
		Parent1.GetComponent<Renderer>().enabled=false;
		Parent2.GetComponent<Renderer>().enabled=false;
		Parent1.GetComponent<Collider>().enabled=false;
		Parent2.GetComponent<Collider>().enabled=false;
		
		Parent2.parent=Parent1;
		ThisCamera.parent=Parent2;
		
		Parent2.localEulerAngles=new Vector3(45,0,0);
		ThisCamera.localPosition=new Vector3(0,0,-StartDistance);
		ThisCamera.localEulerAngles=Vector3.zero;
		
		MousePosLast=Input.mousePosition;
	}
	
	// Update is called once per frame
	void Update () 
	{
		MousePosNow=Input.mousePosition;
		//鏃嬭浆zz
		if(Input.GetMouseButtonDown(0))
		{
			BaseY=Parent1.localEulerAngles.y;
			BaseX=Parent2.localEulerAngles.x;
			BaseMousePos=Input.mousePosition;
		}
		if(Input.GetMouseButton(0))
		{
			Parent1.localEulerAngles=new Vector3(0,Input.mousePosition.x-BaseMousePos.x+BaseY,0);
			Parent2.localEulerAngles=new Vector3(BaseMousePos.y-Input.mousePosition.y+BaseX,0,0);
		}
		
		//鎺ㄦ媺zz
		if(Input.GetMouseButtonDown(1))
		{
			BaseMousePos2=Input.mousePosition;
			BasePosZ=ThisCamera.localPosition.z;
		}
		if(Input.GetMouseButton(1))
		{
			ThisCamera.localPosition=new Vector3 (0,0,-(Input.mousePosition.y-BaseMousePos2.y)/100+BasePosZ);
		}
		
		
		///
		if(Input.GetMouseButtonDown(2))
		{
		}
		if(Input.GetMouseButton(2))
		{
			Parent2.Translate((MousePosNow.x-MousePosLast.x)/1000*ThisCamera.localPosition.z,0,0);
			Parent1.transform.position=Parent2.position;
			Parent2.localPosition=Vector3.zero;
			Parent1.Translate(0,(MousePosNow.y-MousePosLast.y)/1000*ThisCamera.localPosition.z,0);
		}
		//
		if(Input.GetKey(KeyCode.F))//褰掗浂zz
		{
			Parent1.position=Vector3.zero;
			ThisCamera.localPosition=new Vector3 (0,0,-5);
		}
		
		MousePosLast=Input.mousePosition;
	}
}
