using UnityEngine;

[ProtoBuf.ProtoContract(ImplicitFields = ProtoBuf.ImplicitFields.AllFields)]
public class WeaponBase : ITableItem
{
    public int ID;
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

//外接的武器定义
public class PluginWeaponMng: TableManagerEx<WeaponBase, PluginWeaponMng>
{
    public override string TableName() { return Application.persistentDataPath + "/Plugins/Def/Weapon.txt"; }
}

//外接的物品定义
public class PluginItemMng:TableManagerEx<ItemBase, PluginItemMng>
{
    public override string TableName() { return Application.persistentDataPath + "/Plugins/Def/Item.txt"; }
}

public class AnimationMng : TableManagerEx<AnimationBase, AnimationMng>
{
    public override string TableName() { return "AnimationTable.txt"; }
}