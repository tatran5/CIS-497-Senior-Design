using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class FractalTerrain : MonoBehaviour
{
    Mesh mesh;
    List<Vector3> vertexList;
    List<int> vertexIdxList; // Generate a triangle for every 3 elements 3i, 3i + 1, 3i + 2
    Vector3[] vertexArr;
    int[] vertexIdxArr;

    HashSet<Triangle> triangles; // test triangles
    HashSet<Square> squares; // test squares

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        vertexList = new List<Vector3>();
        vertexIdxList = new List<int>();

        //test
        triangles = new HashSet<Triangle>();
        squares = new HashSet<Square>();
        
        GetComponent<MeshFilter>().mesh = mesh;
        CreateMeshSquares();
        UpdateMesh();
    }

    // Update is called once per frame
    void Update() {
    }

    void CreateMeshSquares()
    {
        // Test points
        vertexList.Add(new Vector3(0, 0, 1));
        vertexList.Add(new Vector3(1, 0, 1));
        vertexList.Add(new Vector3(1, 0, 0));
        vertexList.Add(new Vector3(0, 0, 0));

        Square s0 = new Square(0, 1, 2, 3, 0, squares);
        HashSet<Square> s0SubSquares = s0.Subdivide(vertexList, squares);

        foreach (Square s in squares) s.AddIndices(vertexIdxList);
    } 

    void CreateMeshTriangles()
    {
        //// Test points
        //vertexList.Add(new Vector3(-1, 0, 0));
        //vertexList.Add(new Vector3(0, 0, 1));
        //vertexList.Add(new Vector3(1, 0, 0));

        //Triangle t0 = new Triangle(0, 1, 2, vertexList, 0);
        //t0.AddTriangleVertexIndices(triangleIdxList, triangles);

        //Set<Triangle> triangles = t0.Subdivide(vertexList);
        //foreach (Triangle subTriangle in problem.Questions)
        //{
        //    for (int i = 0; i < triangles.Count; i++)
        //{
        //    triangles[i].AddTriangleVertexIndices(triangleIdxList, triangles);
        //    Set<Triangle> subSubTriangles = triangles[i].Subdivide(vertexList);
        //    for (int j = 0; j < subSubTriangles.Count; j++)
        //    {
        //        subSubTriangles[j].AddTriangleVertexIndices(triangleIdxList, triangles);
        //    }
        //}

        TransferVerticesFromListToArray();
        TransferIndicesFromListToArray();
    }

    void TransferVerticesFromListToArray()
    {
        vertexArr = new Vector3[vertexList.Count];
        for (int i = 0; i < vertexList.Count; i++)
        {
            vertexArr[i] = (Vector3) vertexList[i];
        }
    }

    void TransferIndicesFromListToArray()
    { 
        vertexIdxArr = new int[vertexIdxList.Count];
        for (int i = 0; i < vertexIdxList.Count; i++)
        {
            vertexIdxArr[i] = (int)vertexIdxArr[i];
        }
    }
    

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertexArr;
        mesh.triangles = vertexIdxArr;
        mesh.RecalculateNormals();
    }
}
