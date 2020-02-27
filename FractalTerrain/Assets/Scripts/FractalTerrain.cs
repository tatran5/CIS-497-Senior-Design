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


    // HashSet<Triangle> triangles; // test triangles

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        //CreateMeshTest();

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

        Vector3 normalOrginal = Global.GetNormal(vertices[topLefIdx], vertices[topRigIdx], vertices[botRigIdx]);

        //vertices[topLefIdx] = Global.GetJittered(vertices[topLefIdx], normalOrginal, m_heightRange, reduction);
        //vertices[topRigIdx] = Global.GetJittered(vertices[topRigIdx], normalOrginal, m_heightRange, reduction);
        //vertices[botRigIdx] = Global.GetJittered(vertices[botRigIdx], normalOrginal, m_heightRange, reduction);
        //vertices[botLefIdx] = Global.GetJittered(vertices[botLefIdx], normalOrginal, m_heightRange, reduction);

        // square steps
        while (squareSize > 1)
        {
            for (int z = 0; z < m_res - 1; z += squareSize)
            {
                for (int x = 0; x < m_res - 1; x += squareSize)
                {
                    SquareStep(x, z, squareSize, reduction);
                }
            }

            int halfSquareSize = squareSize / 2;
            for (int i = halfSquareSize; i < m_res * m_res - 1; i += squareSize)
            {
                    DiamondStep(i, squareSize, reduction);
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
                triangles[count++] = topLef;
                triangles[count++] = topRig;
                triangles[count++] = botRig;
                triangles[count++] = topLef;
                triangles[count++] = botRig;
                triangles[count++] = botLef;
            }
        }
    }

    // (x, z) is the coordinate of the top lef corner of tthe square
    // Find the center,set its height as the average of the four corners plus some random value
    void SquareStep(int x, int z, int sideLength, float reduction)
    {
        int topLefIdx = z * sideLength + x;
        int topRigIdx = topLefIdx + sideLength;
        int botRigIdx = topRigIdx + sideLength * m_res;
        int botLefIdx = botRigIdx - sideLength;

        Vector3 normal = Global.GetNormal(vertices[topLefIdx], vertices[topRigIdx], vertices[botRigIdx]);
        int centerIdx = Global.Average4Int(topLefIdx, topRigIdx, botRigIdx, botLefIdx);
        vertices[centerIdx] = Global.Average4Vector(vertices[topLefIdx], vertices[topRigIdx],
            vertices[botRigIdx], vertices[botLefIdx]);
        Debug.Log("centerIdx: " + centerIdx + "; " + vertices[centerIdx]);
        // vertices[centerIdx] = Global.GetJittered(vertices[centerIdx], normal, m_heightRange, reduction);
    }

    // (x, z) is the coordinate ofsome point that needs to be averaged by the four "diamond points" around it
    // Find it, set its height by the average plus some random value
    void DiamondStep(int currentIdx, int sideLength, float reduction)
    {
        int halfSideLength = sideLength / 2;

        int topIdx = currentIdx - halfSideLength * m_res;
        int rigIdx = currentIdx + halfSideLength;
        int botIdx = currentIdx + halfSideLength * m_res;
        int lefIdx = currentIdx - halfSideLength;
        Vector3 normal = new Vector3(0, 1, 0);

        //int onTopBorder = topIdx - m_res;
        //int onBotBorder = botIdx / m_res - (m_res - 1);
        //int onLefBorder = lefIdx % m_res;
        //int onRigBorder = rigIdx % m_res - (m_res - 1);

        // Check for edge cases where the current point is on the edge of the original sqaure
        if (topIdx < 0)
        {
            Debug.Log("topIdx: " + topIdx + "; ");
            Debug.Log("botIdx: " + botIdx + "; " + vertices[botIdx]);
            Debug.Log("rigIdx: " + rigIdx + "; " + vertices[rigIdx]);
            Debug.Log("lefIdx: " + lefIdx + "; " + vertices[lefIdx]);
            normal = Global.GetNormal(vertices[rigIdx], vertices[botIdx], vertices[lefIdx]);
            vertices[currentIdx] = new Vector3((vertices[lefIdx].x + vertices[rigIdx].x) / 2f, 0, (vertices[lefIdx].z + vertices[rigIdx].z) / 2f); // Global.Average3Vector(vertices[rigIdx], vertices[botIdx], vertices[lefIdx]);
        }
        else if (botIdx > m_res * m_res - 1)
        {
            Debug.Log("topIdx: " + topIdx + "; " + vertices[topIdx]);
            Debug.Log("botIdx: " + botIdx + "; " );
            Debug.Log("rigIdx: " + rigIdx + "; " + vertices[rigIdx]);
            Debug.Log("lefIdx: " + lefIdx + "; " + vertices[lefIdx]);
            normal = Global.GetNormal(vertices[topIdx], vertices[rigIdx], vertices[lefIdx]);
            vertices[currentIdx] = new Vector3((vertices[lefIdx].x + vertices[rigIdx].x) / 2f, 0, (vertices[lefIdx].z + vertices[rigIdx].z) / 2f); ;// Global.Average3Vector(vertices[topIdx], vertices[rigIdx], vertices[lefIdx]);
        }
        else if (rigIdx % m_res == m_res - 1)
        {
            Debug.Log("topIdx: " + topIdx + "; " + vertices[topIdx]);
            Debug.Log("botIdx: " + botIdx + "; " + vertices[botIdx]);
            Debug.Log("rigIdx: " + rigIdx + "; ");
            Debug.Log("lefIdx: " + lefIdx + "; " + vertices[lefIdx]);
            normal = Global.GetNormal(vertices[botIdx], vertices[lefIdx], vertices[topIdx]);
            vertices[currentIdx] = new Vector3((vertices[topIdx].x + vertices[botIdx].x) / 2f, 0, (vertices[topIdx].z + vertices[botIdx].z) / 2f); // Global.Average3Vector(vertices[botIdx], vertices[lefIdx], vertices[topIdx]);
        }
        else if (lefIdx % m_res == 0)
        {
            Debug.Log("topIdx: " + topIdx + "; " + vertices[topIdx]);
            Debug.Log("botIdx: " + botIdx + "; " + vertices[botIdx]);
            Debug.Log("rigIdx: " + rigIdx + "; " + vertices[rigIdx]);
            Debug.Log("lefIdx: " + lefIdx + "; ");
            normal = Global.GetNormal(vertices[topIdx], vertices[rigIdx], vertices[botIdx]);
            vertices[currentIdx] = new Vector3((vertices[topIdx].x + vertices[botIdx].x) / 2f, 0, (vertices[topIdx].z + vertices[botIdx].z) / 2f);//  Global.Average3Vector(vertices[topIdx], vertices[rigIdx], vertices[botIdx]);
        }
        else
        {
            Debug.Log("topIdx: " + topIdx + "; " + vertices[topIdx]);
            Debug.Log("botIdx: " + botIdx + "; " + vertices[botIdx]);
            Debug.Log("rigIdx: " + rigIdx + "; " + vertices[rigIdx]);
            Debug.Log("lefIdx: " + lefIdx + "; " + vertices[lefIdx]);
            normal = Global.GetNormal(vertices[topIdx], vertices[rigIdx], vertices[botIdx]);
           // normal = Global.GetNormal(vertices[topIdx], vertices[rigIdx], vertices[botIdx]);
            vertices[currentIdx] = new Vector3((vertices[topIdx].x + vertices[botIdx].x) / 2f, 0, (vertices[topIdx].z + vertices[botIdx].z) / 2f);//Global.Average4Vector(vertices[topIdx], vertices[rigIdx],
                                                                                                                                                  // vertices[botIdx], vertices[lefIdx]);
        }

        Debug.Log("currentIdx: " + currentIdx + "; " + vertices[currentIdx]);

        //vertices[currentIdx] = Global.GetJittered(vertices[currentIdx], normal, m_heightRange, reduction);
    }


    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
