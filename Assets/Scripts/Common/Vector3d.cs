using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Vector3d {
    public double x, y, z;
    public double magnitude;
    public double sqrMagnitude;

    public Vector3d(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        sqrMagnitude = x * x + y * y + z * z;
        magnitude = Math.Sqrt(sqrMagnitude);
    }

    public Vector3d(Vector3 vecf)
    {
        this.x = vecf.x;
        this.y = vecf.y;
        this.z = vecf.z;
        sqrMagnitude = x * x + y * y + z * z;
        magnitude = Math.Sqrt(sqrMagnitude);
    }

    public Vector3 ToVector3() {
        return new Vector3(
            (float)this.x,
            (float)this.y,
            (float)this.z
        );
    }

    public static Vector3d operator +(Vector3d a, Vector3d b) {
        return new Vector3d(
            a.x + b.x,
            a.y + b.y,
            a.z + b.z
        );
    }

    public static Vector3d operator -(Vector3d a, Vector3d b) {
        return new Vector3d(
            a.x - b.x,
            a.y - b.y,
            a.z - b.z
        );
    }

    public static Vector3d operator *(Vector3d a, double b) {
        return new Vector3d(
            a.x * b,
            a.y * b,
            a.z * b
        );
    }

    public static Vector3d operator /(Vector3d a, double b) {
        return new Vector3d(
            a.x / b,
            a.y / b,
            a.z / b
        );
    }

    public static implicit operator Vector3(Vector3d a) {
        return a.ToVector3();
    }
}
