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

    ////爆炸物件  List
    //public List<GameObject> EObjectList;
    //public GameObject EObject = null;

    ////爆炸物件 个数
    //public int EObjNum = 10;

    ////爆炸半径
    //public float ERadius = 5.0f;

    ////爆炸高度
    //public float EHeight = 5.0f;

    ////爆炸时间
    //public float ETime = 1.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
    //void Update () {
	
    //}

    //EObjectList 爆炸物品列表, InitPosition 爆炸物品中心位置, EObjCopyNum爆炸复制次数, ERadius爆炸半径, EHeight爆炸高度, ETime爆炸时间
    //ExplosionType 爆炸类型  1金币 2掉落物品
    static public void iTweenExplosion01(int ExplosionType, ref List<GameObject> EObjectList, Vector3 InitPosition, int EObjCopyNum = 1, float ERadius = 3.0f, float EHeight = 2.5f, float ETime = 0.2f)
    {
        if (EObjCopyNum < 1)
            return;

        GameObject ExplosionObj = null;
        bool lastCopy = false;//是否最后一个循环，把copy都扔出去，如果不是最后一次循环则COPY一份扔出去
        for (int i = 0; i < EObjCopyNum; i++)
        {
            if (i + 1 == EObjCopyNum)
                lastCopy = true;
            else
                lastCopy = false;

            foreach (GameObject eobj in EObjectList)
            {
                if (lastCopy)
                    ExplosionObj = eobj;
                else
                    ExplosionObj = Instantiate(eobj, InitPosition, Quaternion.identity) as GameObject;

                ExplosionObj.transform.position = InitPosition;

                Vector2 randomPosition = Random.insideUnitCircle * ERadius;

                //Vector3 targetpoint = new Vector3(randomPosition.x, eobj.transform.position.y, randomPosition.y);
                Vector3 targetpoint = new Vector3(ExplosionObj.transform.position.x + randomPosition.x, ExplosionObj.transform.position.y, ExplosionObj.transform.position.z + randomPosition.y);

                //targetpoint.y += 0.1f;

                Vector3[] paths = new Vector3[3];
                paths[0] = ExplosionObj.transform.position;//new Vector3(0, 0, 0);
                paths[2] = targetpoint;
                paths[1] = new Vector3(randomPosition.x * 0.6f, EHeight, randomPosition.y * 0.6f);
                paths[1] += paths[0];

                iTween.MoveTo(ExplosionObj, iTween.Hash(
                    "path", paths,
                    "movetopath", true,
                    //"orienttopath", true, 
                    "time", ETime,
                    "easetype", iTween.EaseType.linear,
                    "oncomplete", "MoveComplete",
                    "oncompletetarget", ExplosionObj));

                //if (ExplosionType == 1)
                //    MeteorManager.Instance.AddCoin(ExplosionObj);
                //else if (ExplosionType == 2)
                //    MeteorManager.Instance.AddDropThing(ExplosionObj);
            }
        }
    }

    /// <summary>
    /// 爆物品
    /// </summary>
    /// <param name="EObject">物品</param>
    /// <param name="position">角色脚跟位置</param>
    /// <param name="forward">角色背向</param>
    /// <param name="ERadius">丢落半径</param>
    /// <param name="EGroundHeight">离地高度</param>
    public static void DropItem(GameObject EObject, Vector3 position, Vector3 forward, float ERadius = 100.0f, float EGroundHeight = 10.0f)
    {
        //有可能会丢到墙里的。
        Vector3 targetPos = (position + 50 * Vector3.up) + ERadius * forward;
        Vector3 endPoint = targetPos - 20 * Vector3.up;
        RaycastHit hit;
        if (Physics.Raycast(targetPos, Vector3.down, out hit, 1000, 1 << LayerMask.NameToLayer("Scene")))
            endPoint = hit.point + EGroundHeight * Vector3.up;
        float h = Mathf.Abs((endPoint.y + EGroundHeight) - (position.y + 50));
        float t = Mathf.Sqrt(2 * h / (MeteorUnit.gGravity / 10));
        EObject.AddComponent<ParaCurve>().Init(endPoint, t, () => { OnComplete(EObject); });
        CFX_AutoRotate rotate = EObject.AddComponent<CFX_AutoRotate>();
        rotate.rotation = new Vector3(Random.Range(180, 360), Random.Range(180, 360), Random.Range(180, 360));
    }

    public static void OnComplete(GameObject obj)
    {
        SceneItemAgent agent = obj.GetComponent<SceneItemAgent>();
        agent.tag = "SceneItemAgent";
        CFX_AutoRotate rotate = obj.GetComponent<CFX_AutoRotate>();
        GameObject.Destroy(rotate);

        obj.transform.rotation = Quaternion.Euler(0, obj.transform.eulerAngles.y, 0);
        if (agent != null)
        {
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

    //void iTweenExplosion()
//    static public void iTweenExplosion01(ref List<GameObject> EObjectList, Vector3 InitPosition, GameObject EObject = null, int EObjNum = 10, float ERadius = 3.0f, float EHeight = 2.5f, float ETime = 0.2f)
//    {
//        //单个相同物品复制爆炸物
//        if (EObject!=null)
//        {
//            for (int i = 0; i < EObjNum; i++)
//            {
//                GameObject eobjs;
//                //if (i < EObjNum - 1)
//                    eobjs = Instantiate(EObject, InitPosition, Quaternion.identity) as GameObject;
//                //else
//                    //eobjs = EObject;

//                Vector2 randomPosition = Random.insideUnitCircle * ERadius;

//                Vector3 targetpoint = new Vector3(eobjs.transform.position.x + randomPosition.x, eobjs.transform.position.y, eobjs.transform.position.z + randomPosition.y);

//                targetpoint.y += 0.1f;

//                Vector3[] paths = new Vector3[3];
//                paths[0] = eobjs.transform.position;//new Vector3(0, 0, 0);
//                paths[2] = targetpoint;
//                paths[1] = new Vector3(randomPosition.x * 0.6f, EHeight, randomPosition.y * 0.6f);
//                paths[1] += paths[0];

////                Debug.Log(string.Format("iTweenExplosion01 paths[0] = {0}, paths[1] = {1}", paths[0], paths[1]));

//                iTween.MoveTo(eobjs, iTween.Hash(
//                    "path", paths,
//                    "movetopath", true,
//                    //"orienttopath", true, 
//                    "time", ETime,
//                    "easetype", iTween.EaseType.linear,
//                    "oncomplete", "MoveComplete",
//                    "oncompletetarget", eobjs));

//                UnitManager.Instance.AddCoin(eobjs);
//            }

//            DestroyImmediate(EObject);
//            return;
//        }


//        //多个不同的掉落物件
//        EObjNum = EObjectList.Count;

//        foreach (GameObject eobj in EObjectList)
//        {
//            //GameObject eobjs = Instantiate(EObject, InitPosition, Quaternion.identity) as GameObject;

//            eobj.transform.position = InitPosition;

//            Vector2 randomPosition = Random.insideUnitCircle * ERadius;

//            //Vector3 targetpoint = new Vector3(randomPosition.x, eobj.transform.position.y, randomPosition.y);
//            Vector3 targetpoint = new Vector3(eobj.transform.position.x + randomPosition.x, eobj.transform.position.y, eobj.transform.position.z + randomPosition.y);

//            targetpoint.y += 0.1f;

//            Vector3[] paths = new Vector3[3];
//            paths[0] = eobj.transform.position;//new Vector3(0, 0, 0);
//            paths[2] = targetpoint;
//            paths[1] = new Vector3(randomPosition.x * 0.6f, EHeight, randomPosition.y * 0.6f);
//            paths[1] += paths[0];

//            iTween.MoveTo(eobj, iTween.Hash(
//                "path", paths,
//                "movetopath", true,
//                //"orienttopath", true, 
//                "time", ETime,
//                "easetype", iTween.EaseType.linear,
//                "oncomplete", "MoveComplete",
//                "oncompletetarget", eobj));

//            UnitManager.Instance.AddCoin(eobj);
//        }
//    }
}
