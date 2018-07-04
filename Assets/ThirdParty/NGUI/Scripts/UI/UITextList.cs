//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright ㄟ2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Text list can be used with a UILabel to create a scrollable multi-line text field that's
/// easy to add new entries to. Optimal use: chat window.
/// </summary>

[AddComponentMenu("NGUI/UI/Text List")]
public class UITextList : MonoBehaviour
{
	public enum Style
	{
		Text,
		Chat,
	}

	public Style style = Style.Text;
	public UILabel textLabel;
	public float maxWidth = 0f;
	public float maxHeight = 0f;
	public int maxEntries = 50;
	public bool supportScrollWheel = true;
    public float yDelta = 0f;

	// Text list is made up of paragraphs
	protected class Paragraph
	{
		public string text;		// Original text
		public string[] lines;	// Split lines
	}

	protected char[] mSeparator = new char[] { '\n' };
	protected List<Paragraph> mParagraphs = new List<Paragraph>();
	protected float mScroll = 0f;
	protected bool mSelected = false;
	protected int mTotalLines = 0;

	/// <summary>
	/// Clear the text.
	/// </summary>

	public void Clear ()
	{
		mParagraphs.Clear();
		UpdateVisibleText();
	}

	/// <summary>
	/// Add a new paragraph.
	/// </summary>

	public void Add (string text) { Add(text, true); }

	/// <summary>
	/// Add a new paragraph.
	/// </summary>

	protected void Add (string text, bool updateVisible)
	{
		Paragraph ce = null;

		if (mParagraphs.Count < maxEntries)
		{
			ce = new Paragraph();
		}
		else
		{
			ce = mParagraphs[0];
			mParagraphs.RemoveAt(0);
		}

		ce.text = text;
		mParagraphs.Add(ce);
		
		if (textLabel != null && textLabel.font != null)
		{
			// Rebuild the line
			ce.lines = textLabel.font.WrapText(ce.text, maxWidth / textLabel.transform.localScale.y,
				textLabel.maxLineCount, textLabel.supportEncoding, textLabel.symbolStyle).Split(mSeparator);

			// Recalculate the total number of lines
			mTotalLines = 0;
			for (int i = 0, imax = mParagraphs.Count; i < imax; ++i) mTotalLines += mParagraphs[i].lines.Length;
		}

		// Update the visible text
		if (updateVisible) UpdateVisibleText();
	}

	/// <summary>
	/// Automatically find the values if none were specified.
	/// </summary>

	void Awake ()
	{
		if (textLabel == null) textLabel = GetComponentInChildren<UILabel>();
		if (textLabel != null) textLabel.lineWidth = 0;

		Collider col = GetComponent<Collider>();

		if (col != null)
		{
			// Automatically set the width and height based on the collider
			if (maxHeight <= 0f) maxHeight = col.bounds.size.y / transform.lossyScale.y;
			if (maxWidth  <= 0f) maxWidth  = col.bounds.size.x / transform.lossyScale.x;
		}
	}

	/// <summary>
	/// Remember whether the widget is selected.
	/// </summary>

	void OnSelect (bool selected) { mSelected = selected; }

	/// <summary>
	/// Refill the text label based on what's currently visible.
	/// </summary>

	protected void UpdateVisibleText ()
	{
		if (textLabel != null)
		{
			UIFont font = textLabel.font;

			if (font != null)
			{
				int lines = 0;
				int maxLines = maxHeight > 0 ? Mathf.FloorToInt(maxHeight / textLabel.cachedTransform.localScale.y) : 100000;
				int offset = Mathf.RoundToInt(mScroll);

				// Don't let scrolling to exceed the visible number of lines
				if (maxLines + offset > mTotalLines)
				{
					offset = Mathf.Max(0, mTotalLines - maxLines);
					mScroll = offset;
				}

				if (style == Style.Chat)
				{
					offset = Mathf.Max(0, mTotalLines - maxLines - offset);
				}

				StringBuilder final = new StringBuilder();

				for (int i = 0, imax = mParagraphs.Count; i < imax; ++i)
				{
					Paragraph p = mParagraphs[i];

					for (int b = 0, bmax = p.lines.Length; b < bmax; ++b)
					{
						string s = p.lines[b];

						if (offset > 0)
						{
							--offset;
						}
						else
						{
							if (final.Length > 0) final.Append("\n");
							final.Append(s);
							++lines;
							if (lines >= maxLines) break;
						}
					}
					if (lines >= maxLines) break;
				}
				textLabel.text = final.ToString();
			}
		}
	}

	/// <summary>
	/// Allow scrolling of the text list.
	/// </summary>

	void OnScroll (float val)
	{
		if (mSelected && supportScrollWheel)
		{
			val *= (style == Style.Chat) ? 10f : -10f;
			mScroll = Mathf.Max(0f, mScroll + val);
			UpdateVisibleText();
		}
	}
	
	void OnDrag(Vector2 delta)
	{
		yDelta = delta.y;
		if (mSelected && supportScrollWheel)
		{
			float val = -delta.y * Time.deltaTime * ((style == Style.Chat) ? 5.0f : -5.0f);
			mScroll = Mathf.Max(0f, mScroll + val);
			UpdateVisibleText();
		}
	}		
}