using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeteorManager {
    public MeteorManager(){}
    //public PetController Pet;
    public List<MeteorUnit> UnitInfos = new List<MeteorUnit>();
    public List<MeteorUnit> DeadUnits = new List<MeteorUnit>();
    public List<MeteorUnit> DeadUnitsClone = new List<MeteorUnit>();//备用的
    public SortedDictionary<int, string> LeavedUnits = new SortedDictionary<int, string>();//离场的NPC,设置禁用，取消激活游戏对象，但是可以查询
    public List<SceneItemAgent> SceneItems = new List<SceneItemAgent>();
    public List<GameObject> Coins = new List<GameObject>();
    public List<GameObject> DropThing = new List<GameObject>();
    int UnitInstanceIdx;
    int SceneItemInstanceIdx;
    public void DestroyCoin(GameObject obj)
    {
        Coins.Remove(obj);
    }

    public void AddCoin(GameObject coin)
    {
        Coins.Add(coin);
    }

    public void AddDropThing(GameObject obj)
    {
        DropThing.Add(obj);
    }

    public void OnGenerateUnit(MeteorUnit unit, int playerId = 0)
    {
        if (UnitInfos.Contains(unit))
        {
            Debug.LogError("unit exist");
            return;
        }
        ////让角色间无法穿透，待某个角色使出特殊技能时，关掉碰撞。
        //for (int i = 0; i < UnitInfos.Count; i++)
        //    UnitInfos[i].PhysicalIgnore(unit, false);
        if (Main.Ins.CombatData.GLevelMode == LevelMode.MultiplyPlayer)
            unit.InstanceId = playerId;
        else
        {
            unit.InstanceId = UnitInstanceIdx;
            if (IsFirstMember(unit))
                unit.IsLeader = true;
            UnitInstanceIdx++;
        }
        UnitInfos.Add(unit);
    }

    public bool IsFirstMember(MeteorUnit unit)
    {
        for (int i = 0; i < UnitInfos.Count; i++)
        {
            if (UnitInfos[i].Camp == unit.Camp)
                return false;
        }
        return true;
    }

    public void OnDestroySceneItem(SceneItemAgent item)
    {
        if (!SceneItems.Contains(item))
            return;
        SceneItems.Remove(item);
    }

    public void OnGenerateSceneItem(SceneItemAgent item)
    {
        if (SceneItems.Contains(item))
            return;
        SceneItems.Add(item);
        item.InstanceId = SceneItemInstanceIdx++;
    }

    public void PhysicalIgnore(MeteorUnit self, bool ignore)
    {
        for (int i = 0; i < UnitInfos.Count; i++)
        {
            if (UnitInfos[i] == self)
                continue;
            //如果对方处于无阻塞状态，就忽略角色间的碰撞
            if (self.IgnorePhysical && !ignore && UnitInfos[i].IgnorePhysical)
                continue;
            UnitInfos[i].PhysicalIgnore(self, ignore);
        }
        self.IgnorePhysical = ignore;
    }

    //某个角色从场景删除
    public void OnRemoveUnit(MeteorUnit unit)
    {
        Main.Ins.MeteorManager.PhysicalIgnore(unit, true);
        for (int i = 0; i < UnitInfos.Count; i++)
            UnitInfos[i].OnUnitDead(unit);

        if (UnitInfos.Contains(unit))
            UnitInfos.Remove(unit);
        if (DeadUnits.Contains(unit))
            DeadUnits.Remove(unit);
        LeavedUnits.Add(unit.InstanceId, unit.name);
        Main.Ins.BuffMng.RemoveUnit(unit);
        GameObject.Destroy(unit.gameObject);
    }

    public void OnUnitDead(MeteorUnit unit)
    {
        Main.Ins.MeteorManager.PhysicalIgnore(unit, true);
        unit.OnUnitDead(null);
        UnitInfos.Remove(unit);
        DeadUnits.Add(unit);
        //角色死亡，导致其他角色统统检查目标是否是无效的。
        for (int i = 0; i < UnitInfos.Count; i++)
            UnitInfos[i].OnUnitDead(unit);
        MapArea[] area = GameObject.FindObjectsOfType<MapArea>();
        if (area != null && area.Length != 0)
        {
            for (int i = 0; i < area.Length; i++)
                area[i].OnUnitDead(unit);
        }
    }

    public void Clear()
    {
        for (int i = 0; i < UnitInfos.Count; i++)
        {
            UnitInfos[i].OnUnitDead(null);//清除特效。
#if !STRIP_DBG_SETTING
            WSDebug.Ins.RemoveDebuggableObject(UnitInfos[i]);
#endif
            GameObject.Destroy(UnitInfos[i].gameObject);
        }
        for (int i = 0; i < DeadUnits.Count; i++)
            GameObject.Destroy(DeadUnits[i].gameObject);
        UnitInfos.Clear();
        DeadUnits.Clear();
        LeavedUnits.Clear();
        SceneItems.Clear();
        Main.Ins.LocalPlayer = null;
        UnitInstanceIdx = 0;
        SceneItemInstanceIdx = 0;
    }
}
