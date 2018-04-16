using UnityEngine;
using System.Collections;

public class CoinsTrigger : MonoBehaviour {

	public AudioClip coinsSound;
	//private CoinsNum cn;
	public int num;

	public float dis;
	
	void Awake () {
		//GameObject coinsNumUI = GameObject.Find ("UI Root/Camera/Anchor-TopRight/Panel/Coins");
		//cn = coinsNumUI.gameObject.GetComponent<CoinsNum>();
	}
	
	void OnTriggerEnter (Collider col) {
        //if (col.tag == "PlayerNinja") {
        //    //Destroy(this.gameObject);
        //    StartCoroutine(DestoryWaitTime ());
        //    //cn.CoinsNumAdd (num);
        //    audio.PlayOneShot (coinsSound);
        //}

        MeteorUnit unit = col.gameObject.transform.root.GetComponent<MeteorUnit>();
        if (unit == MeteorManager.Instance.LocalPlayer)
        {
            StartCoroutine(DestoryWaitTime());
            GetComponent<AudioSource>().PlayOneShot(coinsSound);
        }
	}

	IEnumerator DestoryWaitTime () {
		yield return new WaitForSeconds(0.3f);
		Destroy(this.gameObject);
        MeteorManager.Instance.DestroyCoin(gameObject); 
	}
}
