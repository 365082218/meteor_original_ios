using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupItemAgent : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    string model;
    public void SetPickupItem(string des)
    {
        model = des;
        WsGlobal.ShowMeteorObject(model, transform);
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
        StartCoroutine(yMove());
    }

    bool up = true;
    float yHeight = 5.0f;
    float initializeY = 0.0f;
    IEnumerator yMove()
    {
        while (true)
        {
            if (up)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y + 5 * Time.deltaTime, transform.position.z);
                if (transform.position.y >= initializeY + yHeight)
                    up = false;
            }
            else
            {
                transform.position = new Vector3(transform.position.x, transform.position.y - 5 * Time.deltaTime, transform.position.z);
                if (transform.position.y <= initializeY - yHeight)
                    up = true;
            }
            transform.Rotate(new Vector3(0, 90 * Time.deltaTime, 0));
            yield return 0;
        }
    }

    public void OnPickup(MeteorUnit unit)
    {
        if (unit != null && !unit.Dead)
        {
            //满武器，不能捡
            if (unit.Attr.Weapon != 0 && unit.Attr.Weapon2 != 0)
                return;
            //相同武器，不能捡
            ItemBase ib0 = GameData.Instance.FindItemByIdx(unit.Attr.Weapon);
            WeaponBase wb0 = WeaponMng.Instance.GetItem(ib0.UnitId);
            if (wb0 != null && wb0.WeaponR == model)
                return;

            if (unit.Attr.Weapon2 != 0)
            {
                ItemBase ib1 = GameData.Instance.FindItemByIdx(unit.Attr.Weapon2);
                WeaponBase wb1 = WeaponMng.Instance.GetItem(ib1.UnitId);
                if (wb1 != null && wb1.WeaponR == model)
                    return;
            }

            //同类武器不能捡
            int weaponPickup = GameData.Instance.GetWeaponCode(model);
            ItemBase wb = GameData.Instance.FindItemByIdx(weaponPickup);
            if (wb == null)
                return;

            ItemBase wbl = GameData.Instance.FindItemByIdx(unit.Attr.Weapon);
            if (wbl == null)
                return;

            ItemBase wbr = GameData.Instance.FindItemByIdx(unit.Attr.Weapon2);
            if (wb.SubType == wbl.SubType)
                return;

            if (wbr != null && wb.SubType == wbr.SubType)
                return;
            //可以捡取
            unit.Attr.Weapon2 = weaponPickup;
            SFXLoader.Instance.PlayEffect(672, unit.gameObject, true);
            Destroy(gameObject);
        }
    }
}
