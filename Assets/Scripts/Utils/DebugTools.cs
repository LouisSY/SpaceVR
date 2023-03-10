using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DebugTools
{
    public static void DrawVelocity(CelestialBodyBase body) {
        var rigid = body.Rigid;
        var relVel = body.Frame.State.Velocity;
        var T = Matrix4x4.Translate(rigid.position);
        var vel = T.MultiplyPoint(relVel * 100f);
        Debug.DrawLine(rigid.position, vel, Color.blue, Time.fixedDeltaTime, false);
    }

    public static void DrawDirection(Matrix4x4 transform, Vector3 v, Color color, float baseLen = 3f) {
        var v_ = transform.MultiplyPoint(v);
        Debug.DrawLine(transform.MultiplyPoint(Vector3.zero), v_ + baseLen * (v * 1e3f).normalized, color, Time.fixedDeltaTime, false);
    }

    public static void DrawSOI(CelestialBodyBase body) {
        Color color = Color.magenta;
        color.a = 0.5f;
        Gizmos.color = color;
        Gizmos.DrawSphere(body.transform.position, body.SOIRadius);
    }
}
