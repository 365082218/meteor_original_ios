using DG.Tweening;
using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    Sequence hide;
    public override void OnDialogStateEnter(PersistState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }

    Scrollbar vscrollBar;
    void Init()
    {
        Root = Control("LevelTalk");
        WndObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(145, 400);
        vscrollBar = Control("Scrollbar Vertical").GetComponent<Scrollbar>();
    }

    //加入声音UI按钮
    Dictionary<int, byte[]> audioCache = new Dictionary<int, byte[]>();
    public void Add(int playerId, byte[] audio)
    {
        if (Root.transform.childCount >= 20)
        {
            if (audioCache.ContainsKey(0))
                audioCache.Remove(0);
            GameObject.Destroy(Root.transform.GetChild(0).gameObject);
        }
        GameObject obj = GameObject.Instantiate(Resources.Load("AudioMsg")) as GameObject;
        obj.name = (Root.transform.childCount + 1).ToString();
        obj.transform.GetChild(0).GetComponent<Text>().text = Main.Ins.NetWorkBattle.GetNetPlayerName(playerId) + "发来了一条语音信息";
        obj.transform.GetChild(1).GetComponent<UIFunCtrl>().SetEvent((int audioIdx) => { OnPlayAudio(audioIdx); }, Root.transform.childCount);
        obj.transform.SetParent(Root.transform);
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        if (hide != null)
            hide.Kill(false);
        WndObject.GetComponent<CanvasGroup>().alpha = 1.0f;
        hide = DOTween.Sequence();
        hide.AppendInterval(8.0f);
        hide.Append(WndObject.GetComponent<CanvasGroup>().DOFade(0, 5.0f));
        hide.Play();
        audioCache.Add(Root.transform.childCount - 1, audio);
        vscrollBar.value = 0;
    }

    void OnPlayAudio(int audioIndex)
    {
        if (audioCache.ContainsKey(audioIndex))
        {
            Main.Ins.Sound.Stop();
            Main.Ins.Music.Stop();
            AudioClip au = AudioClip.Create("talk", audioCache[audioIndex].Length, 1, MicChat.samplingRate, false);
            MicChat.SetData(au, audioCache[audioIndex]);
            Main.Ins.SoundManager.PlayClip(au);
        }
    }

    public void Add(int playerId, string message)
    {
        if (Root.transform.childCount >= 20)
            GameObject.Destroy(Root.transform.GetChild(0).gameObject);
        GameObject obj = new GameObject();
        obj.name = (Root.transform.childCount + 1).ToString();
        Text txt = obj.AddComponent<Text>();
        txt.text = string.Format("{0}:{1}", Main.Ins.NetWorkBattle.GetNetPlayerName(playerId), message);
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
        if (hide != null)
            hide.Kill(false);
        WndObject.GetComponent<CanvasGroup>().alpha = 1.0f;
        hide = DOTween.Sequence();
        hide.AppendInterval(8.0f);
        hide.Append(WndObject.GetComponent<CanvasGroup>().DOFade(0, 5.0f));
        hide.Play();
        vscrollBar.value = 0;
    }
}