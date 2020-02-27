using System.Collections.Generic;
using UnityEngine;

// Reference: http://micsymposium.org/mics_2011_proceedings/mics2011_submission_30.pdf
public enum Algorithm
{
    MidpointDisplacement, // Fastest but has line artifacts
    DiamondSquare // Slower but has better quality
}
public class Square //: MonoBehaviour
{
    public static float ms_heightRange = 0.5f;
    public static int ms_sideRes; // The side resolution of all squares combined

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

    public void Subdivide(Vector3[] vertices, HashSet<Square> squares, Algorithm algorithm)
    {
        switch (algorithm)
        {
            case Algorithm.MidpointDisplacement:
                SubdivideMidpointDisplacement(vertices, squares);
                break;
            case Algorithm.DiamondSquare:
                SubdivideDiamondSquare(vertices, squares);
                break;
        }
    }

    // Reference: https://stevelosh.com/blog/2016/06/diamond-square/
    public void SubdivideDiamondSquare(Vector3[] vertices, HashSet<Square> squares)
    {
        if (m_topRightIdx - m_topLeftIdx > 1)
        {
            Vector3 n = GetNormal(vertices[m_topLeftIdx], vertices[m_topRightIdx], vertices[m_botLeftIdx]);

            // 1. Intialize corners to random value
            AdjustHeightFourPoints(ref vertices[m_topLeftIdx], ref vertices[m_topRightIdx],
                ref vertices[m_botLeftIdx], ref vertices[m_botRightIdx], n);

            // 2. Set the center of the heightmap to the average of the corners (plus jitter)
            int centerIdx = AdjustHeightCenter(vertices, n);

            // 3. Set the midpoints of the edges as the average of the four points on the 
            // "diamond" around them (plus jitter)
            int midTopIdx = (m_topLeftIdx + m_topRightIdx) / 2;
            int midRightIdx = (m_topRightIdx + m_botRightIdx) / 2;
            int midBotIdx = (m_botRightIdx + m_botLeftIdx) / 2;
            int midLeftIdx = (m_botLeftIdx + m_topLeftIdx) / 2;
            AdjustMidpointsDiamondSquare(midTopIdx, midRightIdx, midBotIdx, midLeftIdx, vertices, n);

            // 4. Create 4 squares and recursively call this on those four squares
            squares.Remove(this);
            Square sTopLeft = new Square(m_topLeftIdx, midTopIdx, centerIdx, midLeftIdx, m_level + 1, squares);
            Square sTopRight = new Square(midTopIdx, m_topRightIdx, midRightIdx, centerIdx, m_level + 1, squares);
            Square sBotRight = new Square(m_topLeftIdx, midTopIdx, centerIdx, midLeftIdx, m_level + 1, squares);
            Square sBotLeft = new Square(midLeftIdx, centerIdx, midBotIdx, m_botLeftIdx, m_level + 1, squares);

            
        }
    }

    public void AdjustMidpointsDiamondSquare(int midTopIdx, int midRightIdx, int midBotIdx, int midLeftIdx,
        Vector3[] vertices, Vector3 normal)
    {
        AdjustMidpointDiamondSquare(midTopIdx, vertices, normal);
        AdjustMidpointDiamondSquare(midRightIdx, vertices, normal);
        AdjustMidpointDiamondSquare(midBotIdx, vertices, normal);
        AdjustMidpointDiamondSquare(midLeftIdx, vertices, normal);
    }

    // Adjust one midpoint and handle the boundary case;
    public void AdjustMidpointDiamondSquare(int midIdx, Vector3[] vertices, Vector3 normal)
    {
        // Find the four "diamond" points around this current point
        int sideRes = m_topRightIdx - m_topLeftIdx;
        int topIdx = midIdx - sideRes * ms_sideRes;
        int rightIdx = midIdx + sideRes;
        int botIdx = midIdx + sideRes * ms_sideRes;
        int leftIdx = midIdx - sideRes;

        // Handle cases of which there's a missing point (aka the midpoint is on the border
        // at the top, bottom, right, left
        if (topIdx < 0)
            vertices[midIdx] = Average3(vertices[rightIdx], vertices[botIdx], vertices[leftIdx]);
        else if (Mathf.Ceil(botIdx / ms_sideRes) == ms_sideRes)
            vertices[midIdx] = Average3(vertices[topIdx], vertices[rightIdx], vertices[leftIdx]);
        else if (leftIdx % ms_sideRes == 0)
            vertices[midIdx] = Average3(vertices[topIdx], vertices[rightIdx], vertices[botIdx]);
        else if (rightIdx % ms_sideRes == ms_sideRes - 1)
            vertices[midIdx] = Average3(vertices[topIdx], vertices[botIdx], vertices[leftIdx]);

        vertices[midIdx] = GetAdjustedHeight(vertices[midIdx], normal);
    }

    // Reference: https://stevelosh.com/blog/2016/02/midpoint-displacement/#s2-resources-code-and-examples
    public void SubdivideMidpointDisplacement(Vector3[] vertices, HashSet<Square> squares)
    {
        if (m_topRightIdx - m_topLeftIdx > 1)
        {
            Vector3 n = GetNormal(vertices[m_topLeftIdx], vertices[m_topRightIdx], vertices[m_botLeftIdx]);

            // 1. Set the four corners of the square to random values along the normal
            AdjustHeightFourPoints(ref vertices[m_topLeftIdx], ref vertices[m_topRightIdx],
                ref vertices[m_botLeftIdx], ref vertices[m_botRightIdx], n);

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
            int centerIdx = AdjustHeightCenter(vertices, n);

            // 4. Create four squares
            squares.Remove(this);
            Square sTopLeft = new Square(m_topLeftIdx, midTopIdx, centerIdx, midLeftIdx, m_level + 1, squares);
            Square sTopRight = new Square(midTopIdx, m_topRightIdx, midRightIdx, centerIdx, m_level + 1, squares);
            Square sBotRight = new Square(centerIdx, midRightIdx, m_botRightIdx, midBotIdx, m_level + 1, squares);
            Square sBotLeft = new Square(midLeftIdx, centerIdx, midBotIdx, m_botLeftIdx, m_level + 1, squares);

            // 5. Call subdivision on them
            sTopLeft.SubdivideMidpointDisplacement(vertices, squares);
            sTopRight.SubdivideMidpointDisplacement(vertices, squares);
            sBotRight.SubdivideMidpointDisplacement(vertices, squares);
            sBotLeft.SubdivideMidpointDisplacement(vertices, squares);
        }
    }

    // Return index of the center and also get the center value and put it into the vertices array
    public int GetCenter(Vector3[] vertices)
    {
        int centerIdx = (m_topLeftIdx + m_topRightIdx + m_botRightIdx + m_botLeftIdx) / 4;
        vertices[centerIdx] = Average4(vertices[m_topLeftIdx], vertices[m_topRightIdx], vertices[m_botRightIdx], vertices[m_botLeftIdx]);
        return centerIdx;
    }

    // Return the index of the center and modify the center value
    public int AdjustHeightCenter(Vector3[] vertices, Vector3 normal)
    {
        int centerIdx = GetCenter(vertices);
        vertices[centerIdx] = GetAdjustedHeight(vertices[centerIdx], normal);
        return centerIdx;
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
        float denominator = (m_level + 1f) * (m_level + 1f);
        float scale = Random.Range(-ms_heightRange / denominator, ms_heightRange / denominator);
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
        return (p0 + p1) / 2f;
    }

    Vector3 Average3(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        return (p0 + p1 + p2) / 3f;
    }

    Vector3 Average4(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return (p0 + p1 + p2 + p3) / 4f;
    }

    Vector3 GetNormal(Vector3 v0, Vector3 v1, Vector3 v2)
    {
        Vector3 v01 = v1 - v0;
        Vector3 v02 = v2 - v0;
        return Vector3.Normalize(Vector3.Cross(v01, v02));
    }
}
