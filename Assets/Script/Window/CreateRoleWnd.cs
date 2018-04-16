//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections;
//using System.Collections.Generic;
//using CoClass;
//using System.IO;
//using ProtoBuf;

//public class CreateRoleWnd : Window<CreateRoleWnd>
//{
//    MeteorUnit Unit;
//    public int[] HeroId = { 0, 1 };
//    public override string PrefabName { get { return "MainWnd"; } }
//    protected override bool OnOpen()
//    {
//        Init();
//        return base.OnOpen();
//    }

//    protected override bool OnClose()
//    {
//        return base.OnClose();
//    }

//    int heroIndex = 0;//0-19都是原流星里的角色
//    Button ctrl0;
//    Button ctrl1;
//	void Init()
//	{
//        ctrl0 = Control("NewGame").GetComponent<Button>();
//        ctrl1 = Control("LoadState").GetComponent<Button>();
//        MonsterUnit mon0 = GameData.FindMonsterById(1000);
//        Control("NewGame").GetComponent<Button>().onClick.AddListener(()=> {
//            OnNewGame();
//        });
//        Control("LoadState").GetComponent<Button>().onClick.AddListener(()=> {
//            if (StateWnd.Exist)
//                StateWnd.Instance.Close();
//            if (!StateWnd.Exist)
//                StateWnd.Instance.Open();
//            StateWnd.Instance.ShowState();//选择一个存档加载
//        });
//        Control("Enter").GetComponent<Button>().onClick.AddListener(OnEnterGame);
//        LoadRoleState();
//	}

//    void CharactorModelInit(int hero)
//    {
//        GameObject obj = new GameObject();
//        obj.transform.localScale = Vector3.one;
//        obj.transform.localRotation = Quaternion.identity;
//        obj.transform.localPosition = Vector3.zero;
//        Unit = obj.gameObject.AddComponent<MeteorUnit>();
//        Unit.Camp = EUnitCamp.EUC_FRIEND;
//        Unit.Init(hero, PlayerEx.Instance.Heros[0], true);
//        MeteorManager.Instance.LocalPlayer = Unit;
//        MeteorManager.Instance.OnGenerateUnit(Unit);
//        obj.transform.SetParent(null);
//        obj.transform.localScale = Vector3.one;
//        obj.transform.localRotation = Quaternion.identity;
//        obj.transform.localPosition = Vector3.zero;
//        int monsterId = hero + 1000;
//        obj.name = monsterId.ToString();
//        Unit.posMng.ChangeAction(CommonAction.Taunt);
//        //PressDrag PDScript = Hero.GetComponentInChildren<PressDrag>();
//        //if (null != PDScript) {
//        //    PDScript.target = Unit.transform;
//        //} else {
//        //    PDScript = Hero.GetComponentInChildren<PressDrag>();
//        //    PDScript.target = Unit.transform;
//        //}
//    }

//    public void UpdateInfo()
//    {
//        if (MeteorManager.Instance.LocalPlayer == Unit)
//        {
//            MeteorManager.Instance.LocalPlayer = null;
//            CharactorModelInit(0);
//            if (FightWndEx.Exist)
//                FightWndEx.Instance.UpdatePlayerInfo();
//        }
//    }

//    void LoadRoleState()
//    {
//        bool success = GameData.LoadState(Startup.ins.state.saveSlot);
//        if (success)
//        {
//            saveSlot = Startup.ins.state.saveSlot;
//            CharactorModelInit(0);
//            FightWndEx.Instance.Open();
//        }
//        else
//        {
//            GameData.Initialize();
//            CharactorModelInit(0);
//            FightWndEx.Instance.Open();
//        }
//    }

//    public void OnEnterGame()
//    {
//        if (Unit != null)
//            WsWindowEx.OpenSinglePanel<AdminDebugPanel>();
//    }

//    bool SaveIsFull(out int EmptySlotIndex)
//    {
//        for (int i = 0; i <= 2; i++)
//        {
//            string file = Application.persistentDataPath + "/" + i + "/" + "role_state.dat";
//            SaveSlot saveslot = null;
//            if (File.Exists(file))
//            {
//                FileStream fs = null;
//                try
//                {
//                    fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
//                    saveslot = Serializer.Deserialize<SaveSlot>(fs);
//                    if (saveslot == null)
//                    {
//                        EmptySlotIndex = i;
//                        fs.Close();
//                        return false;
//                    }
//                    fs.Close();
//                }
//                catch (System.Exception exp)
//                {
//                    Debug.LogError(exp.Message);
//                    if (fs != null)
//                        fs.Close();
//                    saveslot = null;
//                    EmptySlotIndex = i;
//                    return false;
//                }
//            }
//        }
//        EmptySlotIndex = -1;
//        return true;
//    }

//    int saveSlot = -1;
//    void OnNewGame()
//    {
//        if (saveSlot == -1)
//        {
//            //如果全部存档已满，则提示选择一个存档覆盖，否则找到第一个空存档使用
//            if (SaveIsFull(out saveSlot))
//            {
//                StateWnd.Instance.Open();
//                StateWnd.Instance.ShowState();
//                U3D.PopupTip("存档已满,为新的旅程选择存档槽");
//                return;
//            }
//            else
//            {
//                Startup.ins.state.saveSlot = saveSlot;
//            }
//        }
//        //调试关卡走下面，否则直接用当前存档进去.
//        bool exist = GameData.LoadState(Startup.ins.state.saveSlot);
//        if (exist)
//        {
//            try
//            {
//                Startup.ins.GameStartFromSlot();
//            }
//            catch
//            {
//                U3D.PopupTip((Startup.ins.state.saveSlot + 1) + "号存档已经损坏");
//            }
//            return;
//        }
//        else
//        {
//        }

//        //这段代码用于场景调试
//        if (Unit != null)
//            WsWindowEx.OpenSinglePanel<AdminDebugPanel>();
//    }
//}
