using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemInfoDialogState : PersistDialog<ItemInfoDialog>
{
    public override string DialogName { get { return "ItemInfoDialog"; } }
}

public class ItemInfoDialog : Dialog
{
    public static UIItemInfo Item;
    public void AssignItem(UIItemInfo item)
    {
        Item = item;
    }

    public override void OnDialogStateEnter(PersistState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }

    void Init()
    {

    }
}