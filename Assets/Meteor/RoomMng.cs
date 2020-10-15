using protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomMng:Singleton<RoomMng>
{
    public RoomInfo Current;
    public List<RoomInfo> rooms = new List<RoomInfo>();
    public void Register(int id, bool create)
    {
        if (create)
        {
            RoomInfo info = new RoomInfo();
            info.Group1 = 0;
            info.Group2 = 0;
            info.levelIdx = (uint)GameStateMgr.Ins.gameStatus.NetWork.LevelTemplate;
            info.maxPlayer = (uint)GameStateMgr.Ins.gameStatus.NetWork.MaxPlayer;
            info.playerCount = 0;
            info.roomId = (uint)id;
            info.roomName = GameStateMgr.Ins.gameStatus.NetWork.RoomName;
            info.rule = (RoomInfo.RoomRule)GameStateMgr.Ins.gameStatus.NetWork.Mode;
            info.version = (RoomInfo.MeteorVersion)GameStateMgr.Ins.gameStatus.NetWork.Version;
            info.pattern = (RoomInfo.RoomPattern)GameStateMgr.Ins.gameStatus.NetWork.Pattern;
            info.password = 0;
            info.hpMax = (uint)GameStateMgr.Ins.gameStatus.NetWork.Life;
            //
            info.owner = (uint)NetWorkBattle.Ins.PlayerId;
            //info.models.Add();
            rooms.Add(info);
        }
        else
        {

        }
    }

    public void RefreshRooms(List<RoomInfo> room)
    {
        rooms.Clear();
        for (int i = 0; i < room.Count; i++)
            rooms.Add(room[i]);
    }

    public RoomInfo GetRoom(int roomId)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].roomId == roomId)
                return rooms[i];
        }
        return null;
    }

    public int wantJoin { get { return joinRoom; } }
    int joinRoom;
    public void SelectRoom(int roomId) {
        joinRoom = roomId;
    }
}
