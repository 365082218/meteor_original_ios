using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DlcWnd : Window<DlcWnd> {
    public override string PrefabName { get { return "DlcWnd"; } }
    GameObject rootMenu;
    EnhanceScrollView rootCtrl;
    Image background;
    GameObject warningWnd;
    GameObject descPanel;
    protected override bool OnOpen()
    {
        Init();
        if (MainWnd.Exist)
            MainWnd.Instance.Close();
        return base.OnOpen();
    }
    void Init()
    {
        descPanel = Control("DescRoot");
        rootMenu = Control("Content");
        rootCtrl = rootMenu.GetComponent<EnhanceScrollView>();
        background = Control("Background").GetComponent<Image>();
        Control("Yes").GetComponent<Button>().onClick.AddListener(() => {
            OnEnterChapter();
        });
        Control("Cancel").GetComponent<Button>().onClick.AddListener(() => {
            MainWnd.Instance.Open();
            Close();
        });
        warningWnd = Control("WarningWnd");
        
        if (GameData.Instance.gameStatus.pluginChapter != null)
        {
            int insertCount = 0;
            for (int i = 0; i < GameData.Instance.gameStatus.pluginChapter.Count; i++)
            {
                Chapter lev = GameData.Instance.gameStatus.pluginChapter[i];
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
            WndObject.GetComponentInChildren<EnhanceScrollView>().enabled = false;
            return;
        }
        else
        {
            warningWnd.SetActive(false);
        }

        select = lev;
        Control("Title", descPanel).GetComponent<Text>().text = select.Name;
        Control("Desc", descPanel).GetComponent<Text>().text = select.Desc;
        descPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-450, -250);//.position = 0;
        descPanel.GetComponent<CanvasGroup>().alpha = 0;
        seq = descPanel.Fade(1.0f, 250);
        //写上该剧本得剧情概述
    }

    void OnEnterChapter()
    {
        if (select != null)
        {
            //普通关卡对待.
            Global.Instance.Chapter = select;
            string tip = "";
            if (!DlcMng.Instance.CheckDependence(Global.Instance.Chapter, out tip))
                DlcLevelSelect.Instance.Open();
            else
            {
                U3D.PopupTip("Dlc依赖\n" + tip);
            }
        }
        else
        {
            U3D.PopupTip("请先在设置-模组中安装资料片");
        }
    }

    void AddGridItem(Chapter lev, Transform parent)
    {
        GameObject obj = GameObject.Instantiate(rootCtrl.itemPrefab) as GameObject;
        obj.transform.SetParent(parent);
        obj.name = lev.Name;
        //obj.GetComponent<Button>().onClick.AddListener(() => { OnSelectChapter(lev); });
        Texture2D tex = new Texture2D(0, 0);
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
