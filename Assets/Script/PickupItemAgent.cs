using Excel2Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupItemAgent : NetBehaviour {
    protected new void Awake()
    {
        base.Awake();
    }

    protected new void OnDestroy()
    {
        base.OnDestroy();
    }

    public override void NetUpdate()
    {
        if (isWeapon)
        {
            startTick -= FrameReplay.deltaTime;
            if (startTick < 0)
            {
                Destroy(gameObject);
                isWeapon = false;
                return;
            }

            yMove();
        }
    }

    string model;
    bool isWeapon = false;
    float startTick = 0;
    static float delay = 60.0f;
    public void SetPickupItem(string des)
    {
        model = des;
        List<WeaponData> allWeapons = new List<WeaponData>();
        List<WeaponData> weapons = Main.Ins.DataMgr.GetWeaponDatas();
        //WeaponDatas.WeaponDatas[] weapons2 = PluginWeaponMng.Instance.GetAllItem();
        for (int i = 0; i < weapons.Count; i++)
        {
            allWeapons.Add(weapons[i]);
        }
        //for (int i = 0; i < weapons2.Length; i++)
        //{
        //    allWeapons.Add(weapons2[i]);
        //}
        for (int i = 0; i < allWeapons.Count; i++)
        {
            if (allWeapons[i].WeaponL == model || allWeapons[i].WeaponR == model)
            {
                isWeapon = true;
                break;
            }
        }
        Utility.ShowMeteorObject(model, transform);
        startTick = delay;
    }

    public void SetAsDrop()
    {
        Collider[] collider = GetComponentsInChildren<Collider>();
        for (int i = 0; i < collider.Length; i++)
            collider[i].enabled = false;
    }

    [SerializeField]
    private AnimationCurve curve;
    public void OnStart()
    {
        Collider[] collider = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < collider.Length; i++)
        {
            if (!collider[i].gameObject.activeInHierarchy)
                continue;
            collider[i].enabled = true;
            MeshCollider me = collider[i] as MeshCollider;
            if (me != null)
                me.convex = true;
            collider[i].isTrigger = true;
        }

        initializeY = transform.position.y;
        if (curve == null)
        {
            Keyframe[] ks = new Keyframe[2];
            ks[0] = new Keyframe(0, -1);
            ks[1] = new Keyframe(1, 1);
            curve = new AnimationCurve(ks);
        }
        curve.postWrapMode = WrapMode.PingPong;
        curve.preWrapMode = WrapMode.PingPong;
    }

    void ProcessWeaponCollider() {
        if (model == "W2_0") {
            GameObject tri = NodeHelper.Find("Object01", gameObject);
            if (tri != null)
                GameObject.Destroy(tri);
            GameObject wpnFA03 = NodeHelper.Find("wpnFA03", gameObject);
            MeshCollider mr = wpnFA03.GetComponent<MeshCollider>();
            if (mr != null)
                GameObject.Destroy(mr);
            BoxCollider box = wpnFA03.GetComponent<BoxCollider>();
            if (box == null)
                box = wpnFA03.AddComponent<BoxCollider>();
            box.isTrigger = true;
            return;
        }
        Transform weapon = null;//实际武器
        Transform atbox = null;//碰撞范围
        for (int i = 0; i < transform.childCount; i++) {
            Transform tr = transform.GetChild(i);
            MeshRenderer mr = tr.GetComponent<MeshRenderer>();
            if (mr != null) {
                if (mr.enabled)
                    weapon = tr;
                else
                    atbox = tr;
            }
        }

        //一些武器并没有攻击范围盒，自身就是
        if (atbox == null) {
            MeshCollider me = weapon.GetComponent<MeshCollider>();
            if (me != null)
                GameObject.Destroy(me);
            BoxCollider b = weapon.GetComponent<BoxCollider>();
            if (b == null) {
                b = weapon.gameObject.AddComponent<BoxCollider>();
            }
            b.isTrigger = true;
        } else {
            MeshCollider me = weapon.GetComponent<MeshCollider>();
            if (me != null)
                GameObject.Destroy(me);
        }
    }

    bool up = true;
    float yHeight = 5.0f;
    float initializeY = 0.0f;
    void yMove()
    {
        if (up)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + 5 * FrameReplay.deltaTime, transform.position.z);
            if (transform.position.y >= initializeY + yHeight)
                up = false;
        }
        else
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - 5 * FrameReplay.deltaTime, transform.position.z);
            if (transform.position.y <= initializeY - yHeight)
                up = true;
        }
        transform.Rotate(new Vector3(0, 90 * FrameReplay.deltaTime, 0));
    }

    public void OnPickup(MeteorUnit unit)
    {
        if (unit != null && !unit.Dead)
        {
            //满武器，不能捡
            if (unit.Attr.Weapon != 0 && unit.Attr.Weapon2 != 0)
                return;
            //相同武器，不能捡
            ItemData ib0 = Main.Ins.GameStateMgr.FindItemByIdx(unit.Attr.Weapon);
            WeaponData wb0 = U3D.GetWeaponProperty(ib0.UnitId);
            if (wb0 != null && wb0.WeaponR == model)
                return;

            if (unit.Attr.Weapon2 != 0)
            {
                ItemData ib1 = Main.Ins.GameStateMgr.FindItemByIdx(unit.Attr.Weapon2);
                WeaponData wb1 = U3D.GetWeaponProperty(ib1.UnitId);
                if (wb1 != null && wb1.WeaponR == model)
                    return;
            }

            //同类武器不能捡
            int weaponPickup = Main.Ins.GameStateMgr.GetWeaponCode(model);
            ItemData wb = Main.Ins.GameStateMgr.FindItemByIdx(weaponPickup);
            if (wb == null)
                return;

            ItemData wbl = Main.Ins.GameStateMgr.FindItemByIdx(unit.Attr.Weapon);
            if (wbl == null)
                return;

            ItemData wbr = Main.Ins.GameStateMgr.FindItemByIdx(unit.Attr.Weapon2);
            if (wb.SubType == wbl.SubType)
                return;

            if (wbr != null && wb.SubType == wbr.SubType)
                return;
            //可以捡取
            unit.Attr.Weapon2 = weaponPickup;
            Main.Ins.SFXLoader.PlayEffect(672, unit.gameObject, true);
            Destroy(gameObject);
        }
    }
}
