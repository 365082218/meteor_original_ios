using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//取得每个角色-队伍的2个材质
public class SkcMatMng : MonoBehaviour {
    public SkcMatItem[] Player;
    public SkcMatItem[] Player800;
    public SkcMatItem[] Player300;
    //在关卡模式下,不分阵营.
    public Material[] GetPlayerMat(int id, EUnitCamp camp)
    {
        SkcMatItem[] TargetGroup = null;
        if (GameStateMgr.Ins.gameStatus.Quality == 0)
            TargetGroup = Player;
        else if (GameStateMgr.Ins.gameStatus.Quality == 1)
            TargetGroup = Player800;
        else if (GameStateMgr.Ins.gameStatus.Quality == 2)
            TargetGroup = Player300;

        //普通关卡和盟主模式,只有1个阵营的皮肤
        if (CombatData.Ins.GLevelMode <= LevelMode.SinglePlayerTask)
        {
            if (camp == EUnitCamp.EUC_NONE || camp == EUnitCamp.EUC_KILLALL)
            {
                Material[] mat = new Material[TargetGroup[id].PlayerCamp0.Length];
                for (int i = 0; i < TargetGroup[id].PlayerCamp0.Length; i++)
                    mat[i] = TargetGroup[id].PlayerCamp0[i] as Material;
                return mat;
            }
            else if (camp == EUnitCamp.EUC_FRIEND)
            {
                Material[] mat = new Material[TargetGroup[id].PlayerCamp0.Length];
                for (int i = 0; i < TargetGroup[id].PlayerCamp0.Length; i++)
                    mat[i] = TargetGroup[id].PlayerCamp0[i] as Material;
                return mat;
            }
            else if (camp == EUnitCamp.EUC_ENEMY)
            {
                Material[] mat = new Material[TargetGroup[id].PlayerCamp0.Length];
                for (int i = 0; i < TargetGroup[id].PlayerCamp0.Length; i++)
                    mat[i] = TargetGroup[id].PlayerCamp0[i] as Material;
                return mat;
            }
        }
        else if (CombatData.Ins.GLevelMode > LevelMode.SinglePlayerTask && CombatData.Ins.GLevelMode <= LevelMode.MultiplyPlayer)
        {
            if (CombatData.Ins.GGameMode == GameMode.Normal || CombatData.Ins.GGameMode == GameMode.MENGZHU)
            {
                if (camp == EUnitCamp.EUC_NONE || camp == EUnitCamp.EUC_KILLALL)
                {
                    Material[] mat = new Material[TargetGroup[id].PlayerCamp0.Length];
                    for (int i = 0; i < TargetGroup[id].PlayerCamp0.Length; i++)
                        mat[i] = TargetGroup[id].PlayerCamp0[i] as Material;
                    return mat;
                }
                else if (camp == EUnitCamp.EUC_FRIEND)
                {
                    Material[] mat = new Material[TargetGroup[id].PlayerCamp0.Length];
                    for (int i = 0; i < TargetGroup[id].PlayerCamp0.Length; i++)
                        mat[i] = TargetGroup[id].PlayerCamp0[i] as Material;
                    return mat;
                }
                else if (camp == EUnitCamp.EUC_ENEMY)
                {
                    Material[] mat = new Material[TargetGroup[id].PlayerCamp0.Length];
                    for (int i = 0; i < TargetGroup[id].PlayerCamp0.Length; i++)
                        mat[i] = TargetGroup[id].PlayerCamp0[i] as Material;
                    return mat;
                }
            }
            else if (CombatData.Ins.GGameMode == GameMode.ANSHA || CombatData.Ins.GGameMode == GameMode.SIDOU)
            {
                if (camp == EUnitCamp.EUC_NONE || camp == EUnitCamp.EUC_KILLALL)
                {
                    Material[] mat = new Material[TargetGroup[id].PlayerCamp2.Length];
                    for (int i = 0; i < TargetGroup[id].PlayerCamp2.Length; i++)
                        mat[i] = TargetGroup[id].PlayerCamp2[i] as Material;
                    return mat;
                }
                else if (camp == EUnitCamp.EUC_FRIEND)
                {
                    Material[] mat = new Material[TargetGroup[id].PlayerCamp0.Length];
                    for (int i = 0; i < TargetGroup[id].PlayerCamp0.Length; i++)
                        mat[i] = TargetGroup[id].PlayerCamp0[i] as Material;
                    return mat;
                }
                else if (camp == EUnitCamp.EUC_ENEMY)
                {
                    Material[] mat = new Material[TargetGroup[id].PlayerCamp1.Length];
                    for (int i = 0; i < TargetGroup[id].PlayerCamp1.Length; i++)
                        mat[i] = TargetGroup[id].PlayerCamp1[i] as Material;
                    return mat;
                }
            }
        }
        return null;
    }
}
