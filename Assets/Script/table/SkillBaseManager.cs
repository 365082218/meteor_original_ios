[ProtoBuf.ProtoContract(ImplicitFields = ProtoBuf.ImplicitFields.AllFields)]
public class SkillData : ITableItem
{
    public int ID;
    public string Name;
    public int hurtType;//伤害类型
    public int skillType;//技能类型
    public int energy;
    public string action;
    public string icon;
    public float cdTime;
    public int hurtPercent;
    public int initHurt;
    public int upHurt;
    public int attackDistance;
    public int rangeType;
    public string attackRange;
    public int Key() { return ID; }
};

public class SkillDataManager : TableManager<SkillData, SkillDataManager>
{
    public override string TableName() { return "SkillData"; }
}