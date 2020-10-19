using Assets.Code.Idevgame.Common.Util;
using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ChatDialogState : PersistDialog<ChatDialog>
{
    public override string DialogName { get { return "ChatDialog"; } }
}

public class ChatDialog : Dialog
{
    public override void OnDialogStateEnter(PersistState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }

    public override void OnClose()
    {
        //if (checkSound != null) {
        //    checkSound.Stop();
        //    checkSound = null;
        //}
        //if (Microphone.IsRecording(null))
        //{
        //    Record();
        //}
    }


    InputField inputChat;
    LitJson.JsonData jsData;
    void Init()
    {
        inputChat = Control("ChatText", WndObject).GetComponent<InputField>();
        Control("SendShortMsg", WndObject).GetComponent<Button>().onClick.AddListener(() =>
        {
            if (U3D.IsMultiplyPlayer())
            {
                string chatMessage = inputChat.text;
                if (string.IsNullOrEmpty(inputChat.text))
                {
                    U3D.PopupTip("无法发送内容为空的语句");
                    return;
                }
                TcpClientProxy.Ins.SendChatMessage(chatMessage);
                OnBackPress();
            }
        });
        Control("CloseQuickMsg", WndObject).GetComponent<Button>().onClick.AddListener(() =>
        {
            OnBackPress();
        });
        Control("CloseShortMsg", WndObject).GetComponent<Button>().onClick.AddListener(() =>
        {
            OnBackPress();
        });
        Control("CloseHistoryMsg", WndObject).GetComponent<Button>().onClick.AddListener(() => {
            OnBackPress();
        });
        Control("CloseAudioMsg", WndObject).GetComponent<Button>().onClick.AddListener(() =>
        {
            OnBackPress();
        });
        if (jsData == null)
        {
            TextAsset text = Resources.Load("MsgTable") as TextAsset;
            jsData = LitJson.JsonMapper.ToObject(text.text);
        }
        for (int i = 0; i < jsData["Msg"].Count; i++)
        {
            string strQuick = jsData["Msg"][i].ToString();
            Control(i.ToString(), WndObject).GetComponentInChildren<Text>().text = strQuick;
            int j = i;
            Control(i.ToString(), WndObject).GetComponent<Button>().onClick.AddListener(() => { SendQuickMsg(j); });
        }
        //Control("Record", WndObject).GetComponent<Button>().onClick.AddListener(() => { Record(); });
        //Control("SendAudio", WndObject).GetComponent<Button>().onClick.AddListener(() => { SendAudioMsg(); });
        //GameObject objListen = Control("Listen", WndObject);
        //source = objListen.GetComponent<AudioSource>();
        //Listen = objListen.GetComponent<Button>();
        //Listen.onClick.AddListener(() =>
        //{
        //    if (MicChat.clip != null) {
        //        SoundManager.Ins.Mute(true);
        //        source.PlayOneShot(MicChat.clip);
        //        Debug.Log("play clip");
        //        if (checkSound != null) {
        //            checkSound.Stop();
        //        }
        //            checkSound = Timer.loop(0.5f, CheckSound);
        //    }
        //});
        if (RoomChatDialogState.Exist()) {
            string msg = RoomChatDialogState.Instance.GetHistoryMsg();
            Add(msg);
        }
        //CountDown = Control("CountDown", WndObject);
    }

    public void Add(string message) {
        GameObject Root = Control("MsgHistory");
        GameObject obj = new GameObject();
        obj.name = (Root.transform.childCount + 1).ToString();
        Text txt = obj.AddComponent<Text>();
        txt.text = message;
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
    }
    //void CheckSound() {
    //    if (!source.isPlaying) {
    //        checkSound.Stop();
    //        checkSound = null;
    //        SoundManager.Ins.Mute(false);
    //    }
    //}

    //Timer checkSound;
    void SendQuickMsg(int i)
    {
        TcpClientProxy.Ins.SendChatMessage(jsData["Msg"][i].ToString());
        OnBackPress();
    }

    //void SendAudioMsg()
    //{
    //    if (!recording && audioData != null && audioData.Length != 0 && Listen != null && Listen.IsActive())
    //    {
    //        Debug.LogWarning("audio data length:" + audioData.Length);
    //        if (audioData.Length > PacketProxy.MaxSize) {
    //            U3D.PopupTip("语音包太大，无法发送");
    //            return;
    //        }
    //        TcpClientProxy.Ins.SendAudioMessage(audioData);
    //    }
    //    OnBackPress();
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
    //        Stop();
    //        return;
    //    }
    //    else
    //    {
    //        //把音效和音乐全部静止
    //        SoundManager.Ins.Mute(true);
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
    //            RecordTick = 0;
    //            nSeconds = MicChat.maxRecordTime - (int)RecordTick;
    //            CountDown.GetComponentInChildren<Text>().text = string.Format("录制中{0}", nSeconds);
    //        }
    //    }
    //}

    //void Stop() {
    //    SoundManager.Ins.Mute(false);
    //    int length = 0;
    //    clip = null;
    //    MicChat.EndRecording(out length, out clip);
    //    recording = false;
    //    CountDown.SetActive(false);
    //    audioData = clip.GetData();
    //    if (audioData != null && audioData.Length != 0)
    //        Listen.gameObject.SetActive(true);
    //    Control("Record", WndObject).GetComponentInChildren<Text>().text = "录音";
    //}

    //int nSeconds = 0;
    //void Update()
    //{
    //    if (recording) {
    //        RecordTick += FrameReplay.deltaTime;
    //        if (RecordTick >= 8) {
    //            Stop();
    //        } else {
    //            int i = MicChat.maxRecordTime - (int)RecordTick;
    //            if (nSeconds != i) {
    //                nSeconds = i;
    //                CountDown.GetComponentInChildren<Text>().text = string.Format("录制中{0}", nSeconds);
    //            }
    //        }
    //    }
    //}
}
