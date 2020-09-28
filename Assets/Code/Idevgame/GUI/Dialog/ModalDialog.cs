using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalDialogState<T>: PersistDialog<T> where T: ModalDialog {
    public override string DialogName { get { return "ModalDialog"; } }
    protected GameObject ModelDialog;
    public virtual string ModalDialogName {
        get {
            return "";
        }
    }
    protected string ModalDialogResorucePath {
        get {
            return "UI/Dialogs/" + ModalDialogName;
        }
    }
    public override void LoadGui() {
        base.LoadGui();//加载了模态窗
        LoadInnerGui();//加载自身窗体
    }
    protected override void UnloadGui() {
        base.UnloadGui();
        UnityEngine.Object.Destroy(ModelDialog);
        ModelDialog = null;
    }
    public virtual void LoadInnerGui() {
        ModelDialog = GameObject.Instantiate(Resources.Load<GameObject>(ModalDialogResorucePath), Dialog.transform, false);
    }
}
//模态弹窗拥有自己的全屏透明背景，遮挡其他输入
public class ModalDialog : Dialog {
    public override void OnDialogStateEnter(PersistState ownerState, BaseDialogState previousDialog, object data) {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
    }
}
