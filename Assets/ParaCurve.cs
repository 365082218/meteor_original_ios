using System.Collections;
using System.Collections.Generic;

using UnityEngine;

//抛物线函数实现，只要提供一个垂直方向加速度，终止位置，其余的每一帧都由此抛物线实现
public class ParaCurve : NetBehaviour {

    // Use this for initialization
    Vector3 targetPosition;
    private new void Awake() {
        base.Awake();
    }

    private new void OnDestroy() {
        base.OnDestroy();
    }

    // Update is called once per frame
    public override void NetUpdate () {
        velocity.y += g * FrameReplay.deltaTime;
        float s = velocity.y * FrameReplay.deltaTime;
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
        transform.position = new Vector3(transform.position.x + velocity.x * FrameReplay.deltaTime, transform.position.y + s, transform.position.z + velocity.z * FrameReplay.deltaTime);
    }
    Vector3 velocity;
    float g;
    System.Action oncom;
    public void Init(Vector3 targetPos, float duration, System.Action oncomplete = null)
    {
        oncom = oncomplete;
        targetPosition = targetPos;
        Vector3 forw = (targetPosition - transform.position);
        g = (2 * forw.y) / (duration * duration);
        velocity.x = forw.x / duration;
        velocity.z = forw.z / duration;
    }
}
