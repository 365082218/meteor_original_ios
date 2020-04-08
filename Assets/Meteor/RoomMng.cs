using protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomMng
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
            info.levelIdx = (uint)Main.Ins.GameStateMgr.gameStatus.NetWork.LevelTemplate;
            info.maxPlayer = (uint)Main.Ins.GameStateMgr.gameStatus.NetWork.MaxPlayer;
            info.playerCount = 0;
            info.roomId = (uint)id;
            info.roomName = Main.Ins.GameStateMgr.gameStatus.NetWork.RoomName;
            info.rule = (RoomInfo.RoomRule)Main.Ins.GameStateMgr.gameStatus.NetWork.Mode;
            info.version = (RoomInfo.MeteorVersion)Main.Ins.GameStateMgr.gameStatus.NetWork.Version;
            info.pattern = (RoomInfo.RoomPattern)Main.Ins.GameStateMgr.gameStatus.NetWork.Pattern;
            rooms.Add(info);
        }
        else
        {

        }
    }

    public void RegisterRooms(List<RoomInfo> room)
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
}
