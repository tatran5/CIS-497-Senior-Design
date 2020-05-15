using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Triangle
{
    public Vector3 mV1;
    public Vector3 mV2;
    public Vector3 mV3;
    public int mDepth;

    public Triangle() { }
    public Triangle(Vector3 v1, Vector3 v2, Vector3 v3, int depth)
    {
        mV1 = v1;
        mV2 = v2;
        mV3 = v3;
        mDepth = depth;
    }
}

public class MidpointDisplacementTerrain : MonoBehaviour
{
    public int mDepth = 2;
    
    float mSideLength = 1; //test

    Mesh mesh;
    Vector3[] mMeshVertices; // have to store this otherwise mesh will not show up because of local stack
    int[] mMeshTriangles;

    Vector3 mWorldUp;

    List<Triangle> mTriangles;
 

    // Start is called before the first frame update
    void Start()
    {
        if (mDepth < 1)
            Debug.Log("mDepth must be at least 1, which will render a triangle");
        else
        {
            mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;
            mWorldUp = new Vector3(0, 1, 0);
            
            mTriangles = new List<Triangle>();

            Triangle mainTriangle = new Triangle(new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 0), 1);
            mTriangles.Add(mainTriangle);
            int numTris = 0;
            int count = 0;
            while (mTriangles.Count != numTris)
            {
                count++;
                numTris = mTriangles.Count;
                for (int i = 0; i < numTris; i++)
                {
                    Subdivide(mTriangles[i]);
                }
            }
            Debug.Log(count);
            CreateMeshTest();
        }
    }

    void Subdivide(Triangle tri)
    {
        if (tri.mDepth < mDepth)
        {
            mTriangles.Remove(tri);

            Vector3 midPoint1 = Global.Average2Vector(tri.mV2, tri.mV3);
            Vector3 midPoint2 = Global.Average2Vector(tri.mV3, tri.mV1);
            Vector3 midPoint3 = Global.Average2Vector(tri.mV1, tri.mV2);

            // offset midpoint height (y)
            midPoint1.y += mSideLength / Mathf.Pow(2, mDepth);
            midPoint2.y += mSideLength / Mathf.Pow(2, mDepth);
            midPoint3.y += mSideLength / Mathf.Pow(2, mDepth);

            Triangle tri1 = new Triangle(tri.mV1, midPoint3, midPoint2, tri.mDepth + 1);
            Triangle tri2 = new Triangle(tri.mV2, midPoint1, midPoint3, tri.mDepth + 1);
            Triangle tri3 = new Triangle(tri.mV3, midPoint2, midPoint1, tri.mDepth + 1);
            Triangle tri4 = new Triangle(midPoint1, midPoint2, midPoint3, tri.mDepth + 1);

            mTriangles.Add(tri1);
            mTriangles.Add(tri2);
            mTriangles.Add(tri3);
            mTriangles.Add(tri4);
        }
    }

    void CreateMeshTest()
    {
        mesh.Clear();
        mMeshVertices = new Vector3[mTriangles.Count * 3];
        mMeshTriangles = new int[mMeshVertices.Length];

        for (int i = 0; i < mTriangles.Count; i++)
        {
            mMeshVertices[i * 3] = mTriangles[i].mV1;
            mMeshVertices[i * 3 + 1] = mTriangles[i].mV2;
            mMeshVertices[i * 3 + 2] = mTriangles[i].mV3;

            mMeshTriangles[i * 3] = i * 3;
            mMeshTriangles[i * 3 + 1] = i * 3 + 1;
            mMeshTriangles[i * 3 + 2] = i * 3 + 2;
        }
        mesh.vertices = mMeshVertices;
         
        mesh.triangles = mMeshTriangles;
        mesh.RecalculateNormals();
    }
}
