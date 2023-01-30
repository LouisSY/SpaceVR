using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PoseControllerBase
{
    protected SpacecraftBase craft;

    public PoseControllerBase(SpacecraftBase craft)
    {
        this.craft = craft;
    }

    protected Vector3 WithTorqueKinematics(Vector3 w) {
        var body = craft.Body;
        Quaternion q = body.transform.rotation * body.inertiaTensorRotation;
        Vector3 T = q * Vector3.Scale(body.inertiaTensor, Quaternion.Inverse(q) * (w - body.angularVelocity / Time.fixedDeltaTime));
        T = T.normalized * 1000f;
        T.Scale(craft.Kinematics.MaxFlywheelTorque);
        return craft.ToAcceleration(T);
    }
}
