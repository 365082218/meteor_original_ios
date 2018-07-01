using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
public class MainMenu : Window<MainMenu> {
    public override string PrefabName{get{return "MainMenu";}}
    GameObject rootMenu;
    Image background;
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
            OnEnterLevel();
        });
        Control("Cancel").GetComponent<Button>().onClick.AddListener(() => {
            MainWnd.Instance.Open();
            Close();
        });

        for (int i = 1; i <= GameData.gameStatus.Level; i++)
        {
            Level lev = LevelMng.Instance.GetItem(i);
            AddGridItem(lev, rootMenu.transform);
            select = lev;
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
    }

    void OnEnterLevel()
    {
        if (select != null)
        {
            WSLog.LogInfo("u3d.loadlevel");
            U3D.LoadLevel(select.ID);
        }
        else
            WSLog.LogInfo("select == null");
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
