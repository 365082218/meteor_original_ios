using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Lightbeam : MonoBehaviour
{
    public bool IsModifyingMesh = false;
    public Material DefaultMaterial;

    public LightbeamSettings Settings;

    public float RadiusTop { get { return Settings.RadiusTop; } set { Settings.RadiusTop = value; } }
    public float RadiusBottom { get { return Settings.RadiusBottom; } set { Settings.RadiusBottom = value; } }
    public float Length { get { return Settings.Length; } set { Settings.Length = value; } }
    public int Subdivisions { get { return Settings.Subdivisions; } set { Settings.Subdivisions = value; } }
    public int SubdivisionsHeight { get { return Settings.SubdivisionsHeight; } set { Settings.SubdivisionsHeight = value; } }
    public void GenerateBeam() 
    {
        MeshFilter meshFilter = this.GetComponent<MeshFilter>();

        // generate two cylinders, one of them with reversed normals. Combine them.
        CombineInstance[] combineInstance = new CombineInstance[2];
        combineInstance[0].mesh = GenerateMesh(false);
        combineInstance[0].transform = Matrix4x4.identity;
        combineInstance[1].mesh = GenerateMesh(true);
        combineInstance[1].transform = Matrix4x4.identity;
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combineInstance);

        if (meshFilter.sharedMesh == null)
            meshFilter.sharedMesh = new Mesh();

        meshFilter.sharedMesh.Clear();
        meshFilter.sharedMesh.vertices = combinedMesh.vertices;
        meshFilter.sharedMesh.uv = combinedMesh.uv;
        meshFilter.sharedMesh.triangles = combinedMesh.triangles;
        meshFilter.sharedMesh.tangents = combinedMesh.tangents;
        meshFilter.sharedMesh.normals = combinedMesh.normals;
    }

    private Mesh GenerateMesh(bool reverseNormals)
    {
        // vertCount = subdiv * the subdiv height
        int vertexCount = (Settings.Subdivisions * (Settings.SubdivisionsHeight + 1));
        // add verticies for the uv seem
        vertexCount += Settings.SubdivisionsHeight + 1; 
        Vector3[] newVerticies = new Vector3[vertexCount];
        Vector2[] newUVs = new Vector2[vertexCount];
        Vector3[] newNormals = new Vector3[vertexCount];

        int triangleCount = (((Settings.Subdivisions * 2) * Settings.SubdivisionsHeight) * 3);
        int[] newTriangles = new int[triangleCount];

        // verticies' position and uv
        int subDivHeightTrue = Settings.SubdivisionsHeight + 1;
        float angle = (Mathf.PI * 2) / Settings.Subdivisions;
        float lengthFrac = Settings.Length / Settings.SubdivisionsHeight;
        float uvU = 1.0f / Settings.Subdivisions;
        float uvV = 1.0f / Settings.SubdivisionsHeight;
        for (int i = 0; i < Settings.Subdivisions + 1; i++)
        {
            float xAngle = Mathf.Cos(i * angle);
            float yAngle = Mathf.Sin(i * angle);

            Vector3 vertex1 = CalculateVertex(lengthFrac, xAngle, yAngle, 0, Settings.RadiusTop);
            Vector3 vertex2 = CalculateVertex(lengthFrac, xAngle, yAngle, subDivHeightTrue - 1, Settings.RadiusBottom);
            Vector3 dir = vertex2 - vertex1;

            for (int j = 0; j < subDivHeightTrue; j++)
            {
                // calculate vertex position (polar coordinates)
                float radius = Mathf.Lerp(Settings.RadiusTop, Settings.RadiusBottom, uvV * j);
                Vector3 vertex = CalculateVertex(lengthFrac, xAngle, yAngle, j, radius);

                Vector3 normal = Vector3.Cross(dir.normalized, new Vector3(vertex.x, 0, vertex.z).normalized);
                if (reverseNormals)
                    normal = Vector3.Cross(dir.normalized, normal.normalized);
                else
                    normal = Vector3.Cross(normal.normalized, dir.normalized);

                int index = (i * subDivHeightTrue) + j;
                newVerticies[index] = vertex;
                newUVs[index] = new Vector2(uvU * i, 1 - uvV * j);
                newNormals[index] = normal.normalized;
                newUVs[index] = new Vector2(uvU * i, 1 - uvV * j);
            }
        }

        int triangleIndex = 0;
        for (int i = 0; i < Settings.Subdivisions; i++)
        {
            for (int j = 0; j < subDivHeightTrue - 1; j++)
            {
                // first triangle
                int index1 = (i * subDivHeightTrue) + j;
                int index2 = index1 + 1;
                int index3 = index1 + subDivHeightTrue;
                // wrap if out of bounds
                if (index3 >= vertexCount)
                    index3 = index3 % vertexCount;

                if (reverseNormals)
                {
                    newTriangles[triangleIndex++] = index1;
                    newTriangles[triangleIndex++] = index2;
                    newTriangles[triangleIndex++] = index3;
                }
                else
                {
                    newTriangles[triangleIndex++] = index2;
                    newTriangles[triangleIndex++] = index1;
                    newTriangles[triangleIndex++] = index3;                
                }

                // second triangle
                int newindex1 = index1 + 1;
                int newindex2 = index1 + subDivHeightTrue;
                // wrap if out of bounds
                if (newindex2 >= vertexCount)
                    newindex2 = newindex2 % vertexCount;
                int newindex3 = newindex2 + 1;

                if (reverseNormals)
                {
                    newTriangles[triangleIndex++] = newindex1;
                    newTriangles[triangleIndex++] = newindex3;
                    newTriangles[triangleIndex++] = newindex2;
                }
                else
                {
                    newTriangles[triangleIndex++] = newindex1;
                    newTriangles[triangleIndex++] = newindex2;
                    newTriangles[triangleIndex++] = newindex3;             
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.Clear();
        mesh.vertices = newVerticies;
        mesh.uv = newUVs;
        mesh.triangles = newTriangles;
        mesh.normals = newNormals;
        mesh.RecalculateBounds();
        ;
        CalculateMeshTangents(mesh);

        return mesh;
    }

    private static Vector3 CalculateVertex(float lengthFrac, float xAngle, float yAngle, int j, float radius)
    {
        float x = radius * xAngle;
        float z = radius * yAngle;

        Vector3 vertex = new Vector3(x, j * (lengthFrac * -1), z);
        return vertex;
    }

    private static void CalculateMeshTangents(Mesh mesh)
    {
        //speed up math by copying the mesh arrays
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        Vector2[] uv = mesh.uv;
        Vector3[] normals = mesh.normals;

        //variable definitions
        int triangleCount = triangles.Length;
        int vertexCount = vertices.Length;

        Vector3[] tan1 = new Vector3[vertexCount];
        Vector3[] tan2 = new Vector3[vertexCount];

        Vector4[] tangents = new Vector4[vertexCount];

        for (long a = 0; a < triangleCount; a += 3)
        {
            long i1 = triangles[a + 0];
            long i2 = triangles[a + 1];
            long i3 = triangles[a + 2];

            Vector3 v1 = vertices[i1];
            Vector3 v2 = vertices[i2];
            Vector3 v3 = vertices[i3];

            Vector2 w1 = uv[i1];
            Vector2 w2 = uv[i2];
            Vector2 w3 = uv[i3];

            float x1 = v2.x - v1.x;
            float x2 = v3.x - v1.x;
            float y1 = v2.y - v1.y;
            float y2 = v3.y - v1.y;
            float z1 = v2.z - v1.z;
            float z2 = v3.z - v1.z;

            float s1 = w2.x - w1.x;
            float s2 = w3.x - w1.x;
            float t1 = w2.y - w1.y;
            float t2 = w3.y - w1.y;

            float r = 1.0f / (s1 * t2 - s2 * t1);

            Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
            Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

            tan1[i1] += sdir;
            tan1[i2] += sdir;
            tan1[i3] += sdir;

            tan2[i1] += tdir;
            tan2[i2] += tdir;
            tan2[i3] += tdir;
        }

        for (long a = 0; a < vertexCount; ++a)
        {
            Vector3 n = normals[a];
            Vector3 t = tan1[a];

            Vector3.OrthoNormalize(ref n, ref t);
            tangents[a].x = t.x;
            tangents[a].y = t.y;
            tangents[a].z = t.z;

            tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
        }
        mesh.tangents = tangents;
    }
}
