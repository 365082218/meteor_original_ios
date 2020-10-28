using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using DG.Tweening;

public class UGUIJoystick:MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    static UGUIJoystick mInstance = null;
    public static UGUIJoystick Ins { get { return mInstance; } }
    [SerializeField]
    GameButtonEx GamePad;
    [SerializeField]
    Image GamePadGraph;
    public bool mJoyPressed = false;
    public bool ArrowPressed { get { return mInstance.wKey.PointDown || mInstance.sKey.PointDown || mInstance.aKey.PointDown || mInstance.dKey.PointDown; } }
    Vector2 mDelta = Vector2.zero;
    public Vector2 Delta {
        get
        {
            if (mJoyPressed)
            {

            }
            else if (ArrowPressed)
            {
                if (mInstance.wKey.PointDown)
                    mDelta = Vector2.up;
                else if (mInstance.sKey.PointDown)
                    mDelta = Vector2.down;
                else if (mInstance.aKey.PointDown)
                    mDelta = Vector2.left;
                else if (mInstance.dKey.PointDown)
                    mDelta = Vector2.right;
            }
            return mDelta;
        }
    }

    public void SetJoyEnable(bool enable) {
        if (container != null)
            container.gameObject.SetActive(enable);
    }
    public Transform root;//摇杆的顶层，决定了摇杆容器的位置
    public Transform container;//摇杆的父容器.
    public int radius = 200;//摇杆可拖拽半径
    //每个轴拥有一个堆栈，激活后，要再激活，必须先清空堆栈，清空堆栈的前提是，轴离开按键处于的那块范围。或任意激活一个键，其他的键都会被清空
    public GameButton wKey;
    public GameButton sKey;
    public GameButton aKey;
    public GameButton dKey;
    Vector2 InitializePos;
    protected void Awake()
    {
        InitializePosition();
        ResetJoystick();
        mInstance = this;
        if (container == null)
            container = transform;
        if (Main.Ins != null) {
            if (CombatData.Ins.GMeteorInput == null) {
                U3D.PopupTip("主角的输入控制器还未创建 NGUIJoyStick创建按键映射失败.");
                return;
            }
            wKey.OnPress.AddListener(() => { CombatData.Ins.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyW); });
            sKey.OnPress.AddListener(() => { CombatData.Ins.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyS); });
            aKey.OnPress.AddListener(() => { CombatData.Ins.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyA); });
            dKey.OnPress.AddListener(() => { CombatData.Ins.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyD); });

            wKey.OnPressing.AddListener(() => { CombatData.Ins.GMeteorInput.OnAxisKeyPressing(EKeyList.KL_KeyW); });
            sKey.OnPressing.AddListener(() => { CombatData.Ins.GMeteorInput.OnAxisKeyPressing(EKeyList.KL_KeyS); });
            aKey.OnPressing.AddListener(() => { CombatData.Ins.GMeteorInput.OnAxisKeyPressing(EKeyList.KL_KeyA); });
            dKey.OnPressing.AddListener(() => { CombatData.Ins.GMeteorInput.OnAxisKeyPressing(EKeyList.KL_KeyD); });

            wKey.OnRelease.AddListener(() => { CombatData.Ins.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyW); });
            sKey.OnRelease.AddListener(() => { CombatData.Ins.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyS); });
            aKey.OnRelease.AddListener(() => { CombatData.Ins.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyA); });
            dKey.OnRelease.AddListener(() => { CombatData.Ins.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyD); });
        }
    }

    Camera MainUICamera;
    void InitializePosition() {
        if (MainUICamera == null) {
            GameObject cameraObjet = GameObject.FindGameObjectWithTag("UICamera");
            if (cameraObjet != null)
                MainUICamera = cameraObjet.GetComponent<Camera>();
        }
        
        InitializePos = RectTransformUtility.WorldToScreenPoint(MainUICamera, transform.parent.position);
    }

    protected void OnDestroy()
    {
        mInstance = null;
    }

    Vector2 fixAnchor = new Vector2(390, 340);
    public void SetAnchor(Vector2 anchor)
    {
        float joyScale = GameStateMgr.Ins.gameStatus.JoyScale;
        root.GetComponent<RectTransform>().localScale = new Vector3(joyScale, joyScale, 1);
        fixAnchor = anchor;
        root.GetComponent<RectTransform>().anchoredPosition = new Vector2(anchor.x, anchor.y);
        InitializePosition();
    }

    Color PadActive = new Color(1, 1, 1, 0.45f);
    Color PadInActive = new Color(1, 1, 1, 0.1f);
    public void OnBeginDrag(PointerEventData data) {
        GamePadGraph.color = PadActive;
        SetKeyActive(false);
        mJoyPressed = true;
        if (NGUICameraJoystick.Ins != null) {
            NGUICameraJoystick.Ins.enabled = false;
        }
    }

    public void OnEndDrag(PointerEventData data) {
        (this.transform as RectTransform).DOAnchorPos(Vector2.zero, 0.1f, false).SetEase(Ease.OutSine).OnComplete(()=> {
            GamePadGraph.color = PadInActive;
            SetKeyActive(true);
        });
        mJoyPressed = false;
        if (NGUICameraJoystick.Ins != null) {
            NGUICameraJoystick.Ins.enabled = true;
        }
    }

    public void OnDrag(PointerEventData eventData) {
        Vector2 vecTarget = eventData.position - InitializePos;
        if (vecTarget.magnitude > radius) {
            vecTarget = vecTarget.normalized * radius;
        }
        (transform as RectTransform).DOAnchorPos(vecTarget, 0.05f, false).SetEase(Ease.OutSine);
        mDelta = (transform as RectTransform).anchoredPosition.normalized;
    }

    void SetKeyActive(bool active) {
        GamePad.gameObject.SetActive(active);
        wKey.gameObject.SetActive(active);
        aKey.gameObject.SetActive(active);
        sKey.gameObject.SetActive(active);
        dKey.gameObject.SetActive(active);
    }

    public void ResetJoystick()
    {
        container.localPosition = Vector3.zero;
        mDelta = Vector2.zero;
        mJoyPressed = false;
    }

    public void Lock(bool state)
    {
        wKey.enabled = !state;
        sKey.enabled = !state;
        aKey.enabled = !state;
        dKey.enabled = !state;
    }
}
