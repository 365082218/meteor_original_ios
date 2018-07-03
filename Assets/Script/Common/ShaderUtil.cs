using UnityEngine;
using System.Collections;

public class ShaderMng  {
    public static Shader Find(string shader)
    {
        int nindex = shader.LastIndexOf('/');
        string sub = shader.Substring(nindex + 1); 
        Shader sh = Resources.Load<Shader>(sub);
        return sh;
    }
}
