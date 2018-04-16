using UnityEngine;
using System.Collections;

public class ChangeShader :  MonoBehaviour {
	public Shader SourceShader;
    public Shader ReplaceShader;

	public void Change()
	{
        if (SourceShader == null || ReplaceShader == null)
        {
            Debug.LogError("select shader is null");
            return;
        }
        string source = SourceShader.name;
        string target = ReplaceShader.name;

        Renderer[] renders = GetComponentsInChildren< Renderer>();
		foreach(Renderer r in renders)
		{
			if (r  !=  null)
			{
				foreach(Object o in r.sharedMaterials)
				{
					Material m = o as Material;
                    if (m.shader != null && m.shader.name == source)
						m.shader = ReplaceShader;	
				}
			}
		}
	}
	
}
