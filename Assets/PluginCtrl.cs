using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
public class PluginCtrl : MonoBehaviour {
    public Image Preview;
    [SerializeField]
    Text Desc;
    [SerializeField]
    Text Title;
    [SerializeField]
    Image Install;
    [SerializeField]
    Image Progress;
    bool taskIsRunning;
    public void OnClick()
    {
        if (Model != null)
        {
            if (Model.Installed) {
                Main.Ins.EnterState(Main.Ins.TipDialogState, Model);
                Install.GetComponentInChildren<Text>().text = StringUtils.Install;
                Progress.fillAmount = 0.0f;
                return;
            }
            if (!taskIsRunning) {
                //未安装-开始下载
                DownloadTask task = Main.Ins.DownloadManager.AddTask(TaskType.Model, Model.ModelId);
                task.OnProgressChanged = DownLoadProgressChanged;
                Install.GetComponentInChildren<Text>().text = StringUtils.Cancel;
                taskIsRunning = true;
            } else {
                //安装中-取消下载
                Main.Ins.DownloadManager.CancelTask(TaskType.Model, Model.ModelId);
                taskIsRunning = false;
                Install.GetComponentInChildren<Text>().text = StringUtils.Install;
            }
        }

        if (Chapter != null)
        {
            if (Chapter.Installed) {
                Main.Ins.EnterState(Main.Ins.TipDialogState, Chapter);
                Install.GetComponentInChildren<Text>().text = StringUtils.Install;
                Progress.fillAmount = 0.0f;
                return;
            }

            if (!Main.Ins.DlcMng.CompatibleChapter(Chapter)) {
                //弹出一个提示界面，指向最新客户端版本.
                U3D.PopupTip(string.Format("插件需要更新新客户端"));
                return;
            }

            if (!taskIsRunning) {
                DownloadTask task = Main.Ins.DownloadManager.AddTask(TaskType.Chapter, Chapter.ChapterId);
                task.OnProgressChanged = DownLoadProgressChanged;
                Install.GetComponentInChildren<Text>().text = StringUtils.Cancel;
                taskIsRunning = true;
            } else {
                Main.Ins.DownloadManager.CancelTask(TaskType.Chapter, Chapter.ChapterId);
                taskIsRunning = false;
                Install.GetComponentInChildren<Text>().text = StringUtils.Install;
            }
        }
    }

    //刷新任务的状态
    public void OnStateChange() {
        if (Chapter != null) {
            DownloadTask task = Main.Ins.DownloadManager.GetTask(TaskType.Chapter, Chapter.ChapterId);
            if (task != null) {
                taskIsRunning = task.state != TaskStatus.Installed;
            } else {
                taskIsRunning = false;
            }
        }
        Install.GetComponentInChildren<Text>().text = taskIsRunning ? StringUtils.Cancel:StringUtils.Uninstall;
    }

    private void OnDestroy()
    {
        if (Chapter != null) {
            Main.Ins.DownloadManager.RemoveTask(TaskType.ChapterPreview, Chapter.ChapterId);
            //任务后台运行
            DownloadTask task = Main.Ins.DownloadManager.GetTask(TaskType.Chapter, Chapter.ChapterId);
            if (task != null) {
                task.OnProgressChanged = null;
            }
        }
        if (Model != null) {
            Main.Ins.DownloadManager.RemoveTask(TaskType.ModelPreview, Model.ModelId);
            DownloadTask task = Main.Ins.DownloadManager.GetTask(TaskType.Model, Model.ModelId);
            if (task != null) {
                task.OnProgressChanged = null;
            }
        }
    }

    public ModelItem Model;
    public Chapter Chapter;
    
    public void AttachModel(ModelItem it)
    {
        Model = it;
        Title.text = string.Format(StringUtils.ModelName, it.Name);
        Desc.text = it.Desc ?? "";
        //已经安装
        if (Main.Ins.GameStateMgr.gameStatus.IsModelInstalled(Model))
        {
            Model.Installed = true;
            Install.GetComponentInChildren<Text>().text = StringUtils.Uninstall;
            LoadPreview(it.Preview);
            Progress.fillAmount = 1.0f;
        }
        else
        {
            Progress.fillAmount = 0.0f;
            bool prev = LoadPreview(it.Preview);
            if (!prev) {
                DownloadTask task = Main.Ins.DownloadManager.AddTask(TaskType.ModelPreview, Model.ModelId);
                task.OnComplete = OnPreviewLoaded;
            }
        }

        //检查文件任务是否在下载中.把事件对接上来
        DownloadTask inqueue = Main.Ins.DownloadManager.GetTask(TaskType.Model, Model.ModelId);
        if (inqueue != null) {
            inqueue.OnProgressChanged = DownLoadProgressChanged;
            taskIsRunning = true;
            Install.GetComponentInChildren<Text>().text = StringUtils.Cancel;
        } else {
            taskIsRunning = false;
        }
    }

    //如果是
    public void ShowDetail() {
        bool process = false;
        if (Chapter != null) {
            if (Chapter.Installed) {
                process = true;
                Main.Ins.CombatData.Chapter = Chapter;
                Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.LevelDialogState, false);
            }
        } else if (Model != null) {
            if (Model.Installed) {
                process = true;
                Main.Ins.GameStateMgr.gameStatus.UseModel = Model.ModelId;
                U3D.PopupTip(string.Format(StringUtils.SetPlayerModel, Model.Name));
            }
        }
        if (!process)
            U3D.PopupTip("安装后可查看");
    }

    public void AttachDlc(Chapter chapter)
    {
        Chapter = chapter;
        Title.text = string.Format(StringUtils.DlcName, Chapter.Name);
        Desc.text = Chapter.Desc ?? "";
        //已经安装
        Chapter exist;
        if (Main.Ins.GameStateMgr.gameStatus.IsDlcInstalled(Chapter, out exist))
        {
            Progress.fillAmount = 1.0f;
            Chapter = exist;
            Install.GetComponentInChildren<Text>().text = StringUtils.Uninstall;
            LoadPreview(Chapter.Preview);
        }
        else
        {
            Progress.fillAmount = 0.0f;
            bool prev = LoadPreview(Chapter.Preview);
            if (!prev) {
                DownloadTask task = Main.Ins.DownloadManager.AddTask(TaskType.ChapterPreview, Chapter.ChapterId);
                task.OnComplete = OnPreviewLoaded;
            }
        }

        //检查文件任务是否在下载中.把事件对接上来
        DownloadTask inqueue = Main.Ins.DownloadManager.GetTask(TaskType.Chapter, Chapter.ChapterId);
        if (inqueue != null) {
            inqueue.OnProgressChanged = DownLoadProgressChanged;
            taskIsRunning = true;
            Install.GetComponentInChildren<Text>().text = StringUtils.Cancel;
        } else {
            taskIsRunning = false;
        }
    }

    public void OnFailed() {
        U3D.PopupTip(StringUtils.InstallFailed);
    }

    //预览图下载完毕时
    public void OnPreviewLoaded(object sender, AsyncCompletedEventArgs e) {
        if (Chapter != null)
            LoadPreview(Chapter.Preview);
        else if (Model != null)
            LoadPreview(Model.Preview);
    }

    bool LoadPreview(string localPath) {
        if (System.IO.File.Exists(localPath)) {
            byte[] array = System.IO.File.ReadAllBytes(localPath);
            Texture2D tex = new Texture2D(0, 0);
            tex.LoadImage(array);
            Preview.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
            return true;
        }
        return false;
    }

    public void DownLoadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
        Progress.fillAmount = e.ProgressPercentage / 100.0f;
    }
}