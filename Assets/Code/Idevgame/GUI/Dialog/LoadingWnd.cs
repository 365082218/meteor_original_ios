using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingWnd :Dialog
{
	//Image mProgressBar;
 //   public override string PrefabName { get { return "LoadingWnd"; } }
	//Text mProgessLab;
 //   Text LoadingNoticeLabel;
 //   protected override bool OnOpen()
 //   {
	//	if (GameObject.Find("CameraControl")!= null){
	//		GameObject.Find("CameraControl").gameObject.SetActive(false);
	//	}
 //       mProgressBar = Control("Progress").GetComponent<Image>();
 //       mProgessLab = Control("progressText").GetComponent<Text>();
 //       LoadingNoticeLabel = Control("Goal").GetComponent<Text>();
		

 //       Material loadingTexture = null;
 //       if (Global.Instance.GLevelItem != null && !string.IsNullOrEmpty(Global.Instance.GLevelItem.BgTexture))
 //           loadingTexture = Resources.Load<Material>(Global.Instance.GLevelItem.BgTexture) as Material;
 //       else
 //           loadingTexture = Resources.Load<Material>("Scene10") as Material;
 //       if (loadingTexture != null)
 //           Control("ShowLoading").GetComponent<Image>().material = loadingTexture;
 //       SetLoadingNoticeLabel();
	//	mProgressBar.fillAmount = 0f;
 //       return base.OnOpen();
 //   }

 //   protected override bool OnClose()
 //   {
	//	if(GameObject.Find("CameraControl")!= null){
	//		GameObject.Find("CameraControl").gameObject.SetActive(true);
	//	}
 //       return base.OnClose();
 //   }
	
	//public void UpdateProgress(float progress)
	//{
 //       mProgressBar.fillAmount = progress;
	//	mProgessLab.text = string.Format("{0:P0}", progress);
 //   }


 //   public void SetLoadingNoticeLabel()
 //   {
 //       LoadingNoticeLabel.text = "";
 //       //显示关卡目标.
 //       //int nLen = LoadingTipsManager.Instance.GetAllItem().Length;
 //       //int nStart = 1;
 //       //int nID = Random.Range(nStart, nStart + nLen - 1);
 //       //LoadingNoticeLabel.text = Startup.ins.Lang == 0 ? LoadingTipsManager.Instance.GetItem(nID).TipCh : LoadingTipsManager.Instance.GetItem(nID).TipEn;
 //   }
}
