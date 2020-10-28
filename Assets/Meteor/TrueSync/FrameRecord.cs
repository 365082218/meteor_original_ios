using Excel2Json;
using ProtoBuf;
using protocol;
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class GameRecord {
    
    public string AppVersion;//APP版本
    public int MeteorVersion;//流星版本
    public bool EnableGodMode;//一击必杀
    public bool HidePlayer;//隐藏主角
    public bool EnableInfiniteAngry;//无限气
    public bool Undead;
    public string Name;//录像名
    public Guid guid;//录像唯一编号.
    public int Chapter;//剧本ID-单机为0
    public int Id;//关卡ID-单机有效
    //public long RandomSeed;//随机种子
    public GameMode GameMode;
    public LevelMode LevelMode;
    public LevelData Level;//关卡数据
    public Dictionary<int, GameFrame> frames;//操作帧.
    public int screenWidth;
    public int screenHeight;
    public int frameCount;//逻辑帧数量
    public float deltaTime;//固定间隔
    public int duration;
    public long time;//保存日期
    public string screenPng;//图片路径

    public Sprite LoadTexture() {
        byte[] screenBytes = null;
        try {
            screenBytes = System.IO.File.ReadAllBytes(screenPng);
        } catch (Exception exp) {
            Debug.LogError(exp.Message);
            return null;
        }
        Texture2D tex = new Texture2D(screenWidth, screenHeight, TextureFormat.ARGB32, false);
        tex.LoadImage(screenBytes);
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
    }
}

public class RecordMgr : Singleton<RecordMgr> {

    System.Threading.Thread WriteThread;
    string version;
    string recordPath;//路径
    byte[] screenShot;
    int frame;
    int duration;
    float deltaTime;
    public void WriteFile()
    {
        version = Main.Ins.AppInfo.AppVersion();
        if (WriteThread != null)
            return;
        //recordPath = Main.Ins.replayPath;
        screenShot = Utility.CaptureScreen(Main.Ins.MainCamera);
        //frame = FrameReplay.Instance.EndFrame;
        duration = Mathf.FloorToInt(FrameReplay.Ins.time);
        deltaTime = FrameReplay.deltaTime;
        WriteThread = new System.Threading.Thread(new System.Threading.ThreadStart(this.WriteFileCore));
        WriteThread.Start();
    }

    void WriteFileCore()
    {
        GameRecord record = new GameRecord();
        record.AppVersion = version;
        record.MeteorVersion = AppInfo.Ins.MeteorV1();
        record.EnableGodMode = GameStateMgr.Ins.gameStatus.EnableGodMode =
        record.HidePlayer = GameStateMgr.Ins.gameStatus.HidePlayer;
        record.EnableInfiniteAngry = GameStateMgr.Ins.gameStatus.EnableInfiniteAngry;
        record.Undead = GameStateMgr.Ins.gameStatus.Undead;
        record.guid = Guid.NewGuid();
        record.Name = GenerateRecordName(CombatData.Ins.GLevelItem, CombatData.Ins.GLevelMode, CombatData.Ins.GGameMode);
        record.GameMode = CombatData.Ins.GGameMode;
        record.LevelMode = CombatData.Ins.GLevelMode;
        record.Chapter = (int)(CombatData.Ins.Chapter == null ? 0 : CombatData.Ins.Chapter.ChapterId);
        record.Id = (int)(CombatData.Ins.GLevelItem.Id);
        //record.RandomSeed = CombatData.Ins.RandSeed;
        //record.frames = FrameSyncLocal.Ins.Frames;
        record.Level = CombatData.Ins.GLevelItem;
        record.screenWidth = (int)(UIHelper.CanvasWidth / 5);
        record.screenHeight = (int)(UIHelper.CanvasHeight / 5);
        record.screenPng = recordPath + record.guid + ".png";
        Utility.SavePng(record.screenPng, screenShot);
        record.frameCount = frame;
        record.duration = duration;
        record.deltaTime = deltaTime;
        record.time = DateTime.Now.ToFileTime();
        try
        {
            System.IO.FileStream fs = System.IO.File.Create(recordPath + record.Name + "_" + record.guid.ToString() +  ".mrc");
            ProtoBuf.Serializer.Serialize(fs, record);
            fs.Close();
        }
        catch (Exception exp)
        {
            Debug.LogError(exp.Message + exp.StackTrace);
            LocalMsg resultFailed = new LocalMsg();
            resultFailed.Message = (int)LocalMsgType.SaveRecord;
            resultFailed.message = exp.Message;
            resultFailed.Result = 0;
            TcpProtoHandler.Ins.PostMessage(resultFailed);
            WriteThread = null;
            return;
        }
        //do sth
        LocalMsg result = new LocalMsg();
        result.Message = (int)LocalMsgType.SaveRecord;
        result.Result = 1;
        TcpProtoHandler.Ins.PostMessage(result);
        WriteThread = null;
    }

    string GenerateRecordName(LevelData data, LevelMode lm, GameMode gm)
    {
        //是什么类型的，单机，模组，联机，
        //模式-场景-主角-{单机/联机}
        string [] gms = new string[] { "盟主","劫镖","护城", "暗杀", "死斗", "剧情" };
        if (lm == LevelMode.MultiplyPlayer)
        {
            return string.Format("{0}_{1}_{2}_{3}人_联机", gms[(int)gm -1], data.Name, GameStateMgr.Ins.gameStatus.NickName, RoomMng.Ins.Current.maxPlayer);
        }
        return string.Format("{0}-{1}", gms[(int)gm - 1], data.Name);
    }
}
