using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Idevgame.GameState.DialogState;

//调整按键布局
public class UIAdjustDialogState:CommonDialogState<UIAdjustWnd>
{
    public override string DialogName { get { return "UIAdjustWnd"; } }
    public UIAdjustDialogState(MainDialogStateManager stateMgr):base(stateMgr)
    {

    }
}

public class UIAdjustWnd : Dialog {
    public override void OnClose() {
        if (FightState.Exist()) {
            FightState.Instance.EnableJoyStick();
            FightState.Instance.OnRefresh(0, null);
        }
        base.OnClose();
    }

    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data) {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }

   void Init() {
        //禁用方向上的触发器
        if (FightState.Exist())
            FightState.Instance.DisableJoyStick();

        Control("Close").GetComponent<Button>().onClick.AddListener(() => {
            Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.SettingDialogState);
        });
        GameObject click = Control("ClickPanel");
        //RectTransform rc = click.GetComponent<RectTransform>();
        //rc.sizeDelta = new Vector2(Screen.width, Screen.height);
        UIAdjust adjust = Control("LeftJoystick").GetComponent<UIAdjust>();
        GameButton[] buttons = WndObject.GetComponentsInChildren<GameButton>();
        for (int i = 0; i < buttons.Length; i++) {
            int k = i;
            buttons[i].OnPress.AddListener(() => { adjust.OnChangeTarget(k, buttons[k].GetComponent<RectTransform>()); });
            buttons[i].OnRelease.AddListener(() => { adjust.OnChangeTarget(-1, null); });
        }

        for (int i = 0; i < click.transform.childCount; i++) {
            if (Main.Ins.GameStateMgr.gameStatus.HasUIAnchor[i])
                click.transform.GetChild(i).GetComponent<RectTransform>().anchoredPosition = Main.Ins.GameStateMgr.gameStatus.UIAnchor[i];
        }
    }
}
