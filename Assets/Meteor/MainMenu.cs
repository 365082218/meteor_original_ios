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
        return base.OnOpen();
    }
    void Init()
    {
        rootMenu = Control("Panel");
        background = Control("Background").GetComponent<Image>();
        Control("Yes").GetComponent<UIButtonExtended>().onClick.AddListener(() => {
            OnEnterLevel();
        });
        Control("Cancel").GetComponent<UIButtonExtended>().onClick.AddListener(() => {
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
            Log.LogInfo("u3d.loadlevel");
            U3D.LoadLevel(select.ID);
        }
        else
            Log.LogInfo("select == null");
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
