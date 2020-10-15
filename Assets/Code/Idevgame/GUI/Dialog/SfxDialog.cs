using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SfxDialogState : CommonDialogState<SfxDialog>
{
    public override string DialogName { get { return "SfxDialog"; } }
    public SfxDialogState(MainDialogMgr stateMgr):base(stateMgr)
    {

    }

    protected override float GetX() {
        return -550;
    }

}

public class SfxDialog : Dialog
{
   
    public GameObject SfxRoot;
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }

    int pageIndex = 32;
    private const int PageCount = 20;
    int pageMax = 0;
    void Init()
    {
        pageMax = (SFXLoader.Ins.Eff.Length / PageCount) + ((SFXLoader.Ins.Eff.Length % PageCount) != 0 ? 1 : 0);
        SfxRoot = Control("Page");
        Control("Close").GetComponent<Button>().onClick.AddListener(OnBackPress);
        Control("PagePrev").GetComponent<Button>().onClick.AddListener(PrevPage);
        Control("PageNext").GetComponent<Button>().onClick.AddListener(NextPage);
        NextPage();
    }

    void NextPage()
    {
        if (pageIndex == pageMax)
            pageIndex = 1;
        else
            pageIndex += 1;
        if (refresh != null)
            Main.Ins.StopCoroutine(refresh);
        refresh = Main.Ins.StartCoroutine(RefreshSfx(pageIndex));
        Control("PageText").GetComponent<Text>().text = string.Format("{0:d2}/{1:d2}", pageIndex, pageMax);
    }

    void PrevPage()
    {
        if (pageIndex == 1)
            pageIndex = pageMax;
        else
            pageIndex -= 1;
        if (refresh != null)
            Main.Ins.StopCoroutine(refresh);
        refresh = Main.Ins.StartCoroutine(RefreshSfx(pageIndex));
        Control("PageText").GetComponent<Text>().text = string.Format("{0:d2}/{1:d2}", pageIndex, pageMax);
    }

    Coroutine refresh = null;
    IEnumerator RefreshSfx(int page)
    {
        for (int i = Mathf.Min((page - 1) * PageCount, (page) * PageCount); i < (page) * PageCount; i++)
        {
            int j = i - (page - 1) * PageCount;
            if (SFXList.Count > j)
                SFXList[j].SetActive(false);
        }

        for (int i = (page - 1) * PageCount; i < Mathf.Min((page) * PageCount, SFXLoader.Ins.TotalSfx); i++)
        {
            AddSFX(i, (page - 1) * PageCount);
            yield return 0;
        }
    }

    List<GameObject> SFXList = new List<GameObject>();
    void AddSFX(int Idx, int startIdx)
    {
        int j = Idx - startIdx;
        if (SFXList.Count > j)
        {
            SFXList[j].SetActive(true);
            SFXList[j].GetComponent<Button>().onClick.RemoveAllListeners();
            SFXList[j].GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            SFXList[j].GetComponent<Button>().onClick.AddListener(() => { PlaySfx(Idx); });
            SFXList[j].GetComponentInChildren<Text>().text = SFXLoader.Ins.Eff[Idx];
        }
        else
        {
            GameObject obj = GameObject.Instantiate(Resources.Load("GridItemBtn")) as GameObject;
            obj.GetComponent<Button>().onClick.AddListener(() => { PlaySfx(Idx); });
            obj.GetComponentInChildren<Text>().text = SFXLoader.Ins.Eff[Idx];
            obj.transform.SetParent(SfxRoot.transform);
            obj.gameObject.layer = SfxRoot.layer;
            obj.transform.localScale = Vector3.one;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            SFXList.Add(obj);
        }
    }

    void PlaySfx(int idx)
    {
        SFXLoader.Ins.PlayEffect(idx, Main.Ins.LocalPlayer.gameObject, true);
    }
}