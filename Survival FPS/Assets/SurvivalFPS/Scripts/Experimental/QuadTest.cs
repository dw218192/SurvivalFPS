using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBuilder
{
    private List<Vector3> m_Vertices = new List<Vector3>();
    public List<Vector3> Vertices { get { return m_Vertices; } }

    private List<Vector3> m_Normals = new List<Vector3>();
    public List<Vector3> Normals { get { return m_Normals; } }

    private List<Vector2> m_UVs = new List<Vector2>();
    public List<Vector2> UVs { get { return m_UVs; } }

    private List<int> m_Indices = new List<int>();

    public void AddTriangle(int index0, int index1, int index2)
    {
        m_Indices.Add(index0);
        m_Indices.Add(index1);
        m_Indices.Add(index2);
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = m_Vertices.ToArray();
        mesh.triangles = m_Indices.ToArray();

        //Normals are optional. Only use them if we have the correct amount:
        if (m_Normals.Count == m_Vertices.Count)
            mesh.normals = m_Normals.ToArray();

        //UVs are optional. Only use them if we have the correct amount:
        if (m_UVs.Count == m_Vertices.Count)
            mesh.uv = m_UVs.ToArray();

        mesh.RecalculateBounds();

        return mesh;
    }
}

public class QuadTest : MonoBehaviour
{
    [SerializeField] int m_SegmentCnt;
    [SerializeField] int m_Height;
    float m_Length = 1, m_Width = 1;

    Vector3[] vertices = new Vector3[4];
    Vector3[] normals = new Vector3[4];
    Vector2[] uv = new Vector2[4];
    int[] indices = new int[6]; //2 triangles, 3 indices each

    void BuildQuadForGrid(MeshBuilder meshBuilder, Vector3 position, Vector2 uv, 
                          bool buildTriangles, int vertsPerRow)
    {
        meshBuilder.Vertices.Add(position);
        meshBuilder.UVs.Add(uv);

        if (buildTriangles)
        {
            int baseIndex = meshBuilder.Vertices.Count - 1;

            int index0 = baseIndex;
            int index1 = baseIndex - 1;
            int index2 = baseIndex - vertsPerRow;
            int index3 = baseIndex - vertsPerRow - 1;

            meshBuilder.AddTriangle(index0, index2, index1);
            meshBuilder.AddTriangle(index2, index3, index1);
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
