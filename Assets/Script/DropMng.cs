using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class DropMng
{
    public void DropWeapon(MeteorUnit player)
    {
        int mainWeapon = player.Attr.Weapon2;
        if (mainWeapon == 0)
            return;
        ItemDatas.ItemDatas ib = Main.Ins.DataMgr.GetData<ItemDatas.ItemDatas>(mainWeapon);
        WeaponDatas.WeaponDatas wb = U3D.GetWeaponProperty(ib.UnitId);
        GameObject trigget = CreateTriggerObj(wb.WeaponR, player.transform, -player.transform.forward);
        //obj.Add(trigget);
        //ExplosionObject01.iTweenExplosion01(1, ref obj, player.transform.position);
        player.Attr.Weapon2 = 0;
        ExplosionObject01.DropItem(trigget, player.transform.position, -player.transform.forward);
    }

    public void DropWeapon2(int weaponId)
    {
        MeteorUnit player = Main.Ins.LocalPlayer;
        WeaponDatas.WeaponDatas wb = U3D.GetWeaponProperty(weaponId);
        string des = wb.WeaponR;
        GameObject trigget = CreateTriggerObj(des, player.transform, -player.transform.forward);
        ExplosionObject01.DropItem(trigget, player.transform.position, -player.transform.forward);
    }

    public void Drop(MeteorUnit player)
    {
        //player.Attr;
        //List<GameObject> obj = new List<GameObject>();
        //一定爆出角色主武器，有Flag爆出Flag,并且这个Flag，会持续一定时间，若无人拾取，则会重置Flag归位.
        int mainWeapon = player.Attr.Weapon;
        ItemDatas.ItemDatas ib = Main.Ins.DataMgr.GetData<ItemDatas.ItemDatas>(mainWeapon);
        WeaponDatas.WeaponDatas wb = U3D.GetWeaponProperty(ib.UnitId);
        GameObject trigget = CreateTriggerObj(wb.WeaponR, player.transform, -player.transform.forward);
        //obj.Add(trigget);
        //ExplosionObject01.iTweenExplosion01(1, ref obj, player.transform.position);
        ExplosionObject01.DropItem(trigget, player.transform.position, -player.transform.forward);
        //如果角色拥有第二武器，那么第一武器扔掉时，切换到第二武器
        player.DropAndChangeWeapon();

        //扔掉Flag道具
        if (player.GetFlag)
        {
            trigget = CreateObj(player.GetFlagItem.model, player.transform, -player.transform.forward);
            ExplosionObject01.DropItem(trigget, player.transform.position, Quaternion.AngleAxis(30, Vector3.up) * -player.transform.forward);
            player.SetFlag(null, 0);
        }
    }

    //一定不能是武器.角色死亡后镖物之类
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
        Main.Ins.MeteorManager.OnGenerateSceneItem(agent);
        return obj;
    }

    //武器
    GameObject CreateTriggerObj(string des, Transform pos, Vector3 forward)
    {
        GameObject obj = new GameObject(des);
        obj.transform.position = Vector3.zero;
        obj.transform.rotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
        obj.transform.SetParent(Loader.Instance == null ? null : Loader.Instance.transform);
        obj.transform.position = pos.position + Vector3.up * 50 + forward * 35;
        obj.layer = LayerMask.NameToLayer("Trigger");
        obj.tag = "PickupItemAgent";
        PickupItemAgent agent = obj.AddComponent<PickupItemAgent>();
        agent.SetPickupItem(des);
        agent.SetAsDrop();
        return obj;
    }
}
