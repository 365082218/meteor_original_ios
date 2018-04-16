/*
 * 挂到摄像机上面就可以了
 * Unity3D 简单自由相机脚本
 * 主要控制相机在场景里自由的平移，旋转，缩放，而不跟踪一个指定GameObject的实现。
	新建场景，将此脚本拖放到一个摄像机上即可，鼠标中键缩放，中键按下移动，右键旋转。
	代码如下：
 * 
 * http://blog.sina.com.cn/s/blog_78ea87380101i6z8.html
 */

using UnityEngine;
using System.Collections;

public class ldaFreeCam3 : MonoBehaviour {

	//移动速度
	public float m_MoveSpeed = 5.0f;
	//旋转变量;
	public float m_deltX = 255.0f;
	public float m_deltY = 35.0f;
	//上下左右变量
	private float m_movX = 0.0f;
	private float m_movY = 0.0f;
	//缩放变量;
	float m_distance = 0.0f;
	public float m_RotateSpeed = 5.0f;
	//移动变量;
	private Vector3 m_mouseMovePos = Vector3.zero;
	
	void Start()
	{
		//必须要初始化，要不在Unity属性里面改了数值一直保持就乱套了
		m_MoveSpeed = 5.0f;
		m_RotateSpeed = 5.0f;
		m_deltX = 255.0f;
		m_deltY = 35.0f;
		//camera.transform.localPosition = new Vector3(0, m_distance, 0);
		//this.transform.localPosition = new Vector3(0, m_distance, 0);
	}
	
	void Update () {

		//if((GameBattle.Instance!=null&&GameBattle.Instance.FightStatus == EFightStatus.Finish)||Global.PauseAll||Global.Pause)
		//	return;

		//鼠标右键点下控制相机旋转;
		if (Input.GetMouseButton(1))
		{
			m_deltX += Input.GetAxis("Mouse X") * m_RotateSpeed;
			m_deltY -= Input.GetAxis("Mouse Y") * m_RotateSpeed;
			m_deltX = ClampAngle(m_deltX, -360, 360);
			m_deltY = ClampAngle(m_deltY, -90, 90);
			//camera.transform.rotation = Quaternion.Euler(m_deltY, m_deltX, 0);
			this.transform.rotation = Quaternion.Euler(m_deltY, m_deltX, 0);
		}


		//鼠标中键控制相机上下左右移动;
		if (Input.GetMouseButton(2))
		{
			m_movX = Input.GetAxis("Mouse X") * m_MoveSpeed * Time.deltaTime;
			m_movY = Input.GetAxis("Mouse Y") * m_MoveSpeed * Time.deltaTime;
			//this.transform.position = this.transform.position + new Vector3(m_deltX,m_deltY,0);
			this.transform.position = this.transform.position + this.transform.right * m_movX;
			this.transform.position = this.transform.position + this.transform.up * m_movY;
		}

//		// 上下左右运动
//		if (Input.GetKey(KeyCode.I)){
//			m_distance = m_MoveSpeed * Time.deltaTime;
//			//camera.transform.localPosition = camera.transform.position + camera.transform.forward * m_distance;
//			this.transform.localPosition = this.transform.position + this.transform.forward * m_distance;
//		}
//		else if (Input.GetKey(KeyCode.K)){
//			m_distance = m_MoveSpeed * Time.deltaTime;
//			//camera.transform.localPosition = camera.transform.position - camera.transform.forward * m_distance;
//			this.transform.localPosition = this.transform.position - this.transform.forward * m_distance;
//		}
//	
//		if (Input.GetKey(KeyCode.J)){
//			m_distance = m_MoveSpeed * Time.deltaTime;
//			//camera.transform.localPosition = camera.transform.position - camera.transform.right * m_distance;
//			this.transform.localPosition = this.transform.position - this.transform.right * m_distance;
//		}
//		else if (Input.GetKey(KeyCode.L)){
//			m_distance = m_MoveSpeed * Time.deltaTime;
//			//camera.transform.localPosition = camera.transform.position + camera.transform.right * m_distance;
//			this.transform.localPosition = this.transform.position + this.transform.right * m_distance;
//		}
		
		//鼠标中键点下场景缩放（前进后退）;
		if (Input.GetAxis("Mouse ScrollWheel") != 0)
		{
			//自由缩放方式;
			m_distance = Input.GetAxis("Mouse ScrollWheel") * m_MoveSpeed; //* Time.deltaTime;
			//camera.transform.localPosition = camera.transform.position + camera.transform.forward * m_distance;
			this.transform.position = this.transform.position + this.transform.forward * m_distance;//
		}
//		//鼠标点击场景移动;
//		if (Input.GetMouseButtonDown(2))
//		{
//			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);//从摄像机发出到点击坐标的射线
//			RaycastHit hitInfo;
//			if (Physics.Raycast(ray, out hitInfo))
//			{
//				m_mouseMovePos = hitInfo.point;
//			}
//		}else if (Input.GetMouseButton(2))
//		{
//			Vector3 p = Vector3.zero;
//			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);//从摄像机发出到点击坐标的射线
//			RaycastHit hitInfo;
//			if (Physics.Raycast(ray, out hitInfo))
//			{
//				p = hitInfo.point - m_mouseMovePos;
//				p.y = 0f;
//			}
//			//camera.transform.localPosition = camera.transform.position - p * 0.05f; //在原有的位置上，加上偏移的位置量;
//			this.transform.localPosition = this.transform.position - p * 0.05f; //在原有的位置上，加上偏移的位置量;
//		}
		
//		//相机复位远点;
//		if (Input.GetKey(KeyCode.Space))
//		{
//			m_distance = 10.0f;
//			//camera.transform.localPosition = new Vector3(0, m_distance, 0);
//			this.transform.localPosition = new Vector3(0, m_distance, 0);
//		}
	}
	
	//规划角度;
	float ClampAngle(float angle, float minAngle, float maxAgnle)
	{
		if (angle <= -360)
			angle += 360;
		if (angle >= 360)
			angle -= 360;
		
		return Mathf.Clamp(angle, minAngle, maxAgnle);
	}
}
