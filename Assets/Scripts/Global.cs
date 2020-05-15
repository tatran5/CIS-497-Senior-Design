using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global 
{
    
    public static Vector3 GetJittered(Vector3 point, Vector3 direction, float heightRange, float reduction)
    {
        return point + Random.Range(-heightRange, heightRange) / 5 / reduction / reduction * direction;
    }

    public static Vector3 GetNormal(Vector3 v0, Vector3 v1, Vector3 v2)
    {
        Vector3 v01 = v1 - v0;
        Vector3 v02 = v2 - v0;
        return Vector3.Normalize(Vector3.Cross(v01, v02));
    }

    public static int Average4Int(int x, int y, int z, int w)
    {
        return (x + y + z + w) / 4;
    }

    public static float Average4Float(float x, float y, float z, float w)
    {
        return (x + y + z + w) / 4f;
    }

    public static Vector3 Average4Vector(Vector3 x, Vector3 y, Vector3 z, Vector3 w)
    {
        return (x + y + z + w) / 4f;
    }

    public static Vector3 Average2Vector(Vector3 x, Vector3 y)
    {
        return (x + y) / 2f;
    }

    public static Vector3 Average3Vector(Vector3 x, Vector3 y, Vector3 z)
    {
        return (x + y + z) / 3f;
    }

    public static int From2DCoordinateTo1DIndex(int z, int x, int res)
    {
        return z * res + x;
    }
}
