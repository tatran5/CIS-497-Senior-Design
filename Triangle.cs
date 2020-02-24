using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle //: MonoBehaviour
{
    // indices of the vertices within the mesh
    public int m_v0idx;
    public int m_v1idx;
    public int m_v2idx;

    // test
    public int m_level;

    public Triangle() { }

    // Constructor with vertices, their indices as arguments
    // Calculate and set the normal of the trianlge and update the triangle index list and triangle list
    public Triangle(int v0idx, int v1idx, int v2idx, List<Vector3> vertexList, int level)
    {
        m_level = level;
        m_v0idx = v0idx;
        m_v1idx = v1idx;
        m_v2idx = v2idx;
    }

    public void AddTriangleVertexIndices(List<int> vertexIdxList, HashSet<Triangle> triangles)
    {
        vertexIdxList.Add(m_v0idx);
        vertexIdxList.Add(m_v1idx);
        vertexIdxList.Add(m_v2idx);
        triangles.Add(this);
    }

    // Subdivide this triangle and update the input lists, return list of sub-triangles 
    // Remove this current triangle and add the sub triangles instead
    public HashSet<Triangle> Subdivide(List<Vector3> vertexList)
    {
        Vector3 v0 = vertexList[m_v0idx];
        Vector3 v1 = vertexList[m_v1idx];
        Vector3 v2 = vertexList[m_v2idx];
        Vector3 n = FindNormal(vertexList[m_v0idx], vertexList[m_v1idx], vertexList[m_v2idx]);

        // Find midpoint of three edges and adjust their height along the normal with of the current triangle
        Vector3 m0 = AdjustPointHeight(FindMidpoint(v1, v2), n);
        Vector3 m1 = AdjustPointHeight(FindMidpoint(v2, v0), n);
        Vector3 m2 = AdjustPointHeight(FindMidpoint(v0, v1), n);

        // Generate indices for these vertices in triangleList and add them to the list
        int m0idx = vertexList.Count;
        int m1idx = m0idx + 1;
        int m2idx = m0idx + 2;
        vertexList.Add(m0);
        vertexList.Add(m1);
        vertexList.Add(m2);

        // Create sub triangles and add to the list
        Triangle t0 = new Triangle(m_v0idx, m2idx, m1idx, vertexList, m_level + 1);
        Triangle t1 = new Triangle(m_v1idx, m0idx, m2idx, vertexList, m_level + 1);
        Triangle t2 = new Triangle(m_v2idx, m1idx, m0idx, vertexList, m_level + 1);
        Triangle t3 = new Triangle(m0idx, m1idx, m2idx, vertexList, m_level + 1);

        HashSet<Triangle> subTriangles = new HashSet<Triangle>();
        subTriangles.Add(t0);
        subTriangles.Add(t1);
        subTriangles.Add(t2);
        subTriangles.Add(t3);

        return subTriangles;
    }

    Vector3 FindCentroid(List<Vector3> vertexList)
    {
        Vector3 v0 = vertexList[m_v0idx];
        Vector3 v1 = vertexList[m_v1idx];
        Vector3 v2 = vertexList[m_v2idx];
        return 1f / 3f * (v0 + v1 + v2);
    }

    Vector3 FindNormal(Vector3 v0, Vector3 v1, Vector3 v2)
    {
        Vector3 v01 = v1 - v0;
        Vector3 v02 = v2 - v0;
        return Vector3.Normalize(Vector3.Cross(v01, v02));
    }

    // Add some random values to the current height of this point along some direction
    Vector3 AdjustPointHeight(Vector3 p, Vector3 d)
    {
        // Test to get random height
        float rand = 1f / (m_level + 1f) * Random.Range(0f, 1f);
        return p + rand * d;
    }

    Vector3 FindMidpoint(Vector3 v0, Vector3 v1)
    {
        return 0.5f * (v0 + v1);
    }
}
