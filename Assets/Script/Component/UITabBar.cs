using UnityEngine;
using System.Collections;
using System.Collections.Generic;


//using UnityEditor;

[ExecuteInEditMode]
[AddComponentMenu("UI/Component/TabBar")]
public class UITabBar : MonoBehaviour {



	UITable mTable=null;

	public UIAtlas atlas;


	public string spriteName;

	public string selectSpriteName;

	public UIFont font;

	public string texts;

	public string  languages="";

	public int w=200;

	public int h=60;


	public VoidDelegate onClick;

	public delegate void VoidDelegate (int idx);

	public Color SelectedColor  ;

	public Color NormalColor;

	public bool IsOutline;

	int lastIndex;

	public Vector2 SelectedTextPos = Vector2.zero;

	public Vector2 NormalTextPos =Vector2.zero;

	public GameColor.GColor NormalGameColor =  GameColor.GColor.None;
	public GameColor.GColor SelectedGameColor =  GameColor.GColor.None;
	public float gap
	{
		get
		{
			return Table.gap.x;
		}

		set
		{

			Table.gap  = new Vector2(value,value);
		}
	}

	public int fontSize=35;

	public int normalSize;

	public enum Direction
	{
		Horizontal,
		Vertical,
	}

//	Direction mDirection ;
	public Direction direction;
//	{
//		get{
//			return mDirection;
//		}
//
//		set
//		{
//			if(mDirection!=value)
//			{
//				mDirection =value;
//				OnSet();
//			}
//			else
//			{
//				mDirection =value;
//			}
//		}
//	}
	public List<GameObject> Items
	{
		get{return items;}
	}


	List<GameObject> items = new List<GameObject>();

	private UITable Table{

		get{
			if(mTable==null)
			{
				mTable = transform.GetComponent<UITable>();
				if(mTable==null) mTable= transform.gameObject.AddComponent<UITable>();
			}

			return mTable;
		}
	}

	int mSelectedIndex =0;

	public int SelectedIndex
	{
		get
		{
			return mSelectedIndex;
		}

		set
		{
			mSelectedIndex = value;
			SetSelectedIndex(value);
		}
	}


	void Awake()
	{
		if(SelectedColor==null)SelectedColor = new Color(255,255,255,255);
		if(NormalColor==null)NormalColor = new Color(255,255,255,255);
		if(SelectedColor.a==0)SelectedColor = new Color(255,255,255,255);
		if(NormalColor.a==0)NormalColor = new Color(255,255,255,255);
		if(normalSize == 0)normalSize = fontSize;
		OnSet();
	}




	public void OnSet()
	{
//		for(int i=0;i<transform.childCount;i++)
//		{
//			Object.DestroyImmediate(transform.GetChild(i).gameObject);
//		}

		while (transform.childCount>0) 
		{
			Object.DestroyImmediate(transform.GetChild(transform.childCount-1).gameObject);
		}
//		foreach(GameObject go in gos)Object.DestroyImmediate(go);
//
		items=new List<GameObject>();


		if(direction==Direction.Horizontal)
		{
			Table.columns = 0;
		}
		else
		{
			Table.columns = 1;
		}

		if(texts!=null&&texts!=""&&atlas!=null&&font!=null)
		{
			getTexts();
			string[] strs= texts.Split(',');
			
			for( int i=0;i< strs.Length;i++)
			{
				string str  = strs[i];
//				#if UNITY_EDITOR
//				NGUIEditorTools.RegisterUndo("Add a child UI Panel", transform.gameObject);
//				#endif

				GameObject child = new GameObject(NGUITools.GetName<UIPanel>());

//				GameObject child = NGUITools.AddChild(transform.gameObject);
//				child.AddComponent<UIPanel>();
//				child.name+=str;
				child.layer = transform.gameObject.layer;
//				
				child.transform.parent = Table.transform;
				child.transform.localScale = Vector3.one;

				BoxCollider collider = child.AddComponent<BoxCollider>();
				child.AddComponent<UIEventListener>();
				collider.size = new Vector3(w,h,1);
//				child.transform
//				gos.Add(child);
				UISprite sprite =NGUITools.AddSprite(child,atlas,spriteName); //;NGUITools.AddWidget<UISprite>(child.transform.gameObject);//
//				sprite.transform.parent = child.transform;
				sprite.type=UISprite.Type.Sliced;
//				sprite.atlas=Resources.Load(atlas.name, typeof(UIAtlas)) as UIAtlas;
//				sprite.spriteName=spriteName;
				sprite.transform.localScale =new Vector3(w,h,1);

//				sprite.transform.parent = child.transform;

				UIEventListener.Get(child).onClick= OnItemClick;
				UILabel label = NGUITools.AddWidget<UILabel>(child);
				label.font = font;
				label.text=str;
				label.transform.localScale = new Vector3(fontSize,fontSize,1);
				label.transform.localPosition = new Vector3(0,0,-1);
				child.transform.localPosition = new Vector3(0,0,-1);
				if(IsOutline)label.effectStyle = UILabel.Effect.Outline;
				items.Add(child);
			}
			SelectedIndex=0;
		}
		Table.repositionNow = true;

		//Table.Reposition();

	}


	void getTexts()
	{

		if(languages!="")
		{
			texts="";
			string[] strs= languages.Split(',');
			for( int i=0;i< strs.Length;i++)
			{
				string str  = strs[i];
				if(i!=0)
					texts+=","+LanguagesManager.Instance.GetItem(str);
				else
					texts+=LanguagesManager.Instance.GetItem(str);
			}
		}
	}

	void SetSelectedIndex(int  idx)
	{
		for(int i=0;i<items.Count;i++)
		{
			if(i==idx)
			{
				items[i].GetComponentInChildren<UISprite>().spriteName= selectSpriteName;
				items[i].GetComponentInChildren<UILabel>().color= SelectedColor;
				items[i].GetComponentInChildren<UILabel>().transform.localScale= new Vector3(fontSize,fontSize,1);
				items[i].GetComponentInChildren<UILabel>().transform.localPosition = new Vector3(SelectedTextPos.x,SelectedTextPos.y,-1);

			}
			else
			{
				items[i].GetComponentInChildren<UISprite>().spriteName= spriteName;
				items[i].GetComponentInChildren<UILabel>().color= NormalColor;
				items[i].GetComponentInChildren<UILabel>().transform.localScale= new Vector3(normalSize,normalSize,1);
				items[i].GetComponentInChildren<UILabel>().transform.localPosition = new Vector3(NormalTextPos.x,NormalTextPos.y,-1);

			}
		}
	}


	void OnItemClick(GameObject go)
	{
		for(int i=0;i<items.Count;i++)
		{
			if(items[i]==go)
			{
				if(SelectedIndex==i)
					return;
				SelectedIndex = i;
			}
		}

		if (onClick != null) onClick(SelectedIndex);
	}

}






