using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JoyAdjust : MonoBehaviour {
    public RectTransform Target;
    public Vector2 vecPos;
    public Text posLabel;
    Vector2 mClickPos = Vector2.zero;
    void Awake()
    {
        Target.anchoredPosition = GameData.Instance.gameStatus.JoyAnchor;
        posLabel.text = string.Format("{0} {1}", GameData.Instance.gameStatus.JoyAnchor.x, GameData.Instance.gameStatus.JoyAnchor.y);
    }

    Vector2 mFingerDownPos;
    int mLastFingerId = -2;
    bool isPress = false;
    Vector2 leftDown = UIHelper.ScreenPointToUIPoint(new Vector2(0, 0));
    Vector2 leftUp = UIHelper.ScreenPointToUIPoint(new Vector2(0, Screen.height));
    void OnPress(bool pressed)
    {
        if (enabled && gameObject.activeSelf && Target != null)
        {
            isPress = pressed;
            if (pressed)
            {
                Vector2 curPos = UICamera.currentTouch.pos;
                Target.anchoredPosition = new Vector3(curPos.x, curPos.y);
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

                        
                        //mClickPos = UIHelper.ScreenPointToUIPoint(curPos);
                    }
                }
                OnDrag(Vector2.zero);
            }
            else
            {

            }
        }
    }

    public void OnDrag(Vector2 delta)
    {
        if (enabled && gameObject.activeSelf && Target != null)
        {
            if (mLastFingerId == UICamera.currentTouchID)
            {
                //Vector2 touchPos = UICamera.currentTouch.pos - mFingerDownPos;
                Target.anchoredPosition = new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y) * UIHelper.WorldToScreenModify;
                GameData.Instance.gameStatus.JoyAnchor = new Vector2(Target.anchoredPosition.x, Target.anchoredPosition.y);
                posLabel.text = string.Format("{0} {1}", GameData.Instance.gameStatus.JoyAnchor.x, GameData.Instance.gameStatus.JoyAnchor.y);
                if (NGUIJoystick.instance)
                    NGUIJoystick.instance.SetAnchor(GameData.Instance.gameStatus.JoyAnchor);
            }
        }
    }
}
