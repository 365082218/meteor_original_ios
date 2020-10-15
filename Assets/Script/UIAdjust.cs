using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAdjust : MonoBehaviour {
    public RectTransform Target;
    public Vector2 vecPos;
    public Text posLabel;
    public BoxCollider box;
    Vector2 mClickPos = Vector2.zero;
    public int UIIndex = 0;//0指代调整摇杆的位置 1-所有UIBUTTON的位置
    void Awake()
    {
        Target.anchoredPosition = GameStateMgr.Ins.gameStatus.JoyAnchor;
        //posLabel.text = string.Format("{0} {1}", GameStateMgr.Ins.gameStatus.JoyAnchor.x, GameStateMgr.Ins.gameStatus.JoyAnchor.y);
    }

    //Vector2 mFingerDownPos;
    int mLastFingerId = -2;
    //bool isPress = false;
    //Vector2 leftDown = UIHelper.ScreenPointToUIPoint(new Vector2(0, 0));
    //Vector2 leftUp = UIHelper.ScreenPointToUIPoint(new Vector2(0, Screen.height));

    public void OnChangeTarget(int uiIndex, GameButton active)
    {
        RectTransform rect = active == null ? null : active.GetComponent<RectTransform>();
        if (Target != null)
            Target.GetComponent<CanvasGroup>().alpha = GameStateMgr.Ins.gameStatus.UIAlpha;
        Target = rect;
        if (Target != null)
            Target.GetComponent<CanvasGroup>().alpha = 1.0f;
        UIIndex = uiIndex;
        box.enabled = Target != null;
        if (Target == null)
            vecOffset.x = vecOffset.y = 0;
    }

    Vector2 vecOffset = new Vector2(0, 0);
    void OnPress(bool pressed)
    {
        if (enabled && gameObject.activeSelf && Target != null)
        {
            //isPress = pressed;
            if (pressed)
            {
                Vector2 curPos = UICamera.currentTouch.pos;
                vecOffset = curPos - Target.anchoredPosition;
                //Target.anchoredPosition = new Vector3(curPos.x, curPos.y);
                //mClickPos = UIHelper.ScreenPointToUIPoint(curPos);
                if (mLastFingerId == -2 || mLastFingerId != UICamera.currentTouchID)
                {
                    mLastFingerId = UICamera.currentTouchID;
                    //mFingerDownPos = curPos;
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
            //Debug.Log(string.Format("currentTouch.pos {0}-{1}", UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y));
            if (mLastFingerId == UICamera.currentTouchID)
            {
                Target.anchoredPosition = UICamera.currentTouch.pos - vecOffset;
                if (UIIndex == 0)
                {
                    GameStateMgr.Ins.gameStatus.JoyAnchor.x = Mathf.FloorToInt(Target.anchoredPosition.x);
                    GameStateMgr.Ins.gameStatus.JoyAnchor.y = Mathf.FloorToInt(Target.anchoredPosition.y);
                }
                else
                {
                    //左下角是0，0右上方越来越大.
                    Vector2 vec = GameStateMgr.Ins.gameStatus.UIAnchor[UIIndex - 1];
                    vec.x = Mathf.FloorToInt(Target.anchoredPosition.x);
                    vec.y = Mathf.FloorToInt(Target.anchoredPosition.y);
                    GameStateMgr.Ins.gameStatus.UIAnchor[UIIndex - 1] = vec;
                    GameStateMgr.Ins.gameStatus.HasUIAnchor[UIIndex - 1] = true;
                }
                //posLabel.text = string.Format("{0} {1}", Target.anchoredPosition.x, Target.anchoredPosition.y);
            }
        }
    }
}
