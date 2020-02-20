using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle //: MonoBehaviour
{
    // WARNING: The order of the vertices is important because it affects the normal
    public Vector3 m_v0;
    public Vector3 m_v1;
    public Vector3 m_v2;
    
    // indices of the vertices within the mesh
    public int m_v0idx;
    public int m_v1idx;
    public int m_v2idx;

    public Vector3 m_n; // normal of the triangle

    public Triangle() { }

    // Constructor with vertices, their indices as arguments
    // Calculate and set the normal of the trianlge and update the triangle index list and triangle list
    public Triangle(Vector3 v0, Vector3 v1, Vector3 v2, int v0idx, int v1idx, int v2idx)
    {
        m_v0 = v0;
        m_v1 = v1;
        m_v2 = v2;
        m_v0idx = v0idx;
        m_v1idx = v1idx;
        m_v2idx = v2idx;
        m_n = FindNormal();
    }

    public void AddTriangleVertexIndices(ref ArrayList triangleIdxList, ref ArrayList triangleList)
    {
        triangleIdxList.Add(m_v0idx);
        triangleIdxList.Add(m_v1idx);
        triangleIdxList.Add(m_v2idx);
        triangleList.Add(this);
    }

    // Subdivide this triangle and update the input lists, return list of sub-triangles 
    public ArrayList Subdivide(ref ArrayList vertexLocList)
    {
        Vector3 c = AdjustPointHeight(FindCentroid(), m_n);
        int cidx = vertexLocList.Count;
        vertexLocList.Add(c);

        // Create sub triangles and add to the list
        Triangle t_v0v1c = new Triangle(m_v0, m_v1, c, m_v0idx, m_v1idx, cidx);
        Triangle t_cv1v2 = new Triangle(c, m_v1, m_v2, cidx, m_v1idx, m_v2idx);
        Triangle t_v0cv2 = new Triangle(m_v0, c, m_v2, m_v0idx, cidx, m_v2idx);

        ArrayList subTriangleList = new ArrayList();
        subTriangleList.Add(t_v0v1c);
        subTriangleList.Add(t_cv1v2);
        subTriangleList.Add(t_v0cv2);

        return subTriangleList;
    }

    Vector3 FindCentroid()
    {
        return 1f / 3f * (m_v0 + m_v1 + m_v2);
    }

    Vector3 FindNormal()
    {
        Vector3 v01 = m_v1 - m_v0;
        Vector3 v02 = m_v2 - m_v0;
        return Vector3.Normalize(Vector3.Cross(v01, v02));
    }

    // Add some random values to the current height of this point along some direction
    static Vector3 AdjustPointHeight(Vector3 p, Vector3 d)
    {
        // Test to get random height
        float rand = Random.Range(0f, 0.5f);
        return p + rand * d;
    }
}
