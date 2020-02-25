using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class FractalTerrain : MonoBehaviour
{
    // From this resolution factor, we can generate a grid off size (3 + 2^r) x (3 + 2^r)
    public int mResolutionFactor = 1; // need to be at least 1
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;

   // HashSet<Triangle> triangles; // test triangles

    // Start is called before the first frame update
    void Start()
    {
        if (mResolutionFactor <= 0) Debug.Log("mResolutionFactor needs to be at least 1");
        else
        {
            mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;
            CreateMeshSquares();
            UpdateMesh();
        }
    }

    void CreateMeshTest()
    {   
        mesh.Clear();
        mesh.vertices = new Vector3[]
        {
            new Vector3(0, 0, 1),
            new Vector3(1, 0, 1),
            new Vector3(1, 0, 0)
        };
        mesh.triangles = new int[] { 0, 1, 2 };
        mesh.RecalculateNormals();
    }

    // Update is called once per frame
    void Update()
    {
    }



    void CreateMeshSquares()
    {
        HashSet<Square> squares; // test squares
        squares = new HashSet<Square>();

        // Find the resolution (= 2 ^ mResFactor + 1) and create a grid of resolution * resolution)
        // Which is (resolution - 1) * (resolution - 1) number of squares, hence 6 * (resolution - 1) * (resolution - 1) triangles
        int resolution = (int) Mathf.Pow(2, mResolutionFactor) + 1;
        Square.ms_res = resolution;

int size = resolution * resolution;
        int numIndices = 6 * (resolution - 1) * (resolution - 1);
        vertices = new Vector3[size];
        triangles = new int[numIndices];

        // Default corners
        int topLeftIdx = 0;
        int topRightIdx = resolution - 1;
        int botRightIdx = size - 1;
        int botLeftIdx = resolution * (resolution - 1);
        vertices[topLeftIdx] = new Vector3(0, 0, 1); // top left`
        vertices[topRightIdx] = new Vector3(1, 0, 1); // top right
        vertices[botRightIdx] = new Vector3(1, 0, 0); // bot right
        vertices[botLeftIdx] = new Vector3(0, 0, 0); // bot left

        Square sMain = new Square(topLeftIdx, topRightIdx, botRightIdx, botLeftIdx, 0, squares);
        sMain.Subdivide(vertices, squares, Algorithm.MidpointDisplacement);

        int startLoc = 0;
        foreach (Square s in squares) s.AddIndices(ref startLoc, triangles);
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

        //TransferVerticesFromListToArray();
        //TransferIndicesFromListToArray();
    }

    void TransferVerticesFromListToArray()
    {
        //vertexArr = new Vector3[vertexList.Count];
        //for (int i = 0; i < vertexList.Count; i++)
        //{
        //    vertexArr[i] = vertexList[i];
        //}
    }

    void TransferIndicesFromListToArray()
    {
        //vertexIdxArr = new int[vertexIdxList.Count];
        //for (int i = 0; i < vertexIdxList.Count; i++)
        //{
        //    vertexIdxArr[i] = vertexIdxList[i];
        //}
    }


    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
