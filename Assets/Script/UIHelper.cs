using UnityEngine;
using System.Collections;
using AnimationOrTween;

public class UIHelper
{
    public static float DesignWidth = 2160.0f;
    public static float DesignHeight = 1080.0f;
    public static float WorldToScreenModify { get { return DesignHeight / Screen.height; }  }
	public static float UIModify { get { return 1 + (float)Screen.width / Screen.height - 2160.0f / 1080.0f; } }//UI LocalPos the modify
	
    //UI原点在屏幕中间，把屏幕坐标系的点换算为UI坐标系的点.
	public static Vector2 ScreenPointToUIPoint(Vector2 screenPoint)
	{
		return (screenPoint - new Vector2( (float)Screen.width / 2, (float)Screen.height / 2)) * WorldToScreenModify;
	}
	
	public static Vector2 ScreenPointToUIPointNoModify(Vector2 screenPoint)
	{
		return screenPoint  - new Vector2( (float)Screen.width / 2, (float)Screen.height / 2);
	}
	
	public static Vector3 ScreenPointToUIPoint(Vector3 screenPoint)
	{
		Vector2 temp = new Vector2(screenPoint.x,screenPoint.y);
		temp = ScreenPointToUIPoint(temp);
		return new Vector3(temp.x,temp.y,screenPoint.z);
	}
	
	public static Vector2 UIPointToScreenPoint(Vector2 UIPoint)
	{
		return UIPoint / WorldToScreenModify + new Vector2( (float)Screen.width / 2, (float)Screen.height / 2);
	}
	
	public static Vector3 UIPointToScreenPoint(Vector3 UIPoint)
	{
		Vector3 uipoint = UIPointToScreenPoint(new Vector2(UIPoint.x,UIPoint.y));
		uipoint.z = UIPoint.z;
		return uipoint;
	}
	
	public static Vector3 WordPointToUIPoint(Vector3 wordpoint)
	{
		return ScreenPointToUIPoint(Camera.main.WorldToScreenPoint(wordpoint));
	}
	
	public static Vector3 AdaptiveUIPoint(Vector3 UIPoint)
	{
		return new Vector3(UIPoint.x * UIModify,UIPoint.y,UIPoint.z);
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
	
	public static string ValueAddColor(int value1,int value2,string color)
	{
		return value1 + "[" +color + "]  +" + value2 +"[-]";
	}
	
    public static T Find<T>(Transform transform) where T : MonoBehaviour
    {
        return transform.GetComponent<T>();
    }

    public static T Find<T>(Transform transform, string name) where T : MonoBehaviour
    {
        Transform trans = transform.Find(name);
        if (trans != null)
            return trans.GetComponent<T>();

        T[] children = transform.GetComponentsInChildren<T>(true);
        foreach (T item in children)
        {
            if (item.name == name)
				return item;
        }
        return null;
    }

    public static T Find<T>(GameObject obj) where T : MonoBehaviour
    {
        return obj.GetComponent<T>();
    }

    public static T Find<T>(GameObject obj, string name) where T : MonoBehaviour
    {
        return Find<T>(obj.transform, name);
    }

    public static T CreateChild<T>(Transform transform, string prefab, Vector3 pos) where T : MonoBehaviour
    {
		GameObject obj = (GameObject)Object.Instantiate(Resources.Load(prefab), pos, Quaternion.identity);
        if (obj == null)
            return null;

        obj.transform.parent = transform;
        obj.transform.localScale = Vector3.one;

        if (obj == null)
            return null;

        T item = obj.GetComponent<T>();

        return item;
    }

    public static T CreateChild<T>(Transform transform, string prefab) where T : MonoBehaviour
    {
        return CreateChild<T>(transform, prefab, Vector3.zero);
    }

    public static void EnableChildCollider(Transform transform, bool enable)
    {
        Transform[] children = transform.GetComponentsInChildren<Transform>();
        foreach (Transform item in children)
            if (transform != item)
                EnableCollider(item, enable);
    }

    public static void EnableChildCollider(GameObject obj, bool enable)
    {
        EnableChildCollider(obj.transform, enable);
    }

    static void EnableCollider(Transform transform, bool enable)
    {
        BoxCollider bc = transform.GetComponent<BoxCollider>();
        if (bc != null)
            bc.enabled = enable;
    }

    public static void EnableCollider(Transform transform, bool enable, bool recursive)
    {
        if (!recursive)
            EnableCollider(transform, enable);
        else
        {
            Transform[] children = transform.GetComponentsInChildren<Transform>();
            foreach (Transform item in children)
                EnableCollider(item, enable);
        }
    }
	
	public static UIEventListener Register(GameObject obj)
    {
        return UIEventListener.Get(obj);
    }
	
    public static UIEventListener Register(Transform transform, string name)
    {
        return Register(transform.Find(name).gameObject);
    }
	
	public static Vector3 FetchCoord(float x, float y)
	{
		return new Vector3(x, y, -0.001f);
	}
	
	public static void SetActive(GameObject go, bool active)
	{
		SetActive(go,active,true);
	}
	
	public static void SetActive(GameObject go, bool active,bool includeChild)
	{
		SetActive(go,active,includeChild,true);
	}
	
	public static void SetActive(GameObject go, bool active,bool includeChild,bool colliderIfActive)
	{
		if(includeChild)
		{
			UIWidget[] widgets = go.GetComponentsInChildren<UIWidget>(true);
			foreach (UIWidget item in widgets) {
				item.enabled = active;
			}
			if(colliderIfActive)
			{
				Collider[] colliders = go.GetComponentsInChildren<Collider>(true);
				foreach (Collider item in colliders) {
					item.enabled = active;
				}
			}
		}
		else
		{
			UIWidget widget = go.GetComponent<UIWidget>();
			
			if(widget!=null)
				widget.enabled = active;
			Collider collider = go.GetComponent<Collider>();
			
			if(collider!=null)
				collider.enabled = active;
		}
	}
	
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
