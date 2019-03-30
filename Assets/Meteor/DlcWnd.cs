using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DlcWnd : Window<DlcWnd> {
    public override string PrefabName { get { return "DlcWnd"; } }
    GameObject rootMenu;
    Image background;
    GameObject warningWnd;
    protected override bool OnOpen()
    {
        Init();
        if (MainWnd.Exist)
            MainWnd.Instance.Close();
        return base.OnOpen();
    }
    void Init()
    {
        rootMenu = Control("Content");
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
            for (int i = 0; i < GameData.Instance.gameStatus.pluginChapter.Count; i++)
            {
                Chapter lev = GameData.Instance.gameStatus.pluginChapter[i];
                if (lev == null)
                    continue;
                lev.Check();
                if (!lev.Installed)
                    continue;
                AddGridItem(lev, rootMenu.transform);
                select = lev;
            }
        }
        OnSelectChapter(select);
    }

    Chapter select;
    void OnSelectChapter(Chapter lev)
    {
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

        Texture2D tex = new Texture2D(0, 0);
        if (!string.IsNullOrEmpty(lev.Preview))
        {
            if (System.IO.File.Exists(lev.Preview))
            {
                System.IO.FileStream fs = System.IO.File.OpenRead(lev.Preview);
                byte[] bitPreview = new byte[fs.Length];
                fs.Read(bitPreview, 0, (int)fs.Length);
                tex.LoadImage(bitPreview);
                background.overrideSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
            }
        }

        select = lev;
        Control("Task").GetComponent<Text>().text = select.Name;
        //写上该剧本得剧情概述
    }

    void OnEnterChapter()
    {
        if (select != null)
        {
            //普通关卡对待.
            Global.Instance.Chapter = select;
            DlcLevelSelect.Instance.Open();
        }
        else
        {
            U3D.PopupTip("请先在设置-模组中安装资料片");
        }
    }

    void AddGridItem(Chapter lev, Transform parent)
    {
        GameObject objPrefab = Resources.Load("LevelSelectItem", typeof(GameObject)) as GameObject;
        GameObject obj = GameObject.Instantiate(objPrefab) as GameObject;
        obj.transform.SetParent(parent);
        obj.name = lev.Name;
        obj.GetComponent<Button>().onClick.AddListener(() => { OnSelectChapter(lev); });
        obj.GetComponentInChildren<Text>().text = lev.Name;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
    }
}
