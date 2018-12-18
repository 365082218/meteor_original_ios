using UnityEngine;
using System.Collections;
namespace MeowCats_amesyo
{
public class Swap_Tex : MonoBehaviour {

public Texture Face1;
public Texture Face2;
private int swp;
private float ctime = 0;
private float change = 0.1F;
[Header("Establishment = 10 - 95")]
public int eStab = 95;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
			if (ctime < change){
				ctime += Time.deltaTime;
			}else{
				swp = Random.Range(0,100);
				if (swp > eStab) {
					gameObject.GetComponent<Renderer>().materials[1].mainTexture = Face2;
				} else {
					gameObject.GetComponent<Renderer>().materials[1].mainTexture = Face1;
				}
			ctime =0;
			}
		}
	}
}

