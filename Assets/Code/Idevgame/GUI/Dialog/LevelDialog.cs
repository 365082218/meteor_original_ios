using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using Idevgame.GameState.DialogState;
using Excel2Json;

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
    bool normalLevel;//是普通关卡还是资料片关卡
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        normalLevel = (bool)data;
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

        if (normalLevel)
        {
            //单机关卡
            for (int i = 1; i <= Main.Ins.GameStateMgr.gameStatus.Level; i++)
            {
                LevelData lev = Main.Ins.DataMgr.GetLevelData(i);
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
                LevelData lev = Main.Ins.CombatData.Chapter.GetLevel(i);
                if (lev == null)
                    continue;
                Idevgame.Util.LevelUtils.AddGridItem(lev, rootMenu.transform, OnSelectLevel);
                select = lev;
            }
        }
        OnSelectLevel(select);
    }

    LevelData select;
    void OnSelectLevel(LevelData lev)
    {
        Texture2D loadingTexture = null;
        if (!string.IsNullOrEmpty(lev.BgTexture))
        {
            if (normalLevel)
            {
                loadingTexture = GameObject.Instantiate(Resources.Load<Texture2D>(lev.BgTexture));
              }
            else
            {
                for (int i = 0; i < Main.Ins.CombatData.Chapter.resPath.Count; i++)
                {
                    if (Main.Ins.CombatData.Chapter.resPath[i].EndsWith(lev.BgTexture + ".jpg"))
                    {
                        byte[] array = System.IO.File.ReadAllBytes(Main.Ins.CombatData.Chapter.resPath[i]);
                        Texture2D tex = new Texture2D(0, 0);
                        tex.LoadImage(array);
                        loadingTexture = tex;
                        break;
                    }
                }
            }
        }

        if (loadingTexture != null) {
            background.sprite = Sprite.Create(loadingTexture, new Rect(0, 0, loadingTexture.width, loadingTexture.height), Vector2.zero);
            background.color = Color.white;
        }
        Utility.Expand(background, loadingTexture.width, loadingTexture.height);
        select = lev;
        Task.text = select.Name;
    }

    void OnEnterLevel()
    {
        if (select != null)
        {
            //单机全部是普通关卡对待.
            if (normalLevel)
            {
                U3D.LoadLevel(select, LevelMode.SinglePlayerTask, GameMode.Normal);
            }
            else
            {
                Main.Ins.DlcMng.PlayDlc(Main.Ins.CombatData.Chapter, select.Id);
            }
            if (background.material == null)
                OnBackPress();
        }
    }
}