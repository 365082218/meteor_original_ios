using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CoClass;

public class DropMng:Singleton<DropMng>{
    public void DropWeapon(MeteorUnit player)
    {
        int mainWeapon = player.Attr.Weapon2;
        if (mainWeapon == 0)
            return;
        ItemBase ib = GameData.Instance.itemMng.GetRowByIdx(mainWeapon) as ItemBase;
        WeaponBase wb = WeaponMng.Instance.GetItem(ib.UnitId);
        GameObject trigget = CreateObj(wb.WeaponR, player.transform, -player.transform.forward);
        //obj.Add(trigget);
        //ExplosionObject01.iTweenExplosion01(1, ref obj, player.transform.position);
        player.Attr.Weapon2 = 0;
        ExplosionObject01.DropItem(trigget, player.mPos, -player.transform.forward);
    }

    public void DropWeapon2(int weaponId)
    {
        MeteorUnit player = MeteorManager.Instance.LocalPlayer;
        WeaponBase wb = WeaponMng.Instance.GetItem(weaponId);
        string des = wb.WeaponR;
        GameObject trigget = CreateTriggerObj(des, player.transform, -player.transform.forward);
        ExplosionObject01.DropItem(trigget, player.mPos, -player.transform.forward);
    }

    public void Drop(MeteorUnit player)
    {
        //player.Attr;
        //List<GameObject> obj = new List<GameObject>();
        //一定爆出角色主武器，有Flag爆出Flag,并且这个Flag，会持续一定时间，若无人拾取，则会重置Flag归位.
        int mainWeapon = player.Attr.Weapon;
        ItemBase ib = GameData.Instance.itemMng.GetRowByIdx(mainWeapon) as ItemBase;
        WeaponBase wb = WeaponMng.Instance.GetItem(ib.UnitId);
        GameObject trigget = CreateObj(wb.WeaponR, player.transform, -player.transform.forward);
        //obj.Add(trigget);
        //ExplosionObject01.iTweenExplosion01(1, ref obj, player.transform.position);
        ExplosionObject01.DropItem(trigget, player.mPos, -player.transform.forward);
        //如果角色拥有第二武器，那么第一武器扔掉时，切换到第二武器
        player.ChangeNextWeapon();

        //扔掉Flag道具
        if (player.GetFlag)
        {
            trigget = CreateObj(player.GetFlagItem.model, player.transform, -player.transform.forward);
            ExplosionObject01.DropItem(trigget, player.mPos, Quaternion.AngleAxis(30, Vector3.up) * -player.transform.forward);
        }
    }

    GameObject CreateObj(string des, Transform pos, Vector3 forward)
    {
        GameObject obj = new GameObject(des);
        obj.transform.position = Vector3.zero;
        obj.transform.rotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
        obj.transform.SetParent(Loader.Instance == null ? null : Loader.Instance.transform);
        obj.transform.position = pos.position + Vector3.up * 50 + forward * 35;
        SceneItemAgent agent = obj.AddComponent<SceneItemAgent>();
        agent.tag = "SceneItemAgent";
        agent.Load(des);
        agent.ApplyPost();
        agent.SetAsDrop();
        MeteorManager.Instance.OnGenerateSceneItem(agent);
        return obj;
    }

    GameObject CreateTriggerObj(string des, Transform pos, Vector3 forward)
    {
        GameObject obj = new GameObject(des);
        obj.transform.position = Vector3.zero;
        obj.transform.rotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
        obj.transform.SetParent(Loader.Instance == null ? null : Loader.Instance.transform);
        obj.transform.position = pos.position + Vector3.up * 50 + forward * 35;
        obj.layer = LayerMask.NameToLayer("Trigger");
        PickupItemAgent agent = obj.AddComponent<PickupItemAgent>();
        agent.SetPickupItem(des);
        return obj;
    }
}
