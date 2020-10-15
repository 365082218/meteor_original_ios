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
    public int direction = 180;//�޶�����Զ���������Ķ���
    public int reactiveRange = 60;//���Χ��60-100�ڣ����ᴦ�������Χ���ͻἤ����Ӧ�����м���
    //ÿ����ӵ��һ����ջ�������Ҫ�ټ����������ն�ջ����ն�ջ��ǰ���ǣ����뿪�������ڵ��ǿ鷶Χ�������⼤��һ�����������ļ����ᱻ���
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
                U3D.PopupTip("���ǵ������������δ���� NGUIJoyStick��������ӳ��ʧ��.");
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

    //��Ϣ����NGUI CAMERA�����������һ�ε�ʱ���һ�Σ��������NGUI���������
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
