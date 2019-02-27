using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalActor{
    ILocalUpdate Target;
    public void Update()
    {
        if (Target != null)
            Target.LocalUpdate();
    }

    public void Attach(ILocalUpdate Attach)
    {
        Target = Attach;
    }
}
