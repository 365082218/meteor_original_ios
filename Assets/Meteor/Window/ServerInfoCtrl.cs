using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CoClass;
using UnityEngine.UI;

public class ServerInfoCtrl : MonoBehaviour {

    public GameObject serverRoot;
    public Text Account;
    public Dictionary<GameObject, int> ServerItems = new Dictionary<GameObject, int>();
	// Use this for initialization
	void Start () {
	    //if (GameData.server != null)
     //   {
     //       for (int i = 0; i < GameData.server.Count; i++)
     //       {
     //           ServerInfo info = GameData.server[i];
     //           GameObject line = GameObject.Instantiate(Resources.Load("ServerLine") as GameObject);
     //           line.transform.SetParent(serverRoot.transform);
     //           line.transform.localPosition = Vector3.zero;
     //           line.transform.localScale = Vector3.one;
     //           line.transform.localRotation = Quaternion.identity;
     //           ServerItems[line] = info.Idx;
     //           line.GetComponentInChildren<Text>().text = info.ServerName;
     //           if (info.States == 1)
     //           {
     //               line.GetComponent<Image>().color = new Color(42.0f / 255.0f, 93.0f / 255.0f, 99.0f / 255.0f);
     //               line.GetComponent<Button>().onClick.AddListener(delegate() { OnEnterLine(line); });
     //           }
     //           else
     //           {
     //               line.GetComponent<Image>().color = Color.gray;
     //           }
     //       }
     //   }

     //   if (GameData.user != null)
     //       Account.text = GameData.user.account;

    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnEnterLine(GameObject obj)
    {
        //if (ServerItems.ContainsKey(obj))
        //{
        //    U3D.ShowLoading();
        //    ClientProxy.EnterGameServer(ServerItems[obj], OnEnterResult);
        //}
    }

    void OnEnterResult(RBase rsp)
    {
        //U3D.CloseLoading();
        //RoleInfo ret = rsp as RoleInfo;
        //if (ret != null)
        //{
        //    GameData.Role.Clear();//每次进入一条线，其他线的信息就清理掉.
        //    GameData.line = ret.line;
        //    GameData.Role[ret.line] = ret;
        //    Startup.ins.ShowRoleInfo();
        //}
    }
}
