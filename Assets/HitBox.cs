using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour {
    public EUnitCamp Camp;
    RandMonsterCtrl ctrl;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if (ctrl != null)
        {
            DartLoader dart = other.GetComponentInParent<DartLoader>();
            if (dart != null)
            {
                MeteorUnit u = dart.Owner();
                if (u != null && u.Camp != Camp)
                    ctrl.OnDamagedByUnit(u, dart._attack);
            }
            
        }
    }
}
