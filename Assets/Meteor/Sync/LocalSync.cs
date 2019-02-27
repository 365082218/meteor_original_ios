using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalSync : MonoBehaviour {

    Actor[] GameActor = new Actor[0];
	void Update () {
        int n = GameActor.Length;
        for (int i = 0; i < n; i++)
        {
            if (GameActor[i] != null)
                GameActor[i].Update();
        }
	}

    //能够运行Update的，都注册到这里，由这里统一驱动
    Actor RegisterActor(MeteorUnit unit)
    {
        Actor player = new Actor();
        player.Attach(unit);
    }

    Actor RegisterActor(SceneItemAgent unit)
    {

    }
}
