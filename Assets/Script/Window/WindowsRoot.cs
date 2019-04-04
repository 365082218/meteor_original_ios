using UnityEngine;
using System.Collections;

public class WindowsRoot : MonoBehaviour
{
    public static float DesignWidth = 2160.0f;
	public static float DesignHeight = 1080.0f;
	
	static float mManualHeight = 1080.0f;
	public static float ManualHeight { get{ return mManualHeight;} set{ mManualHeight = value;}}

	//
    static float mScaleWidth;
    static float mScaleHeight;
    static float mActualRatio;
    public static float ScaleWidth { get { return mScaleWidth; } set { mScaleWidth = value; } }
    public static float ScaleHeith { get { return mScaleHeight; } set { mScaleHeight = value; } }
    public static float ActualRatio { get { return mActualRatio; } set { mActualRatio = value; } }

	static float mPositionWidth;
	public static float PositionWidth { get { return mPositionWidth; } set { mPositionWidth = value; } }
    static float mPositionHeight;
    public static float PositionHeight { get { return mPositionHeight; } set { mPositionHeight = value; } }
	
	// Use this for initialization
	void Start ()
    {
        float actualRatio = (float)Screen.width / (float)Screen.height;
        float designRatio = DesignWidth / DesignHeight;

		UIRoot RT = GetComponent<UIRoot>();
		mManualHeight = RT.manualHeight;
        if (actualRatio < designRatio){
			RT.manualHeight = (int)Mathf.Round(RT.manualHeight * designRatio / actualRatio );
			mManualHeight = RT.manualHeight;
        }
		GameObject.DontDestroyOnLoad(this);

        mScaleWidth = Mathf.Max((float)Screen.width, DesignWidth);
        mScaleHeight = Mathf.Max((float)Screen.height, DesignHeight);
        //������Ļ��ʵ�ʷֱ��ʵĸ߶�ֵ��Screen.Hight��WindowsRoot.ManualHightֵ֮�Ƚ�������
        mActualRatio = (float)Screen.height / mManualHeight;
		mPositionWidth = (float)Screen.width / mActualRatio;
        mPositionHeight = (float)Screen.height / mActualRatio;
        if (DesignWidth * mActualRatio < Screen.width)
        {
            //��Ȳ���
            mScaleWidth = DesignWidth * ((float)Screen.width / (DesignWidth * mActualRatio));
		}
        if (DesignHeight * mActualRatio <= Screen.height)
        {
            //�߶Ȳ���
            mScaleHeight = DesignHeight * ((float)Screen.height / (DesignHeight * mActualRatio));
        }
	}
}
