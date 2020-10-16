using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Idevgame.GameState.DialogState;

//调整按键布局
public class UIAdjustDialogState : CommonDialogState<UIAdjustWnd> {
    public override string DialogName { get { return "UIAdjustWnd"; } }
    public UIAdjustDialogState(MainDialogMgr stateMgr) : base(stateMgr) {

    }
}

public class UIAdjustWnd : Dialog {
    public override void OnClose() {
        if (FightState.Exist()) {
            FightState.Instance.OnRefresh(0, null);
        }
        if (NGUICameraJoystick.Ins != null) {
            NGUICameraJoystick.Ins.enabled = true;
        }
        base.OnClose();
    }

    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data) {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        //Debug.Log("OnAdjustWndOpen");
        Init();
        if (NGUICameraJoystick.Ins != null) {
            NGUICameraJoystick.Ins.enabled = false;
        }
    }
    //界面每次进入时，都是使用默认数值的。记录下来
    MyVector2[] original = new MyVector2[10];//只记录了右侧按键的位置.
    GameButton SelectButton;//当前选中
    int SelectIndex = -1;//选中按钮序号

    void Init() {

        Control("Close").GetComponent<Button>().onClick.AddListener(() => {
            if (Main.Ins.GameBattleEx != null)
                Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.EscDialogState);
            else
                Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.SettingDialogState);
        });

        Control("Reset").GetComponent<Button>().onClick.AddListener(() => {
            OnReset();
        });
        Control("AlphaSliderBar").GetComponent<Slider>().onValueChanged.AddListener(OnUIAlphaChange);
        Control("JoyScale").GetComponent<Slider>().onValueChanged.AddListener(OnJoyScaleChange);
        Control("ButtonScale").GetComponent<Slider>().onValueChanged.AddListener(OnButtonScaleChange);
        Control("ButtonScale").GetComponent<Slider>().enabled = false;
        //for (int i = 0; i < GameStateMgr.Ins.gameStatus.UIScale.Count; i++) {
        //    Debug.Log(GameStateMgr.Ins.gameStatus.UIScale[i]);
        //}
        UIAdjust adjust = Control("LeftJoystick").GetComponent<UIAdjust>();
        GameButton[] buttons = WndObject.GetComponentsInChildren<GameButton>();
        for (int i = 0; i < buttons.Length; i++) {
            int k = i;
            if (i > 0) {
                original[i - 1] = new MyVector2(buttons[i].GetComponent<RectTransform>().anchoredPosition.x, buttons[i].GetComponent<RectTransform>().anchoredPosition.y);
            }
            buttons[i].OnPress.AddListener(() => {
                adjust.OnChangeTarget(k, buttons[k]);
                if (k == 0) {
                    //设置方向键时，无需同步
                    SelectButton = null;
                    SelectIndex = -1;
                    //禁用功能键缩放
                    Control("ButtonScale").GetComponent<Slider>().enabled = false;
                } else {
                    SelectButton = buttons[k];
                    SelectIndex = k;
                    //设置功能键，缩放要同步到缩放滑块上
                    Control("ButtonScale").GetComponent<Slider>().enabled = true;
                    Control("ButtonScale").GetComponent<Slider>().value = GameStateMgr.Ins.gameStatus.UIScale[k - 1];
                }
            });
            buttons[i].OnRelease.AddListener(() => { adjust.OnChangeTarget(-1, null); });
        }

        RefreshUI();
    }

    void OnReset() {
        //for (int i = 0; i < GameStateMgr.Ins.gameStatus.HasUIAnchor.Length; i++) {
        //    GameStateMgr.Ins.gameStatus.HasUIAnchor[i] = false;
        //}
        //位置同步，只有功能按键
        for (int i = 0; i < original.Length; i++) {
            MyVector2 v = new MyVector2(original[i].x, original[i].y);
            GameStateMgr.Ins.gameStatus.UIAnchor[i] = v;
        }

        for (int i = 0; i < GameStateMgr.Ins.gameStatus.UIScale.Count; i++) {
            GameStateMgr.Ins.gameStatus.UIScale[i] = 1;
        }
        //位置同步
        GameStateMgr.Ins.gameStatus.JoyAnchor = new MyVector2(390, 340);
        GameStateMgr.Ins.gameStatus.JoyScale = 1;
        RefreshUI();
    }

    void RefreshUI() {
        GameButton[] buttons = WndObject.GetComponentsInChildren<GameButton>();
        for (int i = 1; i < buttons.Length; i++) {
            if (GameStateMgr.Ins.gameStatus.HasUIAnchor[i - 1])
                buttons[i].GetComponent<RectTransform>().anchoredPosition = GameStateMgr.Ins.gameStatus.UIAnchor[i - 1];
            float scale = GameStateMgr.Ins.gameStatus.UIScale[i-1];
            buttons[i].GetComponent<RectTransform>().localScale = new Vector3(scale, scale, 1);
        }
        float joyScale = GameStateMgr.Ins.gameStatus.JoyScale;
        Control("JoyArrow").GetComponent<RectTransform>().localScale = new Vector3(joyScale, joyScale, 1);
        Control("JoyArrow").GetComponent<RectTransform>().anchoredPosition = GameStateMgr.Ins.gameStatus.JoyAnchor;

        //透明度设定
        Control("AlphaSliderBar").GetComponent<Slider>().value = GameStateMgr.Ins.gameStatus.UIAlpha;
        
        //方向键缩放
        Control("JoyScale").GetComponent<Slider>().value = GameStateMgr.Ins.gameStatus.JoyScale;
        
        //功能键缩放,需要先选择按键后
        Control("ButtonScale").GetComponent<Slider>().value = 1;
        Control("ButtonScale").GetComponent<Slider>().onValueChanged.AddListener(OnButtonScaleChange);
        Control("ButtonScale").GetComponent<Slider>().enabled = false;
        SelectButton = null;
        SelectIndex = -1;
    }

    void OnUIAlphaChange(float v) {
        GameStateMgr.Ins.gameStatus.UIAlpha = v;
        CanvasGroup[] c = WndObject.GetComponentsInChildren<CanvasGroup>();
        for (int i = 0; i < c.Length; i++)
            c[i].alpha = GameStateMgr.Ins.gameStatus.UIAlpha;
    }

    void OnJoyScaleChange(float s) {
        Control("JoyArrow").GetComponent<RectTransform>().localScale = new Vector3(s, s, 1);
        GameStateMgr.Ins.gameStatus.JoyScale = s;
    }

    void OnButtonScaleChange(float s) {
        if (SelectButton != null && SelectIndex > 0) {
            GameStateMgr.Ins.gameStatus.UIScale[SelectIndex - 1] = s;
            SelectButton.GetComponent<RectTransform>().localScale = new Vector3(s, s, 1);
        }
    }
}
