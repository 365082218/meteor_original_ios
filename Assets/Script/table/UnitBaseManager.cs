using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ProtoBuf.ProtoContract(ImplicitFields = ProtoBuf.ImplicitFields.AllFields)]
public class UnitBase : ITableItem
{
    public SmartInt ID;
    public string Name;
    public string Prefab;
    public string ShowPrefab;
    public string RoleIocn;
    public string Desc;

    public int Key() { return ID; }
};

public class UnitMng : TableManager<UnitBase, UnitMng>
{
    public override string TableName() { return "UnitBase"; }
}

[ProtoBuf.ProtoContract(ImplicitFields = ProtoBuf.ImplicitFields.AllFields)]
public class WeaponBase : ITableItem
{
    public SmartInt ID;
    public string Name;
    public string WeaponL;
    public string WeaponR;
    public string TextureL;
    public string TextureR;//新版本支持仅替换材质的武器，但是任意一件武器（双手武器左右各一个材质）仅允许单材质
    public string PosAWL;
    public string PosAWR;
    public string PosBWL;
    public string PosBWR;
    public int Key() { return ID; }
};

public class WeaponMng : TableManager<WeaponBase, WeaponMng>
{
    public override string TableName() { return "Weapon"; }
}