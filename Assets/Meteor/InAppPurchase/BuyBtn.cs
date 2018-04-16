using UnityEngine;
using System.Collections;

public class BuyBtn : MonoBehaviour {

	public string IapID;
	public string ObjName;
	public string FunName;
	public string ValName;
    public GameObject WaitingOb;
    public GameObject mTipWindow;
	
	void OnClick(){
        Instantiate(WaitingOb);
        mTipWindow.transform.position = new Vector3(3000, 0, 0);
		//AppStore.BuySomething(IapID, ObjName, FunName, ValName);			
	}
}	

