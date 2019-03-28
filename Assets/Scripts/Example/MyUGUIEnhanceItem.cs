using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MyUGUIEnhanceItem : EnhanceItem
{
    private Button m_Button;//按钮组件
    private Image m_Image;//图片组件
	/// <summary>
	/// 点击UGUI的按钮
	/// </summary>
    private void OnClickUGUIButton()
    {
		//调用父类点击项目的方法
        OnClickEnhanceItem();
    }

	#region 重写父类方法
	/// <summary>
	/// unity start函数
	/// </summary>
	protected override void OnStart()
    {
        m_Image = GetComponent<Image>();//得到图片组件
        m_Button = GetComponent<Button>();//得到按钮组件
        m_Button.onClick.AddListener(OnClickUGUIButton);//添加按钮监听事件
    }

	/// <summary>
	/// 设置项目深度
	/// </summary>
	/// <param name="depthCurveValue">层深值</param>
	/// <param name="itemCount">滚动项目数</param>
    protected override void SetItemDepth(float depthCurveValue, float itemCount)
    {
        int newDepth = (int)(depthCurveValue * itemCount);//得到层深id
        this.transform.SetSiblingIndex(newDepth);
    }
	/// <summary>
	/// 设置项目中心状态
	/// </summary>
	/// <param name="isCenter">是否被选中</param>
    public override void SetSelectState(bool isCenter)
    {

        if (m_Image == null)
            m_Image = GetComponent<Image>();
		//图片颜色设置,如果是被选中的白色叠加否则灰色叠加
        m_Image.color = isCenter ? Color.white : Color.gray;
    }
	#endregion
}
