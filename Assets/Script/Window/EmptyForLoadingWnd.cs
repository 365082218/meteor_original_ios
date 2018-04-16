using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EmptyForLoadingWnd : Window<EmptyForLoadingWnd>
{
    public override string PrefabName { get { return "EmptyForLoadingWnd"; } }
	GameObject mGoProgress;
	Text mLabel;
	private static int mCount = 0;
	protected override bool OnOpen()
    {
		//WinStyle = WindowStyle.WS_Ext;
		Init();
        return base.OnOpen();
    }

    protected override bool OnClose()
    {
        return base.OnClose();
    }
	
	public void Progress(string Progress)
	{
		if (string.IsNullOrEmpty(Progress))
		{
			if (mGoProgress.activeSelf )
				mGoProgress.SetActive(false);
		}
		
		if (!mGoProgress.activeSelf)
			mGoProgress.SetActive(true);
		
		mLabel.text = Progress ;
	}
	
	private void Init()
	{
		mGoProgress = Control("Label");
		mLabel = mGoProgress.GetComponent<Text>();
		mGoProgress.SetActive(false);
		mCount++;
	}
}

