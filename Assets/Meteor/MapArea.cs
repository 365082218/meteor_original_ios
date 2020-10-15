using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum MapAreaType
{
    Die,
    Fire,//没5秒一次中毒动作，每次扣50HP
    HpUp,
    AttUp,
    DefUp,
    SpeedUp,
    FlagBox,
    SafeBox,
}
public class MapArea : MonoBehaviour {

    public MapAreaType type;
    float freq = 4;//记录多少S一次;
    public Dictionary<MeteorUnit, float> leftTime = new Dictionary<MeteorUnit, float>();
	// Use this for initialization
	void Start () {
        
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnUnitDead(MeteorUnit unit)
    {
        if (leftTime.ContainsKey(unit))
            leftTime.Remove(unit);
    }

    public void OnCollisionEnter(Collision collision)
    {
        MeteorUnit unit = null;
        switch (type)
        {
            case MapAreaType.Die:
                unit = collision.gameObject.transform.root.GetComponent<MeteorUnit>();
                if (unit != null && !unit.Dead)
                {
                    unit.OnDead();
                }
                break;
            case MapAreaType.Fire:
                unit = collision.gameObject.transform.root.GetComponent<MeteorUnit>();
                if (unit != null && !unit.Dead)
                {
                    unit.GetItem(11);
                }
                break;
        }
    }

    public void OnCollisionStay(Collision collision)
    {
        
    }

    public void OnCollisionExit(Collision collision)
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        MeteorUnit unit = null;
        switch (type)
        {
            case MapAreaType.Die:
                unit = other.gameObject.transform.root.GetComponent<MeteorUnit>();
                if (unit != null && !unit.Dead)
                {
                    unit.OnDead();
                }
                break;
            case MapAreaType.Fire:
                unit = other.gameObject.transform.root.GetComponent<MeteorUnit>();
                if (unit != null && !unit.Dead)
                {
                    unit.GetItem(11);
                }
                break;
            case MapAreaType.FlagBox:
                //进入安全盒，如果身上带有信物，则过关.
                unit = other.gameObject.transform.root.GetComponent<MeteorUnit>();
                if (unit == null)
                    return;
                if (unit.Attr.IsPlayer && unit.GetFlag && CombatData.Ins.GLevelMode == LevelMode.SinglePlayerTask)
                    Main.Ins.GameBattleEx.GameOver(1);
                break;
            case MapAreaType.SafeBox:
                //进入通关区域，过关.
                unit = other.gameObject.transform.root.GetComponent<MeteorUnit>();
                if (unit == null)
                    return;
                if (unit.Attr.IsPlayer && CombatData.Ins.GLevelMode == LevelMode.SinglePlayerTask)
                    Main.Ins.GameBattleEx.GameOver(1);
                break;
        }
    }

    public void OnTriggerStay(Collider other)
    {
        MeteorUnit unit = null;
        switch (type)
        {
            case MapAreaType.Fire:
                unit = other.gameObject.transform.root.GetComponent<MeteorUnit>();
                if (unit != null && !unit.Dead)
                {
                    if (leftTime.ContainsKey(unit))
                    {
                        leftTime[unit] -= FrameReplay.deltaTime;
                        if (leftTime[unit] <= 0.0f)
                        {
                            //不是熔岩骷髅则死
                            //if (unit != null)
                            //    unit.AddBuff(0);
                            leftTime[unit] = freq;
                        }
                    }
                }
                break;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        MeteorUnit unit = null;
        switch (type)
        {
            case MapAreaType.Die:
                unit = other.gameObject.transform.root.GetComponent<MeteorUnit>();
                //不是熔岩骷髅则死
                if (unit != null && !unit.Dead)
                {
                    unit.OnDead();
                }
                break;
            case MapAreaType.Fire:
                unit = other.gameObject.transform.root.GetComponent<MeteorUnit>();
                if (unit == null)
                    return;
                if (leftTime.ContainsKey(unit))
                    leftTime.Remove(unit);
                break;
        }
    }
}
