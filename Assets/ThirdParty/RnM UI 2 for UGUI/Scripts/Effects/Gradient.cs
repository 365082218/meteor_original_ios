using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI
{
	[AddComponentMenu("UI/Effects/Gradient")]
#if UNITY_2017 || UNITY_5_5 || UNITY_5_6 || UNITY_2020
    public class Gradient : BaseMeshEffect
#else
    public class Gradient : BaseVertexEffect
#endif
    {
        [SerializeField] private Color topColor = Color.white;
		[SerializeField] private Color bottomColor = Color.black;

#if UNITY_2017 || UNITY_5_5 || UNITY_5_6 || UNITY_2020
        public override void ModifyMesh(VertexHelper vertexHelper)
        {
            if (!this.IsActive())
                return;

            List<UIVertex> list = new List<UIVertex>();
            vertexHelper.GetUIVertexStream(list);

            ModifyVertices(list);  // calls the old ModifyVertices which was used on pre 5.2

			vertexHelper.Clear();
            vertexHelper.AddUIVertexTriangleStream(list);
        }
#endif

#if UNITY_2017 || UNITY_5_5 || UNITY_5_6 || UNITY_2020
        public void ModifyVertices(List<UIVertex> vertexList)
#else
        public override void ModifyVertices(List<UIVertex> vertexList)
#endif
        {
            if (!this.IsActive())
				return;
			
			int count = vertexList.Count;
			float bottomY = vertexList[0].position.y;
			float topY = vertexList[0].position.y;
			
			for (int i = 1; i < count; i++)
			{
				float y = vertexList[i].position.y;
				if (y > topY)
				{
					topY = y;
				}
				else if (y < bottomY)
				{
					bottomY = y;
				}
			}
			
			float uiElementHeight = topY - bottomY;
			
			for (int i = 0; i < count; i++)
			{
				UIVertex uiVertex = vertexList[i];
				uiVertex.color = uiVertex.color * Color.Lerp(bottomColor, topColor, (uiVertex.position.y - bottomY) / uiElementHeight);
				vertexList[i] = uiVertex;
			}
		}
	}
}