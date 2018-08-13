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
            return (Time.timeScale != 0) ? mDelta.normalized : Vector2.zero;
        }
    }
    public SphereCollider JoyCollider;
	static Vector2 mSlipDelta = Vector2.zero;
	static public Vector2 SlipDelta {get {return mSlipDelta;} set {mSlipDelta = value;} }

    public Transform background;
    public Transform target;
    public float direction = 100f;//限定轴最远可以离中心多少
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
        background.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        mInstance = null;
    }

    void Start()
    {
        if (target == null)
            target = transform;
        direction = 140.0f / UIHelper.WorldToScreenModify;
        reactiveRange = direction * 0.8f;
        wKey.OnPress.AddListener(() => { Global.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyW); JoyCollider.enabled = false; });
        sKey.OnPress.AddListener(() => { Global.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyS); JoyCollider.enabled = false; });
        aKey.OnPress.AddListener(() => { Global.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyA); JoyCollider.enabled = false; });
        dKey.OnPress.AddListener(() => { Global.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyD); JoyCollider.enabled = false; });

        wKey.OnRelease.AddListener(() => { Global.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyW); JoyCollider.enabled = true; });
        sKey.OnRelease.AddListener(() => { Global.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyS); JoyCollider.enabled = true; });
        aKey.OnRelease.AddListener(() => { Global.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyA); JoyCollider.enabled = true; });
        dKey.OnRelease.AddListener(() => { Global.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyD); JoyCollider.enabled = true; });

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

    Vector2 fixAnchor = new Vector2(391, 340);
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
        wKey.gameObject.SetActive(enable);
        sKey.gameObject.SetActive(enable);
        aKey.gameObject.SetActive(enable);
        dKey.gameObject.SetActive(enable);
        wKey.Reset();
        sKey.Reset();
        aKey.Reset();
        dKey.Reset();
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
                background.gameObject.SetActive(true);

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
        if (Global.GMeteorInput != null)
        {
            Global.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyW);
            Global.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyS);
            Global.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyA);
            Global.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyD);
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
                mDelta = new Vector2(deltax, deltay) * UIHelper.UIModify;

                target.localPosition = new Vector3(touchPos.x, touchPos.y, target.localPosition.z) * UIHelper.WorldToScreenModify;
            }
            if (mClickPos.x >= 0)
                mDelta = Vector2.zero;
        }
    }

    public void ResetJoystick()
    {
        JoyCollider.radius = 120;
        target.localPosition = Vector3.zero;
        background.gameObject.SetActive(false);
        if (Global.GMeteorInput != null)
            Global.GMeteorInput.ResetJoy();
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
        JoyCollider.enabled = !state;
        //background.gameObject.SetActive(!state);
    }

    public void SetVectorZ(float z)
    {
        mDelta.y = z * direction;
        target.localPosition = new Vector3(mDelta.x, mDelta.y, target.localPosition.z) * UIHelper.WorldToScreenModify;
        if (mDelta == Vector2.zero)
            target.parent.gameObject.SetActive(false);
        else
            target.parent.gameObject.SetActive(true);
    }

    public void SetVectorX(float x)
    {
        mDelta.x = x * direction;
        target.localPosition = new Vector3(mDelta.x, mDelta.y, target.localPosition.z) * UIHelper.WorldToScreenModify;
        if (mDelta == Vector2.zero)
            target.parent.gameObject.SetActive(false);
        else
            target.parent.gameObject.SetActive(true);
    }
}
