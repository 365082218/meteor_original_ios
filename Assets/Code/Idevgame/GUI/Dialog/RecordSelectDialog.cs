using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecordSelectState : PersistDialog<RecordSelectDialog> {
    public override string DialogName { get { return "RecordSelectDialog"; } }
    public RecordSelectState() {

    }
}

public class RecordSelectDialog : RecordDialog {
    public override void OnDialogStateEnter(PersistState ownerState, BaseDialogState previousDialog, object data) {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }
    protected override void Init() {
        Prefab = Resources.Load("UI/Dialogs/RecordItemSelect") as GameObject;
        base.Init();
        GameObject play = Control("Play");
        if (play != null) {
            play.GetComponent<Button>().onClick.RemoveAllListeners();
            play.GetComponent<Button>().onClick.AddListener(() => {
                if (recCurrent > -1 && recData.Count > recCurrent) {
                    //播放一个路线，弹出2级菜单.
                    GameRecord rec = recData[recCurrent];
                    GameStateMgr.Ins.gameStatus.NetWork.record = rec;
                    CreateRoomDialogState.Instance.OnRefresh(1);
                    OnBackPress();
                }
            });
        }
    }
}