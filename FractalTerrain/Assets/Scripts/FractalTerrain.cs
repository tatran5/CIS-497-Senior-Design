using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class FractalTerrain : MonoBehaviour
{
    // From this resolution factor, we can generate a grid off size (3 + 2^r) x (3 + 2^r)
    public int m_resolutionFactor = 0; // need to be at least 1
    public float m_heightRange = 0.5f;
    int m_res;
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    Vector3 m_worldUp;


    // HashSet<Triangle> triangles; // test triangles

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        //CreateMeshTest();
        m_worldUp = new Vector3(0, 1, 0);
        if (m_resolutionFactor < 0)
        {
            Debug.Log("m_resolutionFactor needs to be at least 0. Default one square instead");
            m_res = 0;
        }

        // Find the resolution (= 2 ^ m_resFactor + 1) and create a grid of resolution * resolution)
        // Which is (resolution - 1) * (resolution - 1) number of squares, hence 6 * (resolution - 1) * (resolution - 1) triangles
        m_res = (int)Mathf.Pow(2, m_resolutionFactor) + 1;
        vertices = new Vector3[m_res * m_res];

        DiamondSquare();
        UpdateMesh();
    }

    void CreateMeshTest()
    {
        mesh.Clear();
        mesh.vertices = new Vector3[]
        {
            new Vector3(0, 0, 1), // top lef
            new Vector3(1, 0, 1), // top rig
            new Vector3(1, 0, 0), // bot rig
            new Vector3(0, 0, 0) // bot lef
    };
        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        mesh.RecalculateNormals();
    }

    // Update is called once per frame
    void Update()
    {
    }

    // Reference: 
    // https://medium.com/@nickobrien/diamond-square-algorithm-explanation-and-c-implementation-5efa891e486f
    // http://jmecom.github.io/blog/2015/diamond-square/
    void DiamondSquare()
    {
        // Debug.Log("m_res: " + m_res);
        int squareSize = m_res - 1; // Should always be an exponent of 2
        int reduction = 1;

        // Pre seed four corners of A with a value
        int topLefIdx = 0;
        int topRigIdx = m_res - 1;
        int botRigIdx = m_res * m_res - 1;
        int botLefIdx = botRigIdx - m_res + 1;

        vertices[topLefIdx] = new Vector3(0, 0, 1);
        vertices[topRigIdx] = new Vector3(1, 0, 1);
        vertices[botRigIdx] = new Vector3(1, 0, 0);
        vertices[botLefIdx] = new Vector3(0, 0, 0);
        Vector3 normalOriginal = Global.GetNormal(vertices[topLefIdx], vertices[topRigIdx], vertices[botRigIdx]);

        //vertices[topLefIdx] = Global.GetJittered(vertices[topLefIdx], m_worldUp, m_heightRange, reduction);
        //vertices[topRigIdx] = Global.GetJittered(vertices[topRigIdx], m_worldUp, m_heightRange, reduction);
        //vertices[botRigIdx] = Global.GetJittered(vertices[botRigIdx], m_worldUp, m_heightRange, reduction);
        //vertices[botLefIdx] = Global.GetJittered(vertices[botLefIdx], m_worldUp, m_heightRange, reduction);

        // square steps
        while (squareSize > 1)
        {
            for (int z = 0; z < m_res - 1; z += squareSize)
            {
                for (int x = 0; x < m_res - 1; x += squareSize)
                {
                    SquareStep(z, x, squareSize, reduction);
                }
            }

            //int halfSquareSize = squareSize / 2;
            for (int z = 0; z < m_res - 1; z += squareSize)
            {
                for (int x = 0; x < m_res - 1; x += squareSize)
                {
                    DiamondStep(z, x, squareSize, reduction);
                }
            }

            squareSize /= 2;
            reduction++;
        }

        AddIndicies();
    }

    void AddIndicies()
    {
        // Iterate through the top left corner of all triangles and add their indices
        int count = 0;
        triangles = new int[6 * (m_res - 1) * (m_res - 1)]; // 6 indices per square because 3 indices per trianglew
        for (int z = 0; z < m_res - 1; z++)
        {
            for (int x = 0; x < m_res - 1; x++)
            {
                int topLef = z * m_res + x;
                int topRig = topLef + 1;
                int botLef = (z + 1) * m_res + x;
                int botRig = botLef + 1;
                // Debug.Log("topLef: " + topLef + "; topRig: " + topRig + "; botRig: " + botRig + "; botLef: " + botLef);
                triangles[count++] = topLef;
                triangles[count++] = topRig;
                triangles[count++] = botRig;
                triangles[count++] = topLef;
                triangles[count++] = botRig;
                triangles[count++] = botLef;
            }
        }
    }

    /* (x, z) is the coordinate of the top lef corner of the square
       Find the center,set its height as the average of the four corners plus some random value*/
    void SquareStep(int z, int x, int sideLength, float reduction)
    {
        int topLefIdx = z * m_res + x;
        int topRigIdx = topLefIdx + sideLength;
        int botRigIdx = topRigIdx + sideLength * m_res;
        int botLefIdx = botRigIdx - sideLength;

        Vector3 normal = Global.GetNormal(vertices[topLefIdx], vertices[topRigIdx], vertices[botRigIdx]);
        int centerIdx = Global.Average4Int(topLefIdx, topRigIdx, botRigIdx, botLefIdx);

        vertices[centerIdx] = Global.Average4Vector(vertices[topLefIdx], vertices[topRigIdx], vertices[botRigIdx], vertices[botLefIdx]);
        vertices[centerIdx] = Global.GetJittered(vertices[centerIdx], normal, m_heightRange, reduction);
    }

    /* (x, z) is the coordinate of the top left corner of the square whose mid point on edges need
      * to be modified. 
      * However, to do this efficiently, 
      *     - we only modify the midpoint on the right edge if the square is all the way to the right of the grid, 
      *     - we only modify the midpoint on the bottom edge if the square is all the way at the bottom of the grid */
    void DiamondStep(int z, int x, int sideLength, float reduction)
    {
        int halfSideLength = sideLength / 2;

        int topLefIdx = z * m_res + x;
        int topRigIdx = topLefIdx + sideLength;
        int botRigIdx = topRigIdx + sideLength * m_res;
        int botLefIdx = topLefIdx + sideLength * m_res;
        int centerIdx = Global.Average4Int(topLefIdx, topRigIdx, botRigIdx, botLefIdx);

        // Check for whether this square is all the way to the top, right, bottom or left of the grid
        bool isTopSquare = topLefIdx - m_res < 0;
        bool isRigSquare = botRigIdx % m_res == m_res - 1;
        bool isBotSquare = botRigIdx + m_res >= m_res * m_res;
        bool isLefSquare = botLefIdx % m_res == 0;

        Vector3 normal = Global.GetNormal(vertices[topLefIdx], vertices[topRigIdx], vertices[botRigIdx]);

        /* Handle top edge: 
         * - If the square is at the top of the grid, the midpoint of the top edge is the average of the current's center and the points on the edge
         * - Otherwise, the midpoint of the top edge is the average of the current's center, the points on the edge and the center of the square above it
         * Similarly, handle for left edge. */
        int centerIdxTopSquare = centerIdx - m_res;
        int midTopIdx = topLefIdx + halfSideLength;
        Debug.Log("midTopIdx " + midTopIdx);
        Debug.Log("isTopSqua " + isTopSquare);
        HandleMidpoint(isTopSquare, centerIdxTopSquare, midTopIdx, centerIdx, topLefIdx, topRigIdx, normal, reduction);

        int centerIdxLefSquare = centerIdx - sideLength;
        int midLefIdx = centerIdx - halfSideLength;
        Debug.Log("midLefIdx " + midLefIdx);
        Debug.Log("isLefSqua " + isLefSquare);
        HandleMidpoint(isLefSquare, centerIdxLefSquare, midLefIdx, centerIdx, topLefIdx, botLefIdx, normal, reduction);

        // Handle the right and bottom midpoints if the square is all the way to the right or at the bottom of the grid
        if (isRigSquare)
        {
            Debug.Log("isRigSquare");
            int midRigIdx = centerIdx + halfSideLength;
            HandleMidpoint(true, -1, midRigIdx, centerIdx + halfSideLength, topRigIdx, botRigIdx, normal, reduction);
        }

        if (isBotSquare)
        {
            Debug.Log("isBotSquare");
            int midBotIdx = botLefIdx + halfSideLength;
            HandleMidpoint(true, -1, midBotIdx, centerIdx, botLefIdx, botRigIdx, normal, reduction);
        }
    }

    void HandleMidpoint(bool isEdgeCase, int adjacentSquareCenterIdx, int midpointIdx, 
        int centerIdx, int endpoint0Idx, int endpoint1Idx, Vector3 normal, float reduction)
    {
        vertices[midpointIdx] = Global.Average2Vector(vertices[endpoint0Idx], vertices[endpoint1Idx]);

        //vertices[midpointIdx] = isEdgeCase ?
        //    Global.Average3Vector(vertices[endpoint0Idx], vertices[endpoint1Idx], vertices[centerIdx]) :
        //    Global.Average4Vector(vertices[endpoint0Idx], vertices[endpoint1Idx], vertices[centerIdx], vertices[adjacentSquareCenterIdx]);
        // vertices[midpointIdx] = Global.GetJittered(vertices[midpointIdx], normal, m_heightRange, reduction);
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
