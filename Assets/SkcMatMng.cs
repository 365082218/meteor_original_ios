using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//取得每个角色-队伍的2个材质
public class SkcMatMng : MonoBehaviour {
    public SkcMatItem[] Player;

    //在关卡模式下,不分阵营.
    public Material[] GetPlayerMat(int id, EUnitCamp camp)
    {
        if (camp == EUnitCamp.EUC_NONE || camp == EUnitCamp.EUC_KILLALL)
        {
            Material[] mat = new Material[Player[id].PlayerCamp0.Length];
            for (int i = 0; i < Player[id].PlayerCamp0.Length; i++)
                mat[i] = Player[id].PlayerCamp0[i] as Material;
            return mat;
        }
        else if (camp == EUnitCamp.EUC_FRIEND)
        {
            Material[] mat = new Material[Player[id].PlayerCamp0.Length];
            for (int i = 0; i < Player[id].PlayerCamp0.Length; i++)
                mat[i] = Player[id].PlayerCamp0[i] as Material;
            return mat;
        }
        else if (camp == EUnitCamp.EUC_ENEMY)
        {
            Material[] mat = new Material[Player[id].PlayerCamp0.Length];
            for (int i = 0; i < Player[id].PlayerCamp0.Length; i++)
                mat[i] = Player[id].PlayerCamp0[i] as Material;
            return mat;
        }
        return null;
    }
}
