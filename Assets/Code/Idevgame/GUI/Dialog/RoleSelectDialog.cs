using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoleSelectDialogState : CommonDialogState<RoleSelectDialog>
{
    public override string DialogName { get { return "RoleSelectDialog"; } }
    public RoleSelectDialogState(MainDialogStateManager stateMgr) : base(stateMgr)
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
    int heroIdx = 0;
    void Init()
    {
        heroImg = Control("Image").GetComponent<Image>();
        heroImg.material = ResMng.Load("Hero0") as Material;
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
        if (heroIdx >= Main.Ins.DataMgr.GetDatasArray<ModelDatas.ModelDatas>().Count)
            heroIdx = 0;
        heroImg.material = ResMng.Load(string.Format("Hero{0}", heroIdx)) as Material;
    }

    void OnPrevHero()
    {
        heroIdx -= 1;
        if (heroIdx < 0)
            heroIdx = Main.Ins.DataMgr.GetDatasArray<ModelDatas.ModelDatas>().Count - 1;
        heroImg.material = ResMng.Load(string.Format("Hero{0}", heroIdx)) as Material;
    }

    void OnSelectHero()
    {
        Main.Ins.NetWorkBattle.heroIdx = heroIdx;
        Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.WeaponSelectDialogState);
    }
}