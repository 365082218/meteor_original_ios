using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
public class TeachMenu : Window<TeachMenu> {
    public override string PrefabName{get{return "TeachMenu"; }}
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

        for (int i = 1; i <= GameData.Instance.gameStatus.Level; i++)
        {
            Level lev = LevelMng.Instance.GetItem(i);
            if (lev == null)
                continue;
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
        Control("Task").GetComponent<Text>().text = select.Name;
    }

    void OnEnterLevel()
    {
        if (select != null)
        {
            //单机全部是普通关卡对待.
            U3D.LoadLevel(select.ID, LevelMode.SinglePlayerTask, GameMode.Normal);
        }
        else
            Log.WriteError("select == null");
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
