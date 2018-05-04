using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//通过控制点（包含起始点），以及规格化参数t,计算贝塞尔曲线任意点.
public class Spline
{
    Vector3[] ControlPath;
    int ControlPoint;
    public Spline(int num)
    {
        ControlPoint = num;
        ControlPath = new Vector3[num];
    }

    public void SetControlPath(IEnumerable<Vector3> vec)
    {
        List<Vector3> vecL = new List<Vector3>();
        vecL.AddRange(vec);
        ControlPath = vecL.ToArray();
        ControlPoint = ControlPath.Length;
    }

    public Vector3 GetPathPoint(int path)
    {
        return ControlPath[path];
    }

    public void SetControlPointsCount(int count)
    {
        ControlPoint = count;
    }

    //得到全部控制点组成的曲线的长度, 大致的等于2个线段的和吧，不然用微积分算很麻烦
    public float GetLength()
    {
        //Debug.LogError("getlength start");
        //for (int i = 0; i < 3; i++)
        //    Debug.LogError("ControlPath:" + i + " :" + ControlPath[i].ToString());
        //Debug.LogError(Vector3.Distance(ControlPath[1], ControlPath[0]));
        //Debug.LogError(Vector3.Distance(ControlPath[1], ControlPath[2]));
        //Debug.LogError("getlength end");
        return Vector3.Distance(ControlPath[1], ControlPath[0]) + Vector3.Distance(ControlPath[1], ControlPath[2]);
    }

    public void SetControlPoint(int i, Vector3 vec)
    {
        ControlPath[i] = vec;
    }

    public List<Vector3> GetEquiDistantPointsOnCurve(int segment)
    {
        float offset = 1.0f / segment;
        List<Vector3> ret = new List<Vector3>();
        for (int i = 0; i < segment; i++)
        {
            float t = (float)i * offset;
            ret.Add(Eval(t));
        }
        return ret;
    }

    public Vector3 Eval(float t)
    {
        //Debug.LogError("t:" + t);
        if (ControlPoint < 2 || ControlPath.Length < 2)
            return Vector3.zero;
        if (t >= 1)
            return ControlPath[ControlPoint - 1];
        if (t <= 0)
            return ControlPath[0];
        if (ControlPoint == 2)
            return (t) * (t * (ControlPath[2] - ControlPath[1]) - t * (ControlPath[1] - ControlPath[0]));

        int totalPoint = ControlPoint;
        //贝塞尔曲线算法
        List<Vector3> vecNext = new List<Vector3>();
        vecNext.AddRange(ControlPath);
        while (totalPoint > 2)
        {
            List<Vector3> vec = new List<Vector3>();
            for (int i = 0; i < totalPoint - 1; i++)
                vec.Add(t * (vecNext[i + 1] - vecNext[i]) + vecNext[i]);
            vecNext.Clear();
            vecNext.AddRange(vec);
            totalPoint--;
        }
        return t * (vecNext[1] - vecNext[0]) + vecNext[0];
    }
}