using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ProtoBuf.ProtoContract(ImplicitFields = ProtoBuf.ImplicitFields.AllFields)]
public class LoadingTips : ITableItem
{
	public int Key () { return Idx; }
    public int Idx;
    public string TipCh;
    public string TipEn;
};

public class LoadingTipsManager : TableManager<LoadingTips, LoadingTipsManager>
{
	public override string TableName() { return "LoadingTips"; }
}
