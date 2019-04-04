using protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetSync : MonoBehaviour {
    //顺序是由playerId由小到大跑
    
    TurnFrames Frame;
    float LogicTime = 0.0f;
    // Update is called once per frame
    Actor[] SceneItem = new Actor[0];
	void Update () {
        LogicTime += Time.deltaTime;
        if (LogicTime >= 0.02f)
        {
            NetUpdate();
            LogicTime = 0.0f;
        }
	}

    public void NetUpdate()
    {
        //if (TurnIndex != Frame.turnIndex)
        //    return;
        //if (FrameIndex >= Frame.Inputs[0].frames.Count)
        //    return;
        
        //for (int i = 0; i < Frame.Inputs.Count; i++)
        //    OnNetInput(Frame.Inputs[i].playerId, Frame.Inputs[i].frames[FrameIndex]);
        //FrameIndex++;
        GameBattleEx.Instance.NetUpdate();
        for (int i = 0; i < SceneItem.Length; i++)
        {
            if (SceneItem[i] != null)
                SceneItem[i].Update();
        }
    }

    void OnNetInput(uint player, InputFrame fInput)
    {
        MeteorUnit unit = NetWorkBattle.Ins.GetNetPlayer((int)player);
        unit.OnNetInput(fInput);
    }
}
