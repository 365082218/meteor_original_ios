using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/Gradient")]
public class Gradient : BaseMeshEffect
{
    [SerializeField]
    private Color32 topColor = Color.white;
    [SerializeField]
    private Color32 bottomColor = new Color32(255, 153, 0, 255);
    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
        {
            return;
        }

        var count = vh.currentVertCount;
        if (count == 0)
            return;

        var vertexs = new List<UIVertex>();
        for (var i = 0; i < count; i++)
        {
            var vertex = new UIVertex();
            vh.PopulateUIVertex(ref vertex, i);
            vertexs.Add(vertex);
        }

        var topY = vertexs[0].position.y;
        var bottomY = vertexs[0].position.y;
        int line = 0;
        Dictionary<int, float> lineBottom = new Dictionary<int, float>();
        for (var i = 1; i < count; i++)
        {
            var y = vertexs[i].position.y;
            if (y > topY)
            {
                topY = y;
            }
            else if (y < bottomY)
            {
                bottomY = y;
                lineBottom[line++] = bottomY;
            }
        }
        for (int i = 0; i < vertexs.Count;)
        {
            ChangeColor(ref vertexs, i, topColor);
            ChangeColor(ref vertexs, i + 1, topColor);
            ChangeColor(ref vertexs, i + 2, bottomColor);
            ChangeColor(ref vertexs, i + 3, bottomColor);
            i += 4;
        }
        for (var i = 0; i < count; i++)
            vh.SetUIVertex(vertexs[i], i);
    }
    private void ChangeColor(ref List<UIVertex> verList, int index, Color color)
    {
        if (verList.Count <= index)
            return;
        UIVertex temp = verList[index];
        temp.color = color;
        verList[index] = temp;
    }
}
