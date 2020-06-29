using UnityEngine;
using System.Collections;
using AnimationOrTween;
using UnityEngine.UI;

public class UIHelper
{
    public static float CanvasWidth = 0;
    public static float CanvasHeight = 0;
    public static float Aspect = 0;//������ʵ����Ļ�ı���.

    //ֻҪ�ڿ�ʼʱ�õ�������Ͳ����ٸ���.
    public static void InitCanvas(Canvas target)
    {
        Canvas CurrntCanvas;
        CanvasScaler CurrentScaler;
        CurrntCanvas = target;
        CurrentScaler = target.GetComponent<CanvasScaler>();
        RectTransform r = target.GetComponent<RectTransform>();
        CanvasWidth = r.sizeDelta.x;
        CanvasHeight = r.sizeDelta.y;
        if (CurrentScaler.screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight)
            Aspect = (CanvasWidth / Screen.width) * (1 - CurrentScaler.matchWidthOrHeight) + CurrentScaler.matchWidthOrHeight * (CanvasHeight / Screen.height);
    }

    //��Ļ����ת����UI����-UI����ϵ�����½�Ϊԭ��-���������󣬺���Ļ����ӳ��-ֻ�뻭������������Ʒֱ����Լ�Match���.
    //�������½�Ϊԭ�����
	public static Vector2 ScreenPointToCanvasPoint(Vector2 screenPoint)
	{
        Vector2 r;
        r.x = CanvasWidth * screenPoint.x / Screen.width;
        r.y = CanvasHeight * screenPoint.y / Screen.height;
		return r;
	}

    //��Ļ����ת����UI����-UI����ϵ
    //�����м�Ϊԭ�����
    public static Vector2 ScreenPointToUIPoint(Vector2 screenPoint)
    {
        Vector2 screenPointOffset = screenPoint - new Vector2((float)Screen.width / 2, (float)Screen.height / 2);
        return ScreenPointToCanvasPoint(screenPointOffset);
    }


    public static Vector2 Size(UILabel label)
    {
        return label.relativeSize * label.cachedTransform.localScale.x;
    }
	
	public static Vector3 GetPixelPerfect(UISprite sprite)
	{
		Texture tex = sprite.mainTexture;
		Rect rect = NGUIMath.ConvertToPixels(sprite.outerUV, tex.width, tex.height, true);
		Vector3 scale = sprite.cachedTransform.localScale;
		float pixelSize = sprite.atlas.pixelSize;
		scale.x = rect.width * pixelSize;
		scale.y = rect.height * pixelSize;
		scale.z = 1f;
		
		return scale;
	}
	
    //��̬Ч��.
	public static void TweenPlay(GameObject go,Direction playDirection)
	{
		TweenPlay(go,0,playDirection,false,EnableCondition.DoNothing,DisableCondition.DoNotDisable,false);
	}

	public static void TweenPlay(GameObject go,Direction playDirection,int tweenGroup)
	{
		TweenPlay(go,tweenGroup,playDirection,false,EnableCondition.DoNothing,DisableCondition.DoNotDisable,false);
	}

	public static void TweenPlay(GameObject go,int tweenGroup,Direction playDirection,bool resetOnPlay,EnableCondition ifDisabledOnPlay,DisableCondition disableWhenFinished,bool includeChildren)
	{
		if (!go.activeSelf)
		{
			// If the object is disabled, don't do anything
			if (ifDisabledOnPlay != EnableCondition.EnableThenPlay) return;

			// Enable the game object before tweening it
			NGUITools.SetActive(go, true);
		}

		// Gather the tweening components
		UITweener[] mTweens = includeChildren ? go.GetComponentsInChildren<UITweener>() : go.GetComponents<UITweener>();

		if (mTweens.Length == 0)
		{
			// No tweeners found -- should we disable the object?
			if (disableWhenFinished != DisableCondition.DoNotDisable) NGUITools.SetActive(go, false);
		}
		else
		{
			bool activated = false;
			
			// Run through all located tween components
			for (int i = 0, imax = mTweens.Length; i < imax; ++i)
			{
				UITweener tw = mTweens[i];

				// If the tweener's group matches, we can work with it
				if (tw.tweenGroup == tweenGroup)
				{
					// Ensure that the game objects are enabled
					if (!activated && !go.activeSelf)
					{
						activated = true;
						NGUITools.SetActive(go, true);
					}

					// Toggle or activate the tween component
					if (playDirection == Direction.Toggle) tw.Toggle();
					else if(playDirection == Direction.Forward) tw.Play(true);
					else tw.Play(false);
					
					if (resetOnPlay) tw.Reset();
				}
			}
		}
	}
}
