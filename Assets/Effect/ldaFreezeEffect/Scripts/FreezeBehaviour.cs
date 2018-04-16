using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class FreezeBehaviour : MonoBehaviour
{

  public Material iceMaterial;
  public bool isFrozen;
  public float FreezeTime = 5;

  private Material oldMaterial;
  private bool isCanSetMaterial = true;
  private float cutoff;
  private FrozenState frozenState = FrozenState.Default;
  // Use this for initialization
  private void Start()
  {
    if(iceMaterial == null) Debug.Log("FreezeBehaviour can't fing Ice material");
    oldMaterial = GetComponent<Renderer>().materials[0];
    if (iceMaterial!=null)
      iceMaterial.SetFloat("_Cutoff", 0);
  }

  //add by Lindean
  public void OnDisable()
  {
    if (iceMaterial != null)
        iceMaterial.SetFloat("_Cutoff", 0);
    cutoff = 0;

    var materials = new Material[1];
    materials[0] = oldMaterial;
    GetComponent<Renderer>().materials = materials;
  }

  // Update is called once per frame
  private void Update()
  {
    if (isFrozen) frozenState = FrozenState.LoadMaterial;
    else if(frozenState!=FrozenState.Default) frozenState = FrozenState.UpdateMaterialUnFreeze;
    if (frozenState==FrozenState.LoadMaterial) LoadMaterial();
    if (frozenState==FrozenState.UpdateMaterialFreeze) UpdateMaterialFreeze();
    if (frozenState==FrozenState.UpdateMaterialUnFreeze) UpdateMaterialUnFreeze();
  }

  private void LoadMaterial()
  {
    isCanSetMaterial = false;
    var materials = new Material[2];
    materials[0] = oldMaterial;
    materials[1] = iceMaterial;
    GetComponent<Renderer>().materials = materials;
    frozenState = FrozenState.UpdateMaterialFreeze;
  }

  private void UpdateMaterialFreeze()
  {
    var time = Time.deltaTime / FreezeTime;
    if (cutoff + time <= 1) {
      cutoff += time;
      iceMaterial.SetFloat("_Cutoff", cutoff);
    }
    else frozenState = FrozenState.Frozen;
  }

  private void UpdateMaterialUnFreeze()
  {
    var time = Time.deltaTime / FreezeTime;
    if (cutoff-time > 0) {
      cutoff -= time;
      iceMaterial.SetFloat("_Cutoff", cutoff);
    }
    else frozenState = FrozenState.Default;
  }

  enum FrozenState
  {
    Default,
    LoadMaterial,
    UpdateMaterialFreeze,
    UpdateMaterialUnFreeze,
    Frozen
  }
}

