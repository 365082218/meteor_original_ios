using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class NGUICameraJoystick : NetBehaviour
{
    static NGUICameraJoystick mInstance = null;
    public static NGUICameraJoystick Ins { get { return mInstance; } }

    static bool mPressed = false;
    static public bool Pressed { get { return mPressed; } }
    Vector2 mFingerDownPos;
    //public Image CameraF;//摄像机
    //public Image CameraI;//背景
    int mLastFingerId = -2;
    BoxCollider[] touchBoxes;
    //public Quaternion lastMeteorUnit = Quaternion.identity;
    private new void Awake()
    {
        base.Awake();
        mInstance = this;
        touchBoxes = GetComponents<BoxCollider>();
        ResetJoystick();
    }
    public Vector2 deltaLast = Vector2.zero;
    private new void OnDestroy()
    {
        base.OnDestroy();
        ResetJoystick();
        mInstance = null;
    }

    public override void NetUpdate()
	{
        if (isPress)
        {
            mPressTime += FrameReplay.deltaTime;
        }
        else
            mPressTime = 0.0f;
	}

	float mPressTime = 0.0f;
	//Vector2 mClickPos = Vector2.zero;
	public float mShowTime = 0.1f;
	bool isPress = false;
    void OnPress(bool pressed)
    {
        if (CombatData.Ins.PauseAll)
            return;
        if (enabled && gameObject.activeSelf)
        {
            if (Main.Ins.LocalPlayer == null || Main.Ins.LocalPlayer.Dead)
                return;
            if (pressed)
            {
                //CameraI.color = new Color(1, 1, 1, 0.1f);
                isPress = mPressed = pressed;
                //print("OnPress");
                //lastMeteorUnit = MeteorManager.Instance.LocalPlayer.transform.rotation;
                //MeteorManager.Instance.LocalPlayer.OnCameraRotateStart();
                Vector2 curPos = UICamera.currentTouch.pos;
                //不允许后面触碰顶替前者触碰，必须是 按下，拖拽，抬起，而不可以是
                if (mLastFingerId == -2)
                {
                    mLastFingerId = UICamera.currentTouchID;
					mFingerDownPos = curPos;
                }
				mPressTime = 0.0f;
                //OnDrag(Vector2.zero);
            }
            else
            {
                if (mLastFingerId == UICamera.currentTouchID)
                {
                    isPress = mPressed = false;
                    ResetJoystick();
                }
                //Debug.LogError("release");
            }
        }
    }

    public void OnDrag(Vector2 delta)
    {
        if (Main.Ins.LocalPlayer == null || Main.Ins.LocalPlayer.Dead)
            return;
        if (isPress && enabled && gameObject.activeSelf)
		{
            //float angle = 0.0f;
            if (mLastFingerId == UICamera.currentTouchID)
            {
                //Vector2 touchPos = UICamera.currentTouch.pos - mFingerDownPos;
                deltaLast = delta;
                //Debug.LogError(string.Format("{0}:{1}", deltaLast.x, deltaLast.y));
                mFingerDownPos = UICamera.currentTouch.pos;
            }
            //else
            //{
            //    deltaLast = Vector2.zero;
            //    Debug.LogError("!isPress && enabled && gameObject.activeSelf");
            //}
            //if (MeteorManager.Instance.LocalPlayer.posMng.CanRotateY)
            //    MeteorManager.Instance.LocalPlayer.SetOrientation(Quaternion.Euler(0, angle, 0), lastMeteorUnit);
            //if ()
        }
        else
        {
            deltaLast = Vector2.zero;
            //Debug.LogError("dragEnd");
        }
    }

    public void ResetJoystick()
    {
        //lastMeteorUnit = Quaternion.identity;
        //CameraI.color = new Color(1, 1, 1, 0);
        //mLastFingerId = -2;
        //mPressed = false;
        //deltaLast = Vector2.zero;
        Lock(false);
    }

    public void Lock(bool state)
    {
        for (int i = 0; i < touchBoxes.Length; i++)
            touchBoxes[i].enabled = !state;
        if (!state)
        {
            mLastFingerId = -2;
            mPressed = false;
            deltaLast = Vector2.zero;
        }
    }
}
