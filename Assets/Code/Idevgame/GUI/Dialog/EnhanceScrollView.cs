using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 滚动视图
/// </summary>
public class EnhanceScrollView : MonoBehaviour
{
	#region 字段
	/// <summary>
	/// 控制项目的缩放曲线
	/// </summary>
	[Tooltip("控制项目的缩放曲线")]
    public AnimationCurve scaleCurve;
	/// <summary>
	/// 控制位置曲线
	/// </summary>
	[Tooltip("控制位置曲线")]
    public AnimationCurve positionCurve;

	/// <summary>
	/// 控制深度,设置SetSiblingIndex()
	/// </summary>
	[Tooltip("控制深度曲线")]
    public AnimationCurve depthCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
	/// <summary>
	/// 游戏开始被选中的项ID
	/// </summary>
	[Tooltip("游戏开始被选中的项ID")]
    public int startCenterIndex = 0;
	/// <summary>
	/// 两个滚动项之间的宽度
	/// </summary>
	[Tooltip("两个滚动项之间的宽度")]
    public float cellWidth = 10f;
	/// <summary>
	/// 水平总宽度
	/// </summary>
    private float totalHorizontalWidth = 500.0f;
	// vertical fixed position value 
	/// <summary>
	/// 垂直固定位置值
	/// </summary>
	[Tooltip("垂直固定位置值")]
    public float yFixedPositionValue = 46.0f;

	// Lerp duration
	/// <summary>
	/// 滚动切换时总持续时间
	/// </summary>
	[Tooltip("滚动切换时总持续时间")]
    public float lerpDuration = 0.2f;
	/// <summary>
	/// 滚动切换已经持续时间
	/// </summary>
	private float mCurrentDuration = 0.0f;
	/// <summary>
	/// 被选中的滚动项id,因为在中心高亮,所以叫中心id
	/// </summary>
    private int mCenterIndex = 0;
	/// <summary>
	/// 正在滚动切换中flag
	/// </summary>
	//[Tooltip("是否允许插值")]
	private bool enableLerpTween = true;

	/// <summary>
	/// 当前选中项
	/// </summary>
    private EnhanceItem curCenterItem;
	/// <summary>
	/// 上一个选中项(缓存)
	/// </summary>
    private EnhanceItem preCenterItem;

	/// <summary>
	/// 是否可以改变选中项
	/// </summary>
    private bool canChangeItem = true;
	/// <summary>
	/// 滚动项目数分之一,保留四位小数
	/// </summary>
	private float dFactor = 0.2f;

	/// <summary>
	/// 原始水平值
	/// </summary>
    private float originHorizontalValue = 0.1f;
	/// <summary>
	/// 当前水平值
	/// </summary>
	[Tooltip("当前水平值")]
    public float curHorizontalValue = 0.5f;

	/// <summary>
	/// 滚动视图中的目标列表。
	/// </summary>
	public List<EnhanceItem> listEnhanceItems;
    public GameObject itemPrefab;
	/// <summary>
	/// 滚动视图中的目标排序列表。
	/// </summary>
	private List<EnhanceItem> listSortedItems = new List<EnhanceItem>();

	/// <summary>
	/// 拖拽效果系数因子(使用的时候除以1000)
	/// </summary>
	public float factor = 1f;

    #endregion

    #region 属性
    //当前中心Item的名字
    private string m_CurItemName = "";
    public string CurItemName
    {
        get { return m_CurItemName; }
        set
        {
            if (m_CurItemName != value)
            {
                //Debug.Log(value);
                m_CurItemName = value;
            }
        }
    }
	#endregion

	#region Unity自带生命周期函数

	void Awake()
    {
    }

    System.Action<Chapter> OnSelect;
    //当插入了所有子项后
    public void Reload(System.Action<Chapter> c = null)
    {
        OnSelect = c;
        listEnhanceItems.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            EnhanceItem item = transform.GetChild(i).GetComponent<EnhanceItem>();
            if (item != null && item.transform != itemPrefab.transform)
                listEnhanceItems.Add(item);
        }

        canChangeItem = true;//是否可以改变目标
        int count = listEnhanceItems.Count;//滚动目标数量
        dFactor = (Mathf.RoundToInt((1f / count) * 10000f)) * 0.0001f;//保留四位小数
                                                                      //中心id赋值
        mCenterIndex = count / 2;
        if (count % 2 == 0)
            mCenterIndex = count / 2 - 1;
        int index = 0;
        //反序遍历滚动视图中的目标列表
        for (int i = count - 1; i >= 0; i--)
        {
            //初始化视图中的项
            listEnhanceItems[i].CurveOffSetIndex = i;
            listEnhanceItems[i].CenterOffSet = dFactor * (mCenterIndex - index);//设置曲线中心偏移
            listEnhanceItems[i].SetSelectState(false);//设置未选中状态
            listEnhanceItems[i].SetScrollView(this);
            GameObject obj = listEnhanceItems[i].gameObject;
            UDragEnhanceView script = obj.GetComponent<UDragEnhanceView>();
            if (script != null)
                script.SetScrollView(this);//设置滚动视图
            index++;
        }

        // set the center item with startCenterIndex
        // 根据传入的初始被选中项id设置被选中物体(该物体在中心高亮提高显示)
        if (startCenterIndex < 0 || startCenterIndex >= count)//初始id合法性判断
        {
            Debug.LogError("## startCenterIndex < 0 || startCenterIndex >= listEnhanceItems.Count  out of index ##");
            startCenterIndex = mCenterIndex;
        }

        // sorted items
        //对项目排序
        listSortedItems = new List<EnhanceItem>(listEnhanceItems.ToArray());
        //设置总宽度
        totalHorizontalWidth = cellWidth * count;
        //设置当前中心项
        curCenterItem = listEnhanceItems[startCenterIndex];
        //设置当前水平值
        curHorizontalValue = 0.5f - curCenterItem.CenterOffSet;
        //插值渐变到目标值
        LerpTweenToTarget(0f, curHorizontalValue, false);
    }

    void Start()
    {
        
    }

    void Update()
    {
		if (enableLerpTween)//正在滚动切换中flag
		{
			//渐变视图到目标值
			TweenViewToTarget();
		}
    }

	#endregion

	#region 公开方法
	/// <summary>
	/// 根据曲线fTime值更新所有滚动项状态
	/// </summary>
	/// <param name="fValue"></param>
	public void UpdateEnhanceScrollView(float fValue)
    {
		//遍历所有的滚动项
        for (int i = 0; i < listEnhanceItems.Count; i++)
        {
            EnhanceItem itemScript = listEnhanceItems[i];
			//获取x坐标值
            float xValue = GetXPosValue(fValue, itemScript.CenterOffSet);
			//获取缩放值
            float scaleValue = GetScaleValue(fValue, itemScript.CenterOffSet);
			//通过层深曲线获得层深值(中间大,两边小)
            float depthCurveValue = depthCurve.Evaluate(fValue + itemScript.CenterOffSet);
			//更新项目状态
            itemScript.UpdateScrollViewItems(xValue, depthCurveValue, listEnhanceItems.Count, yFixedPositionValue, scaleValue);
        }
    }

	/// <summary>
	/// 设置水平目标项目id
	/// </summary>
	/// <param name="selectItem"></param>
	public void SetHorizontalTargetItemIndex(EnhanceItem selectItem)
    {
        if (!canChangeItem)//当前不允许改变选中目标,直接返回
            return;

        if (curCenterItem == selectItem)//选中项为发生变化,直接放回
            return;

        canChangeItem = false;//设置flag为不允许改变选中目标
        preCenterItem = curCenterItem;//缓存上一个选中的项目
        curCenterItem = selectItem;//缓存当前选中的目标

		// 计算运动的方向
		float centerXValue = positionCurve.Evaluate(0.5f) * totalHorizontalWidth;//得到中心x坐标
        bool isRight = false;
        if (selectItem.transform.localPosition.x > centerXValue)//选中项的x坐标大于中心坐标时,目标在右方
            isRight = true;


		// 计算偏移量因子
		int moveIndexCount = GetMoveCurveFactorCount(preCenterItem, selectItem);
		// 得到运动距离
        float dvalue = 0.0f;
        if (isRight)//目标在右方为负
		{
            dvalue = -dFactor * moveIndexCount;
        }
        else
        {
            dvalue = dFactor * moveIndexCount;
        }
        float originValue = curHorizontalValue;
		//插值渐变运算
        LerpTweenToTarget(originValue, curHorizontalValue + dvalue, true);
    }

    // Click the right button to select the next item.
	/// <summary>
	/// 点击"下一个"按钮,一般在右边
	/// </summary>
    public void OnBtnRightClick()
    {
        if (!canChangeItem)//当前不允许改变选中项时,直接退出
            return;
        int targetIndex = curCenterItem.CurveOffSetIndex + 1;
        if (targetIndex > listEnhanceItems.Count - 1)
            targetIndex = 0;
		//设置水平目标项目
        SetHorizontalTargetItemIndex(listEnhanceItems[targetIndex]);
    }

	// Click the left button the select next next item.
	/// <summary>
	/// 点击"上一个"按钮,一般在左边
	/// </summary>
	public void OnBtnLeftClick()
    {
        if (!canChangeItem)
            return;
        int targetIndex = curCenterItem.CurveOffSetIndex - 1;
        if (targetIndex < 0)
            targetIndex = listEnhanceItems.Count - 1;
        SetHorizontalTargetItemIndex(listEnhanceItems[targetIndex]);
    }

	// On Drag Move
	/// <summary>
	/// 正在拖拽中
	/// </summary>
	/// <param name="delta">自上次更新以来的指针delta。</param>
	public void OnDragEnhanceViewMove(Vector2 delta)
    {
        // In developing
        if (Mathf.Abs(delta.x) > 0.0f)//x轴有移动
        {
            curHorizontalValue += delta.x * factor / 1000;//设置当前水平值
			//没有渐变过程的移动
            LerpTweenToTarget(0.0f, curHorizontalValue, false);
        }
    }

    // On Drag End
	/// <summary>
	/// 拖拽结束
	/// </summary>
    public void OnDragEnhanceViewEnd()
    {
		// 查找关闭项以居中
		int closestIndex = 0;
        float value = (curHorizontalValue - (int)curHorizontalValue);//得到当前水平值得小数部分
        float min = float.MaxValue;
        float tmp = 0.5f * (curHorizontalValue < 0 ? -1 : 1);
		//遍历滚动项目列表
        for (int i = 0; i < listEnhanceItems.Count; i++)
        {
			//得到项目和中心的距离
            float dis = Mathf.Abs(Mathf.Abs(value) - Mathf.Abs((tmp - listEnhanceItems[i].CenterOffSet)));
			//距离最小i的赋值给关闭id
            if (dis < min)
            {
                closestIndex = i;
                min = dis;
            }
        }
        originHorizontalValue = curHorizontalValue;//缓存初始水平值
		//设置目标水平值
        float target = ((int)curHorizontalValue + (tmp - listEnhanceItems[closestIndex].CenterOffSet));
        preCenterItem = curCenterItem;//缓存上一个选中项
        curCenterItem = listEnhanceItems[closestIndex];//缓存当前选中项
		//有渐变过程的移动
        LerpTweenToTarget(originHorizontalValue, target, true);
        canChangeItem = false;//设置状态为不可改变目标
    }
	#endregion

	#region 私有方法
	/// <summary>
	/// 插值动画
	/// </summary>
	/// <param name="originValue">初始值</param>
	/// <param name="targetValue">目标值</param>
	/// <param name="needTween">是否需要播放渐变</param>
	private void LerpTweenToTarget(float originValue, float targetValue, bool needTween = false)
    {
        if (!needTween)
        {
			//对滚动项目进行排序
            SortEnhanceItem();
			//设置原始水平值
            originHorizontalValue = targetValue;
			//根据曲线fTime值更新所有滚动项状态
            UpdateEnhanceScrollView(targetValue);
			//动画结束,设置选中状态
            this.OnTweenOver();
        }
        else
        {
			//设置TweenViewToTarget()函数要使用的缓存
            originHorizontalValue = originValue;//缓存初始值
            curHorizontalValue = targetValue;//缓存目标值
            mCurrentDuration = 0.0f;//持续时间归零
		}
        enableLerpTween = needTween;
    }
	/// <summary>
	/// 渐变图像到目标值
	/// </summary>
    private void TweenViewToTarget()
    {
		//设置滚动切换已经持续时间
        mCurrentDuration += Time.deltaTime;
		//持续时间大于规定时间时,直接设置持续时间为规定时间
		if (mCurrentDuration > lerpDuration)
		{
			mCurrentDuration = lerpDuration;
		}

        float percent = mCurrentDuration / lerpDuration;//设置插值比例
        float value = Mathf.Lerp(originHorizontalValue, curHorizontalValue, percent);//得到插值
        UpdateEnhanceScrollView(value);
		//如果持续时间够长了
        if (mCurrentDuration >= lerpDuration)
        {
            canChangeItem = true;//可改变目标flag为真
            enableLerpTween = false;//正在滚动切换状态设置
            OnTweenOver();//渐变结束,设置选中状态
        }
    }
	/// <summary>
	/// 渐变动画结束,设置选中状态
	/// </summary>
    private void OnTweenOver()
    {
        if (preCenterItem != null)//上一个选中项
            preCenterItem.SetSelectState(false);
        if (curCenterItem != null)//当前选中项
            curCenterItem.SetSelectState(true);

        if (curCenterItem != null)
        {
            CurItemName = curCenterItem.name;
        }

        if (mDic.ContainsKey(curCenterItem.gameObject))
        {
            if (OnSelect != null)
                OnSelect(mDic[curCenterItem.gameObject]);
        }
    }

    // Get the evaluate value to set item's scale
	/// <summary>
	/// 根据缩放曲线得到项目的缩放值
	/// </summary>
	/// <param name="sliderValue">基础位置</param>
	/// <param name="added">偏移增量</param>
	/// <returns></returns>
    private float GetScaleValue(float sliderValue, float added)
    {
        float scaleValue = scaleCurve.Evaluate(sliderValue + added);
        return scaleValue;
    }

	// Get the X value set the Item's position
	/// <summary>
	/// 根据位置曲线获取X值设置滚动项的位置
	/// </summary>
	/// <param name="sliderValue">基础位置</param>
	/// <param name="added">偏移增量</param>
	/// <returns></returns>
	private float GetXPosValue(float sliderValue, float added)
    {
        float evaluateValue = positionCurve.Evaluate(sliderValue + added) * totalHorizontalWidth;
        return evaluateValue;
    }
	/// <summary>
	/// 计算偏移量因子
	/// </summary>
	/// <param name="preCenterItem">上一个选中项</param>
	/// <param name="newCenterItem">新的选中项</param>
	/// <returns></returns>
	private int GetMoveCurveFactorCount(EnhanceItem preCenterItem, EnhanceItem newCenterItem)
    {
		//排序滚动视图
        SortEnhanceItem();
		//新的真实索引 - 上一个真实索引
        int factorCount = Mathf.Abs(newCenterItem.RealIndex) - Mathf.Abs(preCenterItem.RealIndex);
		//返回绝对值
        return Mathf.Abs(factorCount);
    }

	// sort item with X so we can know how much distance we need to move the timeLine(curve time line)
	/// <summary>
	/// 用X排序，这样我们就能知道我们需要多少距离来移动时间轴（曲线时间线）
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <returns></returns>
	static public int SortPosition(EnhanceItem a, EnhanceItem b)
	{
		return a.transform.localPosition.x.CompareTo(b.transform.localPosition.x);
	}
	/// <summary>
	/// 对滚动视图项目进行排序
	/// </summary>
    private void SortEnhanceItem()
    {
		//
        listSortedItems.Sort(SortPosition);
		for (int i = listSortedItems.Count - 1; i >= 0; i--)
		{
			listSortedItems[i].RealIndex = i;
		}
    }
    #endregion
    Dictionary<GameObject, Chapter> mDic = new Dictionary<GameObject, Chapter>();
    public void RegisterOnSelect(GameObject obj, Chapter chapter)
    {
        mDic[obj] = chapter;
    }
}