using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecordSelectDialogState : CommonDialogState<RecordSelectDialog>
{
    public override string DialogName { get { return "RecordSelectDialog"; } }
    public RecordSelectDialogState(MainDialogStateManager stateMgr) : base(stateMgr)
    {

    }
}

//显示选择本地录像文件路径（存储到/选择）
public class RecordSelectDialog : Dialog
{
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }

    public override void OnClose()
    {
        if (refreshRec != null)
        {
            Main.Instance.StopCoroutine(refreshRec);
            refreshRec = null;
        }
    }

    static int recCurrent;
    List<string> recList;
    GameObject root;
    Coroutine refreshRec;
    GameObject Prefab;
    void Init()
    {
        if (recList == null)
            recList = new List<string>();
        string recDir = string.Format("{0}/record/", Application.persistentDataPath);
        if (System.IO.Directory.Exists(recDir))
        {
            string[] files = System.IO.Directory.GetFiles(recDir);
            for (int i = 0; i < files.Length; i++)
                recList.Add(files[i]);
        }
        else
        {
            System.IO.Directory.CreateDirectory(recDir);
        }

        //一次加载全部录像存到
        root = Control("content");
        recCurrent = 0;
        //打开本地已存储的录像列表，UI上带新建，选择，删除，关闭/退出等
        Control("Colse").GetComponent<Button>().onClick.AddListener(() =>
        {
            OnPreviousPress();
        });

        Control("Deleted").GetComponent<Button>().onClick.AddListener(() =>
        {
            //如果当前选择了某个录像，则删除
            if (recCurrent != -1 && recList.Count > recCurrent)
                recList.RemoveAt(recCurrent);
        });

        Control("Create").GetComponent<Button>().onClick.AddListener(() =>
        {
            //创建一个文件
            for (int i = 0; i < recList.Count; i++)
            {
                string s = string.Format("{0}/rec/{1}", Application.persistentDataPath, i + ".mrc");
                if (recList[i] != s)
                {
                    if (!System.IO.File.Exists(s))
                    {
                        System.IO.File.Create(s);
                        recList.Insert(i, s);
                        RefreshAuto();
                        return;
                    }
                }
            }

            string file = string.Format("{0}/rec/{1}", Application.persistentDataPath, recList.Count + ".mrc");
            if (!System.IO.File.Exists(file))
            {
                System.IO.File.Create(file);
                recList.Add(file);
                RefreshAuto();
                return;
            }

            U3D.PopupTip("无法创建更多录像文件");
        });

        Prefab = Resources.Load("ButtonNormal") as GameObject;
        if (recList.Count != 0)
        {
            refreshRec = Main.Instance.StartCoroutine(Refresh());
        }
    }

    void RefreshAuto()
    {
        if (refreshRec != null)
        {
            Main.Instance.StopCoroutine(refreshRec);
            refreshRec = null;
        }

        refreshRec = Main.Instance.StartCoroutine(Refresh());
    }

    IEnumerator Refresh()
    {
        for (int i = 0; i < recList.Count; i++)
        {
            AddRecOnGrid(recList[i]);
            yield return 0;
        }
    }

    void AddRecOnGrid(string rec)
    {
        GameObject btn = GameObject.Instantiate(Prefab);
        btn.transform.SetParent(root.transform);
        btn.transform.localPosition = Vector3.zero;
        btn.transform.localRotation = Quaternion.identity;
        btn.transform.localScale = Vector3.one;
        btn.GetComponentInChildren<Text>().text = rec;
        btn.GetComponent<Button>().onClick.AddListener(() =>
        {
            string r = rec;
            recCurrent = recList.IndexOf(r);
            btn.GetComponentInChildren<Text>().color = Color.blue;
        });
    }
}