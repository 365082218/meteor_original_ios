using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManyHeroTest : MonoBehaviour {
    public GameObject hero;
    List<MeteorUnitDebug> heroUnit = new List<MeteorUnitDebug>();
    // Use this for initialization
    void Start () {
        Global.GLevelMode = LevelMode.ANSHA;
        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                GameObject obj = Instantiate(hero);
                MeteorUnitDebug heroClone = obj.GetComponent<MeteorUnitDebug>();
                heroClone.Camp = (EUnitCamp)j;
                heroClone.Init(i);
                heroClone.transform.position = new Vector3(i * 30 - 300.0f, 0, j * 35);
                obj.SetActive(true);
                heroUnit.Add(heroClone);
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void EquipWeapon()
    {
        for (int i = 0; i < heroUnit.Count; i++)
        {
            heroUnit[i].GetComponent<WeaponLoaderEx>().EquipWeapon();
        }
    }

    public void GetOut()
    {
        U3D.GoBack();
    }
}
