
using Idevgame.GameState.DialogState;
using Idevgame.StateManagement;
using UnityEngine;

namespace Idevgame.GameState.DialogState {
    public abstract class BaseDialogState : StateManager<BaseDialogState,DialogAction>.State {

        public BaseDialogStateManager DialogStateManager { get; private set; }
        protected static GameObject mRootUI;
        protected static GameObject mCanvasRoot;
        public static Camera UICamera;
        public virtual string Tag { get { return this.GetType().Name; } }

        public abstract string DialogName { get; }

        public virtual bool CanOpen() {
            return true;
        }

        public virtual bool AutoClear() {
            return false;
        }

        public BaseDialogState(BaseDialogStateManager stateManager) : base(stateManager) {
            DialogStateManager = stateManager;
        }

        public static void InitRoot() {
            if (mRootUI == null) {
                mRootUI = GameObject.Instantiate(Resources.Load<GameObject>("CanvasRoot"), Vector3.zero, Quaternion.identity);
                GameObject.DontDestroyOnLoad(mRootUI);
                mCanvasRoot = mRootUI.transform.Find("Canvas").gameObject;
                UIHelper.InitCanvas(mCanvasRoot.GetComponent<Canvas>());
                UICamera = NodeHelper.Find("UICamera", mRootUI).GetComponent<Camera>();
                UICamera.clearFlags = CameraClearFlags.Color;
                UICamera.backgroundColor = Color.black;
            }
        }
    }
}