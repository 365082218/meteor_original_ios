//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using Idevgame.Util;
using UnityEngine;
using UnityEngine.UI;
using Assets.Code.Idevgame.Common.Util;

namespace Idevgame.GameState.DialogState {
    public abstract class PersistState
    {
        protected PersistState() {
            stateMgr = PersistDialogMgr.Ins;
        }
        protected PersistDialogMgr stateMgr;
        public MonoBehaviour Owner;
        public void Open(object data = null) {
            stateMgr.EnterState(this, data);
        }
        public void Close() {
            stateMgr.ExitState(this);
        }

        public virtual void OnStateEnter(object data = null)
        {
            
        }

        public virtual void OnStateExit()
        {

        }
        protected virtual bool Use3DCanvas()
        {
            return false;
        }

        protected virtual bool CanvasMode()
        {
            return false;
        }

        protected virtual float GetZ()
        {
            return 0;
        }

        public virtual void OnUpdate()
        {

        }

        public virtual void OnLateUpdate()
        {

        }

        public void WaitExit(float exit) {
            if (timerClose != null) {
                timerClose.Stop();
                timerClose = null;
            }
            timerClose = Timer.once(exit, TimeHandler);
        }

        void TimeHandler() {
            Close();
            timerClose = null;
        }
        Timer timerClose;
    }

    //persistDialog-自带画布，持久化，加载场景不删除这些对象.需要手动控制.
    public abstract class PersistDialog<T> : PersistState where T :Dialog
    {
        public PersistDialog():base(){
            State = this;
        }
        protected GameObject Dialog;
        protected GameObject mRootUI;
        public static bool Exist() { return DialogController != null; }
        public static PersistState State;
        public static T Instance
        {
            get { return DialogController; }
        }
        protected static T DialogController;
        public bool UnloadGuiOnExit;
        public abstract string DialogName { get; }
        protected string DialogResorucePath
        {
            get
            {
                return "UI/Dialogs/" + DialogName;
            }
        }
        public override void OnStateEnter(object data = null)
        {
            BaseDialogState.InitRoot();
            LoadGui();
            if (DialogController != null) {
                DialogController.OnDialogStateEnter(this, null, data);
            }
        }
        public override void OnStateExit()
        {
            if (DialogController != null)
                DialogController.OnDialogStateExit();
            DialogController = null;
            if (UnloadGuiOnExit)
            {
                UnloadGuiOnExit = false;
                UnloadGui();
            }
        }

        protected virtual void UnloadGui()
        {
            UnityEngine.Object.Destroy(Dialog);
            Dialog = null;
        }

        public virtual void LoadGui()
        {
            if (Dialog != null) {
                return;
            }

            UnloadGuiOnExit = true;

#if UNITY_2017 || UNITY_5_5 || UNITY_5_6 || UNITY_2020
            if (Use3DCanvas())
            {
                if (mRootUI == null)
                    mRootUI = GameObject.Find("3dCanvas");
                if (mRootUI == null)
                {
                    mRootUI = GameObject.Instantiate(Resources.Load<GameObject>("3dCanvas"), Vector3.zero, Quaternion.identity);
                    mRootUI.name = "3dCanvas";
                }
            }
            if (mRootUI == null)
                mRootUI = GameObject.Find("Canvas");
            if (mRootUI != null)
            {
                if (!CanvasMode())
                    Dialog = GameObject.Instantiate(Resources.Load<GameObject>(DialogResorucePath), Vector3.zero, Quaternion.identity, mRootUI.transform) as GameObject;
                else
                    Dialog = GameObject.Instantiate(Resources.Load<GameObject>(DialogResorucePath));
            }
            else
                Dialog = GameObject.Instantiate(Resources.Load<GameObject>(DialogResorucePath));
            if (mRootUI != null)
            {
                if (!CanvasMode())
                {
                    Dialog.transform.SetParent(mRootUI.transform);
                    Dialog.transform.localScale = Vector3.one;
                    Dialog.transform.localRotation = Quaternion.identity;
                    //Dialog.layer = mRootUI.transform.gameObject.layer;
                }
                RectTransform rectTran = Dialog.GetComponent<RectTransform>();
                if (rectTran != null && rectTran.anchorMin == Vector2.zero && rectTran.anchorMax == Vector2.one)
                {
                    if (rectTran.rect.width == 0 && rectTran.rect.height == 0)
                        rectTran.sizeDelta = new Vector2(0, 0);

                }
                if (rectTran != null)
                    rectTran.anchoredPosition3D = new Vector3(0, 0, GetZ());
            }
#else
            mRootUI = GameObject.Find("Anchor");
            WndObject.transform.parent = mRootUI.transform;
#endif
            Dialog.transform.localScale = Vector3.one;
            Dialog.name = DialogName;
            DialogController = Dialog.GetComponent<T>();
            if (DialogController == null)
                DialogController = Dialog.AddComponent<T>();
        }
    }

    //随时可以创建打开的面板
    public abstract class SimpleDialogController<T> where T:SimpleDialog
    {
        public abstract string DialogName { get; }
        protected GameObject Dialog;
        protected GameObject mRootUI;
        protected string DialogResorucePath
        {
            get
            {
                return "UI/Dialogs/" + DialogName;
            }
        }
        public static bool Exist { get { return Controller != null; } }
        public static T Instance { get { return Controller; } }
        protected static T Controller;
        public void Show()
        {
            LoadGui();
        }

        public void Close()
        {
            if (Dialog != null)
                Dialog.SetActive(false);
        }

        public void Destroy()
        {
            UnloadGui();
        }

        protected void UnloadGui()
        {
            UnityEngine.Object.Destroy(Dialog);
            Dialog = null;
        }

        protected void LoadGui()
        {
            if (Dialog != null)
            {
                return;
            }

#if UNITY_2017 || UNITY_5_5 || UNITY_5_6 || UNITY_2020
            if (Use3DCanvas())
            {
                if (mRootUI == null)
                    mRootUI = GameObject.Find("3dCanvas");
                if (mRootUI == null)
                {
                    mRootUI = GameObject.Instantiate(Resources.Load<GameObject>("3dCanvas"), Vector3.zero, Quaternion.identity);
                    mRootUI.name = "3dCanvas";
                }
            }
            if (mRootUI == null)
                mRootUI = GameObject.Find("Canvas");
            if (mRootUI != null)
            {
                if (!CanvasMode())
                    Dialog = GameObject.Instantiate(Resources.Load<GameObject>(DialogResorucePath), Vector3.zero, Quaternion.identity, mRootUI.transform) as GameObject;
                else
                    Dialog = GameObject.Instantiate(Resources.Load<GameObject>(DialogResorucePath));
            }
            else
                Dialog = GameObject.Instantiate(Resources.Load<GameObject>(DialogResorucePath));
            if (mRootUI != null)
            {
                if (!CanvasMode())
                {
                    Dialog.transform.SetParent(mRootUI.transform);
                    Dialog.transform.localScale = Vector3.one;
                    Dialog.transform.localRotation = Quaternion.identity;
                    //Dialog.layer = mRootUI.transform.gameObject.layer;
                }
                RectTransform rectTran = Dialog.GetComponent<RectTransform>();
                if (rectTran != null && rectTran.anchorMin == Vector2.zero && rectTran.anchorMax == Vector2.one)
                {
                    if (rectTran.rect.width == 0 && rectTran.rect.height == 0)
                        rectTran.sizeDelta = new Vector2(0, 0);

                }
                if (rectTran != null)
                    rectTran.anchoredPosition3D = new Vector3(0, 0, GetZ());
            }
#else
        mRootUI = GameObject.Find("Anchor");
        WndObject.transform.parent = mRootUI.transform;
#endif
            Dialog.transform.localScale = Vector3.one;
            Dialog.name = DialogName;
            Controller = Dialog.GetComponent<T>();
            //若没找到该组件，那么添加一个，在状态进入中，设置所有成员变量与控件的关系.
            if (Controller == null)
                Controller = Dialog.AddComponent<T>();
        }

        protected virtual bool Use3DCanvas()
        {
            return false;
        }

        protected virtual bool CanvasMode()
        {
            return false;
        }

        protected virtual float GetZ()
        {
            return 0;
        }
    }

    public abstract class CommonDialogState<T> : BaseDialogState where T : Dialog {

        protected GameObject Dialog;
        
        public static bool Exist { get { return DialogController != null; } }
        public static T Instance { get { return DialogController; } }
        protected static T DialogController;
        public bool UnloadGuiOnExit;

        protected string DialogResorucePath {
            get {
                return "UI/Dialogs/" + DialogName;
            }
        }

        protected virtual bool UseTopNguiCollider { get { return false; } }

        protected virtual bool ForceUnloadGuiOnExit { get { return false; } }

        public CommonDialogState(BaseDialogStateManager stateManager) : base(stateManager) {

        }

        protected BaseDialogState previousS;
        public override void OnEnter(BaseDialogState previousState, object data) {
            previousS = previousState;
            LoadGui();
            if (DialogController != null) {
                DialogController.OnDialogStateEnter(this, previousState, data);
            }
        }

        public override void OnPreExit(BaseDialogState nextState = null, object data = null) {
            base.OnPreExit(nextState, data);
            //MainUI.Instance.NguiCollider.enabled = false;
            //MainSceneGuiController.Instance.OnDialogExit();
        }

        public override void OnExit(BaseDialogState nextState = null, object data = null) {
            if (DialogController != null)
                DialogController.OnDialogStateExit();
            DialogController = null;
            if (UnloadGuiOnExit) {
                UnloadGuiOnExit = false;
                UnloadGui();
            }
        }



        protected void LoadGui() {
            if (Dialog != null) {
                return;
            }

            UnloadGuiOnExit = true;

#if UNITY_2017 || UNITY_5_5 || UNITY_5_6 || UNITY_2020
            if (Use3DCanvas())
            {
                if (mRootUI == null)
                {
                    mRootUI = GameObject.Instantiate(Resources.Load<GameObject>("3dCanvas"), Vector3.zero, Quaternion.identity);
                    mRootUI.name = "3dCanvas";
                    mCanvasRoot = mRootUI;
                    GameObject.DontDestroyOnLoad(mRootUI);
                }
            }
            InitRoot();
            if (mRootUI != null)
            {
                if (!CanvasMode())
                    Dialog = GameObject.Instantiate(Resources.Load<GameObject>(DialogResorucePath), Vector3.zero, Quaternion.identity, mRootUI.transform) as GameObject;
                else
                    Dialog = GameObject.Instantiate(Resources.Load<GameObject>(DialogResorucePath));
            }
            else
                Dialog = GameObject.Instantiate(Resources.Load<GameObject>(DialogResorucePath));
            if (mRootUI != null)
            {
                if (!CanvasMode())
                {
                    Dialog.transform.SetParent(mCanvasRoot.transform);
                    Dialog.transform.localScale = Vector3.one;
                    Dialog.transform.localRotation = Quaternion.identity;
                    //Dialog.layer = mRootUI.transform.gameObject.layer;
                }
                RectTransform rectTran = Dialog.GetComponent<RectTransform>();
                if (rectTran != null && rectTran.anchorMin == Vector2.zero && rectTran.anchorMax == Vector2.one)
                {
                    if (rectTran.rect.width == 0 && rectTran.rect.height == 0)
                        rectTran.sizeDelta = new Vector2(0, 0);
                }
                if (rectTran != null)
                    rectTran.anchoredPosition3D = new Vector3(GetX(), GetY(), GetZ());
            }
#else
        mRootUI = GameObject.Find("Anchor");
        WndObject.transform.parent = mRootUI.transform;
#endif
            Dialog.transform.localScale = Vector3.one;
            Dialog.name = DialogName;
            DialogController = Dialog.GetComponent<T>();
            //若没找到该组件，那么添加一个，在状态进入中，设置所有成员变量与控件的关系.
            if (DialogController == null)
                DialogController = Dialog.AddComponent<T>();
        }

        protected void UnloadGui() {
            UnityEngine.Object.Destroy(Dialog);
            Dialog = null;
        }

        public override void OnAction(DialogAction dialogAction, object data) {
            switch (dialogAction) {
                case DialogAction.Close:
                case DialogAction.BackButton:
                    ChangeState(null);
                    break;
                case DialogAction.Previous:
                    ChangeState(previousS);
                    break;
                default:
                    break;
            }
        }

        protected virtual bool Use3DCanvas()
        {
            return false;
        }

        protected virtual bool CanvasMode()
        {
            return false;
        }

        protected virtual float GetZ()
        {
            return 0;
        }

        protected virtual float GetX() {
            return 0;
        }

        protected virtual float GetY() {
            return 0;
        }
    }
}