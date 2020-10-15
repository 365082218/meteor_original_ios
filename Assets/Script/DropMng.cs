using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Excel2Json;
using protocol;

public class DropMng:Singleton<DropMng>
{
    public void DropItem(int it, Vector3 position, Vector3 forward) {
        Option item = MenuResLoader.Ins.GetItemInfo(it);
        if (item.IsItem() || item.IsFlag()) {
            GameObject trigget = CreateObj(item.model, position, -1 * forward);
            ExplosionObject01.DropItem(trigget, position, Quaternion.AngleAxis(30, Vector3.up) * (-forward), 100, 10);
        } else if (item.IsWeapon()) {
            GameObject trigget = CreateTriggerObj(item.model, position, -forward);
            ExplosionObject01.DropItem(trigget, position, Quaternion.AngleAxis(30, Vector3.up) * -forward, 100, 10);
        }
    }

    //从角色头顶掉落物件
    public void DropItem(MeteorUnit player, Option item) {
        if (item.IsItem() || item.IsFlag()) {
            GameObject trigget = CreateObj(item.model, player.transform.position, -1 * player.transform.forward);
            ExplosionObject01.DropItem(trigget, player.transform.position, Quaternion.AngleAxis(30, Vector3.up) * (-player.transform.forward), 100, 10);
        } else if (item.IsWeapon()) {
            GameObject trigget = CreateTriggerObj(item.model, player.transform.position, -player.transform.forward);
            ExplosionObject01.DropItem(trigget, player.transform.position, Quaternion.AngleAxis(30, Vector3.up) * -player.transform.forward, 100, 10);
        }
    }

    public void DropWeapon(MeteorUnit player)
    {
        int mainWeapon = player.Attr.Weapon2;
        if (mainWeapon == 0)
            return;
        ItemData ib = DataMgr.Ins.GetItemData(mainWeapon);
        WeaponData wb = U3D.GetWeaponProperty(ib.UnitId);
        GameObject trigget = CreateTriggerObj(wb.WeaponR, player.transform.position, -player.transform.forward);
        //obj.Add(trigget);
        //ExplosionObject01.iTweenExplosion01(1, ref obj, player.transform.position);
        player.Attr.Weapon2 = 0;
        ExplosionObject01.DropItem(trigget, player.transform.position, (-1 * player.transform.forward), 100, 10);
    }

    public void DropWeapon2(int weaponId)
    {
        MeteorUnit player = Main.Ins.LocalPlayer;
        WeaponData wb = U3D.GetWeaponProperty(weaponId);
        string des = wb.WeaponR;
        GameObject trigget = CreateTriggerObj(des, player.transform.position, -player.transform.forward);
        ExplosionObject01.DropItem(trigget, player.transform.position, (-1 * player.transform.forward), 100, 10);
    }

    //丢掉角色的武器，如果角色带有道具，丢掉道具.
    public void Drop(MeteorUnit player)
    {
        //player.Attr;
        //List<GameObject> obj = new List<GameObject>();
        //一定爆出角色主武器，有Flag爆出Flag,并且这个Flag，会持续一定时间，若无人拾取，则会重置Flag归位.
        int mainWeapon = player.Attr.Weapon;
        ItemData ib = DataMgr.Ins.GetItemData(mainWeapon);
        WeaponData wb = U3D.GetWeaponProperty(ib.UnitId);
        GameObject trigget = CreateTriggerObj(wb.WeaponR, player.transform.position, -player.transform.forward);
        //obj.Add(trigget);
        //ExplosionObject01.iTweenExplosion01(1, ref obj, player.transform.position);
        ExplosionObject01.DropItem(trigget, player.transform.position, (-1 * player.transform.forward), 100, 10);
        //如果角色拥有第二武器，那么第一武器扔掉时，切换到第二武器
        player.DropAndChangeWeapon();

        //扔掉Flag道具
        if (player.GetFlag)
        {
            trigget = CreateObj(player.GetFlagItem.model, player.transform.position, -1 * player.transform.forward);
            ExplosionObject01.DropItem(trigget, player.transform.position, Quaternion.AngleAxis(30, Vector3.up) * -player.transform.forward, 100, 10);
            player.SetFlag(null, 0);
        }
    }

    //一定不能是武器.角色死亡后镖物之类
    GameObject CreateObj(string des, Vector3 pos, Vector3 forward)
    {
        GameObject obj = new GameObject(des);
        obj.transform.position = Vector3.zero;
        obj.transform.rotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
        obj.transform.SetParent(Loader.Instance == null ? null : Loader.Instance.transform);
        obj.transform.position = (pos + Vector3.up * 50 + forward * 35);
        SceneItemAgent agent = obj.AddComponent<SceneItemAgent>();
        agent.tag = "SceneItemAgent";
        agent.Load(des);
        agent.ApplyPost();
        agent.SetAsDrop();
        return obj;
    }

    //武器
    GameObject CreateTriggerObj(string des, Vector3 pos, Vector3 forward)
    {
        GameObject obj = new GameObject(des);
        obj.transform.position = Vector3.zero;
        obj.transform.rotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
        obj.transform.SetParent(Loader.Instance == null ? null : Loader.Instance.transform);
        obj.transform.position = pos + Vector3.up * 50 + forward * 35;
        obj.layer = LayerManager.Trigger;
        obj.tag = "PickupItemAgent";
        PickupItemAgent agent = obj.AddComponent<PickupItemAgent>();
        agent.SetPickupItem(des);
        agent.SetAsDrop();
        return obj;
    }
}
