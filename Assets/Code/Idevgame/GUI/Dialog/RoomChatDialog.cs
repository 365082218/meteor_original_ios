using Assets.Code.Idevgame.Common.Util;
using DG.Tweening;
using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class RoomChatDialogState : PersistDialog<RoomChatDialog>
{
    public override string DialogName { get { return "RoomChatDialog"; } }
    public RoomChatDialogState()
    {

    }
}

public class RoomChatDialog : Dialog
{
    GameObject Root;
    Timer time;
    float alphaTime = 1.5f;
    public override void OnDialogStateEnter(PersistState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();

    }

    public override void OnClose() {
        base.OnClose();
        if (time != null) {
            time.Stop();
            time = null;
        }
        //if (checkSound != null) {
        //    checkSound.Stop();
        //    checkSound = null;
        //}
    }

    private void Crossfade() {
        if (Root.transform.childCount != 0) {
            Transform son = Root.transform.GetChild(0);
            Text[] graphs = son.GetComponentsInChildren<Text>();
            for (int i = 0; i < graphs.Length; i++)
                graphs[i].CrossFadeAlpha(0.0f, alphaTime, true);
            time.Stop();
            time = Timer.once(1.5f, Delete);
        } else {
            time.Stop();
        }
    }

    private void Delete() {
        if (Root.transform.childCount != 0) {
            Transform son = Root.transform.GetChild(0);
            //int audio = -1;
            //if (int.TryParse(son.name, out audio)) {
            //    if (audioCache.ContainsKey(audio)) {
            //        audioCache.Remove(audio);
            //    }
            //}
            GameObject.Destroy(son.gameObject);
            StartCheck();
        } else {
            time.Stop();
        }
    }

    Scrollbar vscrollBar;
    void Init()
    {
        Root = Control("LevelTalk");
        WndObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(180, -180);
        vscrollBar = Control("Scrollbar Vertical").GetComponent<Scrollbar>();
        //soundPlay = gameObject.AddComponent<AudioSource>();
    }

    //加入声音UI按钮
    //Dictionary<int, byte[]> audioCache = new Dictionary<int, byte[]>();
    //public void Add(int playerId, byte[] audio)
    //{
    //    GameObject obj = GameObject.Instantiate(Resources.Load("AudioMsg")) as GameObject;
    //    obj.name = (Root.transform.childCount + 1).ToString();
    //    obj.transform.GetChild(0).GetComponent<Text>().text = U3D.GetNetPlayerName(playerId) + "发来了一条语音信息";
    //    obj.transform.GetChild(1).GetComponent<UIFunCtrl>().SetEvent((int audioIdx) => { OnPlayAudio(audioIdx); }, Root.transform.childCount);
    //    obj.transform.SetParent(Root.transform);
    //    obj.transform.localScale = Vector3.one;
    //    obj.transform.localPosition = Vector3.zero;
    //    obj.transform.localRotation = Quaternion.identity;
    //    WndObject.GetComponent<CanvasGroup>().alpha = 1.0f;
    //    audioCache.Add(Root.transform.childCount - 1, audio);
    //    vscrollBar.value = 0;
    //    StartCheck();
    //}

    //AudioSource soundPlay;
    //void OnPlayAudio(int audioIndex)
    //{
    //    if (audioCache.ContainsKey(audioIndex))
    //    {
    //        SoundManager.Ins.Mute(true);
    //        AudioClip au = AudioClip.Create("talk", audioCache[audioIndex].Length, 1, MicChat.samplingRate, false);
    //        MicChat.SetData(au, audioCache[audioIndex]);
    //        soundPlay.PlayOneShot(au);
    //        if (checkSound != null)
    //            checkSound.Stop();
    //        checkSound = Timer.loop(0.5f, CheckSoundPlaying);
    //    }
    //}

    //Timer checkSound;
    //void CheckSoundPlaying() {
    //    if (!soundPlay.isPlaying) {
    //        SoundManager.Ins.Mute(false);
    //        checkSound.Stop();
    //        checkSound = null;
    //    }
    //}

    public List<string> msgHistory = new List<string>();
    public string GetHistoryMsg() {
        string msg = "";
        for (int i = 0; i < msgHistory.Count; i++) {
            msg += msgHistory[i] + "\n";
        }
        return msg;
    }
    public void Add(int playerId, string message)
    {
        GameObject obj = new GameObject();
        obj.name = (Root.transform.childCount + 1).ToString();
        Text txt = obj.AddComponent<Text>();
        string msg = string.Format("{0}[{1}]:{2}", U3D.GetNetPlayerName(playerId), playerId, message);
        txt.text = msg;
        msgHistory.Add(msg);
        //00AAFFFF
        txt.font = Main.Ins.TextFont;
        txt.fontSize = 32;
        txt.alignment = TextAnchor.MiddleLeft;
        txt.raycastTarget = false;
        txt.color = new Color(1.0f, 1.0f, 1.0f, 1f);
        obj.transform.SetParent(Root.transform);
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        WndObject.GetComponent<CanvasGroup>().alpha = 1.0f;
        vscrollBar.value = 0;
        StartCheck();
    }

    void StartCheck() {
        if (time != null)
            time.Stop();
        time = Timer.once(3, Crossfade);
    }
}