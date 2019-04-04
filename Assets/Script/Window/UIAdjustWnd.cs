using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIAdjustWnd : Window<UIAdjustWnd> {
    public override string PrefabName
    {
        get
        {
            return "UIAdjustWnd";
        }
    }
    protected override int GetZ()
    {
        return -10;
    }

    protected override bool OnClose()
    {
        if (FightWnd.Exist)
        {
            FightWnd.Instance.EnableJoyStick();
            FightWnd.Instance.OnRefresh(0, null);
        }
        return base.OnClose();
    }
    protected override bool OnOpen()
    {
        //禁用方向上的触发器
        if (FightWnd.Exist)
            FightWnd.Instance.DisableJoyStick();

        Control("Close").GetComponent<Button>().onClick.AddListener(() => {
            Close();
        });
        GameObject click = Control("ClickPanel");
        //RectTransform rc = click.GetComponent<RectTransform>();
        //rc.sizeDelta = new Vector2(Screen.width, Screen.height);
        UIAdjust adjust = Control("LeftJoystick").GetComponent<UIAdjust>();
        GameButton[] buttons = WndObject.GetComponentsInChildren<GameButton>();
        for (int i = 0; i < buttons.Length; i++)
        {
            int k = i;
            buttons[i].OnPress.AddListener(() => { adjust.OnChangeTarget(k, buttons[k].GetComponent<RectTransform>()); });
            buttons[i].OnRelease.AddListener(() => { adjust.OnChangeTarget(-1, null); });
        }

        for (int i = 0; i < click.transform.childCount; i++)
        {
            if (GameData.Instance.gameStatus.HasUIAnchor[i])
                click.transform.GetChild(i).GetComponent<RectTransform>().anchoredPosition = GameData.Instance.gameStatus.UIAnchor[i];
        }
        return true;
    }
}
