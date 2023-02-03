using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatchedConicSolver : OrbitalSolverBase
{
    public PatchedConicSolver(CelestialBodyBase body): base(body)
    {
        SolverID = OrbitSolverManager.OrbitalSolvers.PatchedConic;
        if (body.SOIRadius < float.Epsilon) {
            body.SOIRadius = body.Mu / 0.002f;
        }
    }

    public override void AddLocalAcceleration(Vector3 acceleration)
    {
        base.AddLocalAcceleration(acceleration);
        assigned.CurrentOrbitalState.UpdateState(assigned);
    }

    public override void AddGlobalAcceleration(Vector3 acceleration)
    {
        base.AddGlobalAcceleration(acceleration);
        assigned.CurrentOrbitalState.UpdateState(assigned);
    }

    float i = Mathf.Deg2Rad;

    public override void SolveTrajectory()
    {
        if (assigned.Anchored || assigned.IsRoot) {
            return;
        }

        CelestialBody center_soi = assigned.SOICenter;
        var state = assigned.CurrentOrbitalState;
        
        if (assigned.Frame.State.Velocity.magnitude < 1e-4) {
            return;
        }

        foreach (var celestia in assigned.Celestias)
        {
            float d = (assigned.transform.position - celestia.transform.position).magnitude;
            if (d > celestia.SOIRadius) {
                continue;
            }

            float currentR = center_soi.SOIRadius;
            float currentD = (assigned.transform.position - center_soi.transform.position).magnitude;
            // SOI intersect, select minimum mass
            if (currentD > currentR || center_soi.Mass >= celestia.Mass) {
                center_soi = celestia;
                continue;
            }
        }

        var a = center_soi.CalculateAcceleration(assigned.transform.position);
        assigned.Acceleration = a;
        assigned.AccelerationMagnitude = a.magnitude;

        if (center_soi.name != assigned.SOICenter.name) {
            Debug.LogFormat("SOI: {0} -> {1}", assigned.SOICenter.name, center_soi.name);
            assigned.SOICenter = center_soi;
            assigned.Frame.ReferenceTo(center_soi.Frame);
            state.UpdateState(assigned);
            return;
        }
        
        Quaternion Qxx = state.Qxx;
        var rp = (assigned.transform.position - center_soi.transform.position);
        float h = state.SpecificAngularMomentum;
        float e = state.Eccentricity;
        float crx = Vector3.Cross(rp.normalized, state.PerigeeDirection).y;
        float dot = Vector3.Dot(rp.normalized, state.PerigeeDirection);
        float anomaly = Mathf.Acos(dot);
        float radv = assigned.CurrentOrbitalState.AngularVelocity / PhysicsConstant.TicksPerSecond;
        float v_ = center_soi.Mu / h;
        float r_ = h * h / (center_soi.Mu * (1f + e * dot));
        
        if (crx < 0) {
            anomaly = 2 * Mathf.PI - anomaly;
        }

        Vector3 dr = new Vector3(
            -Mathf.Sin(anomaly),
            e + Mathf.Cos(anomaly),
            0f
        );

        Vector3 r = new Vector3(
            dot,
            Mathf.Sin(anomaly),
            0f
        );

        dr = OrbitalState.G2U.MultiplyPoint3x4(Qxx * dr) * v_;
        r = OrbitalState.G2U.MultiplyPoint3x4(Qxx * r) * r_;

        // // position correction (due to floating point precision)
        // if (anomaly > i &&  (r - rp).magnitude > 1f) {
        //     assigned.Rigid.MovePosition(r + center_soi.transform.position);
        //     i += Mathf.Deg2Rad;
        // }

        state.RelativeVelocity = dr;
        state.RelativeSpeed = dr.magnitude;
        state.LocalPosition = r;
        state.Altitude = r.magnitude;


        assigned.Frame.UpdateState(r, dr);
        assigned.Rigid.velocity = assigned.Frame.TransformGlobalVelocity();;
        assigned.CurrentOrbitalState.TrueAnomaly = anomaly * Mathf.Rad2Deg;
        DebugTools.DrawVelocity(assigned);
    }
}
