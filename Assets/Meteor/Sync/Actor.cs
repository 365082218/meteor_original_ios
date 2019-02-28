using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface INetUpdate
{
    void NetUpdate();
}

public class Actor{
    INetUpdate Target;
    public void Update()
    {
        if (Target != null)
            Target.NetUpdate();
    }

    public void Attach(INetUpdate Attach)
    {
        Target = Attach;
    }
}
