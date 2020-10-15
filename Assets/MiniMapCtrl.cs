using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCtrl : MonoBehaviour {
    public GameObject Map;
    public GameObject PlayerPrefab;
    public GameObject EnemyPrefab;
    public GameObject FriendPrefab;
    public GameObject FlagPrefab;
    //224 X 180
	// Use this for initialization
	void Start () {
		
	}
    SortedDictionary<int, GameObject> Notify = new SortedDictionary<int, GameObject>();
    List<int> leaved = new List<int>();
    // Update is called once per frame
    void Update () {
	    for (int i = 0; i < MeteorManager.Ins.UnitInfos.Count; i++)
        {
            if (!Notify.ContainsKey(MeteorManager.Ins.UnitInfos[i].InstanceId))
            {
                GameObject obj = null;
                if (Main.Ins.LocalPlayer == MeteorManager.Ins.UnitInfos[i])
                    obj = GameObject.Instantiate(PlayerPrefab);
                else if (MeteorManager.Ins.UnitInfos[i].Camp == EUnitCamp.EUC_FRIEND)
                    obj = GameObject.Instantiate(FriendPrefab);
                else if (MeteorManager.Ins.UnitInfos[i].Camp == EUnitCamp.EUC_ENEMY)
                    obj = GameObject.Instantiate(EnemyPrefab);
                else
                    obj = GameObject.Instantiate(FlagPrefab);
                Notify.Add(MeteorManager.Ins.UnitInfos[i].InstanceId, obj);
                obj.transform.SetParent(Map.transform);
                obj.transform.localScale = Vector3.one;
                obj.transform.localRotation = Quaternion.Euler(0, 0, -MeteorManager.Ins.UnitInfos[i].transform.eulerAngles.y);

                Vector2 vec = ConvertToMainPlayer(MeteorManager.Ins.UnitInfos[i].transform.position);
                obj.transform.localPosition = new Vector3(vec.x, vec.y, 0);
                //主角放最上面,依次是伙伴,最后是敌人
                if (Main.Ins.LocalPlayer == MeteorManager.Ins.UnitInfos[i])
                    obj.transform.SetAsLastSibling();
                else if (MeteorManager.Ins.UnitInfos[i].Camp == EUnitCamp.EUC_ENEMY)
                    obj.transform.SetAsFirstSibling();
                obj.name = MeteorManager.Ins.UnitInfos[i].name;
                obj.SetActive(true);
            }
            else
            {
                Notify[MeteorManager.Ins.UnitInfos[i].InstanceId].transform.localRotation = Quaternion.Euler(0, 0, -MeteorManager.Ins.UnitInfos[i].transform.eulerAngles.y); //MeteorManager.Instance.UnitInfos[i].transform.rotation.y;
                Vector2 vec = ConvertToMainPlayer(MeteorManager.Ins.UnitInfos[i].transform.position);
                Notify[MeteorManager.Ins.UnitInfos[i].InstanceId].transform.localPosition = new Vector3(vec.x, vec.y, 0);
            }
        }
        //检查一下有没有已经离开的
        if (MeteorManager.Ins.LeavedUnits.Count != 0) {
            leaved.Clear();
            foreach (var each in MeteorManager.Ins.LeavedUnits) {
                leaved.Add(each.Key);
            }
        }
        for (int i = 0; i < leaved.Count; i++) {
            if (Notify.ContainsKey(leaved[i])) {
                Notify.Remove(leaved[i]);
            }
        }
    }

    Vector2 ConvertToMainPlayer(Vector3 vec)
    {
        Vector3 vec2 = Main.Ins.LocalPlayer.transform.position - vec;
        return new Vector2(vec2.x / 10.0f, vec2.z / 10.0f); //224像素-从左侧往右侧 224 * 20 = 4480码,角色的视力范围约为450码,角色高度38码
    }

}
