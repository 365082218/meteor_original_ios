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
                Idevgame.Util.LevelUtils.AddGridItem(lev, rootMenu.transform, OnSelectLevel);
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
                Idevgame.Util.LevelUtils.AddGridItem(lev, rootMenu.transform, OnSelectLevel);
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
        {
            if (singlePlayer)
            {
                loadingTexture = GameObject.Instantiate(Resources.Load<Material>(lev.BgTexture)) as Material;
                if (loadingTexture != null)
                    background.material = loadingTexture;
            }
            else
            {
                for (int i = 0; i < Global.Instance.Chapter.resPath.Length; i++)
                {
                    if (Global.Instance.Chapter.resPath[i].EndsWith(lev.BgTexture + ".jpg"))
                    {
                        byte[] array = System.IO.File.ReadAllBytes(Global.Instance.Chapter.resPath[i]);
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
                U3D.LoadLevel(select.ID, LevelMode.SinglePlayerTask, GameMode.Normal);
            }
            else
            {
                Global.Instance.Chapter.LoadLevel(select.ID);
            }
            if (background.material == null)
                OnBackPress();
        }
    }
}