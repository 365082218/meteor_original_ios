using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugUtil {
    public static float speedScale = 1;
    public static void SpeedFast()
    {
        speedScale = speedScale * 2;
        if (speedScale >= 8f)
            speedScale = 8f;
    }

    public static void SpeedSlow()
    {
        speedScale = speedScale / 2;
        if (speedScale <= 0.02f)
            speedScale = 0.02f;
    }

    public static void SpeedFast1()
    {
        speedScale = speedScale + 0.1f;
        if (speedScale >= 8f)
            speedScale = 8f;
    }

    public static void SpeedSlow1()
    {
        speedScale = speedScale - 0.1f;
        if (speedScale <= 0.1f)
            speedScale = 0.1f;
    }

}
