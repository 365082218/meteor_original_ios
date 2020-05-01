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

    public override void OnAction(DialogAction dialogAction, object data)
    {
        switch (dialogAction)
        {
            case DialogAction.Close:
            case DialogAction.BackButton:
                ChangeState(null);
                break;
            case DialogAction.Previous:
                if (previousS != null)
                    ChangeState(previousS);
                else
                    ChangeState(Main.Ins.DialogStateManager.MainMenuState);
                break;
            default:
                break;
        }
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
            for (int i = 1; i <= Main.Ins.GameStateMgr.gameStatus.Level; i++)
            {
                LevelDatas.LevelDatas lev = Main.Ins.DataMgr.GetData<LevelDatas.LevelDatas>(i);
                if (lev == null)
                    continue;
                Idevgame.Util.LevelUtils.AddGridItem(lev, rootMenu.transform, OnSelectLevel);
                select = lev;
            }
        }
        else
        {
            //剧本关卡
            for (int i = 1; i <= Main.Ins.CombatData.Chapter.level; i++)
            {
                LevelDatas.LevelDatas lev = Main.Ins.CombatData.Chapter.GetItem(i);
                if (lev == null)
                    continue;
                Idevgame.Util.LevelUtils.AddGridItem(lev, rootMenu.transform, OnSelectLevel);
                select = lev;
            }
        }
        OnSelectLevel(select);
    }

    LevelDatas.LevelDatas select;
    void OnSelectLevel(LevelDatas.LevelDatas lev)
    {
        Material loadingTexture = null;
        if (!string.IsNullOrEmpty(lev.BgTexture))
        {
            if (singlePlayer)
            {
                loadingTexture = GameObject.Instantiate(Resources.Load<Material>(lev.BgTexture)) as Material;
                if (loadingTexture != null)
                    background.material = loadingTexture;
            }
            else
            {
                for (int i = 0; i < Main.Ins.CombatData.Chapter.resPath.Length; i++)
                {
                    if (Main.Ins.CombatData.Chapter.resPath[i].EndsWith(lev.BgTexture + ".jpg"))
                    {
                        byte[] array = System.IO.File.ReadAllBytes(Main.Ins.CombatData.Chapter.resPath[i]);
                        Texture2D tex = new Texture2D(0, 0);
                        tex.LoadImage(array);
                        loadingTexture = GameObject.Instantiate(Resources.Load<Material>("Scene10")) as Material;
                        loadingTexture.SetTexture("_MainTex", tex);
                        loadingTexture.SetColor("_TintColor", Color.white);
                        background.material = loadingTexture;
                        break;
                    }
                }
            }
        }
        select = lev;
        Task.text = select.Name;
    }

    void OnEnterLevel()
    {
        if (select != null)
        {
            //单机全部是普通关卡对待.
            if (singlePlayer)
            {
                U3D.LoadLevel(select, LevelMode.SinglePlayerTask, GameMode.Normal);
            }
            else
            {
                Main.Ins.DlcMng.PlayDlc(Main.Ins.CombatData.Chapter, select.ID);
            }
            if (background.material == null)
                OnBackPress();
        }
    }
}