//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright ?2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// All children added to the game object with this script will be arranged into a table
/// with rows and columns automatically adjusting their size to fit their content
/// (think "table" tag in HTML).
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Table")]
public class UITable : MonoBehaviour
{
	public delegate void OnReposition ();

    public float OriginParentPanelx = 0.0f;
    public float OriginParentPanely = 0.0f;
    public float OriginParentPanelClipCenterx = 0.0f;
    public float OriginParentPanelClipCentery = 0.0f;

	public enum Direction
	{
		Down,
		Up,
	}

//	public enum HorizontalAlign
//	{
//		Left,
//		Center,
//		Right,
//	}

	public int columns = 0;
	public Direction direction = Direction.Down;
//	public HorizontalAlign horizontalAlign = HorizontalAlign.Center;
	public Vector2 padding = Vector2.zero;
	public Vector2 gap =Vector2.zero;
	public bool sorted = false;
	public bool hideInactive = true;
	public bool repositionNow = false;
	public bool keepWithinPanel = false;
	public OnReposition onReposition;

	UIPanel mPanel;
	UIDraggablePanel mDrag;
	bool mStarted = false;
	List<Transform> mChildren = new List<Transform>();

	/// <summary>
	/// Function that sorts items by name.
	/// </summary>

	static public int SortByName (Transform a, Transform b) { return string.Compare(a.name, b.name); }

	/// <summary>
	/// Returns the list of table's children, sorted alphabetically if necessary.
	/// </summary>

	public List<Transform> children
	{
		get
		{
			if (mChildren.Count == 0)
			{
				Transform myTrans = transform;
				mChildren.Clear();

				for (int i = 0; i < myTrans.childCount; ++i)
				{
					Transform child = myTrans.GetChild(i);

					if (child && child.gameObject && (!hideInactive || NGUITools.GetActive(child.gameObject))) mChildren.Add(child);
				}
				if (sorted) mChildren.Sort(SortByName);
			}
			return mChildren;
		}
	}

	/// <summary>
	/// Positions the grid items, taking their own size into consideration.
	/// </summary>

	void RepositionVariableSize (List<Transform> children)
	{
		float xOffset = 0;
		float yOffset = 0;

		int cols = columns > 0 ? children.Count / columns + 1 : 1;
		int rows = columns > 0 ? columns : children.Count;

		Bounds[,] bounds = new Bounds[cols, rows];
		Bounds[] boundsRows = new Bounds[rows];
		Bounds[] boundsCols = new Bounds[cols];

		int x = 0;
		int y = 0;

		for (int i = 0, imax = children.Count; i < imax; ++i)
		{
			Transform t = children[i];
			Bounds b = NGUIMath.CalculateRelativeWidgetBounds(t);
			Vector3 scale = t.localScale;
			b.min = Vector3.Scale(b.min, scale);
			b.max = Vector3.Scale(b.max, scale);
			bounds[y, x] = b;

			boundsRows[x].Encapsulate(b);
			boundsCols[y].Encapsulate(b);

			if (++x >= columns && columns > 0)
			{
				x = 0;
				++y;
			}
		}

		x = 0;
		y = 0;

		for (int i = 0, imax = children.Count; i < imax; ++i)
		{
			Transform t = children[i];
			Bounds b = bounds[y, x];
			Bounds br = boundsRows[x];
			Bounds bc = boundsCols[y];

			Vector3 pos = t.localPosition;


			pos.x = xOffset + b.extents.x - b.center.x;


			pos.x += b.min.x - br.min.x + padding.x;

//			if((columns!=0&&i%columns!=0)||(columns==0&&i!=0))pos.x += gap.x;
			if(columns!=0)
			{
				pos.x+=gap.x*(i%columns);
			}
			else
			{
				pos.x+=gap.x*i;
			}
			if (direction == Direction.Down)
			{
				pos.y = -yOffset - b.extents.y - b.center.y;
				pos.y += (b.max.y - b.min.y - bc.max.y + bc.min.y) * 0.5f - padding.y;

//				if(Mathf.Ceil(i/columns)>0) pos.y-= gap.y;

				if(columns!=0)pos.y-=gap.y*Mathf.Floor(i/columns);
			}
			else
			{
				pos.y = yOffset + b.extents.y - b.center.y;
				pos.y += (b.max.y - b.min.y - bc.max.y + bc.min.y) * 0.5f - padding.y;

//				if(Mathf.Ceil(i/columns)>0) pos.y+= gap.y;
				if(columns!=0)pos.y+=gap.y*Mathf.Floor(i/columns);
			}




			xOffset += br.max.x - br.min.x ;//+ padding.x * 2f;


//			xOffset += br.max.x - br.min.x + padding.x * 2f;

			t.localPosition = pos;

			if (++x >= columns && columns > 0)
			{
				x = 0;
				++y;

				xOffset = 0f;
				yOffset += bc.size.y;// + padding.y * 2f;
			}
		}
	}

	/// <summary>
	/// Recalculate the position of all elements within the table, sorting them alphabetically if necessary.
	/// </summary>

	public void Reposition ()
	{
		if (mStarted)
		{
			Transform myTrans = transform;
			mChildren.Clear();
			List<Transform> ch = children;
			if (ch.Count > 0) RepositionVariableSize(ch);

			if (mDrag != null)
			{
				mDrag.UpdateScrollbars(true);
				mDrag.RestrictWithinBounds(true);
			}
			else if (mPanel != null)
			{
				mPanel.ConstrainTargetToBounds(myTrans, true);
			}
			if (onReposition != null) onReposition();
		}
		else repositionNow = true;
	}

    public void SetToOriginPos()
    {
        GameObject parent = gameObject.transform.parent.gameObject;
        UIPanel parentpanel = parent.GetComponent<UIPanel>() as UIPanel;
        if (null == parentpanel) return;
        UIDraggablePanel dragpanel = parentpanel.GetComponent<UIDraggablePanel>() as UIDraggablePanel;
        if (null == dragpanel) return;
        if(1 == dragpanel.scale.x)
        {
            parentpanel.transform.localPosition = new Vector3(OriginParentPanelx, parentpanel.transform.localPosition.y, parentpanel.transform.localPosition.z);
            parentpanel.clipRange = new Vector4(OriginParentPanelClipCenterx, parentpanel.clipRange.y, parentpanel.clipRange.z, parentpanel.clipRange.w);
        }
        else if(1 == dragpanel.scale.y)
        {
            parentpanel.transform.localPosition = new Vector3(parentpanel.transform.localPosition.x, OriginParentPanely, parentpanel.transform.localPosition.z);
            parentpanel.clipRange = new Vector4(parentpanel.clipRange.x, OriginParentPanelClipCentery, parentpanel.clipRange.z, parentpanel.clipRange.w);
        }
    }

	/// <summary>
	/// Position the grid's contents when the script starts.
	/// </summary>

	void Start ()
	{
		mStarted = true;

		if (keepWithinPanel)
		{
			mPanel = NGUITools.FindInParents<UIPanel>(gameObject);
			mDrag = NGUITools.FindInParents<UIDraggablePanel>(gameObject);
		}
		Reposition();
	}

	/// <summary>
	/// Is it time to reposition? Do so now.
	/// </summary>

	void LateUpdate ()
	{
		if (repositionNow)
		{
			repositionNow = false;
			Reposition();
		}
	}
}