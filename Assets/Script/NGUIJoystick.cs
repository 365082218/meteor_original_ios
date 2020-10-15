using UnityEngine;
using System.Collections;

public class NGUIJoystick : MonoBehaviour
{
    static NGUIJoystick mInstance = null;
    public static NGUIJoystick Ins { get { return mInstance; } }

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

    public void OnMouseDown()
    {
        Debug.Log("mouseDown");
    }

    public void OnEnabled()
    {
        if (target != null)
            target.gameObject.SetActive(true);
        if (JoyCollider != null)
            JoyCollider.enabled = true;
    }

    public void OnDisabled()
    {
        if (target != null)
            target.gameObject.SetActive(false);
        if (JoyCollider != null)
            JoyCollider.enabled = false;
    }

    public SphereCollider JoyCollider;
	//static Vector2 mSlipDelta = Vector2.zero;
	//static public Vector2 SlipDelta {get {return mSlipDelta;} set {mSlipDelta = value;} }

    //public Transform background;
    public Transform target;
    public int direction = 180;//限定轴最远可以离中心多少
    public int reactiveRange = 60;//激活范围在60-100内，当轴处于这个范围，就会激活相应的连招键。
    //每个轴拥有一个堆栈，激活后，要再激活，必须先清空堆栈，清空堆栈的前提是，轴离开按键处于的那块范围。或任意激活一个键，其他的键都会被清空
    public GameButton wKey;
    public GameButton sKey;
    public GameButton aKey;
    public GameButton dKey;
    Vector2 mFingerDownPos;
    int mLastFingerId = -2;
    public void Awake()
    {
        ResetJoystick();
        mInstance = this;
        if (target == null)
            target = transform;
        reactiveRange = 145;
        if (Main.Ins != null) {
            if (CombatData.Ins.GMeteorInput == null) {
                U3D.PopupTip("主角的输入控制器还未创建 NGUIJoyStick创建按键映射失败.");
                return;
            }
            wKey.OnPress.AddListener(() => { CombatData.Ins.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyW); });
            sKey.OnPress.AddListener(() => { CombatData.Ins.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyS); });
            aKey.OnPress.AddListener(() => { CombatData.Ins.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyA); });
            dKey.OnPress.AddListener(() => { CombatData.Ins.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyD); });

            wKey.OnRelease.AddListener(() => { CombatData.Ins.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyW); });
            sKey.OnRelease.AddListener(() => { CombatData.Ins.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyS); });
            aKey.OnRelease.AddListener(() => { CombatData.Ins.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyA); });
            dKey.OnRelease.AddListener(() => { CombatData.Ins.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyD); });
        }
    }

    public void OnDestroy()
    {
        mInstance = null;
    }

    Vector2 fixAnchor = new Vector2(390, 340);
    public void SetAnchor(Vector2 anchor)
    {
        float joyScale = GameStateMgr.Ins.gameStatus.JoyScale;
        transform.parent.GetComponent<RectTransform>().localScale = new Vector3(joyScale, joyScale, 1);
        fixAnchor = anchor;
        transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(anchor.x, anchor.y);
    }

	//Vector2 mClickPos = Vector2.zero;
	//public float mShowTime = 0.1f;
	bool isPress = false;

	//Vector2 leftDown = UIHelper.ScreenPointToUIPoint(new Vector2(0, 0));
	//Vector2 leftUp = UIHelper.ScreenPointToUIPoint(new Vector2(0, Screen.height));

    //void OnPress(bool pressed)
    //{
    //    if (enabled && gameObject.activeSelf && target != null)
    //    {
    //        isPress = mJoyPressed = pressed;
    //        if (pressed)
    //        {
    //            //Debug.Log("NGUIJoystick OnPress");
    //            JoyCollider.radius = 200;
    //            //background.gameObject.SetActive(true);

    //            Vector2 curPos = UICamera.currentTouch.pos;
    //            mClickPos = UIHelper.ScreenPointToUIPoint(curPos);
    //            if (mLastFingerId == -2 || mLastFingerId != UICamera.currentTouchID)
    //            {
    //                mLastFingerId = UICamera.currentTouchID;
    //                mFingerDownPos = curPos;
    //                if (mClickPos.x < 0)
    //                {
    //                    float PosX;
    //                    float PosY;
    //                    PosX = leftDown.x + 237 * 1.3f / 2;
    //                    if (mClickPos.x < PosX)
    //                    {
    //                        mClickPos.x = PosX;
    //                    }

    //                    PosY = leftUp.y - 237 * 1.3f / 2;
    //                    if (mClickPos.y > PosY)
    //                    {
    //                        mClickPos.y = PosY;
    //                    }

    //                    PosY = leftDown.y + 237 * 1.3f / 2;
    //                    if (mClickPos.y < PosY)
    //                    {
    //                        mClickPos.y = PosY;
    //                    }

    //                    target.localPosition = new Vector3(mClickPos.x, mClickPos.y, target.parent.localPosition.z);
    //                    mClickPos = UIHelper.ScreenPointToUIPoint(curPos);
    //                }
    //            }
    //            OnDrag(Vector2.zero);
    //        }
    //        else
    //        {
    //            ResetJoystick();
    //        }
    //    }
    //}

    //消息由于NGUI CAMERA触发，点击第一次的时候进一次，后面会由NGUI相机继续调
    //public void OnDrag(Vector2 delta)
    //{
    //    if (enabled && gameObject.activeSelf && target != null)
    //    {
    //        if (mLastFingerId == UICamera.currentTouchID)
    //        {
    //            Vector2 touchPos = UICamera.currentTouch.pos - mFingerDownPos;

    //            //if (touchPos.sqrMagnitude > reactiveRange * reactiveRange) { }
    //            //else
    //            //    ClearKeyStack();

    //            if (touchPos.sqrMagnitude > direction * direction)
    //            {
    //                touchPos.Normalize();
    //                touchPos *= direction;
    //            }

    //            float deltax = touchPos.x / (direction / 2);
    //            float deltay = touchPos.y / (direction / 2);
    //            mDelta = new Vector2(deltax, deltay) * UIHelper.Aspect;

    //            target.localPosition = new Vector3(touchPos.x, touchPos.y, target.localPosition.z) * UIHelper.Aspect;
    //        }
    //        if (mClickPos.x >= 0)
    //            mDelta = Vector2.zero;
    //    }
    //}

    public void ResetJoystick()
    {
        JoyCollider.radius = 55;
        target.localPosition = Vector3.zero;
        mDelta = Vector2.zero;
        mLastFingerId = -2;
        mJoyPressed = false;
        //ClearKeyStack();
    }

    public void Lock(bool state)
    {
        wKey.enabled = !state;
        sKey.enabled = !state;
        aKey.enabled = !state;
        dKey.enabled = !state;
    }
}
