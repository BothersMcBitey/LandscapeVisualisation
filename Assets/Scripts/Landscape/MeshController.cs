using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class MeshController : MonoBehaviour
{
    public float d;
    public int s;
    public int n;

    [Range(10, 26000)]
    public int resolution;
    [Header("Y values actually correspond to Z axis")]
    public Vector3 origin;
    public Vector3 size;

    private MeshFilter mf;
    private Mesh mesh;

    // Use this for initialization
    void Start()
    {
        mf = GetComponent<MeshFilter>();
        GenerateMesh();
        UpdateHeight();
        UpdateColor();
        mf.mesh = mesh;
    }

    public void UpdateMesh()
    {
        UpdateHeight();
        UpdateColor();
    }
    
    void UpdateHeight()
    {
        Vector3[] newVs = mesh.vertices;
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            float x = newVs[i].x;
            float z = newVs[i].z;
            float y = 0f;       
            newVs[i] = new Vector3(x, y, z);
        }

        Tuple<int, Vector3>[] ivs = new Tuple<int, Vector3>[newVs.Length];

        for(int i = 0; i < newVs.Length; i++)
        {
            ivs[i] = new Tuple<int, Vector3>(i, newVs[i]);
        }

        float step = Mathf.PI / s;
        float lim = n * 2 * Mathf.PI;
        for (float theta = 0f; theta < lim; theta += step)
        {
            float b = d / (2 * Mathf.PI * n);

            float x = b * theta * Mathf.Cos(theta);
            float z = b * theta * Mathf.Sin(theta);

            Vector3 p = new Vector3(x, 0f, z);

            Tuple<int, Vector3> np = ivs.OrderBy(v => (v.Item2 - p).sqrMagnitude).First();

            Vector3 vertex = np.Item2;

            vertex = new Vector3(vertex.x, ((lim - theta) / lim) * size.y, vertex.z);

            newVs[np.Item1] = vertex;
        }

        

        mesh.vertices = newVs;
    }

    float GetY(float x, float z)
    {


        return 0f;
    }

    void UpdateColor()
    {
        Color[] colors = new Color[mesh.vertexCount];

        float min = float.MaxValue;
        float max = float.MinValue;
        foreach (Vector3 v in mesh.vertices)
        {
            if (v.y < min) min = v.y;
            if (v.y > max) max = v.y;
        }
        Debug.Log(min);
        Debug.Log(max);
        for (int i = 0; i < mesh.vertexCount; i++)
        {

            float y = mesh.vertices[i].y;
            y = (y - min) * (1 / (max-min));

            float h = y * 0.5f;
            h = h - (1f/6f);
            if (h < 0) h = 1f + h;

            colors[i] = Color.HSVToRGB(h, 1f, 1f);
        }

        mesh.colors = colors;
    }

    void GenerateMesh()
    {
        //setup vertex arrays & UVs
        Vector3[] vertices = new Vector3[resolution * resolution];
        Vector2[] uvs = new Vector2[resolution * resolution];

        float stepX = (size.x - origin.x) / resolution;
        float stepZ = (size.z - origin.z) / resolution;

        float x = origin.x;
        //for each row
        for (int i = 0; i < resolution; i++)
        {
            float z = origin.z;
            //for each step ("column")
            for (int j = 0; j < resolution; j++)
            {
                //create vertex
                Vector3 vertex = new Vector3(x, 0f, z);
                Vector2 uv = new Vector2(x, z);

                vertices[i * resolution + j] = vertex;
                uvs[i * resolution + j] = uv;

                z += stepZ;
            }
            x += stepX;
        }

        int[] triangles = new int[3 * 2 * (resolution * resolution)];
        int k = 0;

        //for each pair of rows
        for (int iX = 0; iX < resolution - 1; iX++)
        {
            //for each pair of columns ("square")
            for (int iZ = 0; iZ < resolution - 1; iZ++)
            {
                //create 2 triangles
                int a = iZ + iX * resolution;
                int b = a + 1;
                int d = a + resolution;
                int c = d + 1;

                //triangle 1
                triangles[k] = a;
                k++;
                triangles[k] = b;
                k++;
                triangles[k] = c;
                k++;

                //triangle 2
                triangles[k] = a;
                k++;
                triangles[k] = c;
                k++;
                triangles[k] = d;
                k++;
            }
        }

        mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
    }
}
