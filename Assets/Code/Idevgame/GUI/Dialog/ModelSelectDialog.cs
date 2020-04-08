﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Idevgame.GameState.DialogState;

public class ModelSelectDialogState:CommonDialogState<ModelSelectDialog>
{
    public override string DialogName { get { return "ModelWnd"; } }
    public ModelSelectDialogState(MainDialogStateManager dialogState):base(dialogState)
    {
        
    }
}
//主角模型选择.
public class ModelSelectDialog: Dialog
{
    public GameObject GridViewRoot;
    public Button Close;
    Coroutine loadModel;
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
        Close.onClick.AddListener(() => { OnBackPress(); });
        loadModel = Main.Ins.StartCoroutine(LoadModels());
    }

    void Init()
    {
        Close = Control("Close").GetComponent<Button>();
        GridViewRoot = Control("GridViewRoot");
    }

    IEnumerator LoadModels()
    {
        List<int> allModel = new List<int>();
        for (int i = 0; i < 20; i++)
        {
            allModel.Add(i);
        }

        if (Main.Ins.GameStateMgr.gameStatus.pluginModel != null)
        {
            for (int i = 0; i < Main.Ins.GameStateMgr.gameStatus.pluginModel.Count; i++)
            {
                Main.Ins.GameStateMgr.gameStatus.pluginModel[i].Check();
                if (Main.Ins.GameStateMgr.gameStatus.pluginModel[i].Installed)
                    allModel.Add(Main.Ins.GameStateMgr.gameStatus.pluginModel[i].ModelId);
            }
        }

        for (int i = 0; i < allModel.Count; i++)
        {
            AddModel(allModel[i]);
            yield return 0;
        }
    }

    void AddModel(int Idx)
    {
        UIFunCtrl obj = (GameObject.Instantiate(Resources.Load("GridItemBtn")) as GameObject).AddComponent<UIFunCtrl>();
        obj.SetEvent(ChangeModel, Idx);
        obj.SetText(Main.Ins.CombatData.GetCharacterName(Idx));
        obj.transform.SetParent(GridViewRoot.transform);
        obj.gameObject.layer = GridViewRoot.layer;
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
    }

    void ChangeModel(int id)
    {
        //创建一个角色，去替换掉主角色，
        U3D.ChangePlayerModel(id);
    }

    public override void OnBackPress()
    {
        if (loadModel != null)
        {
            Main.Ins.StopCoroutine(loadModel);
            loadModel = null;
        }
        base.OnBackPress();
    }
}

