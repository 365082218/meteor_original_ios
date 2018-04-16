using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using protocol;

public class NetWorkBattle : MonoBehaviour {

    //�ڷ�������.
    Dictionary<int, MeteorUnit> player = new Dictionary<int, MeteorUnit>();
    SceneInfo Info;

    int RoomId;
    string RoomName;
    bool bSync;
    int FrameIndex;
    int ServerFrameIndex;
    static NetWorkBattle _Ins;
    public static NetWorkBattle Ins { get { return _Ins; } }
    // Use this for initialization
    private void Awake()
    {
        if (_Ins == null)
            _Ins = this;    
    }

    void Start () {
        RoomId = -1;
        RoomName = "";
        bSync = false;
	}
	
	//�ȴ�������֡ͬ��.
	void Update () {
        if (bSync)
        {
            while (FrameIndex < ServerFrameIndex)
            {
                //foreach (var each in player)
                //    each.Value.GameFrame(FrameIndex);
                //GameBattleEx.Instance.GameFrame(FrameIndex);
                FrameIndex++;
            }
        }
	}

    public void OnDisconnect()
    {
        if (GameBattleEx.Instance && RoomId != -1)
        {
            //������ս��������.
            GameBattleEx.Instance.Pause();
            SoundManager.Instance.StopAll();
            BuffMng.Instance.Clear();
            MeteorManager.Instance.Clear();
            FightWnd.Instance.Close();
            player.Clear();
            U3D.GoBack();
            bSync = false;
            FrameIndex = ServerFrameIndex = 0;
        }
        RoomId = -1;
        RoomName = "";
    }


    public void OnEnterLevelSucessed(SceneInfo scene)
    {
        Info = scene;
        FrameIndex = 0;
    }

    public void OnNetWorkBattleStart()
    {
        bSync = true;
    }

    //ͬ��ȫ��������.
    public void OnSyncInput()
    {
        ServerFrameIndex++;
    }

    //ͬ��ȫ������
    public void OnSyncFrame()
    {

    }
}
