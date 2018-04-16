using UnityEngine;
using System.Collections;

public class EquipShow : MonoBehaviour 
{
	public GameObject Target;
	private bool onDrag = false;                                      //是否被拖拽
	public float speed = 1f;                                          //旋转速度
	private float tempSpeed;                                          //阻尼速度
	private float axisX;                                              //鼠标沿水平方向移动的增量
	private float axisY;                                              //鼠标沿垂直方向移动的增量
	private float cXY;                                                //鼠标移动的距离

	/// <summary>
	/// 接收鼠标按下的事件
	// </summary>
	public void OnPress ()
	{
		axisX = 0f;                                                   //为移动的增量赋初值
		axisY = 0f;
	}


	/// <summary>
	/// 鼠标拖拽时的操作
	/// </summary>
	public void OnDrag (Vector2 delta)
	{
		onDrag = true;                                                //被拖拽
		axisX = -delta.x;// -Input.GetAxis ("Mouse X");                            //获得鼠标增量
		axisY = delta.y;//Input.GetAxis ("Mouse Y");
		cXY = Mathf.Sqrt (axisX * axisX + axisY * axisY);              //计算鼠标移动的长度
		if (cXY == 0f) 
		{
			cXY = 1f;
		}
	}
	
	
	
	/// <summary>
	/// 计算阻尼速度
	/// </summary>
	/// <returns>阻尼的值</returns>
	
	public float Rigid ()
	{
		if (onDrag) 
		{
			tempSpeed = speed;
		} else {
			if (tempSpeed > 0) {
				tempSpeed -= speed * 2 * Time.deltaTime / cXY;        //通过除以鼠标移动长度实现拖拽越长速度减缓越慢
			} else {
				tempSpeed = 0;
			}
		}
		return tempSpeed;                                             //返回阻尼的值
	}
	
	
	
	/// <summary>
	/// 
	/// </summary>
	public void Update ()
	{
		
		//		gameObject.transform.rotation+=new 
		//
		//		return ; 
		float rigid = Rigid ();
		if(new Vector3 (axisY, axisX, 0) * rigid!=Vector3.zero)
		{
			Debug.Log(transform.rotation+"   "+new Vector3 (axisY, axisX, 0) * rigid);
			Target.transform.Rotate (new Vector3 (axisY, axisX, 0) * rigid, Space.Self);
			Debug.Log(gameObject.transform.rotation);
			//				float minAngle = 30;
			//				float maxAngle =60;
			//				float angle = Mathf.LerpAngle(minAngle, maxAngle, Time.time); 
			//				gameObject.transform.eulerAngles = Vector3(0, 0, angle);
		}
		
		if (!Input.GetMouseButton (0)) {
			onDrag = false;
		}
	}
}
