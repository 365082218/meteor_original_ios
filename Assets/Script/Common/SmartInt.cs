using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public struct SmartInt
{
    static int[] Mask = new int[3] { 0, 0, 0 };
    int Int0;
    int Int1;
    int Int2;
    int Int3;

    static void Init()
    {
        for (int i = 0; i < 32; i++)
            Mask[UnityEngine.Random.Range(0, 100) % 3] |= 1 << i;
    }

    public SmartInt(int value)
    {
        // init the mask if used.
        if (Mask[0] == 0) while (Mask[0] == 0) Init();

        Int0 = UnityEngine.Random.Range(int.MinValue, int.MaxValue) & (~Mask[0]);
        Int1 = UnityEngine.Random.Range(int.MinValue, int.MaxValue) & (~Mask[1]);
        Int2 = UnityEngine.Random.Range(int.MinValue, int.MaxValue) & (~Mask[2]);
        Int0 |= (value & Mask[0]);
        Int1 |= (value & Mask[1]);
        Int2 |= (value & Mask[2]);
        Int3 = (Int0 ^ Int1 ^ Int2);
    }

    public static implicit operator int(SmartInt value)
    {
        if (value.Int3 != (value.Int0 ^ value.Int1 ^ value.Int2))
            throw new Exception("Data crrupted");
        return (value.Int0 & Mask[0]) | (value.Int1 & Mask[1]) | (value.Int2 & Mask[2]);
    }

    public static implicit operator SmartInt(int value)
    {
        return new SmartInt(value);
    }

    public static SmartInt operator ++(SmartInt value)
    {
        return new SmartInt(((int)value) + 1);
    }

    public static SmartInt operator --(SmartInt value)
    {
        return new SmartInt(((int)value) - 1);
    }

    public override string ToString()
    {
        return ((int)this).ToString();
    }

    public override bool Equals(object obj)
    {
        if (obj is SmartInt)
        {
            SmartInt other = (SmartInt)obj;
            return (int)this == (int)other;
        }
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return ((int)this).GetHashCode();
    }
}