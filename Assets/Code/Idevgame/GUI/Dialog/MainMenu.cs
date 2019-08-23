using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using System.IO;
using ProtoBuf;
using System;
using protocol;
using Idevgame.Util;
using System.Net;
using UnityEngine.Networking;
using DG.Tweening;
using Ionic.Zlib;
using Idevgame.GameState.DialogState;

public class RoomChatWnd:Dialog
{
    //GameObject Root;
    //public override string PrefabName
    //{
    //    get
    //    {
    //        return "RoomChatWnd";
    //    }
    //}

    //Sequence hide;
    //protected override bool OnOpen()
    //{
    //    Init();
    //    return base.OnOpen();
    //}

    //Scrollbar vscrollBar;
    //void Init()
    //{
    //    Root = Control("LevelTalk");
    //    WndObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(145, 400);
    //    vscrollBar = Control("Scrollbar Vertical").GetComponent<Scrollbar>();
    //}

    //protected override bool OnClose()
    //{
    //    return base.OnClose();
    //}

    ////加入声音UI按钮
    //Dictionary<int, byte[]> audioCache = new Dictionary<int, byte[]>();
    //public void Add(int playerId, byte[] audio)
    //{
    //    if (Root.transform.childCount >= 20)
    //    {
    //        if (audioCache.ContainsKey(0))
    //            audioCache.Remove(0);
    //        GameObject.Destroy(Root.transform.GetChild(0).gameObject);
    //    }
    //    GameObject obj = GameObject.Instantiate(Resources.Load("AudioMsg")) as GameObject;
    //    obj.name = (Root.transform.childCount + 1).ToString();
    //    obj.transform.GetChild(0).GetComponent<Text>().text = NetWorkBattle.Instance.GetNetPlayerName(playerId) + "发来了一条语音信息";
    //    obj.transform.GetChild(1).GetComponent<UIFunCtrl>().SetEvent((int audioIdx) => { OnPlayAudio(audioIdx); }, Root.transform.childCount);
    //    obj.transform.SetParent(Root.transform);
    //    obj.transform.localScale = Vector3.one;
    //    obj.transform.localPosition = Vector3.zero;
    //    obj.transform.localRotation = Quaternion.identity;
    //    if (hide != null)
    //        hide.Kill(false);
    //    WndObject.GetComponent<CanvasGroup>().alpha = 1.0f;
    //    hide = DOTween.Sequence();
    //    hide.AppendInterval(8.0f);
    //    hide.Append(WndObject.GetComponent<CanvasGroup>().DOFade(0, 5.0f));
    //    hide.Play();
    //    audioCache.Add(Root.transform.childCount - 1, audio);
    //    vscrollBar.value = 0;
    //}



    //void OnPlayAudio(int audioIndex)
    //{
    //    if (audioCache.ContainsKey(audioIndex))
    //    {
    //        Main.Instance.Sound.Stop();
    //        Main.Instance.Music.Stop();
    //        AudioClip au = AudioClip.Create("talk", audioCache[audioIndex].Length, 1, MicChat.samplingRate, false);
    //        MicChat.SetData(au, audioCache[audioIndex]);
    //        SoundManager.Instance.PlayClip(au);
    //    }
    //}

    //public void Add(int playerId, string message)
    //{
    //    if (Root.transform.childCount >= 20)
    //        GameObject.Destroy(Root.transform.GetChild(0).gameObject);
    //    GameObject obj = new GameObject();
    //    obj.name = (Root.transform.childCount + 1).ToString();
    //    Text txt = obj.AddComponent<Text>();
    //    txt.text = string.Format("{0}:{1}", NetWorkBattle.Instance.GetNetPlayerName(playerId), message);
    //    //00AAFFFF
    //    txt.font = Main.Instance.TextFont;
    //    txt.fontSize = 32;
    //    txt.alignment = TextAnchor.MiddleLeft;
    //    txt.raycastTarget = false;
    //    txt.color = new Color(1.0f, 1.0f, 1.0f, 1f);
    //    obj.transform.SetParent(Root.transform);
    //    obj.transform.localScale = Vector3.one;
    //    obj.transform.localPosition = Vector3.zero;
    //    obj.transform.localRotation = Quaternion.identity;
    //    if (hide != null)
    //        hide.Kill(false);
    //    WndObject.GetComponent<CanvasGroup>().alpha = 1.0f;
    //    hide = DOTween.Sequence();
    //    hide.AppendInterval(8.0f);
    //    hide.Append(WndObject.GetComponent<CanvasGroup>().DOFade(0, 5.0f));
    //    hide.Play();
    //    vscrollBar.value = 0;
    //}
}

public class ChatWnd : Dialog
{
    //public override string PrefabName
    //{
    //    get
    //    {
    //        return "ChatWnd";
    //    }
    //}

    //protected override bool OnOpen()
    //{
    //    Init();
    //    return base.OnOpen();
    //}

    //protected override bool OnClose()
    //{
    //    if (Microphone.IsRecording(null))
    //    {
    //        Record();
    //    }
    //    return base.OnClose();
    //}

    
    //InputField inputChat;
    //LitJson.JsonData jsData;
    //void Init()
    //{
    //    inputChat = Control("ChatText", WndObject).GetComponent<InputField>();
    //    Control("SendShortMsg", WndObject).GetComponent<Button>().onClick.AddListener(() => {
    //        if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
    //        {
    //            string chatMessage = inputChat.text;
    //            if (string.IsNullOrEmpty(inputChat.text))
    //            {
    //                U3D.PopupTip("无法发送内容为空的语句");
    //                return;
    //            }
    //            Common.SendChatMessage(chatMessage);
    //        }
    //    });
    //    Control("CloseQuickMsg", WndObject).GetComponent<Button>().onClick.AddListener(() => {
    //        Close();
    //    });
    //    Control("CloseShortMsg", WndObject).GetComponent<Button>().onClick.AddListener(() => {
    //        Close();
    //    });
    //    Control("CloseAudioMsg", WndObject).GetComponent<Button>().onClick.AddListener(() => {
    //        Close();
    //    });
    //    if (jsData == null)
    //    {
    //        TextAsset text = ResMng.Load("MsgTable") as TextAsset;
    //        jsData = LitJson.JsonMapper.ToObject(text.text);
    //    }
    //    for (int i = 0; i < jsData["Msg"].Count; i++)
    //    {
    //        string strQuick = jsData["Msg"][i].ToString();
    //        Control(i.ToString(), WndObject).GetComponentInChildren<Text>().text = strQuick;
    //        int j = i;
    //        Control(i.ToString(), WndObject).GetComponent<Button>().onClick.AddListener(()=> { SendQuickMsg(j); });
    //    }
    //    Control("Record", WndObject).GetComponent<Button>().onClick.AddListener(()=> { Record(); });
    //    Control("SendAudio", WndObject).GetComponent<Button>().onClick.AddListener(() => { SendAudioMsg(); });
    //    GameObject objListen = Control("Listen", WndObject);
    //    source = objListen.GetComponent<AudioSource>();
    //    Listen = objListen.GetComponent<Button>();
    //    Listen.onClick.AddListener(() => {
    //        if (MicChat.clip != null)
    //            source.PlayOneShot(MicChat.clip);
    //        Debug.Log("play clip");
    //    });
    //    CountDown = Control("CountDown", WndObject);
    //}

    //void SendQuickMsg(int i)
    //{
    //    Common.SendChatMessage(jsData["Msg"][i].ToString());
    //}

    //void SendAudioMsg()
    //{
    //    if (!recording && audioData != null && audioData.Length != 0 && Listen != null && Listen.IsActive())
    //    {
    //        Common.SendAudioMessage(audioData);
    //    }
    //}

    //public static byte[] ConvertBytesZlib(byte[] data, CompressionMode compressionMode)
    //{
    //    CompressionMode mode = compressionMode;
    //    if (mode != CompressionMode.Compress)
    //    {
    //        if (mode != CompressionMode.Decompress)
    //        {
    //            throw new NotImplementedException();
    //        }
    //        return ZlibStream.UncompressBuffer(data);
    //    }
    //    return ZlibStream.CompressBuffer(data);
    //}

    //bool recording = false;
    //byte[] audioData;
    //AudioSource source;
    //AudioClip clip;
    //Button Listen;
    //GameObject CountDown;
    //float RecordTick = 0;
    //void Record()
    //{
    //    if (recording)
    //    {
    //        SoundManager.Instance.Mute(false);
    //        int length = 0;
    //        clip = null;
    //        MicChat.EndRecording(out length, out clip);
    //        recording = false;
    //        CountDown.SetActive(false);
    //        audioData = clip.GetData();
    //        if (audioData != null && audioData.Length != 0)
    //            Listen.gameObject.SetActive(true);
    //        Control("Record", WndObject).GetComponentInChildren<Text>().text = "录音";
    //        if (GameBattleEx.Instance != null)
    //            GameBattleEx.Instance.OnUpdates -= Update;
    //        return;
    //    }
    //    else
    //    {
    //        //把音效和音乐全部静止
    //        SoundManager.Instance.Mute(true);
    //        recording = MicChat.TryStartRecording();
    //        if (recording)
    //        {
    //            Control("Record", WndObject).GetComponentInChildren<Text>().text = "停止";
    //            if (Listen == null)
    //            {
    //                GameObject objListen = Control("Listen", WndObject);
    //                Listen = objListen.GetComponent<Button>();
    //                objListen.SetActive(false);
    //            }
    //            else
    //            {
    //                Listen.gameObject.SetActive(false);
    //            }
    //            CountDown.SetActive(true);
    //            GameBattleEx.Instance.OnUpdates += Update;
    //            RecordTick = 0;
    //            nSeconds = Mathf.CeilToInt(MicChat.maxRecordTime - RecordTick);
    //            CountDown.GetComponentInChildren<Text>().text = string.Format("录制中{0}", nSeconds);
    //        }
    //    }
    //}

    //int nSeconds = 0;
    //void Update()
    //{
    //    Debug.Log("chatwnd update");
    //    RecordTick += FrameReplay.deltaTime;
    //    if (RecordTick >= 8)
    //    {
    //        Record();
    //    }
    //    else
    //    {
    //        int i = Mathf.CeilToInt(MicChat.maxRecordTime - RecordTick);
    //        if (nSeconds != i)
    //        {
    //            nSeconds = i;
    //            CountDown.GetComponentInChildren<Text>().text = string.Format("录制中{0}", nSeconds);
    //        }
    //    }
    //}
}

public class GunShootUI:Dialog
{
    //public override string PrefabName
    //{
    //    get
    //    {
    //        return "GunShootUI";
    //    }
    //}
}

public class RoomOptionWnd:Dialog
{
    //public override string PrefabName
    //{
    //    get
    //    {
    //        return "RoomOption";
    //    }
    //}

    //protected override bool OnOpen()
    //{
    //    Init();
    //    return base.OnOpen();
    //}

    //GameObject TemplateRoot;

    //int[] ConstRoundTime = { 15, 30, 60 };
    //int[] ConstPlayer = { 2, 4, 8, 12, 16 };
    //protocol.RoomInfo.RoomPattern[] ConstPattern = { protocol.RoomInfo.RoomPattern._Normal, protocol.RoomInfo.RoomPattern._Record, RoomInfo.RoomPattern._Replay };
    //InputField roomName;
    //InputField roomSecret;
    //void Init()
    //{
    //    roomName = Control("RoomNameInput").GetComponent<UnityEngine.UI.InputField>();
    //    roomName.text = string.Format("{0}{1}", GameData.Instance.gameStatus.NickName, "的房间");
    //    roomSecret = Control("RoomSecretInput").GetComponent<UnityEngine.UI.InputField>();
    //    roomSecret.text = "";
    //    Control("CreateWorld").GetComponent<Button>().onClick.AddListener(() =>
    //    {
    //        OnCreateRoom();
    //    });
    //    GameObject RuleGroup = Control("RuleGroup", WndObject);
    //    Toggle rule0 = Control("0", RuleGroup).GetComponent<Toggle>();
    //    Toggle rule1 = Control("1", RuleGroup).GetComponent<Toggle>();
    //    Toggle rule2 = Control("2", RuleGroup).GetComponent<Toggle>();
    //    rule0.isOn = GameData.Instance.gameStatus.NetWork.Mode == (int)GameMode.MENGZHU;
    //    rule1.isOn = GameData.Instance.gameStatus.NetWork.Mode == (int)GameMode.ANSHA;
    //    rule2.isOn = GameData.Instance.gameStatus.NetWork.Mode == (int)GameMode.SIDOU;

    //    rule0.onValueChanged.AddListener((bool select) => { if (select) GameData.Instance.gameStatus.NetWork.Mode = (int)GameMode.MENGZHU; });
    //    rule1.onValueChanged.AddListener((bool select) => { if (select) GameData.Instance.gameStatus.NetWork.Mode = (int)GameMode.ANSHA; });
    //    rule2.onValueChanged.AddListener((bool select) => { if (select) GameData.Instance.gameStatus.NetWork.Mode = (int)GameMode.SIDOU; });

    //    GameObject LifeGroup = Control("LifeGroup", WndObject);
    //    Toggle Life0 = Control("0", LifeGroup).GetComponent<Toggle>();
    //    Toggle Life1 = Control("1", LifeGroup).GetComponent<Toggle>();
    //    Toggle Life2 = Control("2", LifeGroup).GetComponent<Toggle>();

    //    Life0.isOn = GameData.Instance.gameStatus.NetWork.Life == 500;
    //    Life1.isOn = GameData.Instance.gameStatus.NetWork.Life == 200;
    //    Life2.isOn = GameData.Instance.gameStatus.NetWork.Life == 100;
    //    Life0.onValueChanged.AddListener((bool select) => { if (select) GameData.Instance.gameStatus.NetWork.Life = 500; });
    //    Life1.onValueChanged.AddListener((bool select) => { if (select) GameData.Instance.gameStatus.NetWork.Life = 200; });
    //    Life2.onValueChanged.AddListener((bool select) => { if (select) GameData.Instance.gameStatus.NetWork.Life = 100; });

    //    Control("Return").GetComponent<Button>().onClick.AddListener(() =>
    //    {
    //        GameData.Instance.SaveState();
    //        MainLobby.Instance.Open();
    //        Close();
    //    });

    //    //地图模板，应该从所有地图表里获取，包括外部载入的地图.
    //    TemplateRoot = Control("WorldRoot", WndObject);
    //    Level[] allLevel = Global.Instance.GetAllLevel();
    //    for (int i = 0; i < allLevel.Length; i++)
    //    {
    //        Level lev = allLevel[i];
    //        if (lev == null)
    //            continue;
    //        AddGridItem(lev, TemplateRoot.transform);
    //    }
    //    select = Global.Instance.GetLevel(GameData.Instance.gameStatus.ChapterTemplate, GameData.Instance.gameStatus.NetWork.LevelTemplate);
    //    OnSelectLevel(select);

    //    GameObject TimeGroup = Control("GameTime", WndObject);
    //    for (int i = 0; i < 3; i++)
    //    {
    //        Toggle TimeToggle = Control(string.Format("{0}", i), TimeGroup).GetComponent<Toggle>();
    //        var k = i;
    //        TimeToggle.isOn = GameData.Instance.gameStatus.NetWork.RoundTime == ConstRoundTime[k];
    //        TimeToggle.onValueChanged.AddListener((bool selected) => { if (selected) GameData.Instance.gameStatus.NetWork.RoundTime = ConstRoundTime[k]; });
    //    }

    //    GameObject PlayerGroup = Control("PlayerGroup", WndObject);
    //    for (int i = 0; i <= 4; i++)
    //    {
    //        Toggle PlayerToggle = Control(string.Format("{0}", i), PlayerGroup).GetComponent<Toggle>();
    //        var k = i;
    //        PlayerToggle.isOn = GameData.Instance.gameStatus.NetWork.MaxPlayer == ConstPlayer[k];
    //        PlayerToggle.onValueChanged.AddListener((bool selected) =>
    //        {
    //            if (selected)
    //                GameData.Instance.gameStatus.NetWork.MaxPlayer = ConstPlayer[k];
    //        });
    //    }
    //    GameObject GameRecord = Control("GameRecord", WndObject);
    //    GameRecord.SetActive(GameData.Instance.gameStatus.NetWork.Pattern != (int)protocol.RoomInfo.RoomPattern._Normal);
    //    GameObject UIFuncItem = Control("UIFuncItem", GameRecord);
    //    GameObject filePath = Control("FilePath", GameRecord);
    //    filePath.GetComponent<Text>().text = "";
    //    UIFuncItem.GetComponent<Button>().onClick.AddListener(()=> {
    //        if (!RecordSelectWnd.Exist)
    //            RecordSelectWnd.Instance.Open();
    //    });
    //    GameObject PatternGroup = Control("PatternGroup", WndObject);
    //    for (int i = 0; i < 3; i++)
    //    {
    //        Toggle PatternToggle = Control(string.Format("{0}", i), PatternGroup).GetComponent<Toggle>();
    //        var k = i;
    //        PatternToggle.isOn = GameData.Instance.gameStatus.NetWork.Pattern == (int)ConstPattern[k];
    //        PatternToggle.onValueChanged.AddListener((bool selected) =>
    //        {
    //            if (selected)
    //                GameData.Instance.gameStatus.NetWork.Pattern = (int)ConstPattern[k];
    //            //隐藏/打开选择录像文件路径面板.
    //            GameRecord.SetActive(GameData.Instance.gameStatus.NetWork.Pattern != (int)protocol.RoomInfo.RoomPattern._Normal);
    //        });
    //    }
    //}

    //Level select;
    //void OnSelectLevel(Level lev)
    //{
    //    select = lev;
    //    GameData.Instance.gameStatus.NetWork.LevelTemplate = lev.ID;
    //    Control("Task").GetComponent<Text>().text = select.Name;
    //}

    //void OnCreateRoom()
    //{
    //    if (select != null)
    //    {
    //        //Global.Instance.PlayerLife = GameData.Instance.gameStatus.NetWork.Life;
    //        //Global.Instance.RoundTime = GameData.Instance.gameStatus.NetWork.RoundTime;
    //        //Global.Instance.MaxPlayer = GameData.Instance.gameStatus.NetWork.MaxPlayer;
    //        if (string.IsNullOrEmpty(roomName.text))
    //        {
    //            U3D.PopupTip("需要设置房间名");
    //            return;
    //        }
    //        GameData.Instance.gameStatus.NetWork.RoomName = roomName.text;
    //        Common.CreateRoom(roomName.text, roomSecret.text);
    //        //U3D.LoadLevel(select.ID, LevelMode.CreateWorld, (GameMode)GameData.Instance.gameStatus.NetWork.Mode);
    //    }
    //}

    //void AddGridItem(Level lev, Transform parent)
    //{
    //    GameObject objPrefab = Resources.Load("LevelSelectItem", typeof(GameObject)) as GameObject;
    //    GameObject obj = GameObject.Instantiate(objPrefab) as GameObject;
    //    obj.transform.SetParent(parent);
    //    obj.name = lev.Name;
    //    obj.GetComponent<Button>().onClick.AddListener(() => { OnSelectLevel(lev); });
    //    obj.GetComponentInChildren<Text>().text = lev.Name;
    //    obj.transform.localPosition = Vector3.zero;
    //    obj.transform.localScale = Vector3.one;
    //}
}

//显示选择本地录像文件路径（存储到/选择）
public class RecordSelectWnd:Dialog
{
    //public override string PrefabName
    //{
    //    get
    //    {
    //        return "RecordSelectWnd";
    //    }
    //}

    //protected override bool OnOpen()
    //{
    //    Init();
    //    return base.OnOpen();
    //}

    //protected override bool OnClose()
    //{
    //    if (refreshRec != null)
    //    {
    //        Main.Instance.StopCoroutine(refreshRec);
    //        refreshRec = null;
    //    }
    //    return base.OnClose();
    //}

    //static int recCurrent;
    //List<string> recList;
    //GameObject root;
    //Coroutine refreshRec;
    //GameObject Prefab;
    //void Init()
    //{
    //    if (recList == null)
    //        recList = new List<string>();
    //    string recDir = string.Format("{0}/record/", Application.persistentDataPath);
    //    if (System.IO.Directory.Exists(recDir))
    //    {
    //        string[] files = System.IO.Directory.GetFiles(recDir);
    //        for (int i = 0; i < files.Length; i++)
    //            recList.Add(files[i]);
    //    }
    //    else
    //    {
    //        System.IO.Directory.CreateDirectory(recDir);
    //    }

    //    //一次加载全部录像存到
    //    root = Control("content");
    //    recCurrent = 0;
    //    //打开本地已存储的录像列表，UI上带新建，选择，删除，关闭/退出等
    //    Control("Colse").GetComponent<Button>().onClick.AddListener(()=> {
    //        Close();
    //    });

    //    Control("Deleted").GetComponent<Button>().onClick.AddListener(() => {
    //        //如果当前选择了某个录像，则删除
    //        if (recCurrent != -1 && recList.Count > recCurrent)
    //            recList.RemoveAt(recCurrent);
    //    });

    //    Control("Create").GetComponent<Button>().onClick.AddListener(() =>
    //    {
    //        //创建一个文件
    //        for (int i = 0; i < recList.Count; i++)
    //        {
    //            string s = string.Format("{0}/rec/{1}", Application.persistentDataPath, i + ".mrc");
    //            if (recList[i] != s)
    //            {
    //                if (!System.IO.File.Exists(s))
    //                {
    //                    System.IO.File.Create(s);
    //                    recList.Insert(i, s);
    //                    RefreshAuto();
    //                    return;
    //                }
    //            }
    //        }
            
    //        string file = string.Format("{0}/rec/{1}", Application.persistentDataPath, recList.Count + ".mrc");
    //        if (!System.IO.File.Exists(file))
    //        {
    //            System.IO.File.Create(file);
    //            recList.Add(file);
    //            RefreshAuto();
    //            return;
    //        }

    //        U3D.PopupTip("无法创建更多录像文件");
    //    });

    //    Prefab = Resources.Load("ButtonNormal") as GameObject;
    //    if (recList.Count != 0)
    //    {
    //        refreshRec = Main.Instance.StartCoroutine(Refresh());
    //    }
    //}

    //void RefreshAuto()
    //{
    //    if (refreshRec != null)
    //    {
    //        Main.Instance.StopCoroutine(refreshRec);
    //        refreshRec = null;
    //    }

    //    refreshRec = Main.Instance.StartCoroutine(Refresh());
    //}

    //IEnumerator Refresh()
    //{
    //    for (int i = 0; i < recList.Count; i++)
    //    {
    //        AddRecOnGrid(recList[i]);
    //        yield return 0;
    //    }
    //}

    //void AddRecOnGrid(string rec)
    //{
    //    GameObject btn = GameObject.Instantiate(Prefab);
    //    btn.transform.SetParent(root.transform);
    //    btn.transform.localPosition = Vector3.zero;
    //    btn.transform.localRotation = Quaternion.identity;
    //    btn.transform.localScale = Vector3.one;
    //    btn.GetComponentInChildren<Text>().text = rec;
    //    btn.GetComponent<Button>().onClick.AddListener(()=> {
    //        string r = rec;
    //        recCurrent = recList.IndexOf(r);
    //        btn.GetComponentInChildren<Text>().color = Color.blue;
    //    });
    //}
}

//单机创建房间面板
public class WorldTemplateWnd : Dialog
{
    //public override string PrefabName
    //{
    //    get
    //    {
    //        return "WorldTemplate";
    //    }
    //}

    //protected override bool OnOpen()
    //{
    //    Init();
    //    return base.OnOpen();
    //}

    //GameObject TemplateRoot;
    
    //int[] ConstRoundTime = {15, 30, 60};
    //int[] ConstPlayer = {2, 4, 8, 12, 16};
    //void Init()
    //{
    //    Control("CreateWorld").GetComponent<Button>().onClick.AddListener(()=>
    //    {
    //        OnEnterLevel();
    //    });
    //    GameObject RuleGroup = Control("RuleGroup", WndObject);
    //    Toggle rule0 = Control("0", RuleGroup).GetComponent<Toggle>();
    //    Toggle rule1 = Control("1", RuleGroup).GetComponent<Toggle>();
    //    Toggle rule2 = Control("2", RuleGroup).GetComponent<Toggle>();
    //    rule0.isOn = GameData.Instance.gameStatus.Single.Mode == (int)GameMode.MENGZHU;
    //    rule1.isOn = GameData.Instance.gameStatus.Single.Mode == (int)GameMode.ANSHA;
    //    rule2.isOn = GameData.Instance.gameStatus.Single.Mode == (int)GameMode.SIDOU;

    //    rule0.onValueChanged.AddListener((bool select) => { if (select) GameData.Instance.gameStatus.Single.Mode = (int)GameMode.MENGZHU;});
    //    rule1.onValueChanged.AddListener((bool select) => { if (select) GameData.Instance.gameStatus.Single.Mode = (int)GameMode.ANSHA; });
    //    rule2.onValueChanged.AddListener((bool select) => { if (select) GameData.Instance.gameStatus.Single.Mode = (int)GameMode.SIDOU; });

    //    GameObject LifeGroup = Control("LifeGroup", WndObject);
    //    Toggle Life0 = Control("0", LifeGroup).GetComponent<Toggle>();
    //    Toggle Life1 = Control("1", LifeGroup).GetComponent<Toggle>();
    //    Toggle Life2 = Control("2", LifeGroup).GetComponent<Toggle>();

    //    Life0.isOn = GameData.Instance.gameStatus.Single.Life == 500;
    //    Life1.isOn = GameData.Instance.gameStatus.Single.Life == 200;
    //    Life2.isOn = GameData.Instance.gameStatus.Single.Life == 100;
    //    Life0.onValueChanged.AddListener((bool select) => { if (select) GameData.Instance.gameStatus.Single.Life = 500; });
    //    Life1.onValueChanged.AddListener((bool select) => { if (select) GameData.Instance.gameStatus.Single.Life = 200; });
    //    Life2.onValueChanged.AddListener((bool select) => { if (select) GameData.Instance.gameStatus.Single.Life = 100; });

    //    GameObject MainWeaponGroup = Control("FirstWeapon", WndObject);
    //    GameObject WeaponGroup = Control("WeaponGroup", MainWeaponGroup);
    //    for (int i = 0; i <= 11; i++)
    //    {
    //        Toggle MainWeapon = Control(string.Format("{0}", i), WeaponGroup).GetComponent<Toggle>();
    //        MainWeapon.isOn = GameData.Instance.gameStatus.Single.Weapon0 == i;
    //        MainWeapon.onValueChanged.AddListener(OnMainWeaponSelected);
    //    }

    //    GameObject SubWeaponGroup = Control("SubWeapon", WndObject);
    //    WeaponGroup = Control("WeaponGroup", SubWeaponGroup);
    //    for (int i = 0; i <= 11; i++)
    //    {
    //        Toggle subWeapon = Control(string.Format("{0}", i), WeaponGroup).GetComponent<Toggle>();
    //        subWeapon.isOn = GameData.Instance.gameStatus.Single.Weapon1 == i;
    //        subWeapon.onValueChanged.AddListener(OnSubWeaponSelected);
    //    }

    //    Control("Return").GetComponent<Button>().onClick.AddListener(() =>
    //    {
    //        GameData.Instance.SaveState();
    //        MainWnd.Instance.Open();
    //        Close();
    //    });

    //    //地图模板，应该从所有地图表里获取，包括外部载入的地图.
    //    TemplateRoot = Control("WorldRoot", WndObject);
    //    Level[] allLevel = Global.Instance.GetAllLevel();
    //    for (int i = 0; i < allLevel.Length; i++)
    //    {
    //        Level lev = allLevel[i];
    //        if (lev == null)
    //            continue;
    //        AddGridItem(lev, TemplateRoot.transform);
    //    }
    //    select = Global.Instance.GetLevel(GameData.Instance.gameStatus.ChapterTemplate, GameData.Instance.gameStatus.Single.LevelTemplate);
    //    OnSelectLevel(select);

    //    GameObject ModelGroup = Control("ModelGroup");
    //    for (int i = 0; i < 20; i++)
    //    {
    //        Toggle modelTog = Control(string.Format("{0}", i), ModelGroup).GetComponent<Toggle>();
    //        Text t = modelTog.GetComponentInChildren<Text>();
    //        t.text = ModelMng.Instance.GetAllItem()[i].Name;
    //        var k = i;
    //        modelTog.isOn = GameData.Instance.gameStatus.Single.Model == i;
    //        modelTog.onValueChanged.AddListener((bool select) => { if (select) GameData.Instance.gameStatus.Single.Model = k; });
    //    }

    //    GameObject TimeGroup = Control("GameTime", WndObject);
    //    for (int i = 0; i < 3; i++)
    //    {
    //        Toggle TimeToggle = Control(string.Format("{0}", i), TimeGroup).GetComponent<Toggle>();
    //        var k = i;
    //        TimeToggle.isOn = GameData.Instance.gameStatus.Single.RoundTime == ConstRoundTime[k];
    //        TimeToggle.onValueChanged.AddListener((bool selected) => { if (selected) GameData.Instance.gameStatus.Single.RoundTime = ConstRoundTime[k]; });
    //    }

    //    GameObject PlayerGroup = Control("PlayerGroup", WndObject);
    //    for (int i = 0; i <= 4; i++)
    //    {
    //        Toggle PlayerToggle = Control(string.Format("{0}", i), PlayerGroup).GetComponent<Toggle>();
    //        var k = i;
    //        PlayerToggle.isOn = GameData.Instance.gameStatus.Single.MaxPlayer == ConstPlayer[k];
    //        PlayerToggle.onValueChanged.AddListener((bool selected) => 
    //        {
    //            if (selected)
    //                GameData.Instance.gameStatus.Single.MaxPlayer = ConstPlayer[k];
    //        });
    //    }

    //    GameObject DisallowGroup = Control("DisallowGroup", WndObject);
    //    Toggle DisallowToggle = Control("0", DisallowGroup).GetComponent<Toggle>();
    //    DisallowToggle.isOn = GameData.Instance.gameStatus.Single.DisallowSpecialWeapon;
    //    DisallowToggle.onValueChanged.AddListener((bool selected) => { GameData.Instance.gameStatus.Single.DisallowSpecialWeapon = selected; });
    //}

    //void OnMainWeaponSelected(bool select)
    //{
    //    if (select)
    //    {
    //        GameObject MainWeaponGroup = Control("FirstWeapon", WndObject);
    //        GameObject WeaponGroup = Control("WeaponGroup", MainWeaponGroup);
    //        for (int i = 0; i <= 11; i++)
    //        {
    //            Toggle MainWeapon = Control(string.Format("{0}", i), WeaponGroup).GetComponent<Toggle>();
    //            if (MainWeapon.isOn)
    //            {
    //                GameData.Instance.gameStatus.Single.Weapon0 = i;
    //                break;
    //            }
                
    //        }
    //    }
    //}

    //void OnSubWeaponSelected(bool select)
    //{
    //    if (select)
    //    {
    //        GameObject MainWeaponGroup = Control("SubWeapon", WndObject);
    //        GameObject WeaponGroup = Control("WeaponGroup", MainWeaponGroup);
    //        for (int i = 0; i <= 11; i++)
    //        {
    //            Toggle subWeapon = Control(string.Format("{0}", i), WeaponGroup).GetComponent<Toggle>();
    //            if (subWeapon.isOn)
    //            {
    //                GameData.Instance.gameStatus.Single.Weapon1 = i;
    //                break;
    //            }
    //        }
    //    }
    //}

    //Level select;
    //void OnSelectLevel(Level lev)
    //{
    //    select = lev;
    //    GameData.Instance.gameStatus.Single.LevelTemplate = lev.ID;
    //    Control("Task").GetComponent<Text>().text = select.Name;
    //}

    //void OnEnterLevel()
    //{
    //    if (select != null)
    //    {
    //        Global.Instance.MainWeapon = GameData.Instance.gameStatus.Single.Weapon0;
    //        Global.Instance.SubWeapon = GameData.Instance.gameStatus.Single.Weapon1;
    //        Global.Instance.PlayerLife = GameData.Instance.gameStatus.Single.Life;
    //        Global.Instance.PlayerModel = GameData.Instance.gameStatus.Single.Model;
    //        Global.Instance.RoundTime = GameData.Instance.gameStatus.Single.RoundTime;
    //        Global.Instance.MaxPlayer = GameData.Instance.gameStatus.Single.MaxPlayer;
    //        U3D.LoadLevel(select.ID, LevelMode.CreateWorld, (GameMode)GameData.Instance.gameStatus.Single.Mode);
    //    }
    //}

    //void AddGridItem(Level lev, Transform parent)
    //{
    //    GameObject objPrefab = Resources.Load("LevelSelectItem", typeof(GameObject)) as GameObject;
    //    GameObject obj = GameObject.Instantiate(objPrefab) as GameObject;
    //    obj.transform.SetParent(parent);
    //    obj.name = lev.Name;
    //    obj.GetComponent<Button>().onClick.AddListener(() => { OnSelectLevel(lev); });
    //    obj.GetComponentInChildren<Text>().text = lev.Name;
    //    obj.transform.localPosition = Vector3.zero;
    //    obj.transform.localScale = Vector3.one;
    //}
}

public class WeaponSelectWnd:Dialog
{
    //public override string PrefabName { get { return "WeaponSelectWnd"; } }
    //protected override bool OnOpen()
    //{
    //    Init();
    //    return base.OnOpen();
    //}
    //Image weaponImg;
    //int weaponIdx = 0;

    //void Init()
    //{
    //    weaponImg = ldaControl("Image").GetComponent<Image>();
    //    weaponImg.material = ResMng.Load("Weapon_0") as Material;
    //    Control("Next").GetComponent<Button>().onClick.AddListener(() =>
    //    {
    //        OnNextWeapon();
    //        U3D.PlayBtnAudio();
    //    });
    //    Control("Prev").GetComponent<Button>().onClick.AddListener(() =>
    //    {
    //        OnPrevWeapon();
    //        U3D.PlayBtnAudio();
    //    });
    //    Control("Select").GetComponent<UIButtonExtended>().onClick.AddListener(() =>
    //    {
    //        OnSelectWeapon();
    //        U3D.PlayBtnAudio();
    //    });
    //}

    //void OnNextWeapon()
    //{
    //    weaponIdx += 1;
    //    if (weaponIdx >= 12)
    //        weaponIdx = 0;
    //    weaponImg.material = ResMng.Load(string.Format("Weapon_{0}", weaponIdx)) as Material;
    //}

    //void OnPrevWeapon()
    //{
    //    weaponIdx -= 1;
    //    if (weaponIdx < 0)
    //        weaponIdx = 11;
    //    weaponImg.material = ResMng.Load(string.Format("Weapon_{0}", weaponIdx)) as Material;
    //}

    //void OnSelectWeapon()
    //{
    //    NetWorkBattle.Instance.weaponIdx = U3D.GetWeaponByCode(weaponIdx);
    //    NetWorkBattle.Instance.EnterLevel();
    //    Close();
    //}
}

public class CampSelectWnd : Dialog
{
    //public override string PrefabName { get { return "CampSelectWnd"; } }
    //protected override bool OnOpen()
    //{
    //    Init();
    //    return base.OnOpen();
    //}

    //void Init()
    //{
    //    Control("Meteor").GetComponent<Button>().onClick.AddListener(()=> { NetWorkBattle.Instance.camp = (int)EUnitCamp.EUC_Meteor; RoleSelectWnd.Instance.Open(); Close(); });
    //    Control("Butterfly").GetComponent<Button>().onClick.AddListener(() => { NetWorkBattle.Instance.camp = (int)EUnitCamp.EUC_Butterfly; RoleSelectWnd.Instance.Open(); Close(); });
    //}
}

public class RoleSelectWnd:Dialog
{
    //public override string PrefabName { get { return "RoleSelectWnd"; } }
    //protected override bool OnOpen()
    //{
    //    Init();
    //    return base.OnOpen();
    //}
    //Image heroImg;
    //int heroIdx = 0;
    //void Init()
    //{
    //    heroImg = ldaControl("Image").GetComponent<Image>();
    //    heroImg.material = ResMng.Load("Hero0") as Material;
    //    Control("Next").GetComponent<Button>().onClick.AddListener(() =>
    //    {
    //        OnNextHero();
    //        U3D.PlayBtnAudio();
    //    });
    //    Control("Prev").GetComponent<Button>().onClick.AddListener(() =>
    //    {
    //        OnPrevHero();
    //        U3D.PlayBtnAudio();
    //    });
    //    Control("Select").GetComponent<UIButtonExtended>().onClick.AddListener(() =>
    //    {
    //        OnSelectHero();
    //        U3D.PlayBtnAudio();
    //    });
    //}

    //void OnNextHero()
    //{
    //    heroIdx += 1;
    //    if (heroIdx >= ModelMng.Instance.GetAllItem().Length)
    //        heroIdx = 0;
    //    heroImg.material = ResMng.Load(string.Format("Hero{0}", heroIdx)) as Material;
    //}

    //void OnPrevHero()
    //{
    //    heroIdx -= 1;
    //    if (heroIdx < 0)
    //        heroIdx = ModelMng.Instance.GetAllItem().Length - 1;
    //    heroImg.material = ResMng.Load(string.Format("Hero{0}", heroIdx)) as Material;
    //}

    //void OnSelectHero()
    //{
    //    NetWorkBattle.Instance.heroIdx = heroIdx;
    //    WeaponSelectWnd.Instance.Open();
    //    Close();
    //}
}

//比赛面板
public class MatchWnd:Dialog
{
    //public override string PrefabName { get { return "MatchWnd"; } }
    //protected override bool OnOpen()
    //{
    //    Init();
    //    return base.OnOpen();
    //}

    //protected override bool OnClose()
    //{
    //    FrameReplay.Instance.OnUpdates -= Update;
    //    return base.OnClose();
    //}

    //Text TimesUsed;
    //GameObject btnLeave;
    //System.Timers.Timer EnterQueueTimeOut;
    //System.Timers.Timer LeaveQueueTimeOut;
    //void Init()
    //{
    //    EnterQueueTimeOut = new System.Timers.Timer(5000);
    //    EnterQueueTimeOut.Elapsed += new System.Timers.ElapsedEventHandler(OnEnterTimeOut);
    //    EnterQueueTimeOut.AutoReset = false;

    //    LeaveQueueTimeOut = new System.Timers.Timer(5000);
    //    LeaveQueueTimeOut.Elapsed += new System.Timers.ElapsedEventHandler(OnLeaveTimeOut);
    //    LeaveQueueTimeOut.AutoReset = false;

    //    //排队预计时间-从排队包里取
    //    Control("TimesLeft").GetComponent<Text>().text = "03:00";
    //    TimesUsed = Control("TimesUsed").GetComponent<Text>();
    //    TimesUsed.text = "00:00";
    //    btnLeave = Control("Leave");
    //    btnLeave.GetComponent<Button>().onClick.AddListener(() => {
    //        MainLobby.Instance.Open();
    //        Close();
    //    });
    //    Control("Enter").GetComponent<Button>().onClick.AddListener(()=> {
    //        LoadingEX.Instance.Open();
    //        Common.EnterQueue();
    //        EnterQueueTimeOut.Start();
    //    });
    //    Control("Quit").GetComponent<Button>().onClick.AddListener(() =>
    //    {
    //        quit = true;
    //        TimesUsed.text = "00:00";
    //        tick = 0;
    //        LoadingEX.Instance.Open();
    //        LeaveQueueTimeOut.Start();
    //        Common.LeaveQueue();
    //    });
    //}

    //public void OnLeaveTimeOut(object sender, System.Timers.ElapsedEventArgs e)
    //{
    //    LoadingEX.Instance.Close();
    //    LeaveQueueTimeOut.Stop();
    //}

    //public void OnEnterTimeOut(object sender, System.Timers.ElapsedEventArgs e)
    //{
    //    LoadingEX.Instance.Close();
    //    EnterQueueTimeOut.Stop();
    //}

    //public void OnEnterQueue()
    //{
    //    LoadingEX.Instance.Close();
    //    EnterQueueTimeOut.Stop();
    //    FrameReplay.Instance.OnUpdates += Update;
    //    btnLeave.SetActive(false);
    //}

    //public void OnLeaveQueue()
    //{
    //    LoadingEX.Instance.Close();
    //    EnterQueueTimeOut.Stop();
    //    FrameReplay.Instance.OnUpdates -= Update;
    //    //queue = false;
    //    btnLeave.SetActive(true);
    //}

    //float tick = 0;
    //int TotalSeconds = 0;
    //bool quit = false;//等待退出
    ////bool queue = false;//排队中
    //void Update()
    //{
    //    tick += Time.deltaTime;
    //    if (quit)
    //    {
    //        if (tick >= 5.0)
    //            OnLeaveQueue();
    //        return;
    //    }
    //    int left = Mathf.FloorToInt(tick);
    //    if (left < 0)
    //        left = 0;
    //    if (left <= TotalSeconds)
    //        return;
    //    TotalSeconds = left;
    //    int minute = left / 60;
    //    int seconds = left % 60;
    //    string t = "";
    //    t = string.Format("{0:D2}:{1:D2}", minute, seconds);
    //    TimesUsed.text = t;
    //}
}

public class PsdWnd : Dialog
{
    //public override string PrefabName { get { return "PsdWnd"; } }
    //protected override bool OnOpen()
    //{
    //    Init();
    //    return base.OnOpen();
    //}

    //protected override bool OnClose()
    //{
    //    return base.OnClose();
    //}

    //public Action OnConfirm;
    //void Init()
    //{
    //    Control("Confirm").GetComponent<Button>().onClick.AddListener(() => { if (OnConfirm != null) OnConfirm(); });
    //    Control("Cancel").GetComponent<Button>().onClick.AddListener(() => { Close(); });
    //}
}

public class MainLobby : Dialog
{
    //public override string PrefabName { get { return "MainLobby"; } }
    //GameObject RoomRoot;
    //List<GameObject> rooms = new List<GameObject>();
    //int SelectRoomId = -1;
    //Button selectedBtn;
    //protected override bool OnOpen()
    //{
    //    Init();
    //    return base.OnOpen();
    //}

    //protected override bool OnClose()
    //{
    //    if (Update != null)
    //    {
    //        Main.Instance.StopCoroutine(Update);
    //        Update = null;
    //    }
    //    return base.OnClose();
    //}

    //public void ClearRooms()
    //{
    //    if (RoomRoot == null)
    //        return;
    //    SelectRoomId = -1;
    //    for (int i = 0; i < rooms.Count; i++)
    //        GameObject.DestroyImmediate(rooms[i]);
    //    rooms.Clear();
    //}

    //public void OnGetRoom(GetRoomRsp rsp)
    //{
    //    ClearRooms();
    //    int cnt = rsp.RoomInLobby.Count;
    //    GameObject prefab = Resources.Load<GameObject>("RoomInfoItem");
    //    for (int i = 0; i < cnt; i++)
    //        InsertRoomItem(rsp.RoomInLobby[i], prefab);
    //}

    //string[] ruleS = new string[5] { "盟主", "劫镖", "防守", "暗杀", "死斗"};
    //public void InsertRoomItem(RoomInfo room, GameObject prefab)
    //{
    //    GameObject roomObj = GameObject.Instantiate(prefab, RoomRoot.transform);
    //    roomObj.layer = RoomRoot.layer;
    //    roomObj.transform.localPosition = Vector3.zero;
    //    roomObj.transform.localScale = Vector3.one;
    //    roomObj.transform.rotation = Quaternion.identity;
    //    roomObj.transform.SetParent(RoomRoot.transform);
    //    Control("Name", roomObj).GetComponent<Text>().text = room.roomName;
    //    Control("Pattern", roomObj).GetComponent<Text>().text = "";
    //    Control("Password", roomObj).GetComponent<Text>().text = "无";
    //    Control("Rule", roomObj).GetComponent<Text>().text = ruleS[(int)room.rule - 1];//盟主，死斗，暗杀
    //    Control("LevelName", roomObj).GetComponent<Text>().text = LevelMng.Instance.GetItem((int)room.levelIdx).Name;
    //    Control("Version", roomObj).GetComponent<Text>().text = AppInfo.Instance.MeteorVersion;
    //    Control("Ping", roomObj).GetComponent<Text>().text = "???";
    //    Control("Group1", roomObj).GetComponent<Text>().text = room.Group1.ToString();
    //    Control("Group2", roomObj).GetComponent<Text>().text = room.Group2.ToString();
    //    Control("PlayerCount", roomObj).GetComponent<Text>().text = room.playerCount.ToString() + "/" + room.maxPlayer;
    //    Button btn = roomObj.GetComponent<Button>();
    //    btn.onClick.AddListener(()=> { OnSelectRoom(room.roomId, btn); });
    //    rooms.Add(roomObj);
    //}

    //void OnSelectRoom(uint id, Button btn)
    //{
    //    if (selectedBtn != null)
    //    {
    //        selectedBtn.image.color = new Color(1, 1, 1, 0);
    //        selectedBtn = null;
    //    }
    //    btn.image.color = new Color(144.0f / 255.0f, 104.0f / 255.0f, 104.0f / 255.0f, 104.0f / 255.0f);
    //    selectedBtn = btn;
    //    SelectRoomId = (int)id;
    //}

    //public void OnSelectService()
    //{
    //    Control("Server").GetComponent<Text>().text = string.Format("服务器:{0}        IP:{1}        端口:{2}", Global.Instance.Server.ServerName,
    //        TcpClientProxy.server == null ? "还未取得" : TcpClientProxy.server.Address.ToString(), TcpClientProxy.server == null ? "还未取得" : TcpClientProxy.server.Port.ToString());
    //}

    //Coroutine Update;//更新服务器列表的协程.
    //void Init()
    //{
    //    Control("JoinRoom").GetComponent<Button>().onClick.AddListener(() =>
    //    {
    //        OnJoinRoom();
    //    });
    //    Control("CreateRoom").GetComponent<Button>().onClick.AddListener(() =>
    //    {
    //        OnCreateRoom();
    //    });
    //    Control("Refresh").GetComponent<Button>().onClick.AddListener(() =>
    //    {
    //        OnRefresh();
    //    });
    //    Control("EnterQueue").GetComponent<Button>().onClick.AddListener(() =>
    //    {
    //        OnEnterQueue();
    //    });
    //    Control("Close").GetComponent<Button>().onClick.AddListener(() =>
    //    {
    //        OnGoback();
    //    });

    //    RoomRoot = Control("RoomRoot");
    //    Control("Version").GetComponent<Text>().text = GameData.Instance.gameStatus.MeteorVersion;
    //    if (Global.Instance.Servers.Count == 0)
    //        Update = Main.Instance.StartCoroutine(UpdateServiceList());
    //    else
    //        OnGetServerListDone();
    //}

    //void OnEnterQueue()
    //{
    //    U3D.PopupTip("功能在设计中，暂时无效");

    //    //MatchWnd.Instance.Open();
    //    //if (MainLobby.Exist)
    //    //    MainLobby.Instance.Close();
    //}

    //GameObject serverRoot;
    //IEnumerator UpdateServiceList()
    //{
    //    UnityWebRequest vFile = new UnityWebRequest();
    //    vFile.url = string.Format(Main.strSFile, Main.strHost, Main.port, Main.strProjectUrl, Main.strServices);
    //    vFile.timeout = 5;
    //    DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
    //    vFile.downloadHandler = dH;
    //    yield return vFile.Send();
    //    if (vFile.isError || vFile.responseCode != 200 || string.IsNullOrEmpty(dH.text))
    //    {
    //        Debug.LogError(string.Format("update version file error:{0} or responseCode:{1}", vFile.error, vFile.responseCode));
    //        vFile.Dispose();
    //        Update = null;
    //        yield break;
    //    }

    //    Global.Instance.Servers.Clear();
    //    LitJson.JsonData js = LitJson.JsonMapper.ToObject(dH.text);
    //    for (int i = 0; i < js["services"].Count; i++)
    //    {
    //        ServerInfo s = new ServerInfo();
    //        if (!int.TryParse(js["services"][i]["port"].ToString(), out s.ServerPort))
    //            continue;
    //        if (!int.TryParse(js["services"][i]["type"].ToString(), out s.type))
    //            continue;
    //        if (s.type == 0)
    //            s.ServerHost = js["services"][i]["addr"].ToString();
    //        else
    //            s.ServerIP = js["services"][i]["addr"].ToString();
    //        s.ServerName = js["services"][i]["name"].ToString();
    //        Global.Instance.Servers.Add(s);
    //    }
    //    Update = null;

    //    //合并所有服务器到全局变量里
    //    for (int i = 0; i < GameData.Instance.gameStatus.ServerList.Count; i++)
    //        Global.Instance.Servers.Add(GameData.Instance.gameStatus.ServerList[i]);

    //    OnGetServerListDone();
    //}

    //public void OnGetServerListDone()
    //{
    //    //拉取到服务器列表后
    //    Control("Servercfg").GetComponent<Button>().onClick.RemoveAllListeners();
    //    Control("Servercfg").GetComponent<Button>().onClick.AddListener(() =>
    //    {
    //        ServerListWnd.Instance.Open();
    //    });

    //    Global.Instance.Server = Global.Instance.Servers[0];
    //    GameObject Services = Control("Services", WndObject);
    //    serverRoot = Control("Content", Services);
    //    int childNum = serverRoot.transform.childCount;
    //    for (int i = 0; i < childNum; i++)
    //    {
    //        GameObject.Destroy(serverRoot.transform.GetChild(i).gameObject);
    //    }
    //    for (int i = 0; i < Global.Instance.Servers.Count; i++)
    //    {
    //        InsertServerItem(Global.Instance.Servers[i], i);
    //    }
    //    TcpClientProxy.ReStart();
    //}

    //void InsertServerItem(ServerInfo svr, int i)
    //{
    //    GameObject btn = GameObject.Instantiate(ResMng.Load("ButtonNormal")) as GameObject;
    //    btn.transform.SetParent(serverRoot.transform);
    //    btn.transform.localScale = Vector3.one;
    //    btn.transform.localPosition = Vector3.zero;
    //    btn.transform.localRotation = Quaternion.identity;
    //    btn.GetComponent<Button>().onClick.AddListener(() => {
    //        if (Global.Instance.Server == Global.Instance.Servers[i])
    //        {
    //            TcpClientProxy.CheckNeedReConnect();
    //            return;
    //        }
    //        TcpClientProxy.Exit();
    //        ClearRooms();
    //        Global.Instance.Server = svr;
    //        TcpClientProxy.ReStart();
    //    });
    //    btn.GetComponentInChildren<Text>().text = svr.ServerName;
    //}

    //void OnGoback()
    //{
    //    MainWnd.Instance.Open();
    //    Close();
    //}

    //void OnRefresh()
    //{
    //    TcpClientProxy.UpdateGameServer();
    //}

    //void OnCreateRoom()
    //{
    //    RoomOptionWnd.Instance.Open();
    //    Close();
    //}

    //void OnJoinRoom()
    //{
    //    if (SelectRoomId != -1)
    //    {
    //        TcpClientProxy.JoinRoom(SelectRoomId);
    //    }
    //}
}

public class ServerListWnd:Dialog
{
    //public const int ADD = 1;
    //public override string PrefabName { get { return "ServerListWnd"; } }
    //protected override bool OnOpen()
    //{
    //    Init();
    //    return base.OnOpen();
    //}
    //protected override bool OnClose()
    //{
    //    return base.OnClose();
    //}

    //public override void OnRefresh(int message, object param)
    //{
    //    switch (message)
    //    {
    //        case ADD:
    //            ServerInfo info = param as ServerInfo;
    //            GameObject prefab = ResMng.LoadPrefab("SelectListItem") as GameObject;
    //            InsertServerItem(info, prefab);
    //            break;
    //    }
    //}

    //GameObject ServerListRoot;
    //void Init()
    //{
    //    ServerListRoot = Control("ServerListRoot");
    //    GameObject prefab = ResMng.LoadPrefab("SelectListItem") as GameObject;
    //    for (int i = 0; i < serverList.Count; i++)
    //    {
    //        GameObject.Destroy(serverList[i]);
    //    }
    //    serverList.Clear();
    //    for (int i = 0; i < GameData.Instance.gameStatus.ServerList.Count; i++)
    //    {
    //        InsertServerItem(GameData.Instance.gameStatus.ServerList[i], prefab);
    //    }
    //    GameObject defaultServer = Control("SelectListItem");
    //    Text text = Control("Text", defaultServer).GetComponent<Text>();
    //    Control("Delete").GetComponent<Button>().onClick.AddListener(()=> {
    //        //不能删除默认
    //        if (selectServer != null)
    //        {
    //            int selectServerId = GameData.Instance.gameStatus.ServerList.IndexOf(selectServer);
    //            if (selectServerId != -1)
    //            {
    //                GameObject.Destroy(serverList[selectServerId]);
    //                serverList.RemoveAt(selectServerId);
    //                Global.Instance.OnServiceChanged(-1, GameData.Instance.gameStatus.ServerList[selectServerId]);
    //                GameData.Instance.gameStatus.ServerList.RemoveAt(selectServerId);
    //            }
    //            if (selectServerId >= serverList.Count)
    //                selectServerId = 0;
    //            selectServer = GameData.Instance.gameStatus.ServerList[selectServerId];
    //            selectedBtn = null;
    //        }
    //    });
    //    Control("Close").GetComponent<Button>().onClick.AddListener(() => { Close(); });
    //    Control("AddHost").GetComponent<Button>().onClick.AddListener(()=> {
    //        if (AddHostWnd.Exist)
    //            AddHostWnd.Instance.Close();
    //        else
    //            AddHostWnd.Instance.Open();
    //    });

    //    text.text = Global.Instance.Server.ServerName + string.Format(":{0}", Global.Instance.Server.ServerPort);
    //}

    //List<GameObject> serverList = new List<GameObject>();
    //Button selectedBtn;
    //ServerInfo selectServer;
    //public void InsertServerItem(ServerInfo server, GameObject prefab)
    //{
    //    GameObject host = GameObject.Instantiate(prefab, ServerListRoot.transform);
    //    host.layer = ServerListRoot.layer;
    //    host.transform.localPosition = Vector3.zero;
    //    host.transform.localScale = Vector3.one;
    //    host.transform.rotation = Quaternion.identity;
    //    host.transform.SetParent(ServerListRoot.transform);
    //    Control("Text", host).GetComponent<Text>().text = server.ServerName + string.Format(":{0}", server.ServerPort);
    //    Button btn = host.GetComponent<Button>();
    //    btn.onClick.AddListener(() => { OnSelectServer(server, btn); });
    //    serverList.Add(host);
    //}

    //void OnSelectServer(ServerInfo svr, Button btn)
    //{
    //    if (selectedBtn != null)
    //    {
    //        selectedBtn.image.color = new Color(1, 1, 1, 0);
    //        selectedBtn = null;
    //    }
    //    btn.image.color = new Color(144.0f / 255.0f, 104.0f / 255.0f, 104.0f / 255.0f, 104.0f / 255.0f);
    //    selectedBtn = btn;
    //    selectServer = svr;
    //}
}

//添加域名和端口的服务器
public class AddHostWnd:Dialog
{
    //public override string PrefabName { get { return "AddHostWnd"; } }
    //protected override bool OnOpen()
    //{
    //    Init();
    //    return base.OnOpen();
    //}
    //protected override bool OnClose()
    //{
    //    return base.OnClose();
    //}

    //InputField serverName;
    //InputField serverHost;
    //InputField serverIP;
    //InputField serverPort;
    //void Init()
    //{
    //    serverName = Control("ServerName").GetComponent<InputField>();
    //    serverHost = Control("ServerHost").GetComponent<InputField>();
    //    serverIP = Control("ServerIP").GetComponent<InputField>();
    //    serverPort = Control("ServerPort").GetComponent<InputField>();
    //    serverPort.onEndEdit.AddListener((string editport)=> {
    //        int p = 0;
    //        if (!int.TryParse(editport, out p))
    //        {
    //            U3D.PopupTip("端口必须是小于65535的正整数");
    //            serverPort.text = "";
    //            return;
    //        }
    //        if (p >= 65535 || p < 0)
    //        {
    //            U3D.PopupTip("端口必须是小于65535的正整数");
    //            serverPort.text = "";
    //            return;
    //        }
    //    });
    //    serverIP.onEndEdit.AddListener((string value) => {
    //        IPAddress address;
    //        if (!IPAddress.TryParse(value, out address))
    //        {
    //            U3D.PopupTip("请输入正确的IP地址");
    //            serverIP.text = "";
    //            return;
    //        }
    //    });
    //    Control("Yes").GetComponent<Button>().onClick.AddListener(() =>
    //    {
    //        if (string.IsNullOrEmpty(serverHost.text) && string.IsNullOrEmpty(serverIP.text))
    //        {
    //            U3D.PopupTip("域名和IP地址必须正确填写其中一项");
    //            return;
    //        }
    //        int port = 0;
    //        if (string.IsNullOrEmpty(serverPort.text) || !int.TryParse(serverPort.text, out port))
    //        {
    //            U3D.PopupTip("端口填写不正确");
    //            return;
    //        }

    //        //如果域名不为空
    //        if (!string.IsNullOrEmpty(serverHost.text))
    //        {
    //            ServerInfo info = new ServerInfo();
    //            info.type = 0;
    //            info.ServerPort = port;
    //            info.ServerName = string.IsNullOrEmpty(serverName.text) ? serverHost.text : serverName.text;
    //            info.ServerHost = serverHost.text;
    //            GameData.Instance.gameStatus.ServerList.Add(info);
    //            if (ServerListWnd.Exist)
    //                ServerListWnd.Instance.OnRefresh(ServerListWnd.ADD, info);
    //            Global.Instance.OnServiceChanged(1, info);
    //            if (MainLobby.Exist)
    //                MainLobby.Instance.OnGetServerListDone();
    //            Close();
    //        }
    //        else if (!string.IsNullOrEmpty(serverIP.text))
    //        {
    //            ServerInfo info = new ServerInfo();
    //            info.type = 1;
    //            info.ServerPort = port;
    //            info.ServerName = string.IsNullOrEmpty(serverName.text) ? serverHost.text : serverName.text;
    //            info.ServerIP = serverIP.text;
    //            GameData.Instance.gameStatus.ServerList.Add(info);
    //            if (ServerListWnd.Exist)
    //                ServerListWnd.Instance.OnRefresh(ServerListWnd.ADD, info);
    //            Global.Instance.OnServiceChanged(1, info);
    //            if (MainLobby.Exist)
    //                MainLobby.Instance.OnGetServerListDone();
    //            Close();
    //        }
    //    });
    //    Control("Cancel").GetComponent<Button>().onClick.AddListener(() =>
    //    {
    //        Close();
    //    });
    //}
}

public class NickName : Dialog
{
    //public InputField Nick;
    //public override string PrefabName { get { return "NickName"; } }
    //protected override bool OnOpen()
    //{
    //    Init();
    //    return base.OnOpen();
    //}
    //protected override bool OnClose()
    //{
    //    return base.OnClose();
    //}

    //void Init()
    //{
    //    Control("Yes").GetComponent<Button>().onClick.AddListener(() => {
    //        OnApply();
    //    });
    //    Control("Cancel").GetComponent<Button>().onClick.AddListener(() => {
    //        OnCancel();
    //    });
    //    Nick = Control("Nick").GetComponent<InputField>();
    //    if (string.IsNullOrEmpty(GameData.Instance.gameStatus.NickName))
    //        Nick.text = "昱泉杀手";
    //    else
    //        Nick.text = GameData.Instance.gameStatus.NickName;
    //}

    //void OnApply()
    //{
    //    if (!string.IsNullOrEmpty(Nick.text))
    //    {
    //        GameData.Instance.gameStatus.NickName = Nick.text;
    //        if (SettingWnd.Exist)
    //            SettingWnd.Instance.OnRefresh(0, null);
    //        Close();
    //    }
    //    else
    //        U3D.PopupTip("昵称不能为空");
        
    //}

    //void OnCancel()
    //{
    //    Close();
    //}
}

public class MainMenuState:CommonDialogState<MainMenu>
{
    public override string DialogName { get { return "MainMenu"; } }
    public MainMenuState(MainDialogStateManager dialogMgr):base(dialogMgr)
    {

    }
}

public class MainMenu : Dialog
{
    bool subMenuOpen = false;
    [SerializeField]
    Text Version;
    [SerializeField]
    Button SinglePlayerMode;
    [SerializeField]
    GameObject SubMenu;
    [SerializeField]
    Button SinglePlayer;
    [SerializeField]
    Button DlcLevel;
    [SerializeField]
    Button TeachingLevel;
    [SerializeField]
    Button CreateBattle;
    [SerializeField]
    Button MultiplePlayer;
    [SerializeField]
    Button PlayerSetting;
    [SerializeField]
    Button Quit;
    [SerializeField]
    Button UploadLog;
    [SerializeField]
    public AudioSource menu;
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        //进入主界面，创建全局
        Main.Instance.PersistState.EnterState(Main.Instance.PersistState.GameOverlay);
        Init();
        Main.Instance.listener.enabled = true;
        menu.volume = GameData.Instance.gameStatus.MusicVolume;
        //每次进入主界面，触发一次更新APP信息的操作，如果
        Main.Instance.UpdateAppInfo();
    }

    void Init()
    {
        Version.text = AppInfo.Instance.MeteorVersion;
        SinglePlayerMode.onClick.AddListener(() =>
        {
            subMenuOpen = !subMenuOpen;
            SubMenu.SetActive(subMenuOpen);
        });
        //单机关卡-官方剧情
        SinglePlayer.onClick.AddListener(() =>
        {
            OnSinglePlayer();
        });
        DlcLevel.onClick.AddListener(() =>
        {
            OnDlcWnd();
        });
        //教学关卡-教导使用招式方式
        TeachingLevel.onClick.AddListener(() =>
        {
            OnTeachingLevel();
        });
        //创建房间-各种单机玩法
        CreateBattle.onClick.AddListener(() =>
        {
            OnCreateRoom();
        });
        //多人游戏-联机
        MultiplePlayer.onClick.AddListener(() =>
        {
            OnlineGame();
        });
        //设置面板
        PlayerSetting.onClick.AddListener(() =>
        {
            OnSetting();
        });
        Quit.onClick.AddListener(() =>
        {
            GameData.Instance.SaveState();
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        });
        if (GameData.Instance.gameStatus.GodLike)
        {
            UploadLog.gameObject.SetActive(true);
        }
        UploadLog.onClick.AddListener(() => { FtpLog.UploadStart(); });
        GamePool.Instance.CloseDbg();
        TcpClientProxy.Exit();
    }

    void OnSinglePlayer()
    {
        //打开单机关卡面板
        Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.LevelDialogState);
    }

    void OnDlcWnd()
    {
        //DlcWnd.Instance.Open();
    }

    //教学关卡.
    void OnTeachingLevel()
    {
        U3D.LoadLevel(31, LevelMode.Teach, GameMode.SIDOU);
    }

    void OnCreateRoom()
    {
        //WorldTemplateWnd.Instance.Open();
        //Close();
    }

    void OnlineGame()
    {
        //MainLobby.Instance.Open();
        //Close();
    }

    //关卡外的设置面板和关卡内的设置面板并非同一个页面.
    void OnSetting()
    {
        //SettingWnd.Instance.Open();
        //NickName.Instance.Open();
    }
}

//角色信息面板去掉.以后再加
//这种界面，一般是全屏界面。
public class PlayerWnd:Dialog
{
    //public override string PrefabName  {  get {  return "PlayerWnd"; }  }
    //protected override bool OnOpen()
    //{
    //    Init();
    //    return base.OnOpen();
    //}

    //protected override int GetZ() { return 200; }
    //protected override bool Use3DCanvas()
    //{
    //    return true;
    //}
    //protected override bool OnClose()
    //{
    //    return base.OnClose();
    //}

    //void Init()
    //{
    //    RectTransform rectTran = WndObject.GetComponent<RectTransform>();
    //    if (rectTran != null)
    //        rectTran.anchoredPosition = new Vector2((1920f - rectTran.sizeDelta.x) / 2.0f, -(1080f - rectTran.sizeDelta.y) / 2.0f);
    //    GameObject obj = Global.ldaControlX("3DParent", WndObject);
    //    GameObject objPlayer = GameObject.Instantiate(Resources.Load("3DUIPlayer")) as GameObject;
    //    MeteorUnitDebug d = objPlayer.GetComponent<MeteorUnitDebug>();
    //    objPlayer.transform.position = Vector3.zero;
    //    objPlayer.transform.rotation = Quaternion.identity;
    //    objPlayer.transform.localScale = Vector3.one;
    //    d.gameObject.layer = obj.gameObject.layer;
    //    d.Init(MeteorManager.Instance.LocalPlayer.UnitId, LayerMask.NameToLayer("3DUIPlayer"));
    //    WeaponBase weaponProperty = U3D.GetWeaponProperty(MeteorManager.Instance.LocalPlayer.weaponLoader.GetCurrentWeapon().Info().UnitId);
    //    d.weaponLoader.StrWeaponR = weaponProperty.WeaponR;
    //    d.weaponLoader.StrWeaponL = weaponProperty.WeaponL;
    //    //d.weaponLoader.EquipWeapon();
    //    d.transform.SetParent(obj.transform);
    //    d.transform.localScale = 8 * Vector3.one;
    //    d.transform.localPosition = new Vector3(0, 0, -300);
    //    d.transform.localRotation = Quaternion.identity;
    //    Global.ldaControlX("Close Button", WndObject).GetComponent<Button>().onClick.AddListener(Close);

    //    SetStat("Stat Label 1", MeteorManager.Instance.LocalPlayer.Attr.hpCur + "/" + MeteorManager.Instance.LocalPlayer.Attr.HpMax);
    //    SetStat("Stat Label 2", MeteorManager.Instance.LocalPlayer.AngryValue.ToString());
    //    SetStat("Stat Label 3", MeteorManager.Instance.LocalPlayer.CalcDamage().ToString());
    //    SetStat("Stat Label 4", MeteorManager.Instance.LocalPlayer.CalcDef().ToString());
    //    SetStat("Stat Label 5", MeteorManager.Instance.LocalPlayer.MoveSpeed.ToString());
    //    SetStat("Stat Label 6", string.Format("{0:f2}", MeteorManager.Instance.LocalPlayer.MoveSpeed / 1000.0f));

    //    //处理背包的点击
    //    UIItemSlot [] slots = Global.ldaControlX("Slots Grid", WndObject).GetComponentsInChildren<UIItemSlot>();
    //    for (int i = 0; i < slots.Length; i++)
    //        slots[i].onClick.AddListener(OnClickItem);
    //}

    //void OnClickItem(UIItemSlot slot)
    //{
    //    if (ItemInfoWnd.Exist)
    //        ItemInfoWnd.Instance.Close();
    //    ItemInfoWnd.Instance.Open();
    //    ItemInfoWnd.Instance.AssignItem(slot.GetItemInfo());
    //}

    //void SetStat(string label, string value)
    //{
    //    GameObject objStat = Global.ldaControlX(label, WndObject);
    //    GameObject objStatValue = Control("Stat Value", objStat);
    //    objStatValue.GetComponent<Text>().text = value;
    //}
}

public class ItemInfoWnd:Dialog
{
    //public override string PrefabName { get { return "ItemInfoWnd"; } }
    //public static UIItemInfo Item;
    //public void AssignItem(UIItemInfo item)
    //{
    //    Item = item;
    //}
    //protected override bool OnOpen()
    //{
    //    Init();
    //    return base.OnOpen();
    //}

    //protected override bool OnClose()
    //{
    //    return base.OnClose();
    //}

    //void Init()
    //{

    //}
}

public class WeaponWnd : Dialog
{
    //public GameObject CameraForWeapon;
    //public GameObject WeaponModelParent;
    //public GameObject WeaponRoot;
    ////public 
    //public override string PrefabName
    //{
    //    get
    //    {
    //        return "WeaponWnd";
    //    }
    //}
    //protected override bool OnOpen()
    //{
    //    Init();
    //    return base.OnOpen();
    //}

    //protected override bool OnClose()
    //{
    //    if (load != null)
    //        Main.Instance.StopCoroutine(load);
    //    if (CameraForWeapon != null)
    //        GameObject.Destroy(CameraForWeapon);
    //    return base.OnClose();
    //}

    //Coroutine load;
    //EquipWeaponType weaponSubType;
    //WeaponLoader wload;
    //int selectWeapon;
    //void Init()
    //{
    //    weaponSubType = EquipWeaponType.Sword;
    //    if (CameraForWeapon == null)
    //    {
    //        CameraForWeapon = GameObject.Instantiate(ResMng.LoadPrefab("CameraForWeapon")) as GameObject;
    //        CameraForWeapon.Identity(null);
    //        WeaponModelParent = Control("WeaponParent", CameraForWeapon);
    //        wload = WeaponModelParent.GetComponent<WeaponLoader>();
    //        wload.Init();
    //    }
    //    WeaponRoot = Control("WeaponRoot");
    //    Control("Equip").GetComponent<Button>().onClick.AddListener(()=> { ChangeWeaponCode(); });
    //    Control("Close").GetComponent<Button>().onClick.AddListener(Close);
    //    for (int i = 0; i < 12; i++)
    //    {
    //        string control = string.Format("Tab{0}", i);
    //        Control(control).GetComponent<UITab>().onValueChanged.AddListener(ChangeWeaponType);
    //    }
    //    if (load == null)
    //        load = Main.Instance.StartCoroutine(AddWeapon());
    //}

    //IEnumerator AddWeapon()
    //{
    //    List<ItemBase> we = GameData.Instance.itemMng.GetFullRow();
    //    int offset = 0;
    //    for (int i = 0; i < we.Count; i++)
    //    {
    //        if (we[i].MainType == 1)
    //        {
    //            if (we[i].SubType == (int)weaponSubType)
    //            {
    //                AddWeaponItem(we[i], offset++);
    //                yield return 0;
    //            }
    //        }
    //    }
    //    ShowWeapon();
    //}

    //List<GameObject> GridWeapon = new List<GameObject>();
    //void AddWeaponItem(ItemBase it, int idx)
    //{
    //    if (GridWeapon.Count > idx)
    //    {
    //        GridWeapon[idx].SetActive(true);
    //        UIFunCtrl ctrl = GridWeapon[idx].GetComponent<UIFunCtrl>();
    //        ctrl.SetEvent(ShowWeapon, it.Idx);
    //        ctrl.SetText(it.Name);
    //    }
    //    else
    //    {
    //        GameObject weapon = GameObject.Instantiate(Resources.Load("GridItemBtn")) as GameObject;
    //        UIFunCtrl obj = weapon.AddComponent<UIFunCtrl>();
    //        obj.SetEvent(ShowWeapon, it.Idx);
    //        obj.SetText(it.Name);
    //        obj.transform.SetParent(WeaponRoot.transform);
    //        obj.gameObject.layer = WeaponRoot.layer;
    //        obj.transform.localScale = Vector3.one;
    //        obj.transform.localPosition = Vector3.zero;
    //        obj.transform.localRotation = Quaternion.identity;
    //        GridWeapon.Add(weapon);
    //    }
    //}

    //void ShowWeapon()
    //{
    //    List<ItemBase> we = GameData.Instance.itemMng.GetFullRow();
    //    for (int i = 0; i < we.Count; i++)
    //    {
    //        if (we[i].MainType == 1)
    //        {
    //            if (we[i].SubType == (int)weaponSubType)
    //            {
    //                selectWeapon = we[i].Idx;
    //                break;
    //            }
    //        }
    //    }
    //    ShowWeapon(selectWeapon);
    //}

    //void ShowWeapon(int idx)
    //{
    //    selectWeapon = idx;
    //    wload.EquipWeapon(selectWeapon);
    //}

    //void ChangeWeaponCode()
    //{
    //    MeteorManager.Instance.LocalPlayer.ChangeWeaponCode(selectWeapon);
    //}

    //void ChangeWeaponType(bool change)
    //{
    //    if (!change)
    //        return;
    //    for (int i = 0; i < 12; i++)
    //    {
    //        string control = string.Format("Tab{0}", i);
    //        if (Control(control).GetComponent<UITab>().isOn)
    //        {
    //            ChangeWeaponType(i);
    //            break;
    //        }
    //    }
        
    //}

    //void ChangeWeaponType(int subType)
    //{
    //    weaponSubType = (EquipWeaponType)subType;
    //    for (int i = 0; i < GridWeapon.Count; i++)
    //        GridWeapon[i].SetActive(false);
    //    if (load != null)
    //        Main.Instance.StopCoroutine(load);
    //    load = Main.Instance.StartCoroutine(AddWeapon());
    //}
}

public class RobotWnd : Dialog
{
    //public GameObject RobotRoot;
    //public int weaponIdx = 0;//0-长剑
    //public int campIdx = 1;//
    //public int hpIdx = 0;
    //int[] hpArray = { 1000, 500, 300, 250, 200, 100 };
    //public override string PrefabName
    //{
    //    get
    //    {
    //        return "RobotWnd";
    //    }
    //}
    //protected override bool OnOpen()
    //{
    //    Init();
    //    return base.OnOpen();
    //}

    //protected override bool OnClose()
    //{
    //    if (refresh != null)
    //        Main.Instance.StopCoroutine(refresh);
    //    return base.OnClose();
    //}

    //void Init()
    //{
    //    RobotRoot = Control("PageView");
    //    Control("Close").GetComponent<Button>().onClick.AddListener(Close);
    //    for (int i = 0; i < 12; i++)
    //    {
    //        int k = i;
    //        Control(string.Format("Weapon{0}", i)).GetComponent<Toggle>().onValueChanged.AddListener((bool select) => 
    //        {
    //            if (select)
    //            {
    //                weaponIdx = k;
    //            }
    //        });
    //    }
    //    for (int i = 0; i < 3; i++)
    //    {
    //        int k = i;
    //        Control(string.Format("Camp{0}", i)).GetComponent<Toggle>().onValueChanged.AddListener((bool select) =>
    //        {
    //            if (select)
    //            {
    //                campIdx = k;
    //            }
    //        });
    //    }

    //    for (int i = 0; i < 6; i++)
    //    {
    //        int k = i;
    //        Control(string.Format("HP{0}", i)).GetComponent<Toggle>().onValueChanged.AddListener((bool select) =>
    //        {
    //            if (select)
    //            {
    //                hpIdx = k;
    //            }
    //        });
    //    } 
    //    refresh = Main.Instance.StartCoroutine(RefreshRobot());
    //}

    //Coroutine refresh = null;
    //IEnumerator RefreshRobot()
    //{
    //    List<int> l = U3D.GetUnitList();
    //    for (int i = 0; i < l.Count; i++)
    //    {
    //        AddRobot(l[i]);
    //        yield return 0;
    //    }
    //    refresh = null;
    //}

    //Dictionary<int, GameObject> RobotList = new Dictionary<int, GameObject>();
    //void AddRobot(int Idx)
    //{
    //    if (RobotList.ContainsKey(Idx))
    //    {
    //        RobotList[Idx].GetComponent<Button>().onClick.RemoveAllListeners();
    //        RobotList[Idx].GetComponent<Button>().onClick.AddListener(() => { SpawnRobot(Idx, (EUnitCamp)campIdx); });
    //        RobotList[Idx].GetComponentInChildren<Text>().text = string.Format("{0}", Global.Instance.GetCharacterName(Idx));
    //    }
    //    else
    //    {
    //        GameObject obj = GameObject.Instantiate(Resources.Load("GridItemBtn")) as GameObject;
    //        obj.GetComponent<Button>().onClick.AddListener(() => { SpawnRobot(Idx, (EUnitCamp)campIdx); });
    //        obj.GetComponentInChildren<Text>().text = string.Format("{0}", Global.Instance.GetCharacterName(Idx));
    //        obj.transform.SetParent(RobotRoot.transform);
    //        obj.gameObject.layer = RobotRoot.layer;
    //        obj.transform.localScale = Vector3.one;
    //        obj.transform.localPosition = Vector3.zero;
    //        obj.transform.localRotation = Quaternion.identity;
    //        RobotList.Add(Idx, obj);
    //    }
    //}

    //void SpawnRobot(int idx, EUnitCamp camp)
    //{
    //    U3D.SpawnRobot(idx, camp, weaponIdx, hpArray[hpIdx]);
    //}
}

public class SfxWnd: Dialog
{
    //public GameObject SfxRoot;
    //public override string PrefabName
    //{
    //    get
    //    {
    //        return "SfxWnd";
    //    }
    //}
    //protected override bool OnOpen()
    //{
    //    Init();
    //    return base.OnOpen();
    //}

    //protected override bool OnClose()
    //{
    //    //if (load != null)
    //    //    Main.Instance.StopCoroutine(load);
    //    return base.OnClose();
    //}

    ////Coroutine load;
    //int pageIndex = 32;//1-max
    //private const int PageCount = 20;
    //int pageMax = 0;
    ////int pageMin = 0;
    //void Init()
    //{
    //    pageMax = (SFXLoader.Instance.Eff.Length / PageCount) + ((SFXLoader.Instance.Eff.Length % PageCount) != 0 ? 1 : 0);
    //    SfxRoot = Control("Page");
    //    Control("Close").GetComponent<Button>().onClick.AddListener(Close);
    //    Control("PagePrev").GetComponent<Button>().onClick.AddListener(PrevPage);
    //    Control("PageNext").GetComponent<Button>().onClick.AddListener(NextPage);
    //    NextPage();
    //}

    //void NextPage()
    //{
    //    if (pageIndex == pageMax)
    //        pageIndex = 1;
    //    else
    //        pageIndex += 1;
    //    if (refresh != null)
    //        Main.Instance.StopCoroutine(refresh);
    //    refresh = Main.Instance.StartCoroutine(RefreshSfx(pageIndex));
    //    Control("PageText").GetComponent<Text>().text = string.Format("{0:d2}/{1:d2}", pageIndex, pageMax);
    //}

    //void PrevPage()
    //{
    //    if (pageIndex == 1)
    //        pageIndex = pageMax;
    //    else
    //        pageIndex -= 1;
    //    if (refresh != null)
    //        Main.Instance.StopCoroutine(refresh);
    //    refresh = Main.Instance.StartCoroutine(RefreshSfx(pageIndex));
    //    Control("PageText").GetComponent<Text>().text = string.Format("{0:d2}/{1:d2}", pageIndex, pageMax);
    //}

    //Coroutine refresh = null;
    //IEnumerator RefreshSfx(int page)
    //{
    //    for (int i = Mathf.Min((page - 1) * PageCount, (page) * PageCount); i < (page) * PageCount; i++)
    //    {
    //        int j = i - (page - 1) * PageCount;
    //        if (SFXList.Count > j)
    //            SFXList[j].SetActive(false);
    //    }

    //    for (int i = (page - 1)* PageCount; i < Mathf.Min((page)* PageCount, SFXLoader.Instance.TotalSfx); i++)
    //    {
    //        AddSFX(i, (page - 1) * PageCount);
    //        yield return 0;
    //    }
    //}

    //List<GameObject> SFXList = new List<GameObject>();
    //void AddSFX(int Idx, int startIdx)
    //{
    //    int j = Idx - startIdx;
    //    if (SFXList.Count > j)
    //    {
    //        SFXList[j].SetActive(true);
    //        SFXList[j].GetComponent<Button>().onClick.RemoveAllListeners();
    //        SFXList[j].GetComponent<RectTransform>().sizeDelta = Vector2.zero;
    //        SFXList[j].GetComponent<Button>().onClick.AddListener(() => { PlaySfx(Idx); });
    //        SFXList[j].GetComponentInChildren<Text>().text = SFXLoader.Instance.Eff[Idx];
    //    }
    //    else
    //    {
    //        GameObject obj = GameObject.Instantiate(Resources.Load("GridItemBtn")) as GameObject;
    //        obj.GetComponent<Button>().onClick.AddListener(() => { PlaySfx(Idx); });
    //        obj.GetComponentInChildren<Text>().text = SFXLoader.Instance.Eff[Idx];
    //        obj.transform.SetParent(SfxRoot.transform);
    //        obj.gameObject.layer = SfxRoot.layer;
    //        obj.transform.localScale = Vector3.one;
    //        obj.transform.localPosition = Vector3.zero;
    //        obj.transform.localRotation = Quaternion.identity;
    //        SFXList.Add(obj);
    //    }
    //}

    //void PlaySfx(int idx)
    //{
    //    SFXLoader.Instance.PlayEffect(idx, MeteorManager.Instance.LocalPlayer.gameObject, true);
    //}
}

public class BattleStatusWnd: Dialog
{
    //public override string PrefabName
    //{
    //    get
    //    {
    //        return "BattleStatusWnd";
    //    }
    //}

    //protected override bool OnOpen()
    //{
    //    Init();
    //    return base.OnOpen();
    //}

    //protected override bool OnClose()
    //{
    //    return base.OnClose();
    //}

    //GameObject BattleResult;
    ////GameObject BattleTitle;
    //Transform MeteorResult;
    //Transform ButterflyResult;
    //Dictionary<int, BattleResultItem> battleResult = new Dictionary<int, BattleResultItem>();
    //public void Init()
    //{
    //    //拷贝一份对战数据
    //    battleResult.Clear();
    //    foreach (var each in GameBattleEx.Instance.BattleResult)
    //    {
    //        battleResult.Add(each.Key, each.Value);
    //    }
    //    MeteorResult = Control("MeteorResult").transform;
    //    ButterflyResult = Control("ButterflyResult").transform;
    //    BattleResult = Global.ldaControlX("AllResult", WndObject);
    //    Control("CampImage", WndObject).SetActive(Global.Instance.GGameMode != GameMode.MENGZHU);
    //    Control("Title", WndObject).SetActive(Global.Instance.GGameMode != GameMode.MENGZHU);
    //    Control("Result", WndObject).SetActive(Global.Instance.GGameMode != GameMode.MENGZHU);
    //    Control("CampImage1", WndObject).SetActive(Global.Instance.GGameMode != GameMode.MENGZHU);
    //    Control("Title1", WndObject).SetActive(Global.Instance.GGameMode != GameMode.MENGZHU);
    //    Control("Result1", WndObject).SetActive(Global.Instance.GGameMode != GameMode.MENGZHU);
    //    Control("CampImageAll", WndObject).SetActive(Global.Instance.GGameMode == GameMode.MENGZHU);
    //    Control("TitleAll", WndObject).SetActive(Global.Instance.GGameMode == GameMode.MENGZHU);
    //    Control("ResultAll", WndObject).SetActive(Global.Instance.GGameMode == GameMode.MENGZHU);
    //    //BattleTitle = Global.ldaControlX("BattleTitle", WndObject);
    //    for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
    //    {
    //        if (battleResult.ContainsKey(MeteorManager.Instance.UnitInfos[i].InstanceId))
    //        {
    //            InsertPlayerResult(MeteorManager.Instance.UnitInfos[i].InstanceId, battleResult[MeteorManager.Instance.UnitInfos[i].InstanceId]);
    //            battleResult.Remove(MeteorManager.Instance.UnitInfos[i].InstanceId);
    //        }
    //        else
    //            InsertPlayerResult(MeteorManager.Instance.UnitInfos[i].InstanceId, MeteorManager.Instance.UnitInfos[i].InstanceId, 0, 0, MeteorManager.Instance.UnitInfos[i].Camp);
    //    }

    //    foreach (var each in battleResult)
    //        InsertPlayerResult(each.Key, each.Value);
    //}

    //void InsertPlayerResult(int instance, int id, int killed, int dead, EUnitCamp camp)
    //{
    //    GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>("ResultItem"));
    //    if (Global.Instance.GGameMode == GameMode.MENGZHU)
    //    {
    //        obj.transform.SetParent(BattleResult.transform);
    //    }
    //    else
    //        obj.transform.SetParent(camp == EUnitCamp.EUC_FRIEND ? MeteorResult : ButterflyResult);
    //    obj.layer = MeteorResult.gameObject.layer;
    //    obj.transform.localRotation = Quaternion.identity;
    //    obj.transform.localScale = Vector3.one;
    //    obj.transform.localPosition = Vector3.zero;

    //    Text Idx = ldaControl("Idx", obj).GetComponent<Text>();
    //    Text Name = ldaControl("Name", obj).GetComponent<Text>();
    //    if (Global.Instance.GGameMode == GameMode.MENGZHU)
    //    {

    //    }
    //    else
    //    {
    //        Text Camp = ldaControl("Camp", obj).GetComponent<Text>();
    //        Camp.text = U3D.GetCampStr(camp);
    //    }
            
    //    Text Killed = ldaControl("Killed", obj).GetComponent<Text>();
    //    Text Dead = ldaControl("Dead", obj).GetComponent<Text>();
    //    Idx.text = (id + 1).ToString();
    //    Name.text = U3D.GetUnit(instance).Name;
        
    //    Killed.text = killed.ToString();
    //    Dead.text = dead.ToString();
    //    MeteorUnit u = U3D.GetUnit(id);
    //    if (u != null)
    //    {
    //        if (u.Dead)
    //        {
    //            Idx.color = Color.red;
    //            Name.color = Color.red;
    //            Killed.color = Color.red;
    //            Dead.color = Color.red;
    //        }
    //    }
    //    else
    //    {
    //        //得不到信息了。说明该NPC被移除掉了
    //        Idx.color = Color.red;
    //        Name.color = Color.red;
    //        Killed.color = Color.red;
    //        Dead.color = Color.red;
    //    }
    //}

    //void InsertPlayerResult(int instanceId, BattleResultItem result)
    //{
    //    GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>("ResultItem"));
    //    if (Global.Instance.GGameMode == GameMode.MENGZHU)
    //    {
    //        obj.transform.SetParent(BattleResult.transform);
    //    }
    //    else
    //        obj.transform.SetParent(result.camp == (int)EUnitCamp.EUC_FRIEND ? MeteorResult : ButterflyResult);
    //    obj.layer = MeteorResult.gameObject.layer;
    //    obj.transform.localRotation = Quaternion.identity;
    //    obj.transform.localScale = Vector3.one;
    //    obj.transform.localPosition = Vector3.zero;

    //    Text Idx = ldaControl("Idx", obj).GetComponent<Text>();
    //    Text Name = ldaControl("Name", obj).GetComponent<Text>();
    //    if (Global.Instance.GGameMode == GameMode.MENGZHU)
    //    {

    //    }
    //    else
    //    {
    //        Text Camp = ldaControl("Camp", obj).GetComponent<Text>();
    //        Camp.text = U3D.GetCampStr((EUnitCamp)result.camp);
    //    }
    //    Text Killed = ldaControl("Killed", obj).GetComponent<Text>();
    //    Text Dead = ldaControl("Dead", obj).GetComponent<Text>();
    //    Idx.text = (result.id + 1).ToString();
    //    Name.text = U3D.GetUnit(instanceId).Name;
    //    //Camp.text = result.camp == 1 ""
    //    Killed.text = result.killCount.ToString();
    //    Dead.text = result.deadCount.ToString();
    //    MeteorUnit u = U3D.GetUnit(result.id);
    //    if (u != null)
    //    {
    //        if (u.Dead)
    //        {
    //            Idx.color = Color.red;
    //            Name.color = Color.red;
    //            Killed.color = Color.red;
    //            Dead.color = Color.red;
    //        }
    //    }
    //}
}

public class BattleResultWnd : Dialog
{
    //public override string PrefabName
    //{
    //    get
    //    {
    //        return "BattleResultWnd";
    //    }
    //}

    //protected override bool OnOpen()
    //{
    //    Init();
    //    return base.OnOpen();
    //}

    //protected override bool OnClose()
    //{
    //    return base.OnClose();
    //}

    //GameObject BattleResultAll;
    //GameObject BattleResult;
    //GameObject BattleTitle;
    //Transform MeteorResult;
    //Transform ButterflyResult;
    //public void SetResult(int result)
    //{
    //    if (result == 1)
    //    {
    //        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
    //        {
    //            if (MeteorManager.Instance.UnitInfos[i].Robot != null)
    //                MeteorManager.Instance.UnitInfos[i].Robot.StopMove();
    //            MeteorManager.Instance.UnitInfos[i].controller.Input.ResetVector();
    //            MeteorManager.Instance.UnitInfos[i].OnGameResult(result);
    //        }
    //    }

    //    if (Global.Instance.GGameMode == GameMode.MENGZHU)
    //    {
    //        U3D.InsertSystemMsg("回合结束");
    //    }
    //    else
    //    {
    //        string mat = "";
    //        Text txt;
    //        switch (result)
    //        {
    //            case -1:
    //            case 0:
    //                mat = "BattleLose";
    //                txt = Control("ButterflyWin").GetComponent<Text>();
    //                U3D.InsertSystemMsg("蝴蝶阵营 获胜");
    //                txt.text = "1";
    //                break;
    //            case 1:
    //            case 2:
    //                mat = "BattleWin";
    //                txt = Control("MeteorWin").GetComponent<Text>();
    //                U3D.InsertSystemMsg("流星阵营 获胜");
    //                txt.text = "1";
    //                break;
    //            case 3:
    //                mat = "BattleNone";
    //                U3D.InsertSystemMsg("和局");
    //                break;

    //        }
    //        BattleResult.GetComponent<Image>().material = Resources.Load<Material>(mat);
    //        BattleResult.SetActive(true);
    //        BattleTitle.SetActive(true);
    //    }
    //    Control("Close").SetActive(true);
    //    Control("Close").GetComponent<Button>().onClick.AddListener(() =>
    //    {
    //        if (SettingWnd.Exist)
    //            SettingWnd.Instance.Close();
    //        GameData.Instance.SaveState();
    //        GameBattleEx.Instance.Pause();
    //        Main.Instance.StopAllCoroutines();
    //        SoundManager.Instance.StopAll();
    //        BuffMng.Instance.Clear();
    //        MeteorManager.Instance.Clear();
    //        Close();
    //        if (FightWnd.Exist)
    //            FightWnd.Instance.Close();
    //        if (GameOverlayWnd.Exist)
    //            GameOverlayWnd.Instance.ClearSystemMsg();
    //        //离开副本
    //        if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
    //            UdpClientProxy.LeaveLevel();
    //        else
    //        {
    //            FrameReplay.Instance.OnDisconnected();
    //            Main.Instance.PlayEndMovie(result == 1);
    //        }
    //    });
    //}

    //public void Init()
    //{
    //    GamePool.Instance.CloseDbg();
    //    MeteorResult = Control("MeteorResult").transform;
    //    ButterflyResult = Control("ButterflyResult").transform;
    //    BattleResult = Global.ldaControlX("BattleResult", WndObject);
    //    BattleTitle = Global.ldaControlX("BattleTitle", WndObject);
    //    Control("Close").SetActive(false);
    //    BattleResultAll = Global.ldaControlX("AllResult", WndObject);
    //    Control("CampImage", WndObject).SetActive(Global.Instance.GGameMode != GameMode.MENGZHU);
    //    Control("Title", WndObject).SetActive(Global.Instance.GGameMode != GameMode.MENGZHU);
    //    Control("Result", WndObject).SetActive(Global.Instance.GGameMode != GameMode.MENGZHU);
    //    Control("CampImage1", WndObject).SetActive(Global.Instance.GGameMode != GameMode.MENGZHU);
    //    Control("Title1", WndObject).SetActive(Global.Instance.GGameMode != GameMode.MENGZHU);
    //    Control("Result1", WndObject).SetActive(Global.Instance.GGameMode != GameMode.MENGZHU);
    //    Control("CampImageAll", WndObject).SetActive(Global.Instance.GGameMode == GameMode.MENGZHU);
    //    Control("TitleAll", WndObject).SetActive(Global.Instance.GGameMode == GameMode.MENGZHU);
    //    Control("ResultAll", WndObject).SetActive(Global.Instance.GGameMode == GameMode.MENGZHU);

    //    for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
    //    {
    //        if (GameBattleEx.Instance.BattleResult.ContainsKey(MeteorManager.Instance.UnitInfos[i].InstanceId))
    //        {
    //            InsertPlayerResult(MeteorManager.Instance.UnitInfos[i].InstanceId, GameBattleEx.Instance.BattleResult[MeteorManager.Instance.UnitInfos[i].InstanceId]);
    //            GameBattleEx.Instance.BattleResult.Remove(MeteorManager.Instance.UnitInfos[i].InstanceId);
    //        }
    //        else
    //            InsertPlayerResult(MeteorManager.Instance.UnitInfos[i].InstanceId, MeteorManager.Instance.UnitInfos[i].InstanceId, 0, 0, MeteorManager.Instance.UnitInfos[i].Camp);
    //    }

    //    foreach (var each in GameBattleEx.Instance.BattleResult)
    //        InsertPlayerResult(each.Key, each.Value);
    //    GameBattleEx.Instance.BattleResult.Clear();
    //}

    //void InsertPlayerResult(int instanceId, int id, int killed, int dead, EUnitCamp camp)
    //{
    //    GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>("ResultItem"));
    //    if (Global.Instance.GGameMode == GameMode.MENGZHU)
    //    {
    //        obj.transform.SetParent(BattleResultAll.transform);
    //    }
    //    else
    //        obj.transform.SetParent(camp == EUnitCamp.EUC_FRIEND ? MeteorResult : ButterflyResult);
    //    //obj.transform.SetParent(camp ==  EUnitCamp.EUC_FRIEND ? MeteorResult : ButterflyResult);
    //    obj.layer = MeteorResult.gameObject.layer;
    //    obj.transform.localRotation = Quaternion.identity;
    //    obj.transform.localScale = Vector3.one;
    //    obj.transform.localPosition = Vector3.zero;

    //    Text Idx = ldaControl("Idx", obj).GetComponent<Text>();
    //    Text Name = ldaControl("Name", obj).GetComponent<Text>();
    //    if (Global.Instance.GGameMode == GameMode.MENGZHU)
    //    {

    //    }
    //    else
    //    {
    //        Text Camp = ldaControl("Camp", obj).GetComponent<Text>();
    //        Camp.text = U3D.GetCampStr(camp);
    //    }
    //    //Text Camp = ldaControl("Camp", obj).GetComponent<Text>();
    //    Text Killed = ldaControl("Killed", obj).GetComponent<Text>();
    //    Text Dead = ldaControl("Dead", obj).GetComponent<Text>();
    //    Idx.text = (id + 1).ToString();
    //    Name.text = U3D.GetUnit(instanceId).Name;
    //    //Camp.text = result.camp == 1 ""
    //    Killed.text = killed.ToString();
    //    Dead.text = dead.ToString();
    //    MeteorUnit u = U3D.GetUnit(id);
    //    if (u != null)
    //    {
    //        if (u.Dead)
    //        {
    //            Idx.color = Color.red;
    //            Name.color = Color.red;
    //            Killed.color = Color.red;
    //            Dead.color = Color.red;
    //        }
    //    }
    //    else
    //    {
    //        //得不到信息了。说明该NPC被移除掉了
    //        Idx.color = Color.red;
    //        Name.color = Color.red;
    //        Killed.color = Color.red;
    //        Dead.color = Color.red;
    //    }
    //}

    //void InsertPlayerResult(int instanceId, BattleResultItem result)
    //{
    //    GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>("ResultItem"));
    //    if (Global.Instance.GGameMode == GameMode.MENGZHU)
    //    {
    //        obj.transform.SetParent(BattleResultAll.transform);
    //    }
    //    else
    //        obj.transform.SetParent(result.camp == (int)EUnitCamp.EUC_FRIEND ? MeteorResult : ButterflyResult);
    //    obj.layer = MeteorResult.gameObject.layer;
    //    obj.transform.localRotation = Quaternion.identity;
    //    obj.transform.localScale = Vector3.one;
    //    obj.transform.localPosition = Vector3.zero;

    //    Text Idx = ldaControl("Idx", obj).GetComponent<Text>();
    //    Text Name = ldaControl("Name", obj).GetComponent<Text>();
    //    Text Killed = ldaControl("Killed", obj).GetComponent<Text>();
    //    Text Dead = ldaControl("Dead", obj).GetComponent<Text>();
    //    Idx.text = (result.id + 1).ToString();
    //    Name.text = U3D.GetUnit(instanceId).Name;
    //    if (Global.Instance.GGameMode == GameMode.MENGZHU)
    //    {

    //    }
    //    else
    //    {
    //        Text Camp = ldaControl("Camp", obj).GetComponent<Text>();
    //        Camp.text = U3D.GetCampStr((EUnitCamp)result.camp);
    //    }
    //    Killed.text = result.killCount.ToString();
    //    Dead.text = result.deadCount.ToString();
    //    MeteorUnit u = U3D.GetUnit(result.id);
    //    if (u != null)
    //    {
    //        if (u.Dead)
    //        {
    //            Idx.color = Color.red;
    //            Name.color = Color.red;
    //            Killed.color = Color.red;
    //            Dead.color = Color.red;
    //        }
    //    }
    //}
}