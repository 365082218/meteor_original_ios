using UnityEngine;
using System.Collections;

using System.Linq;
using System.Collections.Generic;
//using UnityEditor;
//武器单双手-各自内部还有子骨骼，要把左右手东西都加载对就OK
public class WeaponLoaderEx : MonoBehaviour
{
    Transform LP;//武器父-除非角色整个换了，否则不会删除。武器挂载点
    Transform RP;//武器父
    Transform L;//武器
    Transform R;//武器
    MeteorUnitDebug owner;
    public bool GenerateMaterial; 
    public string StrWeaponL;
    public string StrWeaponR;
    public void Init()
    {
        //在TPOS放一个物体到武器点，以后所有武器点都放此点下
        if (!init)
        {
            owner = GetComponent<MeteorUnitDebug>();
            LP = new GameObject().transform;
            LP.localPosition = Vector3.zero;
            LP.localScale = Vector3.one;
            LP.rotation = Quaternion.identity;
            LP.SetParent(owner.WeaponL);
            LP.localPosition = Vector3.zero;
            LP.localScale = Vector3.one;
            LP.name = "WeaponL_TPos";
            RP = new GameObject().transform;
            RP.localPosition = Vector3.zero;
            RP.localScale = Vector3.one;
            RP.rotation = Quaternion.identity;
            RP.SetParent(owner.WeaponR);
            RP.localPosition = Vector3.zero;
            RP.localScale = Vector3.one;
            RP.name = "WeaponR_TPos";
            init = true;
        }
    }


    public void UnEquipWeapon()
    {
        if (L != null)
        {
            DestroyImmediate(L.gameObject);
            L = null;
        }
        if (R != null)
        {
            DestroyImmediate(R.gameObject);
            R = null;
        }
    }

    bool init = false;
    //把背包里的物品，装备到身上.
    public void EquipWeapon()
    {
        UnEquipWeapon();
        Init();
        DesLoader.Instance.Refresh();
        GMCLoader.Instance.Refresh();
        GMBLoader.Instance.Refresh();
        if (!string.IsNullOrEmpty(StrWeaponL))
        {
            GameObject weaponPrefab = Resources.Load<GameObject>(StrWeaponL);
            //如果新武器系统包含了这件武器。
            if (weaponPrefab == null)
            {
                if (L != null)
                    DestroyImmediate(L);
                L = new GameObject().transform;
                L.SetParent(LP);
                L.gameObject.layer = LP.gameObject.layer;
                L.localRotation = Quaternion.identity;
                L.localPosition = Vector3.zero;
                L.localScale = Vector3.one;
                L.name = StrWeaponR;
                WsGlobal.ShowMeteorObject(StrWeaponL, L.transform, GenerateMaterial);
                WsGlobal.SetObjectLayer(L.gameObject, L.gameObject.layer);
            }
            else
            {
                if (L != null)
                    DestroyImmediate(L);
                GameObject objWeapon = GameObject.Instantiate(weaponPrefab);
                L = objWeapon.transform;
                L.SetParent(LP);
                L.gameObject.layer = LP.gameObject.layer;
                L.localPosition = Vector3.zero;
                //这种导入来的模型，需要Y轴旋转180，与原系统的物件坐标系存在一些问题
                L.localRotation = new Quaternion(0, 1, 0, 0);
                L.name = StrWeaponL;
                L.localScale = Vector3.one;
                WsGlobal.SetObjectLayer(L.gameObject, L.gameObject.layer);
            }
            
        }
        if (!string.IsNullOrEmpty(StrWeaponR))
        {
            GameObject weaponPrefab = Resources.Load<GameObject>(StrWeaponR);
            if (weaponPrefab == null)
            {
                if (R != null)
                    DestroyImmediate(R);
                R = new GameObject().transform;
                R.SetParent(RP);
                R.gameObject.layer = RP.gameObject.layer;
                R.localRotation = Quaternion.identity;
                R.localPosition = Vector3.zero;
                R.localScale = Vector3.one;
                R.name = StrWeaponR;
                WsGlobal.ShowMeteorObject(StrWeaponR, R.transform, GenerateMaterial);
                WsGlobal.SetObjectLayer(R.gameObject, R.gameObject.layer);
            }
            else
            {
                //预设要绕Y旋转180
                if (R != null)
                    DestroyImmediate(R);
                GameObject objWeapon = GameObject.Instantiate(weaponPrefab);
                R = objWeapon.transform;
                R.SetParent(RP);
                R.gameObject.layer = RP.gameObject.layer;
                R.localPosition = Vector3.zero;
                R.localRotation = new Quaternion(0, 1, 0, 0);
                R.name = StrWeaponR;
                R.localScale = Vector3.one;
                WsGlobal.SetObjectLayer(R.gameObject, R.gameObject.layer);
            }
        }
    }

    
}
