using UnityEngine;
using System.Collections;

public class ShaderUtil  {

	public static void  FiexdActionAndGodDoubleSide(GameObject go)
	{
		Animation[] anis = go.GetComponentsInChildren<Animation>(true);
		foreach(Animation ani in anis)ani.cullingType = AnimationCullingType.BasedOnRenderers;
		Renderer[] shds = go.GetComponentsInChildren<Renderer>(true);
		foreach(Renderer sd in shds){
			foreach(Material m in sd.sharedMaterials ){
				if(m!=null && m.shader!=null && !m.shader.name.Equals("")&& !m.shader.name.Equals("GOD/DoubleSide")){
					continue;
				}else if(m!=null &&( m.shader==null||m.shader.name=="")){
					m.shader=Shader.Find("GOD/DoubleSide");
				}else continue;
				m.SetColor("_Color",Color.black);
				m.SetColor("_Emission",new Color(0.5f,0.5f,0.5f,0.6f));
			}
		}
	}

    public static Shader Find(string shader)
    {
        int nindex = shader.LastIndexOf('/');
        string sub = shader.Substring(nindex + 1); 
        Shader sh = Resources.Load<Shader>(sub);
        return sh;
    }
}
