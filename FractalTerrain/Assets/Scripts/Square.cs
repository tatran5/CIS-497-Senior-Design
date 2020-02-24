using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Square //: MonoBehaviour
{
    public static float m_heightRange = 0.5f;

    int m_level;
    // Indices of the four vertices that make up this square
    int m_topLeftIdx;
    int m_topRightIdx;
    int m_botRightIdx;
    int m_botLeftIdx;

    // Create a square & potentially add it to a set of squares
    // Argument: vertex indices for the four corners of this square
    // and centers of the squares around it. (if there is none, the input is -1)
    public Square(int topLeftIdx, int topRightIdx, int botRightIdx, int botLeftIdx, int level, HashSet<Square> squares)
    {
        m_topLeftIdx = topLeftIdx;
        m_topRightIdx = topRightIdx;
        m_botRightIdx = botRightIdx;
        m_botLeftIdx = botLeftIdx;
        m_level = level;
        if (squares != null) squares.Add(this);
    }

    public void Subdivide(Vector3[] vertices, HashSet<Square> squares)
    {
        if (m_topRightIdx - m_topLeftIdx > 1)
        {
            Vector3 n = GetNormal(vertices[m_topLeftIdx], vertices[m_topRightIdx], vertices[m_botLeftIdx]);

            // 1. Set the four corners of the square to random values along the normal
            //AdjustHeightFourPoints(ref vertices[m_topLeftIdx], ref vertices[m_topRightIdx],
            //    ref vertices[m_botLeftIdx], ref vertices[m_botRightIdx], n);

            // 2. Find midpoints, set to random values along the normal
            int midTopIdx = (m_topLeftIdx + m_topRightIdx) / 2;
            int midRightIdx = (m_topRightIdx + m_botRightIdx) / 2;
            int midBotIdx = (m_botRightIdx + m_botLeftIdx) / 2;
            int midLeftIdx = (m_botLeftIdx + m_topLeftIdx) / 2;
            GetMidpoints(vertices[m_topLeftIdx], vertices[m_topRightIdx], vertices[m_botRightIdx], vertices[m_botLeftIdx],
                ref vertices[midTopIdx], ref vertices[midRightIdx], ref vertices[midBotIdx], ref vertices[midLeftIdx]);
            AdjustHeightFourPoints(ref vertices[midTopIdx], ref vertices[midRightIdx],
                ref vertices[midBotIdx], ref vertices[midLeftIdx], n);

            // 3. Get the center by averaging 4 points and add a random displacement
            int centerIdx = (m_topLeftIdx + m_topRightIdx + m_botRightIdx + m_botLeftIdx) / 4;
            vertices[centerIdx] = Average4(vertices[m_topLeftIdx], vertices[m_topRightIdx], vertices[m_botRightIdx], vertices[m_botLeftIdx]);
            vertices[centerIdx] = GetAdjustedHeight(vertices[centerIdx], n);

            // 4. Create four squares
            squares.Remove(this);
            Square sTopLeft = new Square(m_topLeftIdx, midTopIdx, centerIdx, midLeftIdx, m_level + 1, squares);
            Square sTopRight = new Square(midTopIdx, m_topRightIdx, midRightIdx, centerIdx, m_level + 1, squares);
            Square sBotRight = new Square(centerIdx, midRightIdx, m_botRightIdx, midBotIdx, m_level + 1, squares);
            Square sBotLeft = new Square(midLeftIdx, centerIdx, midBotIdx, m_botLeftIdx, m_level + 1, squares);

            // 5. Call subdivision on them
            sTopLeft.Subdivide(vertices, squares);
            sTopRight.Subdivide(vertices, squares);
            sBotRight.Subdivide(vertices, squares);
            sBotLeft.Subdivide(vertices, squares);
        }
    }

    // !!! Not working
    public HashSet<Square> SubdivideMultiple(List<Vector3> vertices, HashSet<Square> squares, int subdivisionLevel)
    {
        HashSet<Square> subSquares = new HashSet<Square>();
        if (subdivisionLevel > 0)
        {
            subSquares = Subdivide(vertices, squares);
            foreach (Square subSquare in subSquares) subSquare.SubdivideMultiple(vertices, squares, subdivisionLevel - 1);
        }
        return subSquares;
    }


    // Input: indicies of the two vertices making this edge and the centers of 2 squares adjacent to this edge
    // If the edge is on the boundary (there is no adjacent square on one side of the edge), the midpoint is the 
    // average of the two indicies
    // Want to get the midpoint between p0 and p1, but using the average of p0, p1, c0, c1
    Vector3 GetMidpointOfEdge(int p0Idx, int p1Idx, int c0Idx, int c1Idx, List<Vector3> vertices)
    {
        if (c0Idx == -1 || c1Idx == -1) return Average2(vertices[p0Idx], vertices[p1Idx]);
        return Average4(vertices[p0Idx], vertices[p1Idx], vertices[c0Idx], vertices[c1Idx]);
    }

    // !!! Not working
    public HashSet<Square> Subdivide(List<Vector3> vertices, HashSet<Square> squares)
    {
        // Get the four corners first & adjust their height
        Vector3 topLeft = vertices[m_topLeftIdx];
        Vector3 topRight = vertices[m_topRightIdx];
        Vector3 botRight = vertices[m_botRightIdx];
        Vector3 botLeft = vertices[m_botLeftIdx];
        Vector3 n = GetNormal(topLeft, topRight, botRight);
        AdjustHeightFourPoints(ref topLeft, ref topRight, ref botRight, ref botLeft, n);

        // Midpoints for the 4 edges and center of the square
        Vector3 midTop = new Vector3(), midRight = new Vector3(), midBot = new Vector3(), midLeft = new Vector3();
        GetMidpoints(topLeft, topRight, botRight, botLeft, ref midTop, ref midRight, ref midBot, ref midLeft);
        AdjustHeightFourPoints(ref midTop, ref midLeft, ref midBot, ref midRight, n);

        // Get the center and modify it
        Vector3 center = GetAdjustedHeight(Average4(midTop, midLeft, midBot, midRight), n);

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

    public void AddIndices(ref int startLoc, int[] vertexIndices)
    {
        vertexIndices[startLoc] = m_topLeftIdx;
        vertexIndices[startLoc + 1] = m_topRightIdx;
        vertexIndices[startLoc + 2] = m_botRightIdx;

        vertexIndices[startLoc + 3] = m_topLeftIdx;
        vertexIndices[startLoc + 4] = m_botRightIdx;
        vertexIndices[startLoc + 5] = m_botLeftIdx;

        startLoc += 6;
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

    void GetMidpoints(Vector3 topLeft, Vector3 topRight, Vector3 botRight, Vector3 botLeft, 
        ref Vector3 midTop, ref Vector3 midRight, ref Vector3 midBot, ref Vector3 midLeft)
    {
       midTop = Average2(topLeft, topRight);
       midLeft = Average2(topLeft, botLeft);
       midBot = Average2(botRight, botLeft);
       midRight = Average2(botRight, topRight);
    }

    // Randomly adjust an input point (p) along a vector (n) based on heightRange and level
    Vector3 GetAdjustedHeight(Vector3 p, Vector3 n)
    {
        float denominator = 7 * (m_level + 1f);
        float scale = Random.Range(-m_heightRange / denominator, m_heightRange / denominator);
        return scale * n + p;
    }

    void AdjustHeightFourPoints(ref Vector3 p0, ref Vector3 p1, ref Vector3 p2, ref Vector3 p3, Vector3 n)
    {
        p0 = GetAdjustedHeight(p0, n);
        p1 = GetAdjustedHeight(p1, n);
        p2 = GetAdjustedHeight(p2, n);
        p3 = GetAdjustedHeight(p3, n);
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
