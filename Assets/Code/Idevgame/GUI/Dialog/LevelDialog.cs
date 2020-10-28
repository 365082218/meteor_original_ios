using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using Idevgame.GameState.DialogState;
using Excel2Json;
using System.Collections.Generic;

public class LevelDialogState:CommonDialogState<LevelDialog>
{
    public override string DialogName { get { return "LevelDialog"; } }
    public LevelDialogState(MainDialogMgr stateMgr):base(stateMgr)
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
    Dictionary<string, Button> levelBtns = new Dictionary<string, Button>();
    Button selectedBtn;
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
            for (int i = 1; i <= GameStateMgr.Ins.gameStatus.Level; i++)
            {
                LevelData lev = DataMgr.Ins.GetLevelData(i);
                if (lev == null)
                    continue;
                Idevgame.Util.LevelUtils.AddGridItem(lev, rootMenu.transform, OnSelectLevel);
                select = lev;
            }
        }
        else
        {
            //剧本关卡
            for (int i = 1; i <= CombatData.Ins.Chapter.level; i++)
            {
                LevelData lev = CombatData.Ins.Chapter.GetLevel(i);
                if (lev == null)
                    continue;
                Idevgame.Util.LevelUtils.AddGridItem(lev, rootMenu.transform, OnSelectLevel);
                select = lev;
            }
        }
        for (int i = 0; i < rootMenu.transform.childCount; i++) {
            Transform tri = rootMenu.transform.GetChild(i);
            Button btn = tri.GetComponent<Button>();
            levelBtns.Add(tri.name, btn);
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
                for (int i = 0; i < CombatData.Ins.Chapter.resPath.Count; i++)
                {
                    if (CombatData.Ins.Chapter.resPath[i].EndsWith(lev.BgTexture + ".jpg"))
                    {
                        byte[] array = System.IO.File.ReadAllBytes(CombatData.Ins.Chapter.resPath[i]);
                        Texture2D tex = new Texture2D(0, 0, TextureFormat.ARGB32, false);
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
        Control("Image").GetComponent<RectTransform>().sizeDelta = background.GetComponent<RectTransform>().sizeDelta;
        select = lev;
        Task.text = select.Name;
        if (selectedBtn != null) {
            selectedBtn.image.color = new Color(1, 1, 1, 0);
            selectedBtn = null;
        }
        selectedBtn = levelBtns[lev.Name];
        selectedBtn.image.color = new Color(144.0f / 255.0f, 104.0f / 255.0f, 104.0f / 255.0f, 104.0f / 255.0f);
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
                DlcMng.Ins.PlayDlc(CombatData.Ins.Chapter, select.Id);
            }
            if (background.material == null)
                OnBackPress();
        }
    }
}