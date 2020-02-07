using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle : MonoBehaviour
{
    // WARNING: The order of the vertices is important because it affects the normal
    public Vector3 m_v0;
    public Vector3 m_v1;
    public Vector3 m_v2;
    public Vector3 m_n; // normal of the triangle

    public Triangle() { }

    // Constructor with vertices and normal as arguments
    public Triangle(Vector3 v0, Vector3 v1, Vector3 v2)
    {
        m_v0 = v0;
        m_v1 = v1;
        m_v2 = v2;
        m_n = FindNormal();
    }

    // Start is called before the first frame update
    void Start() {}

    // Update is called once per frame
    void Update() {}

    // Subdivide this triangle and return all the sub-triangles
    public ArrayList Subdivide()
    {
        // Find the midpoints of edges of this triangle
        Vector3 m0 = GetAdjustedMidpoint(m_v1, m_v2, m_n);
        Vector3 m1 = GetAdjustedMidpoint(m_v2, m_v0, m_n);
        Vector3 m2 = GetAdjustedMidpoint(m_v0, m_v1, m_n);

        // Create sub triangles and add to the list
        Triangle t_m2v1m0 = new Triangle(m2, m_v1, m0);
        Triangle t_v0m2m1 = new Triangle(m_v0, m2, m1);
        Triangle t_m1m0v2 = new Triangle(m1, m0, m_v2);
        Triangle t_m2m0m1 = new Triangle(m2, m0, m1);

        ArrayList subTriangles = new ArrayList();
        subTriangles.Add(t_m2v1m0);
        subTriangles.Add(t_v0m2m1);
        subTriangles.Add(t_m1m0v2);
        subTriangles.Add(t_m2m0m1);

        return subTriangles;
    }

    Vector3 FindNormal()
    {
        Vector3 v10 = m_v0 - m_v1;
        Vector3 v12 = m_v2 - m_v1;
        return Vector3.Cross(v10, v12);
    }

    // Given two vertices, find the midpoint on the segment created by those and 
    // adjust its height randomly based on the input direction to adjust the point along 
    static Vector3 GetAdjustedMidpoint(Vector3 v0, Vector3 v1, Vector3 d)
    {
        Vector3 adjustedMidpoint = FindMidpoint(v0, v1);
        adjustedMidpoint = AdjustPointHeight(adjustedMidpoint, d);
        return adjustedMidpoint;
    }

    // Given two vertices, find the midpoint on the segment created by those
    static Vector3 FindMidpoint(Vector3 v0, Vector3 v1)
    {
        return 0.5f * (v0 + v1);
    }

    // Add some random values to the current height of this point along some direction
    static Vector3 AdjustPointHeight(Vector3 p, Vector3 d)
    {
        // Test to get random height
        float rand = Random.Range(0f, 5f);
        return p + rand * d;
    }
}
