using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorExtension
{
    public static bool IsNan(this Vector3 v) {
        return float.IsNaN(v.x) || float.IsNaN(v.z) || float.IsNaN(v.y);
    }
}
