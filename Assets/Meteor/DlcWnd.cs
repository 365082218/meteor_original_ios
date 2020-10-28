using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Idevgame.GameState.DialogState;
using System.Collections.Generic;

public class DlcDialogState : CommonDialogState<DlcWnd> {
    public override string DialogName { get { return "DlcWnd"; } }
    public DlcDialogState(MainDialogMgr stateMgr) : base(stateMgr) {

    }
}

public class DlcWnd : Dialog {
    [SerializeField]
    GameObject rootMenu;
    [SerializeField]
    EnhanceScrollView rootCtrl;
    [SerializeField]
    Image background;
    [SerializeField]
    GameObject warningWnd;
    [SerializeField]
    GameObject descPanel;
    [SerializeField]
    Button Yes;
    [SerializeField]
    Button Cancel;
    [SerializeField]
    Text Title;
    [SerializeField]
    Text Desc;
    [SerializeField]
    Text PageLabel;
    List<Chapter> Chapters = new List<Chapter>();
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }

    void Init()
    {
        Chapters.Clear();
        Yes.onClick.AddListener(() =>
        {
            OnEnterChapter();
        });

        Cancel.onClick.AddListener(() =>
        {
            Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.MainMenuState);
        });

        if (GameStateMgr.Ins.gameStatus.pluginChapter != null)
        {
            int insertCount = 0;
            for (int i = 0; i < GameStateMgr.Ins.gameStatus.pluginChapter.Count; i++)
            {
                Chapter lev = GameStateMgr.Ins.gameStatus.pluginChapter[i];
                if (lev == null)
                    continue;
                lev.Check();
                if (!lev.Installed)
                    continue;
                AddGridItem(lev, rootMenu.transform);
                insertCount++;
                select = lev;
            }

            if (insertCount != 0)
                rootCtrl.Reload(OnSelectChapter);
        }
        if (Chapters.Count != 0)
            PageLabel.text = string.Format("剧本:{0}/{1}", 1, Chapters.Count);
        else
            PageLabel.text = "剧本:未安装任何剧本";
        OnSelectChapter(select);
    }

    Chapter select;
    Sequence seq = null;
    void OnSelectChapter(Chapter lev)
    {
        if (seq != null)
        {
            if (!seq.IsComplete())
                seq.Kill();
        }

        if (lev == null)
        {
            //还未安装任何资料片，要下载资料片需要在主界面-设置-模组内安装对应的资料片
            warningWnd.SetActive(true);
            rootCtrl.enabled = false;
            return;
        }
        else
        {
            warningWnd.SetActive(false);
        }



        select = lev;
        Title.text = select.Name;
        Desc.text = select.Desc;
        descPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-450, -250);
        descPanel.GetComponent<CanvasGroup>().alpha = 0;
        seq = descPanel.Fade(1.0f, 250);

        for (int i = 0; i < Chapters.Count; i++) {
            if (Chapters[i].ChapterId == select.ChapterId) {
                PageLabel.text = string.Format("剧本:{0}/{1}", i+1, Chapters.Count);
                break;
            }
        }
    }

    void OnEnterChapter()
    {
        if (select != null)
        {
            //普通关卡对待.
            CombatData.Ins.Chapter = select;
            Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.LevelDialogState, false);
        }
        else
        {
            U3D.PopupTip("请先在主页-资料片中安装对应剧本");
        }
    }

    void AddGridItem(Chapter lev, Transform parent)
    {
        Chapters.Add(lev);
        GameObject obj = GameObject.Instantiate(rootCtrl.itemPrefab) as GameObject;
        obj.transform.SetParent(parent);
        obj.name = lev.Name;
        //obj.GetComponent<Button>().onClick.AddListener(() => { OnSelectChapter(lev); });
        Texture2D tex = new Texture2D(0, 0, TextureFormat.ARGB32, false);
        if (System.IO.File.Exists(lev.Preview))
        {
            byte[] bit = System.IO.File.ReadAllBytes(lev.Preview);
            if (bit != null && bit.Length != 0)
            {
                tex.LoadImage(bit);
                obj.GetComponentInChildren<Image>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
            }
        }
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
        obj.SetActive(true);
        rootCtrl.RegisterOnSelect(obj, lev);
    }
}
