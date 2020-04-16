using UnityEngine;
using System.Collections;

public class NGUIJoystick : MonoBehaviour
{
    static NGUIJoystick mInstance = null;
    public static NGUIJoystick instance { get { return mInstance; } }

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
            return Time.timeScale * mDelta.normalized;
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
	static Vector2 mSlipDelta = Vector2.zero;
	static public Vector2 SlipDelta {get {return mSlipDelta;} set {mSlipDelta = value;} }

    //public Transform background;
    public Transform target;
    public float direction = 180f;//限定轴最远可以离中心多少
    public float reactiveRange = 60f;//激活范围在60-100内，当轴处于这个范围，就会激活相应的连招键。
    //每个轴拥有一个堆栈，激活后，要再激活，必须先清空堆栈，清空堆栈的前提是，轴离开按键处于的那块范围。或任意激活一个键，其他的键都会被清空
    public GameButton wKey;
    public GameButton sKey;
    public GameButton aKey;
    public GameButton dKey;
    Vector2 mFingerDownPos;
    int mLastFingerId = -2;
    void Awake()
    {
        ResetJoystick();
        mInstance = this;
        //background.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        mInstance = null;
    }

    void Start()
    {
        if (target == null)
            target = transform;
        reactiveRange = 145.0f;
        if (Main.Ins != null)
        {
            wKey.OnPress.AddListener(() => { Main.Ins.CombatData.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyW); JoyCollider.enabled = false; });
            sKey.OnPress.AddListener(() => { Main.Ins.CombatData.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyS); JoyCollider.enabled = false; });
            aKey.OnPress.AddListener(() => { Main.Ins.CombatData.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyA); JoyCollider.enabled = false; });
            dKey.OnPress.AddListener(() => { Main.Ins.CombatData.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyD); JoyCollider.enabled = false; });

            wKey.OnRelease.AddListener(() => { Main.Ins.CombatData.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyW); if (!Main.Ins.GameStateMgr.gameStatus.DisableJoystick) { JoyCollider.enabled = true; } });
            sKey.OnRelease.AddListener(() => { Main.Ins.CombatData.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyS); if (!Main.Ins.GameStateMgr.gameStatus.DisableJoystick) { JoyCollider.enabled = true; } });
            aKey.OnRelease.AddListener(() => { Main.Ins.CombatData.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyA); if (!Main.Ins.GameStateMgr.gameStatus.DisableJoystick) { JoyCollider.enabled = true; } });
            dKey.OnRelease.AddListener(() => { Main.Ins.CombatData.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyD); if (!Main.Ins.GameStateMgr.gameStatus.DisableJoystick) { JoyCollider.enabled = true; } });
        }
        //if (wKey != null)
        //    wKey.SetKeyActive(false);
        //if (sKey != null)
        //    sKey.SetKeyActive(false);
        //if (aKey != null)
        //    aKey.SetKeyActive(false);
        //if (dKey != null)
        //    dKey.SetKeyActive(false);
    }

	void Update() 
	{
		//if (slipCD >= 0)
		//	slipCD -= Time.deltaTime;
		//else
		//	SlipTimes = 0;

		if (isPress)
		{
			mPressTime += Time.deltaTime;

//			if (mPressTime >= mShowTime && mClickPos.x < 0)
//			{
//				target.parent.gameObject.SetActive(true);
//				target.parent.GetComponent<UIPanel>().alpha = 1f;
//				target.parent.localPosition = new Vector3(mClickPos.x, mClickPos.y, target.parent.localPosition.z);
//			}
		}
		else
		{
			mPressTime = 0.0f;
		}
	}

    Vector2 fixAnchor = new Vector2(390, 340);
    public void SetAnchor(Vector2 anchor)
    {
        fixAnchor = anchor;
        transform.parent.GetComponent<RectTransform>().anchoredPosition = anchor;
    }

	float mPressTime = 0.0f;
	Vector2 mClickPos = Vector2.zero;
	public float mShowTime = 0.1f;
	bool isPress = false;

	Vector2 leftDown = UIHelper.ScreenPointToUIPoint(new Vector2(0, 0));
	Vector2 leftUp = UIHelper.ScreenPointToUIPoint(new Vector2(0, Screen.height));
    void EnableArrowButton(bool enable)
    {
        //wKey.gameObject.SetActive(enable);
        //sKey.gameObject.SetActive(enable);
        //aKey.gameObject.SetActive(enable);
        //dKey.gameObject.SetActive(enable);
        //wKey.Reset();
        //sKey.Reset();
        //aKey.Reset();
        //dKey.Reset();
    }

    void OnPress(bool pressed)
    {
        if (enabled && gameObject.activeSelf && target != null)
        {
            isPress = mJoyPressed = pressed;
            if (pressed)
            {
                //Debug.Log("NGUIJoystick OnPress");
                EnableArrowButton(false);
                JoyCollider.radius = 200;
                //background.gameObject.SetActive(true);

                Vector2 curPos = UICamera.currentTouch.pos;
                mClickPos = UIHelper.ScreenPointToUIPoint(curPos);
                if (mLastFingerId == -2 || mLastFingerId != UICamera.currentTouchID)
                {
                    mLastFingerId = UICamera.currentTouchID;
                    mFingerDownPos = curPos;
                    if (mClickPos.x < 0)
                    {
                        float PosX;
                        float PosY;
                        PosX = leftDown.x + 237 * 1.3f / 2;
                        if (mClickPos.x < PosX)
                        {
                            mClickPos.x = PosX;
                        }

                        PosY = leftUp.y - 237 * 1.3f / 2;
                        if (mClickPos.y > PosY)
                        {
                            mClickPos.y = PosY;
                        }

                        PosY = leftDown.y + 237 * 1.3f / 2;
                        if (mClickPos.y < PosY)
                        {
                            mClickPos.y = PosY;
                        }

                        target.localPosition = new Vector3(mClickPos.x, mClickPos.y, target.parent.localPosition.z);
                        mClickPos = UIHelper.ScreenPointToUIPoint(curPos);
                    }
                }
                mPressTime = 0.0f;
                OnDrag(Vector2.zero);
            }
            else
            {
                ResetJoystick();
            }
        }
    }


    void ClearKeyStack()
    {
        //wKey.SetKeyActive(false);
        //sKey.SetKeyActive(false);
        //aKey.SetKeyActive(false);
        //dKey.SetKeyActive(false);
        if (Main.Ins != null && Main.Ins.CombatData.GMeteorInput != null)
        {
            Main.Ins.CombatData.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyW);
            Main.Ins.CombatData.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyS);
            Main.Ins.CombatData.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyA);
            Main.Ins.CombatData.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyD);
        }
    }

    public void RecalcKeyActive(Vector2 diff)
    {
        //特殊的X无法作为0被其他数除以,把圆周看作有8个方向组成的，每个方向45度，每个轴控制其二侧的22.5度
        //if (diff.x == 0)
        //{
        //    if (diff.y < 0)
        //    {
        //        if (Global.GMeteorInput != null)
        //        {
        //            if (!Global.GMeteorInput.IsKeyOnActive(EKeyList.KL_KeyS))
        //            {
        //                sKey.SetKeyActive(true);
        //                Global.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyS);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        if (Global.GMeteorInput != null)
        //        {
        //            if (!Global.GMeteorInput.IsKeyOnActive(EKeyList.KL_KeyW))
        //            {
        //                wKey.SetKeyActive(true);
        //                Global.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyW);
        //            }
        //        }
        //    }
        //}
        //else
        //if (diff.x > 0 && diff.y > 0)
        //{
        //    //1象限取值0-90
        //    float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        //    Debug.Log("x > 0 y > 0 angle = " + angle);
        //    if (angle > 67.5f)
        //    {
        //        //查看堆栈是否被清空，若无清空表明上次点击W按键后，还没弹起，不能重复按一个键不放
        //        //这个堆栈数据，放在当前操纵角色的输入器内。
        //        if (Global.GMeteorInput != null)
        //        {
        //            if (!Global.GMeteorInput.IsKeyOnActive(EKeyList.KL_KeyW))
        //            {
        //                wKey.SetKeyActive(true);
        //                Global.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyW);
        //            }
        //        }
        //    }
        //    else if (angle < 22.5f)
        //    {
        //        if (Global.GMeteorInput != null)
        //        {
        //            if (!Global.GMeteorInput.IsKeyOnActive(EKeyList.KL_KeyD))
        //            {
        //                dKey.SetKeyActive(true);
        //                Global.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyD);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        if (Global.GMeteorInput != null)
        //        {
        //            wKey.SetKeyActive(false);
        //            dKey.SetKeyActive(false);
        //            Global.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyW);
        //            Global.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyD);
        //        }
        //    }
        //}
        //else if (diff.x < 0 && diff.y > 0)
        //{
        //    float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        //    Debug.Log("x < 0 y > 0 angle = " + angle);
        //    if (angle < 112.5f)
        //    {
        //        if (!Global.GMeteorInput.IsKeyOnActive(EKeyList.KL_KeyW))
        //        {
        //            wKey.SetKeyActive(true);
        //            Global.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyW);
        //        }
        //    }
        //    else if (angle > 157.5f)
        //    {
        //        if (!Global.GMeteorInput.IsKeyOnActive(EKeyList.KL_KeyA))
        //        {
        //            aKey.SetKeyActive(true);
        //            Global.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyA);
        //        }
        //    }
        //    else
        //    {
        //        if (Global.GMeteorInput != null)
        //        {
        //            wKey.SetKeyActive(false);
        //            aKey.SetKeyActive(false);
        //            Global.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyW);
        //            Global.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyA);
        //        }
        //    }
        //}
        //else if (diff.x < 0 && diff.y < 0)
        //{
        //    float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        //    Debug.Log("x < 0 y < 0 angle = " + angle);
        //    if (angle < -157.5f)
        //    {
        //        if (!Global.GMeteorInput.IsKeyOnActive(EKeyList.KL_KeyA))
        //        {
        //            aKey.SetKeyActive(true);
        //            Global.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyA);
        //        }
        //    }
        //    else if (angle > -112.5f)
        //    {
        //        if (!Global.GMeteorInput.IsKeyOnActive(EKeyList.KL_KeyS))
        //        {
        //            sKey.SetKeyActive(true);
        //            Global.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyS);
        //        }
        //    }
        //    else
        //    {
        //        if (Global.GMeteorInput != null)
        //        {
        //            aKey.SetKeyActive(false);
        //            sKey.SetKeyActive(false);
        //            Global.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyA);
        //            Global.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyS);
        //        }
        //    }
        //}
        //else if (diff.x > 0 && diff.y < 0)
        //{
        //    float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        //    Debug.Log("x > 0 y < 0 angle = " + angle);
        //    if (angle < -67.5f)
        //    {
        //        if (!Global.GMeteorInput.IsKeyOnActive(EKeyList.KL_KeyS))
        //        {
        //            sKey.SetKeyActive(true);
        //            Global.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyS);
        //        }
        //    }
        //    else if (angle > -22.5f)
        //    {
        //        if (!Global.GMeteorInput.IsKeyOnActive(EKeyList.KL_KeyD))
        //        {
        //            dKey.SetKeyActive(true);
        //            Global.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyD);
        //        }
        //    }
        //    else
        //    {
        //        if (Global.GMeteorInput != null)
        //        {
        //            sKey.SetKeyActive(false);
        //            dKey.SetKeyActive(false);
        //            Global.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyS);
        //            Global.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyD);
        //        }
        //    }
        //}
    }

    //消息由于NGUI CAMERA触发，点击第一次的时候进一次，后面会由NGUI相机继续调
    public void OnDrag(Vector2 delta)
    {
        if (enabled && gameObject.activeSelf && target != null)
        {
            if (mLastFingerId == UICamera.currentTouchID)
            {
                Vector2 touchPos = UICamera.currentTouch.pos - mFingerDownPos;

                if (touchPos.sqrMagnitude > reactiveRange * reactiveRange)
                    RecalcKeyActive(touchPos);
                else
                    ClearKeyStack();

                if (touchPos.sqrMagnitude > direction * direction)
                {
                    touchPos.Normalize();
                    touchPos *= direction;
                }

                float deltax = touchPos.x / (direction / 2);
                float deltay = touchPos.y / (direction / 2);
                mDelta = new Vector2(deltax, deltay) * UIHelper.Aspect;

                target.localPosition = new Vector3(touchPos.x, touchPos.y, target.localPosition.z) * UIHelper.Aspect;
            }
            if (mClickPos.x >= 0)
                mDelta = Vector2.zero;
        }
    }

    public void ResetJoystick()
    {
        JoyCollider.radius = 55;
        target.localPosition = Vector3.zero;
        //background.gameObject.SetActive(false);
        mDelta = Vector2.zero;
        mLastFingerId = -2;
        mJoyPressed = false;
        EnableArrowButton(true);
        ClearKeyStack();
    }

    public void Lock(bool state)
    {
        wKey.enabled = !state;
        sKey.enabled = !state;
        aKey.enabled = !state;
        dKey.enabled = !state;
        if (!Main.Ins.GameStateMgr.gameStatus.DisableJoystick)
            JoyCollider.enabled = !state;
        //background.gameObject.SetActive(!state);
    }
}
