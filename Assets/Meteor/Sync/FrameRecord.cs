﻿using Excel2Json;
using ProtoBuf;
using protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class GameRecord
{
    public string RecordVersion = "1.0.0";
    public string Name;//录像名
    public Guid guid;//录像唯一编号.
    public int Mode;//录制时游戏模式
    public int Chapter;//剧本ID-单机为0
    public int Id;//关卡ID-单机有效
    public long RandomSeed;//随机种子
    public List<GameFrames> frames;//操作帧.
    public int screenWidth;
    public int screenHeight;
    public int frameCount;//逻辑帧数量
    public int frameRate;
    public byte[] ScreenPng;//截图文件
}

public class RecordMgr : Singleton<RecordMgr> {

    System.Threading.Thread WriteThread;
    string version;
    string recordPath;//路径
    byte[] screenShot;
    int frame;
    int frameRate;
    public void WriteFile()
    {
        version = Main.Ins.AppInfo.AppVersion();
        if (WriteThread != null)
            return;
        recordPath = string.Format("{0}/Record/", Application.persistentDataPath);
        screenShot = Utility.CaptureScreen(Main.Ins.MainCamera);
        frame = FrameReplay.Instance.globalFrame;
        frameRate = 1000 / FrameReplay.Instance.LogicFrameLength;
        WriteThread = new System.Threading.Thread(new System.Threading.ThreadStart(this.WriteFileCore));
        WriteThread.Start();
    }

    void WriteFileCore()
    {
        GameRecord record = new GameRecord();
        record.RecordVersion = version;
        record.guid = Guid.NewGuid();
        record.Name = GenerateRecordName(Main.Ins.CombatData.GLevelItem, Main.Ins.CombatData.GLevelMode, Main.Ins.CombatData.GGameMode);
        record.Mode = (int)Main.Ins.CombatData.GLevelMode;
        record.Chapter = (int)(Main.Ins.CombatData.Chapter == null ? 0 : Main.Ins.CombatData.Chapter.ChapterId);
        record.Id = (int)(Main.Ins.CombatData.GLevelItem.Id);
        record.RandomSeed = Main.Ins.CombatData.RandSeed;
        record.frames = Main.Ins.FrameSync.Frames;
        record.screenWidth = (int)(UIHelper.CanvasWidth / 5);
        record.screenHeight = (int)(UIHelper.CanvasHeight / 5);
        record.ScreenPng = screenShot;
        record.frameCount = frame;
        record.frameRate = frameRate;
        try
        {
            System.IO.FileStream fs = System.IO.File.Create(recordPath + record.Name + "_" + record.guid.ToString() +  ".mrc");
            ProtoBuf.Serializer.Serialize(fs, record);
            fs.Close();
        }
        catch (Exception exp)
        {
            LocalMsg resultFailed = new LocalMsg();
            resultFailed.Message = (int)LocalMsgType.SaveRecord;
            resultFailed.message = exp.Message;
            resultFailed.Result = 0;
            ProtoHandler.PostMessage(resultFailed);
            WriteThread = null;
            return;
        }
        //do sth
        LocalMsg result = new LocalMsg();
        result.Message = (int)LocalMsgType.SaveRecord;
        result.Result = 1;
        ProtoHandler.PostMessage(result);
        WriteThread = null;
    }

    string GenerateRecordName(LevelData data, LevelMode lm, GameMode gm)
    {
        //是什么类型的，单机，模组，联机，
        //模式-场景-主角-{单机/联机}
        string [] gms = new string[] { "盟主","劫镖","护城", "暗杀", "死斗", "剧情" };
        if (lm == LevelMode.MultiplyPlayer)
        {
            return string.Format("{0}_{1}_{2}_{3}人_联机", gms[(int)gm -1], data.Name, Main.Ins.GameStateMgr.gameStatus.NickName, Main.Ins.RoomMng.Current.maxPlayer);
        }
        return string.Format("{0}-{1}", gms[(int)gm - 1], data.Name);
    }
}
