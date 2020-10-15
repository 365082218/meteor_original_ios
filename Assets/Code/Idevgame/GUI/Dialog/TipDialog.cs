using Idevgame.GameState.DialogState;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
public class TipDialogState : ModalDialogState<TipDialog> {
    public override string ModalDialogName { get { return "TipDialog"; } }
}

//只负责处理安装过程，卸载过程的动画表现
//安装过程只有旋转图
//卸载过程有进度条
public class TipDialog : ModalDialog {
    Image UnInstallProgress;
    Image Preview;
    Text Tips;
    Text Title;
    Chapter chapter;
    ModelItem model;
    public override void OnDialogStateEnter(PersistState ownerState, BaseDialogState previousDialog, object data) {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        chapter = data as Chapter;
        model = data as ModelItem;
        Init();
    }

    void Init() {
        UnInstallProgress = Control("Progress").GetComponent<Image>();
        Preview = Control("Preview").GetComponent<Image>();
        Tips = Control("Tips").GetComponent<Text>();
        Title = Control("Title").GetComponent<Text>();
        SetProgress(0);
        if (chapter != null) {
            if (chapter.Installed) {
                OnUninstall();
                return;
            }
            TipDialogState.State.Open();
            return;
        }

        if (model != null) {
            if (model.Installed) {
                OnUninstall();
                return;
            }
            TipDialogState.State.Close();
            return;
        }
    }

    void OnUninstall() {
        if (model != null) {
            Title.text = string.Format(StringUtils.ModelName, model.ModelId, model.Name);
            Tips.text = string.Format(StringUtils.UninstallModel, model.Name);
            Utility.LoadPreview(Preview, model.Preview);
            ClearModel();
            return;
        }
        if (chapter != null) {
            Title.text = string.Format(StringUtils.DlcName, chapter.ChapterId, chapter.Name);
            Tips.text = string.Format(StringUtils.UninstallChapter, chapter.Name);
            Utility.LoadPreview(Preview, chapter.Preview);
            ClearChapter();
            return;
            
        }
    }

    int totals;
    int process;


    void ClearChapter() {
        totals = chapter.resPath != null ? chapter.resPath.Count : 1;
        process = 0;
        StartCoroutine(CleanChapterAsync());
    }

    void ClearModel() {
        totals = model.resPath != null ? model.resPath.Count:1;
        process = 0;
        StartCoroutine(CleanModelAsync());
    }

    public IEnumerator CleanChapterAsync() {

        if (chapter.resPath != null) {
            for (int j = 0; j < chapter.resPath.Count; j++) {
                try {
                    if (System.IO.File.Exists(chapter.resPath[j])) {
                        System.IO.File.Delete(chapter.resPath[j]);
                    }
                } catch (Exception exp) {
                    Log.WriteError(exp.StackTrace);
                }
                process += 1;
                SetProgress(process / (float)totals);
                yield return 0;
            }
            chapter.resPath = null;
        }
        try {
            if (System.IO.File.Exists(chapter.LocalPath))
                System.IO.File.Delete(chapter.LocalPath);
        }
        catch (Exception exp) {
            Log.WriteError(exp.StackTrace);
        }
        yield return 0;
        chapter.Installed = false;
        GameStateMgr.Ins.gameStatus.UnRegisterDlc(chapter);
        Save();
        Exit();
    }

    public IEnumerator CleanModelAsync() {
        if (model.resPath != null) {
            for (int j = 0; j < model.resPath.Count; j++) {
                if (System.IO.File.Exists(model.resPath[j])) {
                    System.IO.File.Delete(model.resPath[j]);
                }
                process += 1;
                SetProgress(process / (float)totals);
                yield return 0;
            }
        }
        if (System.IO.File.Exists(model.LocalPath))
            System.IO.File.Delete(model.LocalPath);
        yield return 0;
        model.Installed = false;
        GameStateMgr.Ins.gameStatus.UnRegisterModel(model);
        Save();
        Exit();
    }

    void SetProgress(float p) {
        UnInstallProgress.fillAmount = p;
    }

    void Save() {
        GameStateMgr.Ins.SaveState();
    }

    void Exit() {
        TipDialogState.State.WaitExit(1.0f);
    }
}