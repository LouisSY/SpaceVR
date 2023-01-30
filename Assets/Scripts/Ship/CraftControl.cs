using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CraftControl
{
    [ReadOnly]
    public float ThrottleRatio = 0f;

    [Tooltip("Maximum torque (kNm) on each axis that reaction wheel can apply")]
    public Vector3 MaxFlywheelTorque = Vector3.one;

    [Tooltip("Thruster's thrust in kN")]
    public float MaxThrust = 1f;
    public float DeltaThrustRatio = 0.1f;

    [Tooltip("RCS thrust (N)")]
    public Vector3 RCSThrust = Vector3.one;

    public float GetAcceleration(MassObject obj) {
        // a = r * F * 1000 / m / 1000
        if (ThrottleRatio <= float.Epsilon) {
            return 0f;
        }
        return ThrottleRatio * MaxThrust / PhysicsConstant.AbsoluteMass(obj.Mass);
    }

    public void IncreaseThrottle() {
        ThrottleRatio += DeltaThrustRatio;
        if (ThrottleRatio > 1f) {
            ThrottleRatio = 1f;
        }
    }

    public void DecreaseThrottle() {
        ThrottleRatio -= DeltaThrustRatio;
        if (ThrottleRatio < 0) {
            ThrottleRatio = 0;
        }
    }
}
