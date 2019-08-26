using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using Idevgame.GameState.DialogState;
public class LevelDialogState:CommonDialogState<LevelDialog>
{
    public override string DialogName { get { return "LevelDialog"; } }
    public LevelDialogState(MainDialogStateManager stateMgr):base(stateMgr)
    {

    }
}

//关卡选择界面
public class LevelDialog : Dialog {
    [SerializeField]
    GameObject rootMenu;
    [SerializeField]
    Image background;
    [SerializeField]
    Button Yes;
    [SerializeField]
    Button Cancel;
    [SerializeField]
    Text Task;
    bool singlePlayer;
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        singlePlayer = (bool)data;
        Init();
    }

    void Init()
    {
        Yes.onClick.AddListener(() =>
        {
            OnEnterLevel();
        });
        Cancel.onClick.AddListener(() =>
        {
            OnPreviousPress();
        });

        if (singlePlayer)
        {
            //单机关卡
            for (int i = 1; i <= GameData.Instance.gameStatus.Level; i++)
            {
                Level lev = LevelMng.Instance.GetItem(i);
                if (lev == null)
                    continue;
                AddGridItem(lev, rootMenu.transform);
                select = lev;
            }
        }
        else
        {
            //剧本关卡
            for (int i = 1; i <= Global.Instance.Chapter.level; i++)
            {
                Level lev = Global.Instance.Chapter.GetItem(i);
                if (lev == null)
                    continue;
                AddGridItem(lev, rootMenu.transform);
                select = lev;
            }
        }
        OnSelectLevel(select);
    }

    Level select;
    void OnSelectLevel(Level lev)
    {
        Material loadingTexture = null;
        if (!string.IsNullOrEmpty(lev.BgTexture))
            loadingTexture = Resources.Load<Material>(lev.BgTexture) as Material;
        if (loadingTexture != null)
            background.material = loadingTexture;
        select = lev;
        Task.text = select.Name;
    }

    void OnEnterLevel()
    {
        if (select != null)
        {
            //单机全部是普通关卡对待.
            U3D.LoadLevel(select.ID, LevelMode.SinglePlayerTask, GameMode.Normal);
            if (background.material == null)
                OnBackPress();
        }
    }

    void AddGridItem(Level lev, Transform parent)
    {
        GameObject objPrefab = Resources.Load("LevelSelectItem", typeof(GameObject)) as GameObject;
        GameObject obj = GameObject.Instantiate(objPrefab) as GameObject;
        obj.transform.SetParent(parent);
        obj.name = lev.Name;
        obj.GetComponent<Button>().onClick.AddListener(() => { OnSelectLevel(lev); });
        obj.GetComponentInChildren<Text>().text = lev.Name;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
    }
}