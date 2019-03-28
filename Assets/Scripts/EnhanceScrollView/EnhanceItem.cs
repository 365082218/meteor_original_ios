using UnityEngine;
using System.Collections;
/// <summary>
/// 滚动项的父类
/// </summary>
public class EnhanceItem : MonoBehaviour
{
	// Start index
	/// <summary>
	/// 初始id
	/// </summary>
	private int curveOffSetIndex = 0;

	/// <summary>
	/// 滚动视图
	/// </summary>
	private EnhanceScrollView enhanceScrollView;

	/// <summary>
	/// 初始id
	/// </summary>
	public int CurveOffSetIndex
    {
        get { return this.curveOffSetIndex; }
        set { this.curveOffSetIndex = value; }
    }

	/// <summary>
	/// 设置滚动视图
	/// </summary>
	/// <param name="view">滚动视图</param>
	public void SetScrollView(EnhanceScrollView view)
	{
		enhanceScrollView = view;
	}
	// Runtime real index(Be calculated in runtime)
	/// <summary>
	/// 运行时真实索引(在运行时计算)
	/// </summary>
	private int curRealIndex = 0;
	/// <summary>
	/// 运行时真实索引(在运行时计算)
	/// </summary>
	public int RealIndex
    {
        get { return this.curRealIndex; }
        set { this.curRealIndex = value; }
    }

	// Curve center offset 
	/// <summary>
	/// 曲线中心偏移
	/// </summary>
	private float dCurveCenterOffset = 0.0f;
	/// <summary>
	/// 曲线中心偏移
	/// </summary>
	public float CenterOffSet
    {
        get { return this.dCurveCenterOffset; }
        set { dCurveCenterOffset = value; }
    }
	/// <summary>
	/// 访问加速transform
	/// </summary>
    private Transform mTrs;

    void Awake()
    {
        mTrs = this.transform;
        OnAwake();
    }

    void Start()
    {
        OnStart();
    }

	// Update Item's status
	// 1. position
	// 2. scale
	// 3. "depth" is 2D or z Position in 3D to set the front and back item
	/// <summary>
	/// 更新项目的状态
	/// </summary>
	/// <param name="xValue">x坐标</param>
	/// <param name="depthCurveValue">层深值</param>
	/// <param name="itemCount">滚动项目数</param>
	/// <param name="yValue">y坐标</param>
	/// <param name="scaleValue">缩放系数</param>
	public void UpdateScrollViewItems(
        float xValue,
        float depthCurveValue,
        float itemCount,
        float yValue,
        float scaleValue)
    {
        Vector3 targetPos = Vector3.one;
        Vector3 targetScale = Vector3.one;
        // 设置位置
        targetPos.x = xValue;
        targetPos.y = yValue;
        mTrs.localPosition = targetPos;

		// 设置层深
        SetItemDepth(depthCurveValue, itemCount);
        // 设置缩放
        targetScale.x = targetScale.y = scaleValue;
        mTrs.localScale = targetScale;
    }

	#region 定义子类方法
	/// <summary>
	/// 点击滚动项目
	/// </summary>
	protected virtual void OnClickEnhanceItem()
    {
        //EnhanceScrollView.Instance.SetHorizontalTargetItemIndex(this);
		enhanceScrollView.SetHorizontalTargetItemIndex(this);
    }
	/// <summary>
	/// Start()函数
	/// </summary>
    protected virtual void OnStart()
    {
    }
	/// <summary>
	/// Awake()函数
	/// </summary>
    protected virtual void OnAwake()
    {
    }
	/// <summary>
	/// 设置项目深度
	/// </summary>
	/// <param name="depthCurveValue">层深值</param>
	/// <param name="itemCount">滚动项目数</param>
    protected virtual void SetItemDepth(float depthCurveValue, float itemCount)
    {
    }

	// Set the item center state
	/// <summary>
	/// 设置项目中心状态
	/// </summary>
	/// <param name="isCenter"></param>
	public virtual void SetSelectState(bool isCenter)
    {
    }
	#endregion
}
