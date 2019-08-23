//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using Idevgame.Util;
using UnityEngine;
using UnityEngine.UI;
namespace Idevgame.GameState.DialogState {
    public abstract class PersistState
    {
        public MonoBehaviour Owner;
        public virtual void OnStateEnter()
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
    }

    //persistDialog-自带画布，持久化，加载场景不删除这些对象.需要手动控制.
    public abstract class PersistDialog<T> : PersistState where T :Dialog
    {
        protected GameObject Dialog;
        protected GameObject mRootUI;
        public static bool Exist() { return DialogController == null; }
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
        public override void OnStateEnter()
        {
            LoadGui();
            GameObject.DontDestroyOnLoad(Dialog);
            if (DialogController != null)
            {
                DialogController.OnDialogStateEnter(this, null, null);
            }

        }
        public override void OnStateExit()
        {
            DialogController = null;
            if (UnloadGuiOnExit)
            {
                UnloadGuiOnExit = false;
                UnloadGui();
            }
        }

        protected void UnloadGui()
        {
            UnityEngine.Object.Destroy(Dialog);
            Dialog = null;
        }

        public virtual void LoadGui()
        {
            if (Dialog != null)
            {
                return;
            }

            UnloadGuiOnExit = true;

#if UNITY_2017 || UNITY_5_5
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
                    Dialog.layer = mRootUI.transform.gameObject.layer;
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
        }
    }

    public abstract class CommonDialogState<T> : BaseDialogState where T : Dialog {

        protected GameObject Dialog;
        protected GameObject mRootUI;
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

        public CommonDialogState(MainDialogStateManager stateManager) : base(stateManager) {

        }

        protected BaseDialogState previousS;
        public override void OnEnter(BaseDialogState previousState, object data) {
            previousS = previousState;
            LoadGui();
            if (DialogController != null) {
                DialogController.OnDialogStateEnter(this, previousState, data);
            }
        }

        public override void OnPreExit(BaseDialogState nextState, object data) {
            base.OnPreExit(nextState, data);
            //MainUI.Instance.NguiCollider.enabled = false;
            //MainSceneGuiController.Instance.OnDialogExit();
        }

        public override void OnExit(BaseDialogState nextState, object data) {
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

#if UNITY_2017 || UNITY_5_5
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
                    Dialog.layer = mRootUI.transform.gameObject.layer;
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
    }
}