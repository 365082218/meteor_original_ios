using UnityEngine;
using System.Collections;
using ProtoBuf;
using System;
using System.Collections.Generic;

[Serializable]
[ProtoContract]
public class SpawnPoint
{
    [ProtoBuf.ProtoMember(1)]
    public int MonsterIdx;
    [ProtoBuf.ProtoMember(2)]
    public int TotalNum;
    [ProtoMember(3)]
    public int Num;//刷怪总数不超过，比如5，那么产生的活着的怪，不许超过这个数量.只记录这个位置产生的怪
    [ProtoMember(4)]
    public float Delay;//2次刷怪间隔 1次只允许刷一只
    public Transform Pos;//产生位置.
    [ProtoMember(5)]
    public MyVector PosV;//位置存档.
    [ProtoMember(6)]
    public float Rotate;//旋转存档
}

[Serializable]
[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class SpawnDat
{
    public SpawnPoint[] spawnInfo = new SpawnPoint[0];
    public float[] spawnTick;
    public int[] spawnCnt;
    public int spawnType;
    public bool StartSpawn;
    public int Index;
}

public enum SpawnType
{
    ActiveTrigger = 0,
    Allways = 1,
    ActiveByOther = 2,
}

public class SpawnBox : MonoBehaviour {
    BoxCollider col;
    public SpawnDat Data;
    public int SpawnBoxIdx;
    // Use this for initialization
    private void Awake()
    {
        col = GetComponent<BoxCollider>();
    }
    void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        if (Data == null)
            return;
        if (!Data.StartSpawn)
            return;
        for (int i = 0; i < Data.spawnInfo.Length; i++)
        {
            Data.spawnTick[i] -= Time.deltaTime;
            if (Data.spawnTick[i] < 0)
            {
                Spawn(i);
            }
        }
	}

    //在加载存档时重建此表，找到上次哪些怪通过此表恢复的.
    Dictionary<MeteorUnit, int> monster = new Dictionary<MeteorUnit, int>();

    void Spawn(int i)
    {
        //此产生点已经刷完所有怪物.
        if (Data.spawnInfo[i].TotalNum == 0)
            return;
        //同时在场上数量超过上限
        if (Data.spawnCnt[i] >= Data.spawnInfo[i].Num)
            return;
        //MapObject obj = null;
        //MeteorUnit un = SceneMng.Spawn(Data.spawnInfo[i].MonsterIdx, SpawnBoxIdx, i);
        //un.transform.position = Data.spawnInfo[i].PosV;
        //un.transform.rotation = Quaternion.Euler(new Vector3(0, Data.spawnInfo[i].Rotate, 0));
        //Data.spawnTick[i] = Data.spawnInfo[i].Delay;
        //Data.spawnInfo[i].TotalNum--;
        //Data.spawnCnt[i]++;
        //monster.Add(un, i);
        //MapUnitCtrl ctrl = un.gameObject.AddComponent<MapUnitCtrl>();
        //ctrl.Init(obj);
    }

    public void OnMonsterDead(MeteorUnit unit)
    {
        //if (unit.Attr.SpawnBoxIdx == 0)
        //    return;
        //if (unit.Attr.SpawnBoxIdx == SpawnBoxIdx)
        //    Data.spawnCnt[unit.Attr.SpawnPointIdx]--;
        //else
        //{
        //    if (monster.ContainsKey(unit))
        //    {
        //        Data.spawnCnt[monster[unit]]--;
        //    }
        //}
    }

    private void OnTriggerEnter(Collider other)
    {
        MeteorUnit unit = other.transform.root.GetComponent<MeteorUnit>();
        if (unit == null)
            return;
        if (unit.Attr != null)
            return;
        if (Data.spawnType == (int)SpawnType.ActiveTrigger)
        {
            Data.StartSpawn = true;
            col.enabled = false;
        }
    }

    //用预设本身带的数据来初始化.
    public void SetUp()
    {
        if (Data != null)
        {
            if (Data.spawnType == (int)SpawnType.Allways)
            {
                col.enabled = false;
                Data.StartSpawn = true;
            }
        }
    }

    //用存档传递过来的Data初始化
    public void Restore()
    {
        if (Data != null)
        {
            if (Data.spawnType == (int)SpawnType.Allways)
            {
                col.enabled = false;
                Data.StartSpawn = true;
            }
            else if (Data.spawnType == (int)SpawnType.ActiveTrigger)
            {
                if (Data.StartSpawn)
                    col.enabled = false;
                else
                {
                    col.enabled = true;
                    col.isTrigger = true;
                }
            }
            else if (Data.spawnType == (int)SpawnType.ActiveByOther)
            {
                col.enabled = false;
            }
        }
    }
}
