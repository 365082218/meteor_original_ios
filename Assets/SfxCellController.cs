using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SfxCellController : AbstractCellController
{

    [SerializeField]
    private UnityEngine.UI.Text SfxText = null;
    [SerializeField]
    private CenterDistanceResizer CenterDistanceResizer = null;

    private Transform ScalingParent = null;

    public override void UpdateCell(AbstractCellData data)
    {
        base.UpdateCell(data);

        int sfx = ((SfxCellData)data).Sfx;


        SfxText.text = SFXLoader.Ins.Eff[sfx];


        Transform distanceResizerParent = transform.parent.parent.parent;
        CenterDistanceResizer.SetParent(distanceResizerParent);
        ScalingParent = distanceResizerParent.parent.parent.parent.parent;
    }

    private void Update()
    {
        if (Data.CellControllerRef != null)
        {
            CenterDistanceResizer.Scale = ScalingParent.localScale.x;
        }
    }
}