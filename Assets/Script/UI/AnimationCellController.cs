using UnityEngine;
public class AnimationCellController : AbstractCellController {

    [SerializeField] private UnityEngine.UI.Text AnimationText = null;
    [SerializeField] private CenterDistanceResizer CenterDistanceResizer = null;

    private Transform ScalingParent = null;

    public override void UpdateCell(AbstractCellData data) {
        base.UpdateCell(data);

        int act = ((AnimationCellData) data).animation;


        AnimationText.text = act.ToString();


        Transform distanceResizerParent = transform.parent.parent.parent;
        CenterDistanceResizer.SetParent(distanceResizerParent);
        ScalingParent = distanceResizerParent.parent.parent.parent.parent;
    }

    private void Update() {
        if (Data.CellControllerRef != null) {
            CenterDistanceResizer.Scale = ScalingParent.localScale.x;
        }
    }
}

