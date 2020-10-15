using UnityEngine;
using System.Collections;


// an Editor method to create a cone primitive (so far no end caps)
// the top center is placed at (0/0/0)
// the bottom center is placed at (0/0/length)
// if either one of the radii is 0, the result will be a cone, otherwise a truncated cone
// note you will get inevitable breaks in the smooth shading at cone tips
// note the resulting mesh will be created as an asset in Assets/Editor
// Author: Wolfram Kresse

public class SfxMeshGenerator:Singleton<SfxMeshGenerator>
{
    public int numVertices = 12;
    public float radiusTop = 0f;
    public float radiusBottom = 1f;
    public float length = 1f;
    public float openingAngle = 0f; // if >0, create a cone with this angle by setting radiusTop to 0, and adjust radiusBottom according to length;
    public bool outside = true;
    public bool inside = false;
    //public bool addCollider = false;
    //默认左手坐标系
    public Mesh CreateCylinder(float raidstop, float radisbottom, float height)
    {
        radiusTop = raidstop;
        radiusBottom = radisbottom;
        length = height;
        if (openingAngle > 0 && openingAngle < 180)
        {
            radiusTop = 0;
            radiusBottom = length * Mathf.Tan(openingAngle * Mathf.Deg2Rad / 2);
        }
        //string meshName = newCone.name + numVertices + "v" + radiusTop + "t" + radiusBottom + "b" + length + "l" + length + (outside ? "o" : "") + (inside ? "i" : "");
        //string meshPrefabPath = "Assets/Editor/" + meshName + ".asset";
        Mesh mesh = null;// (Mesh)AssetDatabase.LoadAssetAtPath(meshPrefabPath, typeof(Mesh));
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "MeteorCylinder";
            int multiplier = (outside ? 1 : 0) + (inside ? 1 : 0);
            int offset = (outside && inside ? 2 * numVertices : 0);
            Vector3[] vertices = new Vector3[2 * multiplier * numVertices]; // 0..n-1: top, n..2n-1: bottom
            Vector3[] normals = new Vector3[2 * multiplier * numVertices];
            Vector2[] uvs = new Vector2[2 * multiplier * numVertices];
            int[] tris;
            float slope = Mathf.Atan((radiusBottom - radiusTop) / length); // (rad difference)/height
            float slopeSin = Mathf.Sin(slope);
            float slopeCos = Mathf.Cos(slope);
            int i;

            for (i = 0; i < numVertices; i++)
            {
                float angle = 2 * Mathf.PI * i / numVertices;
                float angleSin = Mathf.Sin(angle);
                float angleCos = Mathf.Cos(angle);
                float angleHalf = 2 * Mathf.PI * (i + 0.5f) / numVertices; // for degenerated normals at cone tips
                float angleHalfSin = Mathf.Sin(angleHalf);
                float angleHalfCos = Mathf.Cos(angleHalf);

                vertices[i] = new Vector3((radiusTop * angleCos), (radiusTop * angleSin), 0);
                vertices[i + numVertices] = new Vector3((radiusBottom * angleCos), (radiusBottom * angleSin), length);

                if (radiusTop == 0)
                    normals[i] = new Vector3((angleHalfCos * slopeCos), (angleHalfSin * slopeCos), -1 * slopeSin);
                else
                    normals[i] = new Vector3((angleCos * slopeCos), (angleSin * slopeCos), -1 * slopeSin);
                if (radiusBottom == 0)
                    normals[i + numVertices] = new Vector3((angleHalfCos * slopeCos), (angleHalfSin * slopeCos), -1 * slopeSin);
                else
                    normals[i + numVertices] = new Vector3((angleCos * slopeCos), (angleSin * slopeCos), -1 * slopeSin);

                uvs[i] = new Vector2(1.0f * i / numVertices, 1);
                uvs[i + numVertices] = new Vector2(1.0f * i / numVertices, 0);

                if (outside && inside)
                {
                    // vertices and uvs are identical on inside and outside, so just copy
                    vertices[i + 2 * numVertices] = vertices[i];
                    vertices[i + 3 * numVertices] = vertices[i + numVertices];
                    uvs[i + 2 * numVertices] = uvs[i];
                    uvs[i + 3 * numVertices] = uvs[i + numVertices];
                }
                if (inside)
                {
                    // invert normals
                    normals[i + offset] = -normals[i];
                    normals[i + numVertices + offset] = -normals[i + numVertices];
                }
            }

            //把模型绕X轴旋转270度
            if (true)
            {
                Vector3[] vec = new Vector3[vertices.Length];
                for (int j = 0; j < vertices.Length; j++)
                {
                    vec[j] = new Quaternion(0.7170f, 0, 0, -0.7170f) * vertices[j];
                    //vec[j] = new Vector3(vertices[j].x, vertices[j].z, vertices[j].y);
                }
                mesh.vertices = vec;
            }
            //else
            //    mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;

            // create triangles
            // here we need to take care of point order, depending on inside and outside
            int cnt = 0;
            if (radiusTop == 0)
            {
                // top cone
                tris = new int[numVertices * 3 * multiplier];
                if (outside)
                    for (i = 0; i < numVertices; i++)
                    {
                        tris[cnt++] = i + numVertices;
                        tris[cnt++] = i;
                        if (i == numVertices - 1)
                            tris[cnt++] = numVertices;
                        else
                            tris[cnt++] = i + 1 + numVertices;
                    }
                if (inside)
                    for (i = offset; i < numVertices + offset; i++)
                    {
                        tris[cnt++] = i;
                        tris[cnt++] = i + numVertices;
                        if (i == numVertices - 1 + offset)
                            tris[cnt++] = numVertices + offset;
                        else
                            tris[cnt++] = i + 1 + numVertices;
                    }
            }
            else if (radiusBottom == 0)
            {
                // bottom cone
                tris = new int[numVertices * 3 * multiplier];
                if (outside)
                    for (i = 0; i < numVertices; i++)
                    {
                        tris[cnt++] = i;
                        if (i == numVertices - 1)
                            tris[cnt++] = 0;
                        else
                            tris[cnt++] = i + 1;
                        tris[cnt++] = i + numVertices;
                    }
                if (inside)
                    for (i = offset; i < numVertices + offset; i++)
                    {
                        if (i == numVertices - 1 + offset)
                            tris[cnt++] = offset;
                        else
                            tris[cnt++] = i + 1;
                        tris[cnt++] = i;
                        tris[cnt++] = i + numVertices;
                    }
            }
            else
            {
                // truncated cone
                tris = new int[numVertices * 6 * multiplier];
                if (outside)
                    for (i = 0; i < numVertices; i++)
                    {
                        int ip1 = i + 1;
                        if (ip1 == numVertices)
                            ip1 = 0;

                        tris[cnt++] = i;
                        tris[cnt++] = ip1;
                        tris[cnt++] = i + numVertices;

                        tris[cnt++] = ip1 + numVertices;
                        tris[cnt++] = i + numVertices;
                        tris[cnt++] = ip1;
                    }
                if (inside)
                    for (i = offset; i < numVertices + offset; i++)
                    {
                        int ip1 = i + 1;
                        if (ip1 == numVertices + offset)
                            ip1 = offset;

                        tris[cnt++] = ip1;
                        tris[cnt++] = i;
                        tris[cnt++] = i + numVertices;

                        tris[cnt++] = i + numVertices;
                        tris[cnt++] = ip1 + numVertices;
                        tris[cnt++] = ip1;
                    }
            }
            mesh.triangles = tris;
        }
        
        return mesh;
    }

    public Mesh CreateSphere(float radius)
    {
        Mesh mesh;
        Vector3[] vertices;
        Vector2[] uv;
        int[] triangles;
        Vector3[] normals;
        mesh = new Mesh();
        int m = 12; //row    
        int n = 12;  //col    
        float width = 1;
        float height = 1;
        vertices = new Vector3[(m + 1) * (n + 1)];//the positions of vertices    
        uv = new Vector2[(m + 1) * (n + 1)];
        normals = new Vector3[(m + 1) * (n + 1)];
        triangles = new int[6 * m * n];
        for (int i = 0; i < vertices.Length; i++)
        {
            float x = i % (n + 1);
            float y = i / (n + 1);
            float x_pos = x / n * width;
            float y_pos = y / m * height;
            vertices[i] = new Vector3(x_pos, y_pos, 0);
            float u = x / n;
            float v = y / m;
            uv[i] = new Vector2(u, v);
        }
        for (int i = 0; i < 2 * m * n; i++)
        {
            int[] triIndex = new int[3];
            if (i % 2 == 0)
            {
                triIndex[0] = i / 2 + i / (2 * n);
                triIndex[1] = triIndex[0] + 1;
                triIndex[2] = triIndex[0] + (n + 1);
            }
            else
            {
                triIndex[0] = (i + 1) / 2 + i / (2 * n);
                triIndex[1] = triIndex[0] + (n + 1);
                triIndex[2] = triIndex[1] - 1;

            }
            triangles[i * 3] = triIndex[0];
            triangles[i * 3 + 1] = triIndex[1];
            triangles[i * 3 + 2] = triIndex[2];
        }

        float r = radius;
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v;
            v.x = (r * Mathf.Cos(vertices[i].x / width * 2 * Mathf.PI) * Mathf.Cos(vertices[i].y / height * Mathf.PI - Mathf.PI / 2));
            v.y = (r * Mathf.Sin(vertices[i].x / width * 2 * Mathf.PI) * Mathf.Cos(vertices[i].y / height * Mathf.PI - Mathf.PI / 2));
            v.z = (r * Mathf.Sin(vertices[i].y / height * Mathf.PI - Mathf.PI / 2));
            //v = vertices[i];  
            vertices[i] = v;
            normals[i] = new Vector3(0, 1, 0);
        }

        if (true)
        {
            Vector3[] vec = new Vector3[vertices.Length];
            for (int j = 0; j < vertices.Length; j++)
            {
                vec[j] = new Quaternion(0.7170f, 0, 0, -0.7170f) * vertices[j];//绕x转270
            }
            mesh.vertices = vec;
        }

        mesh.normals = normals;
        mesh.uv = uv;
        mesh.triangles = triangles;
        return mesh;
    }

    public Mesh CreateBox(float width, float height, float depth)
    {
        //Mesh mesh = new Mesh();
        //float halfLong = depth / 2;
        //float halfHeight = height / 2;
        //float halfWidth = width / 2;
        //Vector3[] pVector = new Vector3[24];
        //int [] pTriangles = new int[pVector.Length];

        ////forword
        //pVector[0] = new Vector3(-halfLong, halfHeight, -halfWidth);
        //pVector[1] = new Vector3(0.0f, halfHeight, 0.0f);
        //pVector[2] = new Vector3(halfWidth, 0.0f, 0.0f);

        //pVector[3] = new Vector3(halfWidth, 0.0f, 0.0f);
        //pVector[4] = new Vector3(0.0f, halfHeight, 0.0f);
        //pVector[5] = new Vector3(halfWidth, halfHeight, 0.0f);
        ////back
        //pVector[6] = new Vector3(0.0f, 0.0f, halfLong);
        //pVector[7] = new Vector3(0.0f, halfHeight, halfLong);
        //pVector[8] = new Vector3(halfWidth, 0.0f, halfLong);

        //pVector[9] = new Vector3(halfWidth, 0.0f, halfLong);
        //pVector[10] = new Vector3(0.0f, halfHeight, halfLong);
        //pVector[11] = new Vector3(halfWidth, halfHeight, halfLong);
        ////left
        //pVector[12] = new Vector3(0.0f, 0.0f, 0.0f);
        //pVector[13] = new Vector3(0.0f, 0.0f, 1.0f);
        //pVector[14] = new Vector3(0.0f, 1.0f, 1.0f);

        //pVector[15] = new Vector3(0.0f, 1.0f, 1.0f);
        //pVector[16] = new Vector3(0.0f, 1.0f, 0.0f);
        //pVector[17] = new Vector3(0.0f, 0.0f, 0.0f);
        ////right
        //pVector[18] = new Vector3(1.0f, 0.0f, 0.0f);
        //pVector[19] = new Vector3(1.0f, 0.0f, 1.0f);
        //pVector[20] = new Vector3(1.0f, 1.0f, 0.0f);

        //pVector[21] = new Vector3(1.0f, 1.0f, 0.0f);
        //pVector[22] = new Vector3(1.0f, 0.0f, 1.0f);
        //pVector[23] = new Vector3(1.0f, 1.0f, 1.0f);

        //for (int nIndex = 0; nIndex < pTriangles.length; ++nIndex){
        //    pTriangles[nIndex] = nIndex;
        //}
        //pMesh.Clear();
        //pMesh.vertices = pVector;//网格顶点
        //pMesh.triangles = pTriangles;//三角形
        //pMesh.RecalculateBounds();
        //GameObject
        //return mesh;
        return null;
    }

    public Mesh CreatePlane(float width, float height)
    {
        Mesh m = new Mesh();
        width = width + 1;
        height = height + 1;
        m.vertices = new Vector3[] {new Vector3(-(width / 2), (height / 2), 0), new Vector3((width/2), (height/2), 0), new Vector3((width/2), -(height/2), 0), new Vector3(-(width/2), -(height/2), 0)};
        m.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        m.normals = new Vector3[] { new Vector3(0, 0, -1), new Vector3(0, 0, -1), new Vector3(0, 0, -1) , new Vector3(0, 0, -1) };
        m.uv = new Vector2[] { new Vector2(0,1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0)};
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }
}
