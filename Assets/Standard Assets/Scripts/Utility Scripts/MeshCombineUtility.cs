using UnityEngine;
using System.Collections;

public class MeshCombineUtility {
	
	public struct MeshInstance
	{
		public Mesh      mesh;
		public int       subMeshIndex;            
		public Matrix4x4 transform;
        public int       childIdx;
	}
	
    public class MeshElement
    {
        public Vector3[] vertices;
        public Vector3[] normals;
        public Vector4[] tangents;
        public Vector2[] uv;
        public Vector2[] uv1;
        public Color[] colors;
        public int[] triangles;
        public int[] strip;
        public Mesh mesh;
    }

	public static Mesh Combine (MeshInstance[] combines, bool generateStrips, ref MeshElement element, bool save = false)
	{
		int vertexCount = 0;
		int triangleCount = 0;
		int stripCount = 0;
		foreach( MeshInstance combine in combines )
		{
			if (combine.mesh)
			{
				vertexCount += combine.mesh.vertexCount;
				
				if (generateStrips)
				{
					// SUBOPTIMAL FOR PERFORMANCE
					int curStripCount = combine.mesh.GetTriangles(combine.subMeshIndex).Length;
					if (curStripCount != 0)
					{
						if( stripCount != 0 )
						{
							if ((stripCount & 1) == 1 )
								stripCount += 3;
							else
								stripCount += 2;
						}
						stripCount += curStripCount;
					}
					else
					{
						generateStrips = false;
					}
				}
			}
		}
		
		// Precomputed how many triangles we need instead
		if (!generateStrips)
		{
			foreach( MeshInstance combine in combines )
			{
				if (combine.mesh)
				{
					triangleCount += combine.mesh.GetTriangles(combine.subMeshIndex).Length;
				}
			}
		}

        if (save)
        {
            if (element == null)
            {
                element = new MeshElement();
                element.vertices = new Vector3[vertexCount];
                element.normals = new Vector3[vertexCount];
                element.tangents = new Vector4[vertexCount];
                element.uv = new Vector2[vertexCount];
                element.uv1 = new Vector2[vertexCount];
                element.colors = new Color[vertexCount];
                element.triangles = new int[triangleCount];
                element.strip = new int[stripCount];
                element.mesh = new Mesh();
            }
        }

        Vector3[] vertices = save ? element.vertices:new Vector3[vertexCount];
        Vector3[] normals = save ? element.normals : new Vector3[vertexCount];
        Vector4[] tangents = save ? element.tangents: new Vector4[vertexCount];
        Vector2[] uv = save ? element.uv :new Vector2[vertexCount];
        Vector2[] uv1 = save ? element.uv1:new Vector2[vertexCount];
        Color[] colors = save ? element.colors:new Color[vertexCount];
        int[] triangles = save ? element.triangles :new int[triangleCount];
        int[] strip = save ? element.strip :new int[stripCount];


        int offset;
		
		offset=0;
		foreach( MeshInstance combine in combines )
		{
			if (combine.mesh)
				Copy(combine.mesh.vertexCount, combine.mesh.vertices, vertices, ref offset, combine.transform);
		}

		offset=0;
		foreach( MeshInstance combine in combines )
		{
			if (combine.mesh)
			{
				Matrix4x4 invTranspose = combine.transform;
				invTranspose = invTranspose.inverse.transpose;
				CopyNormal(combine.mesh.vertexCount, combine.mesh.normals, normals, ref offset, invTranspose);
			}
				
		}
		offset=0;
		foreach( MeshInstance combine in combines )
		{
			if (combine.mesh)
			{
				Matrix4x4 invTranspose = combine.transform;
				invTranspose = invTranspose.inverse.transpose;
				CopyTangents(combine.mesh.vertexCount, combine.mesh.tangents, tangents, ref offset, invTranspose);
			}
				
		}
		offset=0;
		foreach( MeshInstance combine in combines )
		{
			if (combine.mesh)
				Copy(combine.mesh.vertexCount, combine.mesh.uv, uv, ref offset);
		}
		
		offset=0;
		foreach( MeshInstance combine in combines )
		{
			if (combine.mesh)
				Copy(combine.mesh.vertexCount, combine.mesh.uv2, uv1, ref offset);
		}
		
		offset=0;
		foreach( MeshInstance combine in combines )
		{
			if (combine.mesh)
				CopyColors(combine.mesh.vertexCount, combine.mesh.colors, colors, ref offset);
		}
		
		int triangleOffset=0;
		int stripOffset=0;
		int vertexOffset=0;
		foreach( MeshInstance combine in combines )
		{
			if (combine.mesh)
			{
				if (generateStrips)
				{
					int[] inputstrip = combine.mesh.GetTriangles(combine.subMeshIndex);
					if (stripOffset != 0)
					{
						if ((stripOffset & 1) == 1)
						{
                            strip[stripOffset+0] = strip[stripOffset-1];
                            strip[stripOffset+1] = inputstrip[0] + vertexOffset;
                            strip[stripOffset+2] = inputstrip[0] + vertexOffset;
							stripOffset+=3;
						}
						else
						{
                            strip[stripOffset+0] = strip[stripOffset-1];
                            strip[stripOffset+1] = inputstrip[0] + vertexOffset;
							stripOffset+=2;
						}
					}
					
					for (int i=0;i<inputstrip.Length;i++)
					{
                        strip[i+stripOffset] = inputstrip[i] + vertexOffset;
					}
					stripOffset += inputstrip.Length;
				}
				else
				{
					int[]  inputtriangles = combine.mesh.GetTriangles(combine.subMeshIndex);
					for (int i=0;i<inputtriangles.Length;i++)
					{
                        triangles[i+triangleOffset] = inputtriangles[i] + vertexOffset;
					}
					triangleOffset += inputtriangles.Length;
				}
				
				vertexOffset += combine.mesh.vertexCount;
			}
		}
		
		Mesh mesh = save ? element.mesh : new Mesh();
        if (!save)
		    mesh.name = "Combined Mesh";
		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.colors = colors;
		mesh.uv = uv;
		mesh.uv2 = uv1;
		mesh.tangents = tangents;
		if (generateStrips)
			mesh.SetTriangles(strip, 0);
		else
			mesh.triangles = triangles;
		return mesh;
	}
	
	static void Copy (int vertexcount, Vector3[] src, Vector3[] dst, ref int offset, Matrix4x4 transform)
	{
		for (int i=0;i<src.Length;i++)
			dst[i+offset] = transform.MultiplyPoint(src[i]);
		offset += vertexcount;
	}

	static void CopyNormal (int vertexcount, Vector3[] src, Vector3[] dst, ref int offset, Matrix4x4 transform)
	{
		for (int i=0;i<src.Length;i++)
			dst[i+offset] = transform.MultiplyVector(src[i]).normalized;
		offset += vertexcount;
	}

	static void Copy (int vertexcount, Vector2[] src, Vector2[] dst, ref int offset)
	{
		for (int i=0;i<src.Length;i++)
			dst[i+offset] = src[i];
		offset += vertexcount;
	}

	static void CopyColors (int vertexcount, Color[] src, Color[] dst, ref int offset)
	{
		for (int i=0;i<src.Length;i++)
			dst[i+offset] = src[i];
		offset += vertexcount;
	}

    static Vector3 p;
	static void CopyTangents (int vertexcount, Vector4[] src, Vector4[] dst, ref int offset, Matrix4x4 transform)
	{
		for (int i=0;i<src.Length;i++)
		{
			//Vector4 p4 = src[i];
            p.x = src[i].x;
            p.y = src[i].y;
            p.z = src[i].z;
			p = transform.MultiplyVector(p).normalized;
            dst[i + offset].x = p.x;// = new Vector4(p.x, p.y, p.z, p4.w);
            dst[i + offset].y = p.y;
            dst[i + offset].z = p.z;
            dst[i + offset].w = src[i].w;
        }
			
		offset += vertexcount;
	}
}
