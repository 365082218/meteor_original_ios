using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class PlaneShadowCaster : MonoBehaviour {
    //public Transform reciever;

    List<Material> UpdateMaterials = new List<Material>();

    public bool DebugResetShadowMatrix = false;

    public bool IsShowShadow = true;
	public float shadowPlaneHeight=-100000f;
    GameObject MasterGo;

    void OnEnable()
    {
        DebugResetShadowMatrix = true;
    }
	private bool IsShowShadowPre=true;
	void Update () {

        //renderer.sharedMaterial.SetMatrix("_World2Ground",reciever.renderer.worldToLocalMatrix);
        //renderer.sharedMaterial.SetMatrix("_Ground2World", reciever.renderer.localToWorldMatrix);

        //Vector3 coinposition = this.transform.position;
        //RaycastHit hit = new RaycastHit();
        //if (Physics.Linecast(coinposition, new Vector3(coinposition.x, coinposition.y - 10, coinposition.z), out hit, (1 << LayerMask.NameToLayer("SceneShadow")) | (1 << LayerMask.NameToLayer("Scene"))))
        //    coinposition = new Vector3(hit.point.x, hit.point.y + 0.1f, hit.point.z);
        //this.transform.position = coinposition;

        //for (int i = 0; i < UpdateMaterials.Count; i++)
        //{
        //    UpdateMaterials[i].SetMatrix("_World2Ground", this.transform.renderer.worldToLocalMatrix);
        //    UpdateMaterials[i].SetMatrix("_Ground2World", this.transform.renderer.localToWorldMatrix);
        //}

		Vector3 p=this.transform.parent.transform.position;
		if((p.y==shadowPlaneHeight+0.05f)&& IsShowShadowPre ==IsShowShadow )return;
		p.y=shadowPlaneHeight+0.05f;
		this.transform.position=p;

	   // if (IsShowShadow)
	    {
	        for (int i = 0; i < UpdateMaterials.Count; i++)
	        {
                if (IsShowShadow)
                {
                    UpdateMaterials[i].shader.maximumLOD = 100;//100 to show shadow ,>100  and default hidden
                }
                else
                {
                    //UpdateMaterials[i].shader.maximumLOD = 200;//100 to show shadow ,>100  and default hidden
                    UpdateMaterials[i].shader.maximumLOD = 200;//100 to show shadow ,>100  and default hidden
                    p.y += 10000;
                    this.transform.position = p;
                }

	            UpdateMaterials[i].SetMatrix("_World2Ground", this.transform.worldToLocalMatrix);
	            UpdateMaterials[i].SetMatrix("_Ground2World", this.transform.localToWorldMatrix);

	        }
	    }
		IsShowShadowPre=IsShowShadow;
	    DebugResetShadowMatrix = false;
        
	}

    //脚本必须挂在Prefab下第一级节点
    void Start()
    {
        transform.localRotation = Quaternion.Euler(0, 0, 0);
        transform.localScale = Vector3.one;
		transform.localPosition = new Vector3(0,0.01f,0);

		MasterGo = gameObject.transform.parent.gameObject;
		UpdateMaterials.Clear();
        if (MasterGo == null)
            return;

        SkinnedMeshRenderer[] skinnedmeshrenderers = MasterGo.GetComponentsInChildren<SkinnedMeshRenderer>(true);

        foreach (SkinnedMeshRenderer smr in skinnedmeshrenderers)
        {
			Material[] materials = smr.materials;

			foreach (Material m in materials)
            {
                if (m.shader.name != "GOD/DoubleSide")
                    continue;

                UpdateMaterials.Add(m);
                m.SetMatrix("_World2Ground", this.transform.worldToLocalMatrix);
                m.SetMatrix("_Ground2World", this.transform.localToWorldMatrix);

				m.shader.maximumLOD = 200;// ?????

            }
			this.enabled=false;
        }

        //Material[] materials = .sharedMaterials;
        //foreach (Material m in materials)
        //{
        //    m.SetMatrix("_World2Ground", this.transform.renderer.worldToLocalMatrix);
        //    m.SetMatrix("_Ground2World", this.transform.renderer.localToWorldMatrix);
        //}
    }
}
