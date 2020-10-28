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
    public UnityEvent OnPressing = new UnityEvent();
    public bool PointDown { get { return isPointDown; } }
    public void Reset()
    {
        isPointDown = false;
    }
    [SerializeField] private bool ChangeColor = false;
    PolygonCollider2D polygonCollider;
    private bool isPointDown = false;
    private float lastInvokeTime;
    // Use this for initialization
    Image Img;
    Color hilight;
    Color normal;
    private void Awake()
    {
        if (ChangeColor)
        {
            Img = GetComponent<Image>();
            hilight = Color.red;
            normal = Img.color;
        }
        polygonCollider = transform.GetComponent<PolygonCollider2D>();
    }

    void Start()
    {
    }

    // Update is called once per frame
    int pointDownFrame;
    void Update()
    {
        if (isPointDown && pointDownFrame != Time.frameCount) {
            if (OnPressing != null) {
                OnPressing.Invoke();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (polygonCollider != null && UICamera.currentCamera != null)
        {
            if (polygonCollider.OverlapPoint(UICamera.currentCamera.ScreenToWorldPoint(eventData.position)))
            {
                if (OnPress != null)
                {
                    if (ChangeColor)
                        Img.color = hilight;
                    OnPress.Invoke();
                }
                UIButtonScale sc = gameObject.GetComponent<UIButtonScale>();
                if (sc != null)
                    sc.OnPress(true);
                isPointDown = true;
                lastInvokeTime = Time.time;
                pointDownFrame = Time.frameCount;
            }
        }
        else
        {
            if (OnPress != null)
            {
                if (ChangeColor)
                    Img.color = hilight;
                OnPress.Invoke();
            }
            UIButtonScale sc = gameObject.GetComponent<UIButtonScale>();
            if (sc != null)
                sc.OnPress(true);
            isPointDown = true;
            lastInvokeTime = Time.time;
            pointDownFrame = Time.frameCount;
        }
    }

    private bool Contains(Vector2[] pVertexs, Vector2 pPoint)
    {
        var crossNumber = 0;

        for (int i = 0, count = pVertexs.Length; i < count; i++)
        {
            var vec1 = pVertexs[i];
            var vec2 = i == count - 1 // 如果当前已到最后一个顶点，则下一个顶点用第一个顶点的数据
                ? pVertexs[0]
                : pVertexs[i + 1];


            if (((vec1.y <= pPoint.y) && (vec2.y > pPoint.y))
                || ((vec1.y > pPoint.y) && (vec2.y <= pPoint.y)))
            {
                if (pPoint.x < vec1.x + (pPoint.y - vec1.y) / (vec2.y - vec1.y) * (vec2.x - vec1.x))
                {
                    crossNumber += 1;
                }
            }
        }
        return (crossNumber & 1) == 1;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isPointDown) {
            isPointDown = false;
            if (OnRelease != null) {
                if (ChangeColor)
                    Img.color = normal;
                OnRelease.Invoke();
            }
            UIButtonScale sc = gameObject.GetComponent<UIButtonScale>();
            if (sc != null)
                sc.OnPress(false);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isPointDown) {
            OnPointerUp(eventData);
        }
        isPointDown = false;
    }
}