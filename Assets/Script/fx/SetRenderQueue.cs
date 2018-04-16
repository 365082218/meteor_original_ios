using UnityEngine;
using System.Collections;
[ExecuteInEditMode]
public class SetRenderQueue : MonoBehaviour 
{
    public int mRendererQueue;
	Renderer[] mRd;
    //void Start()
    //{
    //    mRd = GetComponentsInChildren<Renderer>();
    //}

	
    //// Update is called once per frame
    //void Update () 
    //{
		
    //    if(mRd != null && mRd.Length>0)
    //    {
    //        foreach(Renderer tmpR in mRd)
    //        {
    //            if(tmpR!=null && tmpR.sharedMaterials!=null && tmpR.sharedMaterials.Length>0)
    //            {
    //                foreach(Material tmpM in tmpR.sharedMaterials)
    //                {
    //                    tmpM.renderQueue =tmpM.shader.renderQueue+mRendererQueue;
    //                    //Debug.Log("Material : "tmpM.name + "renderQueue=" +tmpM.renderQueue);
    //                }
    //            }
    //        }
    //    }
	
    //}
}