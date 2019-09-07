using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Idevgame.GameState.DialogState;
using UnityEngine.UI;

//资料片详情页面.和资料片选择界面，点击预览图 展示资料片详情
public class DlcInfoDialogState:CommonDialogState<DlcInfoDialog>
{
    public override string DialogName { get { return "DlcInfo"; } }
    public DlcInfoDialogState(MainDialogStateManager stateMgr):base(stateMgr)
    {

    }
}

public class DlcInfoDialog : Dialog {
    Text levelDesc;
    Transform LevelRoot;
    Chapter chapter;
    Image background;
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        chapter = data as Chapter;
        Init();
    }

    public override void OnClose()
    {
        base.OnClose();
    }

    Coroutine ScanLevel;
    void Init()
    {
        levelDesc = Control("LevelDescription").GetComponent<Text>();
        LevelRoot = Control("LevelRoot").transform;
        background = Control("Background").GetComponent<Image>();
        Control("Yes").GetComponent<Button>().onClick.AddListener(OnEnterChapter);
        Control("Cancel").GetComponent<Button>().onClick.AddListener(OnPreviousPress);
        if (chapter != null)
        {
            //默认选择到当前剧本通关最远的一关
            ScanLevel = StartCoroutine(Scan());
        }
        OnSelectLevel(chapter.GetItem(chapter.level));
    }

    void OnEnterChapter()
    {
        Global.Instance.Chapter = this.chapter;
        string tip = "";
        if (!DlcMng.Instance.CheckDependence(Global.Instance.Chapter, out tip))
        {
            Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.LevelDialogState, false);
        }
        else
        {
            U3D.PopupTip("模组依赖\n" + tip);
        }
    }

    IEnumerator Scan()
    {
        Level[] all = chapter.LoadAll();
        for (int i = 0; i < all.Length; i++)
        {
            Idevgame.Util.LevelUtils.AddGridItem(all[i], LevelRoot, OnSelectLevel);
            yield return 0;
        }
        
    }

    //所有资料片的关卡必须填写desc，描述该关卡主要任务
    Level select;
    void OnSelectLevel(Level lev)
    {
        Material loadingTexture = null;
        if (!string.IsNullOrEmpty(lev.BgTexture))
            loadingTexture = Resources.Load<Material>(lev.BgTexture) as Material;
        if (loadingTexture != null)
            background.material = loadingTexture;
        select = lev;
        levelDesc.text = string.Format("{0}\n{1}", select.Name, select.Desc);
    }
}
