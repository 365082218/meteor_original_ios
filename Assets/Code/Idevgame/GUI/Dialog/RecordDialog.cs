using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecordDialogState : CommonDialogState<RecordDialog>
{
    public override string DialogName { get { return "RecordDialog"; } }
    public RecordDialogState(MainDialogStateManager stateMgr) : base(stateMgr)
    {

    }
}

//显示选择本地录像文件路径（存储到/选择）
public class RecordDialog : Dialog
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
            Main.Ins.StopCoroutine(refreshRec);
            refreshRec = null;
        }
    }

    static int recCurrent;
    List<string> recList = new List<string>();//文件列表
    List<GameRecord> recData = new List<GameRecord>();//录像列表
    List<GameObject> recItems = new List<GameObject>();//UI路线控件列表
    GameObject root;
    Coroutine refreshRec;
    GameObject Prefab;
    string recDir;
    void Init()
    {
        recDir = string.Format("{0}/record/", Application.persistentDataPath);
        Prefab = Resources.Load("UI/Dialogs/RecordItem") as GameObject;
        if (System.IO.Directory.Exists(recDir))
            RefreshAuto();
        else
            System.IO.Directory.CreateDirectory(recDir);

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
            {
                string file = recList[recCurrent];
                if (System.IO.File.Exists(file))
                    System.IO.File.Delete(file);
                GameObject removed = recItems[recCurrent];
                recList.RemoveAt(recCurrent);
                recItems.RemoveAt(recCurrent);
                GameObject.Destroy(removed);
                recCurrent -= 1;
            }
        });

        Control("Play").GetComponent<Button>().onClick.AddListener(() =>
        {
            if (recCurrent > -1 && recData.Count > recCurrent)
            {
                //播放一个路线，弹出2级菜单.
                GameRecord rec = recData[recCurrent];
                U3D.PlayRecord(rec);
            }
        });

        Control("Upload").GetComponent<Button>().onClick.AddListener(() => {
            if (recCurrent > -1 && recData.Count > recCurrent) {
                U3D.PopupTip("上传录像");
                //播放一个路线，弹出2级菜单.
                GameRecord rec = recData[recCurrent];
                TcpClientProxy.UploadRecord(rec);
            }
        });
    }

    void RefreshAuto()
    {
        if (refreshRec != null)
        {
            Main.Ins.StopCoroutine(refreshRec);
            refreshRec = null;
        }

        refreshRec = Main.Ins.StartCoroutine(Refresh());
    }

    IEnumerator Refresh()
    {
        string[] files = System.IO.Directory.GetFiles(recDir, "*.mrc", System.IO.SearchOption.TopDirectoryOnly);
        System.IO.FileStream fs = null;
        for (int i = 0; i < files.Length; i++)
        {
            string file = files[i];
            try
            {
                fs = System.IO.File.Open(file, System.IO.FileMode.Open);
                GameRecord rec = ProtoBuf.Serializer.Deserialize<GameRecord>(fs);
                recData.Add(rec);
                recList.Add(file);
                fs.Close();
            }
            catch
            {
                if (fs != null)
                {
                    fs.Close();
                    fs = null;
                }
            }
            yield return 0;
        }

        for (int i = 0; i < recData.Count; i++)
        {
            AddRecOnGrid(recData[i]);
            yield return 0;
        }
    }

    void AddRecOnGrid(GameRecord rec)
    {
        GameObject recordItem = GameObject.Instantiate(Prefab);
        recordItem.transform.SetParent(root.transform);
        Utility.Zero(recordItem);
        recItems.Add(recordItem);
        GameObject recordButton = NodeHelper.Find("RecordName", recordItem);
        recordButton.GetComponent<Text>().text = rec.Name;
        //NodeHelper.Find("CaptureScreen", recordItem).GetComponent<Image>().sprite = rec.Screen;
        recordButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            recCurrent = recData.IndexOf(rec);
            recordButton.GetComponent<Text>().color = Color.blue;
            //recordButton.GetComponent<Image>().color = new Color(1, 1, 1, 26.0f / 255.0f);
            for (int i = 0; i < recData.Count; i++)
            {
                if (recData[i] == rec)
                    continue;
                //recItems[i].GetComponent<Image>().color = new Color(1, 1, 1, 0);
                NodeHelper.Find("RecordName", recItems[i]).GetComponent<Text>().color = Color.blue;
            }
        });
    }
}