using System.Collections;
using System.Collections.Generic;

using UnityEngine;

//手动控制相机的位置-录像播放时，如果使用自由相机，则可以用此控制相机的XZ坐标
public class CameraController : MonoBehaviour {
    public SphereCollider JoyCollider;
    public Transform target;//整个摇杆
    public Transform joystick;//摇杆球
    public float direction = 180f;//限定轴最远可以离中心多少
    public float time = 3f;//显示持续多久消失
    public static CameraController Ins;
    Vector2 fixAnchor = new Vector2(390, 340);
    Vector2 mFingerDownPos;
    Vector2 mClickPos = Vector2.zero;
    Vector2 mDelta = Vector2.zero;

    Vector2 leftDown = UIHelper.ScreenPointToUIPoint(new Vector2(0, 0));
    Vector2 leftUp = UIHelper.ScreenPointToUIPoint(new Vector2(0, Screen.height));
    bool show = false;
    private void Awake() {
        Ins = this;
        UIHelper.InitCanvas(GameObject.Find("Canvas").GetComponent<Canvas>());
    }

    private void OnDestroy() {
        Ins = null;
    }
    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (!mJoyPressed) {
            if (show) {
                time -= Time.deltaTime;
                if (time < 0) {
                    time = 3;
                    show = false;
                    target.gameObject.SetActive(false);
                }
            }
        }
    }

    //点击了碰撞盒后,在手指处出现UI，后续移动，只改变摇杆球
    public bool mJoyPressed = false;
    public void OnMouseDown() {
        if (enabled && gameObject.activeSelf && target != null) {
            mJoyPressed = true;
            if (mJoyPressed) {
                target.gameObject.SetActive(true);
                show = true;
                Debug.Log("OnMouseDown");
                Vector2 curPos = Input.mousePosition;
                mClickPos = UIHelper.ScreenPointToUIPoint(curPos);
                mFingerDownPos = curPos;
                if (mClickPos.x < 0) {
                    float PosX;
                    float PosY;
                    PosX = leftDown.x + 237 * 1.3f / 2;
                    if (mClickPos.x < PosX) {
                        mClickPos.x = PosX;
                    }

                    PosY = leftUp.y - 237 * 1.3f / 2;
                    if (mClickPos.y > PosY) {
                        mClickPos.y = PosY;
                    }

                    PosY = leftDown.y + 237 * 1.3f / 2;
                    if (mClickPos.y < PosY) {
                        mClickPos.y = PosY;
                    }

                    joystick.localPosition = new Vector3(mClickPos.x, mClickPos.y, target.parent.localPosition.z);
                }
            } else {
                ResetJoystick();
            }
        }
    }

    private void OnMouseUp() {
        mJoyPressed = false;
    }

    private void OnMouseDrag() {
        Debug.Log("OnMouseDrag");
        if (enabled && gameObject.activeSelf && target != null) {
            Vector2 touchPos = (Vector2)Input.mousePosition - mFingerDownPos;
            if (touchPos.sqrMagnitude > direction * direction) {
                touchPos.Normalize();
                touchPos *= direction;
            }

            float deltax = touchPos.x / (direction / 2);
            float deltay = touchPos.y / (direction / 2);
            mDelta = new Vector2(deltax, deltay) * UIHelper.Aspect;
            joystick.localPosition = new Vector3(touchPos.x, touchPos.y, target.localPosition.z) * UIHelper.Aspect;
        }
    }

    public void SetAnchor(Vector2 anchor) {
        fixAnchor = anchor;
        transform.parent.GetComponent<RectTransform>().anchoredPosition = anchor;
    }

    public void ResetJoystick() {
        joystick.localPosition = Vector3.zero;
        target.gameObject.SetActive(false);
        mDelta = Vector2.zero;
        mJoyPressed = false;
    }
}
