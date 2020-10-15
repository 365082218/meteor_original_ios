using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeteorManager:Singleton<MeteorManager> {
    public MeteorManager(){}
    //public PetController Pet;
    public List<MeteorUnit> UnitInfos = new List<MeteorUnit>();
    public List<MeteorUnit> DeadUnits = new List<MeteorUnit>();
    public List<MeteorUnit> DeadUnitsClone = new List<MeteorUnit>();//备用的
    public SortedDictionary<int, string> LeavedUnits = new SortedDictionary<int, string>();//离场的NPC,设置禁用，取消激活游戏对象，但是可以查询
    Dictionary<string, SceneItemAgent> SceneNameHash = new Dictionary<string, SceneItemAgent>();
    public List<SceneItemAgent> SceneItems = new List<SceneItemAgent>();
    public List<PickupItemAgent> PickupItems = new List<PickupItemAgent>();
    //public List<GameObject> Coins = new List<GameObject>();
    //public List<GameObject> DropThing = new List<GameObject>();
    int UnitInstanceIdx;
    int SceneItemInstanceIdx;
    int PickupItemInstanceIdx;

    //开始新一轮游戏前，重置场景内全体角色状态
    public void Reset() {
        for (int i = 0; i < UnitInfos.Count; i++) {
            if (UnitInfos[i] != null) {
                if (UnitInfos[i] == Main.Ins.LocalPlayer)
                    continue;
                GameObject.Destroy(UnitInfos[i].gameObject);
            }
        }
        UnitInfos.Clear();
        for (int i = 0; i < DeadUnits.Count; i++) {
            if (DeadUnits[i] != null) {
                if (DeadUnits[i] == Main.Ins.LocalPlayer)
                    continue;
                GameObject.Destroy(DeadUnits[i].gameObject);
            }
        }
        DeadUnits.Clear();
        LeavedUnits.Clear();
        //删除动态物件
        DestroyItems();
    }

    public void DestroyItems() {
        //动态物件删除
        for (int i = 0; i < SceneItems.Count; i++) {
            GameObject.Destroy(SceneItems[i].gameObject);
        }

        for (int i = 0; i < PickupItems.Count; i++) {
            GameObject.Destroy(PickupItems[i].gameObject);
        }
        SceneItems.Clear();
        PickupItems.Clear();
        SceneNameHash.Clear();
        //pickup物件删除
    }

    //public void DestroyCoin(GameObject obj)
    //{
    //    Coins.Remove(obj);
    //}

    //public void AddCoin(GameObject coin)
    //{
    //    Coins.Add(coin);
    //}

    //public void AddDropThing(GameObject obj)
    //{
    //    DropThing.Add(obj);
    //}

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
        if (CombatData.Ins.GLevelMode == LevelMode.MultiplyPlayer)
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

    public void OnDestroyPickupItem(PickupItemAgent item) {
        if (!PickupItems.Contains(item))
            return;
        PickupItems.Remove(item);
    }

    public void OnGeneratePickupItem(PickupItemAgent item) {
        if (PickupItems.Contains(item))
            return;
        PickupItems.Add(item);
        item.InstanceId = PickupItemInstanceIdx++;
    }

    public void OnDestroySceneItem(SceneItemAgent item)
    {
        if (!SceneItems.Contains(item))
            return;
        if (SceneNameHash.ContainsKey(item.name))
            SceneNameHash.Remove(item.name);
        SceneItems.Remove(item);
    }

    public void OnGenerateSceneItem(SceneItemAgent item)
    {
        if (SceneItems.Contains(item))
            return;
        if (!SceneNameHash.ContainsKey(item.name))
            SceneNameHash.Add(item.name, item);
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
        MeteorManager.Ins.PhysicalIgnore(unit, true);
        for (int i = 0; i < UnitInfos.Count; i++)
            UnitInfos[i].OnUnitDead(unit);

        if (UnitInfos.Contains(unit))
            UnitInfos.Remove(unit);
        if (DeadUnits.Contains(unit))
            DeadUnits.Remove(unit);
        LeavedUnits.Add(unit.InstanceId, unit.name);
        BuffMng.Ins.RemoveUnit(unit);
        GameObject.Destroy(unit.gameObject);
    }

    public void OnUnitDead(MeteorUnit unit)
    {
        MeteorManager.Ins.PhysicalIgnore(unit, true);
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
        //for (int i = 0; i < UnitInfos.Count; i++)
        //{
        //    UnitInfos[i].OnUnitDead(null);//清除特效。
        //    GameObject.Destroy(UnitInfos[i].gameObject);
        //}
        //for (int i = 0; i < DeadUnits.Count; i++)
        //    GameObject.Destroy(DeadUnits[i].gameObject);
        UnitInfos.Clear();
        DeadUnits.Clear();
        LeavedUnits.Clear();
        SceneItems.Clear();
        Main.Ins.LocalPlayer = null;
        UnitInstanceIdx = 0;
        SceneItemInstanceIdx = 0;
        SceneNameHash.Clear();
        PickupItems.Clear();
    }

    public SceneItemAgent FindSceneItem(string name) {
        if (SceneNameHash.ContainsKey(name))
            return SceneNameHash[name];
        return U3D.GetSceneItem(name);
    }
}
