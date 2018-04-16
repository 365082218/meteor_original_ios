using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using UnityEngine;

public class NetCommon
{
    // constant values.
    public const byte USER_CREATE_GAME = 10;
    public const byte USER_JOIN_GAME = 11;
    public const byte USER_LEAVE_GAME = 12;
    public const byte USER_MESSAGES = 13;
    public const byte SERVER_RESPONSE = 20;
    public const byte SERVER_MASTER_CHANGE = 21;

    // server response values.
    public const byte RESPONSE_OK = 0;
    public const byte RESPONSE_GAME_ALREAY_EXIST = 1;
    public const byte RESPONSE_GAME_NOT_EXIST = 2;
    public const byte RESPONSE_USER_ALREADY_EXIST = 10;
    public const byte RESPONSE_USER_NOT_EXIST = 11;
    public const byte RESPONSE_INCORRECT_PASS = 12;
    public const byte RESPONSE_USER_NOT_INGAME = 13;
    public const byte RESPONSE_UNKOWN_ERROR = 20;

    public const float SyncSinceAction = 0.04f; // if the action is send and moving, we should sync fast.
    public const float SyncSinceSync = 0.25f; // last sync.
    public const float GPredictionTime = SyncSinceSync + 0.1f;

    public static Int16 EncodeRotate(float rotate)
    {
        return (Int16)(rotate * Mathf.Rad2Deg + 0.5f);
    }

    public static float DecodeRotate(Int16 dir)
    {
        return dir * Mathf.Deg2Rad;
    }

    public static void Encode(Vector3 pos, float rotate, ref Int16 x, ref Int16 y, ref Int16 z, ref Int16 dir)
    {
        x = (Int16)(pos.x * 100.0f + 0.5f);
        y = (Int16)(pos.y * 100.0f + 0.5f);
        z = (Int16)(pos.z * 100.0f + 0.5f);
        dir = EncodeRotate(rotate);
    }

    public static void Decode(Int16 x, Int16 y, Int16 z, Int16 dir, ref Vector3 pos, ref float rotate)
    {
        pos.x = x * 0.01f;
        pos.y = (y + 1) * 0.01f;
        pos.z = z * 0.01f;
        rotate = DecodeRotate(dir);
    }
}

public enum MessageId
{
    None = 0,
    Moving,
    PlayAction,
    SyncPlayer,
    SyncMove,
    SyncRotate,
    HitData,
    Dead,
    UpdateUserInfo,
    MasterGameStated,
    MasterGameFinished,
    Buff,
}
