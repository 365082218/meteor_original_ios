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
                TipDialogState.State.Open(Model);
                Install.GetComponentInChildren<Text>().text = StringUtils.Install;
                Progress.fillAmount = 0.0f;
                return;
            }
            if (!taskIsRunning) {
                //未安装-开始下载
                DownloadTask task = DownloadManager.Ins.AddTask(TaskType.Model, Model.ModelId);
                task.OnProgressChanged = DownLoadProgressChanged;
                Install.GetComponentInChildren<Text>().text = StringUtils.Cancel;
                taskIsRunning = true;
            } else {
                //安装中-取消下载
                DownloadManager.Ins.CancelTask(TaskType.Model, Model.ModelId);
                taskIsRunning = false;
                Install.GetComponentInChildren<Text>().text = StringUtils.Install;
                Progress.fillAmount = 0;
            }
        }

        if (Chapter != null)
        {
            if (Chapter.Installed) {
                TipDialogState.State.Open(Chapter);
                Install.GetComponentInChildren<Text>().text = StringUtils.Install;
                Progress.fillAmount = 0.0f;
                return;
            }

            if (!DlcMng.Ins.CompatibleChapter(Chapter)) {
                //弹出一个提示界面，指向最新客户端版本.
                U3D.PopupTip(string.Format("插件需要更新新客户端"));
                return;
            }

            if (!taskIsRunning) {
                DownloadTask task = DownloadManager.Ins.AddTask(TaskType.Chapter, Chapter.ChapterId);
                task.OnProgressChanged = DownLoadProgressChanged;
                Install.GetComponentInChildren<Text>().text = StringUtils.Cancel;
                taskIsRunning = true;
            } else {
                DownloadManager.Ins.CancelTask(TaskType.Chapter, Chapter.ChapterId);
                taskIsRunning = false;
                Install.GetComponentInChildren<Text>().text = StringUtils.Install;
                Progress.fillAmount = 0;
            }
        }
    }

    //刷新任务的状态
    public void OnStateChange(int id, TaskType type) {
        if (Chapter != null) {
            DownloadTask task = DownloadManager.Ins.GetTask(TaskType.Chapter, Chapter.ChapterId);
            if (Chapter.Installed) {
                Install.GetComponentInChildren<Text>().text = StringUtils.Uninstall;
                taskIsRunning = false;
            } else {
                UpdateUI(task);
            }
        } else if (Model != null) {
            DownloadTask task = DownloadManager.Ins.GetTask(TaskType.Model, Model.ModelId);
            if (Model.Installed) {
                Install.GetComponentInChildren<Text>().text = StringUtils.Uninstall;
                taskIsRunning = false;
            } else {
                UpdateUI(task);
            }
        }
    }

    void UpdateUI(DownloadTask task) {
        if (task == null)
            return;
        if (task.state == TaskStatus.Error)
            Install.GetComponentInChildren<Text>().text = StringUtils.Error;
        else if (task.state == TaskStatus.Normal)
            Install.GetComponentInChildren<Text>().text = StringUtils.Cancel;
        else if (task.state == TaskStatus.Loaded)
            Install.GetComponentInChildren<Text>().text = StringUtils.Unzip;
    }

    private void OnDestroy()
    {
        if (Chapter != null) {
            DownloadManager.Ins.RemoveTask(TaskType.ChapterPreview, Chapter.ChapterId);
            //任务后台运行
            DownloadTask task = DownloadManager.Ins.GetTask(TaskType.Chapter, Chapter.ChapterId);
            if (task != null) {
                task.OnProgressChanged = null;
            }
        }
        if (Model != null) {
            DownloadManager.Ins.RemoveTask(TaskType.ModelPreview, Model.ModelId);
            DownloadTask task = DownloadManager.Ins.GetTask(TaskType.Model, Model.ModelId);
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
        Title.text = string.Format(StringUtils.ModelName, it.ModelId, it.Name);
        Desc.text = it.Desc ?? "";
        //已经安装
        if (GameStateMgr.Ins.gameStatus.IsModelInstalled(Model))
        {
            Model.Installed = true;
            Install.GetComponentInChildren<Text>().text = StringUtils.Uninstall;
            Utility.LoadPreview(Preview, it.Preview);
            Progress.fillAmount = 1.0f;
        }
        else
        {
            Progress.fillAmount = 0.0f;
            bool prev = Utility.LoadPreview(Preview, it.Preview);
            if (!prev) {
                DownloadTask task = DownloadManager.Ins.AddTask(TaskType.ModelPreview, Model.ModelId);
                task.OnComplete = OnPreviewLoaded;
            }
        }

        //检查文件任务是否在下载中.把事件对接上来
        DownloadTask inqueue = DownloadManager.Ins.GetTask(TaskType.Model, Model.ModelId);
        if (inqueue != null) {
            inqueue.OnProgressChanged = DownLoadProgressChanged;
            taskIsRunning = true;
            Progress.fillAmount = inqueue.progress / 100.0f;
            UpdateUI(inqueue);
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
                CombatData.Ins.Chapter = Chapter;
                Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.LevelDialogState, false);
            }
        } else if (Model != null) {
            if (Model.Installed) {
                process = true;
                GameStateMgr.Ins.gameStatus.UseModel = Model.ModelId;
                U3D.PopupTip(string.Format(StringUtils.SetPlayerModel, Model.Name));
            }
        }
        if (!process)
            U3D.PopupTip("安装后可查看");
    }

    public void AttachDlc(Chapter chapter)
    {
        Chapter = chapter;
        Title.text = string.Format(StringUtils.DlcName, Chapter.ChapterId, Chapter.Name);
        Desc.text = Chapter.Desc ?? "";
        //已经安装
        Chapter exist;
        if (GameStateMgr.Ins.gameStatus.IsDlcInstalled(Chapter, out exist))
        {
            Progress.fillAmount = 1.0f;
            Chapter = exist;
            Install.GetComponentInChildren<Text>().text = StringUtils.Uninstall;
            Utility.LoadPreview(Preview, Chapter.Preview);
        }
        else
        {
            Progress.fillAmount = 0.0f;
            bool prev = Utility.LoadPreview(Preview, Chapter.Preview);
            if (!prev) {
                DownloadTask task = DownloadManager.Ins.AddTask(TaskType.ChapterPreview, Chapter.ChapterId);
                task.OnComplete = OnPreviewLoaded;
            }
        }

        //检查文件任务是否在下载中.把事件对接上来
        DownloadTask inqueue = DownloadManager.Ins.GetTask(TaskType.Chapter, Chapter.ChapterId);
        if (inqueue != null) {
            inqueue.OnProgressChanged = DownLoadProgressChanged;
            Progress.fillAmount = inqueue.progress / 100.0f;
            taskIsRunning = true;
            UpdateUI(inqueue);
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
            Utility.LoadPreview(Preview, Chapter.Preview);
        else if (Model != null)
            Utility.LoadPreview(Preview, Model.Preview);
    }

    public void DownLoadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
        Progress.fillAmount = e.ProgressPercentage / 100.0f;
    }
}