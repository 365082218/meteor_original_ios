using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
/// <summary>
/// 滚动项的拖拽事件
/// </summary>
public class UDragEnhanceView : EventTrigger
{
	/// <summary>
	/// 滚动视图
	/// </summary>
	private EnhanceScrollView enhanceScrollView;
	/// <summary>
	/// 设置滚动视图
	/// </summary>
	/// <param name="view">滚动视图</param>
    public void SetScrollView(EnhanceScrollView view)
    {
        enhanceScrollView = view;
    }
	/// <summary>
	/// 开始拖动
	/// </summary>
	/// <param name="eventData"></param>
    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
    }
	/// <summary>
	/// 正在拖动
	/// </summary>
	/// <param name="eventData"></param>
    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
        if (enhanceScrollView != null)
            enhanceScrollView.OnDragEnhanceViewMove(eventData.delta);//调用视图的正在拖动中
    }
	/// <summary>
	/// 拖动结束
	/// </summary>
	/// <param name="eventData"></param>
    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        if (enhanceScrollView != null)
            enhanceScrollView.OnDragEnhanceViewEnd();//调用视图的拖动结束
    }
}
