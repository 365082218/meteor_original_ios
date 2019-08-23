using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Idevgame.GameState.DialogState;

public class PopupTipState:CommonDialogState<PopupTip>
{
    public override string DialogName { get { return "PopupTip"; } }
    public PopupTipState(MainDialogStateManager stateMgr):base(stateMgr)
    {

    }

    public override bool CanOpen()
    {
        return true;
    }

    public override bool AutoClear()
    {
        return true;
    }
}

public class PopupTip : Dialog {

    public Text message;
    public Text title;
    public float duraction = 3.0f;
    // Use this for initialization
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        title.text = "消息";
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Popup(data as string);
    }
	
	// Update is called once per frame
	void Update () {
        duraction -= Time.deltaTime;
        if (duraction <= 0.0f)
            OnBackPress();
    }

    public void Popup(string str)
    {
        message.text = str;
        gameObject.FlyTo(duraction - 1.0f, 100.0f);
    }
}
