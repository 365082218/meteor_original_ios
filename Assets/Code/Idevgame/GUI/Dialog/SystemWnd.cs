using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using ProtoBuf;
using Idevgame.GameState.DialogState;
using Idevgame.StateManagement;
using Idevgame.StateManagement.DialogStateManagement;
using Idevgame.StateManagement.GameStateManagement;

public class ModelSelectDialogState:CommonDialogState<ModelSelectUiController>
{
    public override string DialogName { get { return "ModelWnd"; } }
    public ModelSelectDialogState(MainDialogStateManager dialogState):base(dialogState)
    {
        
    }
}
//主角模型选择.
public class ModelSelectUiController: Dialog
{
    public GameObject GridViewRoot;
    public Button Close;
    Coroutine loadModel;
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Close.onClick.AddListener(() => { OnBackPress(); });
        loadModel = Main.Instance.StartCoroutine(LoadModels());
    }

    IEnumerator LoadModels()
    {
        List<int> allModel = new List<int>();
        for (int i = 0; i < 20; i++)
        {
            allModel.Add(i);
        }

        if (GameData.Instance.gameStatus.pluginModel != null)
        {
            for (int i = 0; i < GameData.Instance.gameStatus.pluginModel.Count; i++)
            {
                GameData.Instance.gameStatus.pluginModel[i].Check();
                if (GameData.Instance.gameStatus.pluginModel[i].Installed)
                    allModel.Add(GameData.Instance.gameStatus.pluginModel[i].ModelId);
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
        obj.SetText(Global.Instance.GetCharacterName(Idx));
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
            Main.Instance.StopCoroutine(loadModel);
            loadModel = null;
        }
        base.OnBackPress();
    }
}

