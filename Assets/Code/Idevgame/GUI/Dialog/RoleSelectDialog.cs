using Excel2Json;
using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoleSelectDialogState : CommonDialogState<RoleSelectDialog>
{
    public override string DialogName { get { return "RoleSelectDialog"; } }
    public RoleSelectDialogState(MainDialogMgr stateMgr) : base(stateMgr)
    {

    }
}

public class RoleSelectDialog : Dialog
{
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }

    Image heroImg;
    int heroIdx = -1;
    void Init()
    {
        heroImg = Control("Image").GetComponent<Image>();
        heroImg.color = Color.white;
        heroIdx = -1;
        OnNextHero();
        Control("Next").GetComponent<Button>().onClick.AddListener(() =>
        {
            OnNextHero();
            U3D.PlayBtnAudio();
        });
        Control("Prev").GetComponent<Button>().onClick.AddListener(() =>
        {
            OnPrevHero();
            U3D.PlayBtnAudio();
        });
        Control("Select").GetComponent<UIButtonExtended>().onClick.AddListener(() =>
        {
            OnSelectHero();
            U3D.PlayBtnAudio();
        });
    }

    void OnNextHero()
    {
        heroIdx += 1;
        if (heroIdx >= DataMgr.Ins.GetModelDatas().Count)
            heroIdx = 0;
        LoadHeroTex();
    }

    void OnPrevHero()
    {
        heroIdx -= 1;
        if (heroIdx < 0)
            heroIdx = DataMgr.Ins.GetModelDatas().Count - 1;
        LoadHeroTex();
    }

    void LoadHeroTex() {
        Texture2D tex = Resources.Load<Texture2D>(string.Format("P00{0}", heroIdx + 1)) as Texture2D;
        if (tex != null) {
            heroImg.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
            //Utility.Expand(heroImg, tex.width, tex.height);
            //heroImg.color = Color.white;
        }
        Utility.Expand(heroImg, tex.width, tex.height);
    }

    void OnSelectHero()
    {
        NetWorkBattle.Ins.heroIdx = heroIdx;//0-19
        Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.WeaponSelectDialogState);
    }
}