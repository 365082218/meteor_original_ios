using UnityEngine;

[ProtoBuf.ProtoContract(ImplicitFields = ProtoBuf.ImplicitFields.AllFields)]
public class WeaponBase : ITableItem
{
    public int ID;
    public string Name;
    public string WeaponL;
    public string WeaponR;
    public string TextureL;
    public string TextureR;//�°汾֧�ֽ��滻���ʵ���������������һ��������˫���������Ҹ�һ�����ʣ�����������
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

//��ӵ���������
public class PluginWeaponMng: TableManagerEx<WeaponBase, PluginWeaponMng>
{
    public override string TableName() { return Application.persistentDataPath + "/Plugins/Def/Weapon.txt"; }
}

//��ӵ���Ʒ����
public class PluginItemMng:TableManagerEx<ItemBase, PluginItemMng>
{
    public override string TableName() { return Application.persistentDataPath + "/Plugins/Def/Item.txt"; }
}

public class AnimationMng : TableManagerEx<AnimationBase, AnimationMng>
{
    public override string TableName() { return "AnimationTable.txt"; }
}