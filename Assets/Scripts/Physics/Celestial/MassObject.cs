using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MassObject : MonoBehaviour
{
    public const string TAG = "_mass_obj";

    public float Mass = 1f;

    Rigidbody self_body;

    public Rigidbody Rigid { get => self_body; }

    public float Mu {
        get {
            return PhysicsConstant.G * Mass;
        }
    }

    public double Mud {
        get {
            return (double)PhysicsConstant.G * (double)Mass;
        }
    }

    void Awake() {
        if (!TryGetComponent<Rigidbody>(out this.self_body)) {
            throw new System.Exception("Mass object is not rigid body");
        }
        this.tag = TAG;
    }

    public Vector3 CalculateGravitational(Vector3 pos, float mass) {
        Vector3 r =  this.transform.position - pos;
        Vector3 F = r.normalized * Mass * mass * PhysicsConstant.G / r.sqrMagnitude;
        
        return F;
    }

    public Vector3 CalculateAcceleration(Vector3 pos) {
        Vector3 r = this.transform.position - pos;
        Vector3d F = new Vector3d(r.normalized) * Mud / (double)r.sqrMagnitude;
        
        return F;
    }

    public static Vector3 AccelerationBetween(float center_mass, Vector3 pos, Vector3 centered) {
        Vector3 r =  pos - centered;
        Vector3d F = new Vector3d(r.normalized) * (double)center_mass * (double)PhysicsConstant.G / (double)r.sqrMagnitude;
        
        return F;
    }
}
