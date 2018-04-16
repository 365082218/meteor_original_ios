using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField]
    public UnityEvent OnPress = new UnityEvent();
    public UnityEvent OnRelease = new UnityEvent();
    public bool PointDown { get { return isPointDown; } }
    public void Reset()
    {
        isPointDown = false;
    }
    private bool isPointDown = false;
    private float lastInvokeTime;
    public bool repeatScan = false;
    // Use this for initialization
    void Start()
    {
    }
    
    // Update is called once per frame
    void Update()
    {
        if (isPointDown && repeatScan)
            if (OnPress != null)
                OnPress.Invoke();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (OnPress != null)
            OnPress.Invoke();
        UIButtonScale sc = gameObject.GetComponent<UIButtonScale>();
        if (sc != null)
            sc.OnPress(true);
        isPointDown = true;
        lastInvokeTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointDown = false;
        if (OnRelease != null)
            OnRelease.Invoke();
        UIButtonScale sc = gameObject.GetComponent<UIButtonScale>();
        if (sc != null)
            sc.OnPress(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointDown = false;
    }
}