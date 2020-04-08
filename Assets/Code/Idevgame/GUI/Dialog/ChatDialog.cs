using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatDialogState : PersistDialog<ChatDialog>
{
    public override string DialogName { get { return "ChatDialog"; } }
    public ChatDialogState()
    {

    }
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
        if (Microphone.IsRecording(null))
        {
            Record();
        }
    }


    InputField inputChat;
    LitJson.JsonData jsData;
    void Init()
    {
        inputChat = Control("ChatText", WndObject).GetComponent<InputField>();
        Control("SendShortMsg", WndObject).GetComponent<Button>().onClick.AddListener(() =>
        {
            if (Main.Ins.CombatData.GLevelMode == LevelMode.MultiplyPlayer)
            {
                string chatMessage = inputChat.text;
                if (string.IsNullOrEmpty(inputChat.text))
                {
                    U3D.PopupTip("无法发送内容为空的语句");
                    return;
                }
                Common.SendChatMessage(chatMessage);
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
        Control("CloseAudioMsg", WndObject).GetComponent<Button>().onClick.AddListener(() =>
        {
            OnBackPress();
        });
        if (jsData == null)
        {
            TextAsset text = ResMng.Load("MsgTable") as TextAsset;
            jsData = LitJson.JsonMapper.ToObject(text.text);
        }
        for (int i = 0; i < jsData["Msg"].Count; i++)
        {
            string strQuick = jsData["Msg"][i].ToString();
            Control(i.ToString(), WndObject).GetComponentInChildren<Text>().text = strQuick;
            int j = i;
            Control(i.ToString(), WndObject).GetComponent<Button>().onClick.AddListener(() => { SendQuickMsg(j); });
        }
        Control("Record", WndObject).GetComponent<Button>().onClick.AddListener(() => { Record(); });
        Control("SendAudio", WndObject).GetComponent<Button>().onClick.AddListener(() => { SendAudioMsg(); });
        GameObject objListen = Control("Listen", WndObject);
        source = objListen.GetComponent<AudioSource>();
        Listen = objListen.GetComponent<Button>();
        Listen.onClick.AddListener(() =>
        {
            if (MicChat.clip != null)
                source.PlayOneShot(MicChat.clip);
            Debug.Log("play clip");
        });
        CountDown = Control("CountDown", WndObject);
    }

    void SendQuickMsg(int i)
    {
        Common.SendChatMessage(jsData["Msg"][i].ToString());
    }

    void SendAudioMsg()
    {
        if (!recording && audioData != null && audioData.Length != 0 && Listen != null && Listen.IsActive())
        {
            Common.SendAudioMessage(audioData);
        }
    }

    bool recording = false;
    byte[] audioData;
    AudioSource source;
    AudioClip clip;
    Button Listen;
    GameObject CountDown;
    float RecordTick = 0;
    void Record()
    {
        if (recording)
        {
            Main.Ins.SoundManager.Mute(false);
            int length = 0;
            clip = null;
            MicChat.EndRecording(out length, out clip);
            recording = false;
            CountDown.SetActive(false);
            audioData = clip.GetData();
            if (audioData != null && audioData.Length != 0)
                Listen.gameObject.SetActive(true);
            Control("Record", WndObject).GetComponentInChildren<Text>().text = "录音";
            return;
        }
        else
        {
            //把音效和音乐全部静止
            Main.Ins.SoundManager.Mute(true);
            recording = MicChat.TryStartRecording();
            if (recording)
            {
                Control("Record", WndObject).GetComponentInChildren<Text>().text = "停止";
                if (Listen == null)
                {
                    GameObject objListen = Control("Listen", WndObject);
                    Listen = objListen.GetComponent<Button>();
                    objListen.SetActive(false);
                }
                else
                {
                    Listen.gameObject.SetActive(false);
                }
                CountDown.SetActive(true);
                RecordTick = 0;
                nSeconds = Mathf.CeilToInt(MicChat.maxRecordTime - RecordTick);
                CountDown.GetComponentInChildren<Text>().text = string.Format("录制中{0}", nSeconds);
            }
        }
    }

    int nSeconds = 0;
    void Update()
    {
        Debug.Log("chatwnd update");
        RecordTick += FrameReplay.deltaTime;
        if (RecordTick >= 8)
        {
            Record();
        }
        else
        {
            int i = Mathf.CeilToInt(MicChat.maxRecordTime - RecordTick);
            if (nSeconds != i)
            {
                nSeconds = i;
                CountDown.GetComponentInChildren<Text>().text = string.Format("录制中{0}", nSeconds);
            }
        }
    }
}
