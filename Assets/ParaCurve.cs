using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//抛物线函数实现，只要提供一个垂直方向加速度，终止位置，其余的每一帧都由此抛物线实现
public class ParaCurve : MonoBehaviour {

    // Use this for initialization
    Vector3 targetPosition;
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        velocity.y += g * Time.deltaTime;
        float s = velocity.y * Time.deltaTime;
        if (transform.position.y > targetPosition.y)
        {
            if (transform.position.y + s <= targetPosition.y)
            {
                if (oncom != null)
                    oncom.Invoke();
                GameObject.Destroy(this);
                return;
            }
            
        }
        else
        {
            if (transform.position.y + s >= targetPosition.y)
            {
                if (oncom != null)
                    oncom.Invoke();
                GameObject.Destroy(this);
                return;
            }

        }
        transform.position = new Vector3(transform.position.x + velocity.x * Time.deltaTime, transform.position.y + s, transform.position.z + velocity.z * Time.deltaTime);
    }
    Vector3 velocity;
    float g;
    System.Action oncom;
    public void Init(Vector3 targetPos, float duration, System.Action oncomplete = null)
    {
        oncom = oncomplete;
        targetPosition = targetPos;
        //last = duration;
        //float h = targetPosition.y - transform.position.y;
        Vector3 forw = (targetPosition - transform.position);
        //forw.y = 0;
        //forw = Vector3.Normalize(forw);
        g = (2 * forw.y) / (duration * duration);
        velocity.x = forw.x / duration;
        velocity.z = forw.z / duration;
        //if (targetPos.y < transform.position.y)
        //{

        //}
        //GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //obj.name = "source";
        //obj.transform.position = transform.position;
        //obj.transform.localScale = Vector3.one;

        //GameObject target = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //target.name = "target";
        //target.transform.position = targetPos;
        //target.transform.localScale = Vector3.one;
    }
}
