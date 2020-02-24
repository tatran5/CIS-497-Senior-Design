using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Square //: MonoBehaviour
{
    public static float m_heightRange = 1f;

    int m_level;
    // Indices of the four vertices that make up this square
    int m_topLeftIdx;
    int m_topRightIdx;
    int m_botRightIdx;
    int m_botLeftIdx;

    // Create a square & potentiall add it to a set of squares
    public Square(int v0, int v1, int v2, int v3, int level, HashSet<Square> squares) 
    {
        m_topLeftIdx = v0;
        m_topRightIdx = v1;
        m_botRightIdx = v2;
        m_botLeftIdx = v3;
        m_level = level;
        if (squares != null) squares.Add(this);
    }

    public HashSet<Square> Subdivide(List<Vector3> vertices, HashSet<Square> squares)
    {
        // Change the four corners first
        Vector3 topLeft = vertices[m_topLeftIdx];
        Vector3 topRight = vertices[m_topRightIdx];
        Vector3 botRight = vertices[m_botRightIdx];
        Vector3 botLeft = vertices[m_botLeftIdx];
        Vector3 n = GetNormal(topLeft, topRight, botRight);

        // Midpoints for the 4 edges and center of the square
        Vector3 midTop = Average2(topLeft, topRight);
        Vector3 midLeft = Average2(topLeft, botLeft);
        Vector3 midBot = Average2(botRight, botLeft);
        Vector3 midRight = Average2(botRight, topRight);
        Vector3 center = Average4(midTop, midLeft, midBot, midRight);

        // Adjust the height of the midpoints and center
        midTop = GetAdjustedHeight(midTop, n);
        midLeft = GetAdjustedHeight(midLeft, n);
        midBot = GetAdjustedHeight(midBot, n);
        midRight = GetAdjustedHeight(midRight, n);
        center = GetAdjustedHeight(center, n);

        // Add the new vertices to the input lists
        int midTopIdx = vertices.Count;
        int midLeftIdx = midTopIdx + 1;
        int midBotIdx = midTopIdx + 2;
        int midRightIdx = midTopIdx + 3;
        int centerIdx = midTopIdx + 4;
        vertices.Add(midTop);
        vertices.Add(midLeft);
        vertices.Add(midBot);
        vertices.Add(midRight);
        vertices.Add(center);

        // Delete this square from the set and add its four smaller squares
        squares.Remove(this);
        Square topLeftSquare = new Square(m_topLeftIdx, midTopIdx, centerIdx, midLeftIdx, m_level + 1, squares);
        Square topRightSquare = new Square(midTopIdx, m_topRightIdx, midRightIdx, centerIdx, m_level + 1, squares);
        Square botRightSquare = new Square(centerIdx, midRightIdx, m_botRightIdx, midBotIdx, m_level + 1, squares);
        Square botLeftSquare = new Square(midLeftIdx, centerIdx, midBotIdx, m_botLeftIdx, m_level + 1, squares);

        // Return these squares
        HashSet<Square> subSquares = new HashSet<Square>();
        subSquares.Add(topLeftSquare);
        subSquares.Add(topRightSquare);
        subSquares.Add(botRightSquare);
        subSquares.Add(botLeftSquare);
        return subSquares;
    }

    public void AddIndices(List<int> vertexIndices)
    {
        vertexIndices.Add(m_topLeftIdx);
        vertexIndices.Add(m_topRightIdx);
        vertexIndices.Add(m_botRightIdx);
        
        vertexIndices.Add(m_topLeftIdx);
        vertexIndices.Add(m_botRightIdx);
        vertexIndices.Add(m_botLeftIdx);
    }

    // Randomly adjust an input point (p) along a vector (n) based on heightRange and level
    Vector3 GetAdjustedHeight(Vector3 p, Vector3 n)
    {
        float scale = 1f / (m_level + 1f) * Random.Range(0f, m_heightRange);
        return scale * n + p;
    }

    Vector3 Average2(Vector3 p0, Vector3 p1)
    {
        return 0.5f * (p0 + p1);
    }

    Vector3 Average4(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return 0.25f * (p0 + p1 + p2 + p3);
    }

    Vector3 GetNormal(Vector3 v0, Vector3 v1, Vector3 v2)
    {
        Vector3 v01 = v1 - v0;
        Vector3 v02 = v2 - v0;
        return Vector3.Normalize(Vector3.Cross(v01, v02));
    }
}
