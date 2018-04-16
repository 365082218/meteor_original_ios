using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class RepeatPressButton : MonoBehaviour ,IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public float interval = 0.1f;
    [SerializeField]
    UnityEvent m_OnLongpress = new UnityEvent();

    private bool isPointDown = false;
    private float lastInvokeTime;
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (isPointDown)
        {
            if (Time.time - lastInvokeTime > interval)
            {
                //触发点击;
                m_OnLongpress.Invoke();
                lastInvokeTime = Time.time;
            }
        }

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        m_OnLongpress.Invoke();
        isPointDown = true;
        lastInvokeTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointDown = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointDown = false;
    }
}