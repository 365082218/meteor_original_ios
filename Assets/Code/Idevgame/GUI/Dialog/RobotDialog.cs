using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RobotDialogState : CommonDialogState<RobotDialog>
{
    public override string DialogName { get { return "RobotDialog"; } }
    public RobotDialogState(MainDialogMgr stateMgr) : base(stateMgr)
    {

    }
}

public class RobotDialog : Dialog
{
    public GameObject RobotRoot;
    public int weaponIdx = 0;//0-长剑
    public int campIdx = 1;//
    public int hpIdx = 0;
    int[] hpArray = { 1000, 500, 300, 250, 200, 100 };
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }

    public override void OnClose()
    {
        if (refresh != null)
            Main.Ins.StopCoroutine(refresh);
        refresh = null;
    }

    void Init()
    {
        RobotRoot = Control("PageView");
        Control("Close").GetComponent<Button>().onClick.AddListener(OnBackPress);
        for (int i = 0; i < 12; i++)
        {
            int k = i;
            Control(string.Format("Weapon{0}", i)).GetComponent<Toggle>().onValueChanged.AddListener((bool select) =>
            {
                if (select)
                {
                    weaponIdx = k;
                }
            });
        }
        for (int i = 0; i < 3; i++)
        {
            int k = i;
            Control(string.Format("Camp{0}", i)).GetComponent<Toggle>().onValueChanged.AddListener((bool select) =>
            {
                if (select)
                {
                    campIdx = k;
                }
            });
        }

        for (int i = 0; i < 6; i++)
        {
            int k = i;
            Control(string.Format("HP{0}", i)).GetComponent<Toggle>().onValueChanged.AddListener((bool select) =>
            {
                if (select)
                {
                    hpIdx = k;
                }
            });
        }
        refresh = Main.Ins.StartCoroutine(RefreshRobot());
    }

    Coroutine refresh = null;
    IEnumerator RefreshRobot()
    {
        List<int> l = U3D.GetUnitList();
        for (int i = 0; i < l.Count; i++)
        {
            AddRobot(l[i]);
            yield return 0;
        }
        refresh = null;
    }

    Dictionary<int, GameObject> RobotList = new Dictionary<int, GameObject>();
    void AddRobot(int Idx)
    {
        if (RobotList.ContainsKey(Idx))
        {
            RobotList[Idx].GetComponent<Button>().onClick.RemoveAllListeners();
            RobotList[Idx].GetComponent<Button>().onClick.AddListener(() => { SpawnRobot(Idx, (EUnitCamp)campIdx); });
            RobotList[Idx].GetComponentInChildren<Text>().text = string.Format("{0}", CombatData.Ins.GetCharacterName(Idx));
        }
        else
        {
            GameObject obj = GameObject.Instantiate(Resources.Load("GridItemBtn")) as GameObject;
            obj.GetComponent<Button>().onClick.AddListener(() => { SpawnRobot(Idx, (EUnitCamp)campIdx); });
            obj.GetComponentInChildren<Text>().text = string.Format("{0}", CombatData.Ins.GetCharacterName(Idx));
            obj.transform.SetParent(RobotRoot.transform);
            obj.gameObject.layer = RobotRoot.layer;
            obj.transform.localScale = Vector3.one;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            RobotList.Add(Idx, obj);
        }
    }

    void SpawnRobot(int idx, EUnitCamp camp)
    {
        U3D.SpawnRobot(idx, camp, weaponIdx, hpArray[hpIdx]);
    }
}
