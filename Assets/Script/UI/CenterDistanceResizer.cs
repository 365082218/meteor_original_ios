using UnityEngine;
using UnityEngine.EventSystems;
public class CenterDistanceResizer : UIBehaviour {
    [SerializeField] private Transform Parent = null;
    [SerializeField] private Color FromLerp = new Color(1, 1, 1, 1);
    [SerializeField] private Color ToLerp = new Color(1, 1, 1, 1);
    [SerializeField] private bool ColorLerp = false;
    [SerializeField] private AnimationCurve AnimationCurve = null;

    public float Scale = 1f;

    private UnityEngine.UI.Text Text = null;
    private float DiffDistance = int.MinValue;
    public float MaxDistance;

    public void SetParent(Transform t) {
        Parent = t;
    }

    protected override void Awake() {
        base.Awake();
    }

    protected override void Start() {
        base.Start();
        Text = gameObject.GetComponentInChildren<UnityEngine.UI.Text>();
        UpdateScale(true);
    }

    protected override void OnEnable() {
        base.OnEnable();
        Canvas.willRenderCanvases += UpdateScale;
    }

    protected override void OnDisable() {
        base.OnDisable();
        Canvas.willRenderCanvases -= UpdateScale;
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        Canvas.willRenderCanvases -= UpdateScale;
    }

    private void UpdateScale() {
        UpdateScale(false);   
    }

    private void UpdateScale(bool force) {
        float distance = Mathf.Abs(Vector3.Distance(transform.position, Parent.position)) / Scale;
        if (distance != DiffDistance || force) {
            DiffDistance = distance;
            float scale = Mathf.Clamp01((MaxDistance - distance) / MaxDistance);
            if (AnimationCurve != null) {
                if (!float.IsNaN(scale)) {
                    scale = AnimationCurve.Evaluate(scale);
                } else {
                    scale = 0f;
                }
            }
            transform.localScale = new Vector3(scale, scale, scale);

            if (Text != null) {
                Color c;
                if (ColorLerp) {
                    c = Color.Lerp(ToLerp, FromLerp, scale);
                } else {
                    c = new Color(Text.color.r, Text.color.g, Text.color.b, scale);
                }

                if (AnimationCurve == null && scale < 0.3f) {
                    c = new Color(Text.color.r, Text.color.g, Text.color.b, 0f);
                }
                Text.color = c;
            }
        }
    }
}