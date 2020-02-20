using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class FractalTerrain : MonoBehaviour
{
    // test
    Mesh mesh;
    ArrayList vertexLocList; // Vector3
    ArrayList triangleList; // Trianlge
    ArrayList triangleIdxList; // int
 
    Vector3[] vertexArr;
    int[] triangleIdxArr;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        vertexLocList = new ArrayList();
        triangleList = new ArrayList();
        triangleIdxList = new ArrayList();
        GetComponent<MeshFilter>().mesh = mesh;
        CreateMesh();
        UpdateMesh();
    }

    // Update is called once per frame
    void Update() {
    }

    void CreateMesh()
    {
        // Test points
        Vector3 v0 = new Vector3(-1, 0, 0);
        Vector3 v1 = new Vector3(0, 0, 1);
        Vector3 v2 = new Vector3(1, 0, 0);
        int v0idx = 0;
        int v1idx = 1;
        int v2idx = 2;

        vertexLocList.Add(v0);
        vertexLocList.Add(v1);
        vertexLocList.Add(v2);

        triangleIdxList.Add(v0idx);
        triangleIdxList.Add(v1idx);
        triangleIdxList.Add(v2idx);

        Triangle t = new Triangle(v0, v1, v2, v0idx, v1idx, v2idx);
        ArrayList subTriangleList = t.Subdivide(ref vertexLocList);

        for (int i = 0; i < subTriangleList.Count; i++)
        {
            ((Triangle)subTriangleList[i]).AddTriangleVertexIndices(ref triangleIdxList, ref triangleList);
        }

        ArrayList subTriangleList1 = ((Triangle)subTriangleList[subTriangleList.Count - 1]).Subdivide(ref vertexLocList);
        for (int i = 0; i < subTriangleList1.Count; i++)
        {
            ((Triangle)subTriangleList1[i]).AddTriangleVertexIndices(ref triangleIdxList, ref triangleList);
        }

        TransferVerticesFromListToArray();
        TransferIndicesFromListToArray();
    }

    void TransferVerticesFromListToArray()
    {
        vertexArr = new Vector3[vertexLocList.Count];
        for (int i = 0; i < vertexLocList.Count; i++)
        {
            vertexArr[i] = (Vector3) vertexLocList[i];
        }
    }

    void TransferIndicesFromListToArray()
    { 
        triangleIdxArr = new int[triangleIdxList.Count];
        for (int i = 0; i < triangleIdxList.Count; i++)
        {
            triangleIdxArr[i] = (int) triangleIdxList[i];
        }
    }
    

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertexArr;
        mesh.triangles = triangleIdxArr;
        mesh.RecalculateNormals();
    }
}
