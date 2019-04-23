using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//退出二次确认框
public class EscConfirmWnd : Window<EscConfirmWnd>
{
    public override string PrefabName { get { return "EscConfirmWnd"; } }
	
    protected override bool OnOpen()
    {
		Init();
		return base.OnOpen();
    }

    protected override bool OnClose()
    {
        return base.OnClose();
    }
	
	void Init()
	{
		
	}
}
