using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class NGUICameraJoystick : MonoBehaviour
{
    static NGUICameraJoystick mInstance = null;
    public static NGUICameraJoystick instance { get { return mInstance; } }

    static bool mPressed = false;
    static public bool Pressed { get { return mPressed; } }

    public float width = 980f;
    Vector2 mFingerDownPos;
    //public Image CameraF;//ÉãÏñ»ú
    //public Image CameraI;//±³¾°
    int mLastFingerId = -2;
    //public Quaternion lastMeteorUnit = Quaternion.identity;
    void Awake()
    {
        ResetJoystick();
        mInstance = this;
    }
    public Vector2 deltaLast = Vector2.zero;
    void OnDestroy()
    {
        ResetJoystick();
        mInstance = null;
    }

    void Start()
    {
        width = width / UIHelper.WorldToScreenModify;
    }

	void Update() 
	{
		if (isPress)
			mPressTime += Time.deltaTime;
		else
			mPressTime = 0.0f;
	}

	float mPressTime = 0.0f;
	//Vector2 mClickPos = Vector2.zero;
	public float mShowTime = 0.1f;
	bool isPress = false;

	Vector2 leftDown = UIHelper.ScreenPointToUIPoint(new Vector2(0, 0));
	Vector2 leftUp = UIHelper.ScreenPointToUIPoint(new Vector2(0, Screen.height));
    void OnPress(bool pressed)
    {
        //if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        //    return;
        if (Global.PauseAll)
            return;
        if (enabled && gameObject.activeSelf)
        {
            if (pressed)
            {
                if (MeteorManager.Instance.LocalPlayer.Dead)
                    return;
                //CameraI.color = new Color(1, 1, 1, 0.1f);
                isPress = mPressed = pressed;
                //print("OnPress");
                //lastMeteorUnit = MeteorManager.Instance.LocalPlayer.transform.rotation;
                //MeteorManager.Instance.LocalPlayer.OnCameraRotateStart();
                Vector2 curPos = UICamera.currentTouch.pos;
                if (mLastFingerId == -2 || mLastFingerId != UICamera.currentTouchID)
                {
                    mLastFingerId = UICamera.currentTouchID;
					mFingerDownPos = curPos;
                }
				mPressTime = 0.0f;
                OnDrag(Vector2.zero);
            }
            else
            {
                isPress = mPressed = false;
                ResetJoystick();
            }
        }
    }

    public void OnDrag(Vector2 delta)
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            return;
        if (MeteorManager.Instance.LocalPlayer.Dead)
            return;
		if (isPress && enabled && gameObject.activeSelf)
		{
            //float angle = 0.0f;
            if (mLastFingerId == UICamera.currentTouchID)
			{
				Vector2 touchPos = UICamera.currentTouch.pos - mFingerDownPos;
                deltaLast = delta;
                mFingerDownPos = UICamera.currentTouch.pos;
                //if (touchPos.x > width)
                //    touchPos.x = width;
                //else if (touchPos.x < -width)
                //    touchPos.x = -width;
                //angle = touchPos.x / width * 144.0f;
			}
            //if (MeteorManager.Instance.LocalPlayer.posMng.CanRotateY)
            //    MeteorManager.Instance.LocalPlayer.SetOrientation(Quaternion.Euler(0, angle, 0), lastMeteorUnit);
            //if ()
        }
    }

    public void ResetJoystick()
    {
        //lastMeteorUnit = Quaternion.identity;
        //CameraI.color = new Color(1, 1, 1, 0);
        mLastFingerId = -2;
        mPressed = false;
        deltaLast = Vector2.zero;
        Lock(false);
    }

    public void Lock(bool state)
    {
        GetComponent<BoxCollider>().enabled = !state;
    }
}
