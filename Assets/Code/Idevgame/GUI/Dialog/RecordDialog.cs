using Idevgame.GameState.DialogState;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecordDialogState : CommonDialogState<RecordDialog>
{
    public override string DialogName { get { return "RecordDialog"; } }
    public RecordDialogState(MainDialogMgr stateMgr) : base(stateMgr)
    {

    }
}

//显示选择本地录像文件路径（存储到/选择）
//选择面板，预设布局不一样但是树是一样的，逻辑也一样
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

    protected static int recCurrent;
    protected List<string> recList = new List<string>();//文件列表
    protected List<GameRecord> recData = new List<GameRecord>();//录像列表
    protected List<GameObject> recItems = new List<GameObject>();//UI路线控件列表
    GameObject root;
    Coroutine refreshRec;
    protected GameObject Prefab;
    string recDir;
    protected virtual void Init()
    {
        //recDir = Main.Ins.replayPath;
        if (Prefab == null)
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

        Control("Next").GetComponent<Button>().onClick.AddListener(OnNextPage);
        Control("Prev").GetComponent<Button>().onClick.AddListener(OnPrevPage);

        GameObject deleted = Control("Deleted");
        if (deleted != null) {
            deleted.GetComponent<Button>().onClick.AddListener(() =>
            {
                //如果当前选择了某个录像，则删除
                if (recCurrent != -1 && recList.Count > recCurrent) {
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
        }

        Control("Play").GetComponent<Button>().onClick.AddListener(() =>
        {
            if (recCurrent > -1 && recData.Count > recCurrent)
            {
                //播放一个路线，弹出2级菜单.
                GameRecord rec = recData[recCurrent];
                U3D.PlayRecord(rec);
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
                if (string.IsNullOrEmpty(rec.Name))
                    continue;
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
        recData.Sort((a, b) => 
        {
            return b.time.CompareTo(a.time);
        });
        pageMax = recData.Count / recordPerPage + ((recData.Count % recordPerPage == 0) ? 0 : 1);
        PluginPageRefreshEx();
    }

    int recordPage;
    const int recordPerPage = 10;
    int pageMax;
    IEnumerator RecPageRefresh() {
        for (int i = 0; i < recItems.Count; i++) {
            GameObject.Destroy(recItems[i].gameObject);
        }
        recItems.Clear();
        for (int i = recordPage * recordPerPage; i < Mathf.Min((recordPage + 1) * recordPerPage, recData.Count); i++) {
            AddRecOnGrid(recData[i]);
            yield return 0;
        }
        refreshRec = null;
    }


    void OnPrevPage() {
        if (recordPage != 0)
            recordPage--;
        else
            return;
        PluginPageRefreshEx();
    }

    void OnNextPage() {
        if (recordPage < pageMax - 1)
            recordPage++;
        else
            return;
        PluginPageRefreshEx();
    }

    Coroutine RecPageUpdate;
    void PluginPageRefreshEx() {
        Control("Total").GetComponent<Text>().text = string.Format("录像:{0}", recData.Count);
        Control("Pages").GetComponent<Text>().text = recData.Count != 0 ? (recordPage + 1) + "/" + pageMax : "-/-";
        if (RecPageUpdate != null)
            Main.Ins.StopCoroutine(RecPageUpdate);
        RecPageUpdate = Main.Ins.StartCoroutine(RecPageRefresh());
    }

    void AddRecOnGrid(GameRecord rec)
    {
        byte[] screenBytes = null;
        try {
            screenBytes = System.IO.File.ReadAllBytes(rec.screenPng);
        }
        catch (Exception exp) {
            Debug.LogError(exp.Message);
            return;
        }

        GameObject recordItem = GameObject.Instantiate(Prefab);
        recordItem.transform.SetParent(root.transform);
        Utility.Zero(recordItem);
        recItems.Add(recordItem);
        GameObject recordButton = NodeHelper.Find("RecordName", recordItem);
        recordButton.GetComponent<Text>().text = rec.Name;
        Texture2D tex = new Texture2D(rec.screenWidth, rec.screenHeight, TextureFormat.ARGB32, false);

        tex.LoadImage(screenBytes);
        NodeHelper.Find("CaptureScreen", recordItem).GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        NodeHelper.Find("CaptureScreen", recordItem).GetComponent<Image>().preserveAspect = true;
        NodeHelper.Find("Duration", recordItem).GetComponent<Text>().text = string.Format("时长:" + Utility.GetDuration(rec.duration));
        Control("RecordDate", recordItem).GetComponent<Text>().text = string.Format("保存日期:" + DateTime.FromFileTime(rec.time).ToString("yyyy-MM-dd HH:mm:ss"));
        recordItem.GetComponent<Button>().onClick.AddListener(() =>
        {
            recCurrent = recData.IndexOf(rec);
            recordButton.GetComponent<Text>().color = Color.blue;
            //recordButton.GetComponent<Image>().color = new Color(1, 1, 1, 26.0f / 255.0f);
            for (int i = 0; i < recData.Count; i++)
            {
                if (recData[i] == rec)
                    continue;
                //recItems[i].GetComponent<Image>().color = new Color(1, 1, 1, 0);
                NodeHelper.Find("RecordName", recItems[i]).GetComponent<Text>().color = Color.white;
            }
        });
    }
}