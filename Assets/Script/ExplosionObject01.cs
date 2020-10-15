using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

//using System.IO;
//using System.Xml.Serialization;
//using System.Runtime.Serialization.Formatters.Binary;
//using System.Xml;
//using System.Text;

/**
 * 模拟爆炸掉落物品
 */
public class ExplosionObject01 : MonoBehaviour {
	void Start () {
	
	}
	
    /// <summary>
    /// 爆物品
    /// </summary>
    /// <param name="EObject">物品</param>
    /// <param name="position">角色脚跟位置</param>
    /// <param name="forward">角色背向</param>
    /// <param name="ERadius">丢落半径</param>
    /// <param name="EGroundHeight">离地高度</param>
    public static void DropItem(GameObject EObject, Vector3 position, Vector3 forward, float ERadius, float EGroundHeight)
    {
        //有可能会丢到墙里的。
        Vector3 targetPos = (position + 50 * Vector3.up) + ERadius * forward;
        Vector3 endPoint = targetPos - 20 * Vector3.up;
        RaycastHit hit;
        if (Physics.Raycast(targetPos, Vector3.down, out hit, 1000, LayerManager.SceneMask))
            endPoint = hit.point + EGroundHeight * Vector3.up;
        float h = Mathf.Abs((endPoint.y + EGroundHeight) - (position.y + 50));
        float t = Mathf.Sqrt(2 * h / (CombatData.Ins.gGravity / 10));
        EObject.AddComponent<ParaCurve>().Init(endPoint, t, () => { OnComplete(EObject); });
        CFX_AutoRotate rotate = EObject.AddComponent<CFX_AutoRotate>();
        rotate.rotation = new Vector3(Utility.Range(180, 360), Utility.Range(180, 360), Utility.Range(180, 360));
    }

    public static void OnComplete(GameObject obj)
    {
        SceneItemAgent agent = obj.GetComponent<SceneItemAgent>();
        CFX_AutoRotate rotate = obj.GetComponent<CFX_AutoRotate>();
        GameObject.Destroy(rotate);
        obj.transform.rotation = Quaternion.Euler(0, obj.transform.eulerAngles.y, 0);
        if (agent != null)
        {
            agent.tag = "SceneItemAgent";
            agent.OnStart();
            //agent.SetAsDrop();
            if (agent.ItemInfo != null && agent.ItemInfo.IsFlag())
                agent.SetAutoDestroy(3.0f);//镖物归位
        }
        else
        {
            PickupItemAgent a = obj.GetComponent<PickupItemAgent>();
            if (a != null)
                a.OnStart();
        }
    }
}
