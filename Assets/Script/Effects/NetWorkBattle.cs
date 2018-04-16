using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using protocol;

public class NetWorkBattle : MonoBehaviour {

    //在房间的玩家.
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
	
	//等待服务器帧同步.
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
            //在联机战斗场景中.
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

    //同步全部的输入.
    public void OnSyncInput()
    {
        ServerFrameIndex++;
    }

    //同步全部数据
    public void OnSyncFrame()
    {

    }
}
