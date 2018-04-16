using System;

[ProtoBuf.ProtoContract(ImplicitFields = ProtoBuf.ImplicitFields.AllFields)]
public class PatrolData : ITableItem
{
	public int ID;
	public int MonsterId;
	public int PatrolType;
	public float PatrolRadius;
	public string PatrolList;
	public int Paused;
	public float PausedTime;
	public int Key(){return ID;}
}

public class PatrolManager: TableManager<PatrolData, PatrolManager>
{
	public override string TableName() { return "Patrol"; }
}
