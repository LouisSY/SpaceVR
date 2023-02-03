using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NBodySolver : OrbitalSolverBase
{
    public NBodySolver(CelestialBodyBase body): base(body)
    {
        SolverID = OrbitSolverManager.OrbitalSolvers.NBody;
    }

    public override void AddLocalAcceleration(Vector3 acceleration)
    {
        base.AddLocalAcceleration(acceleration);
        assigned.Rigid.AddForce(assigned.Frame.TransformGlobalVelocity(), ForceMode.VelocityChange);
    }

    public override void AddGlobalAcceleration(Vector3 acceleration)
    {
        base.AddGlobalAcceleration(acceleration);
        assigned.Rigid.AddForce(acceleration, ForceMode.Acceleration);
    }

    public override void SolveTrajectory()
    {
        if (assigned.Anchored) {
            return;
        }

        assigned.Frame.SyncState();

        Vector3 a = Vector3.zero;
        float magnitude = 0f;
        CelestialBody center_soi = null;
        Matrix4x4 T = Matrix4x4.Translate(assigned.transform.position);

        foreach (var celestia in assigned.Celestias)
        {
            Vector3 a_mass = celestia.CalculateAcceleration(assigned.transform.position);
            if (a_mass.magnitude > magnitude) {
                magnitude = a_mass.magnitude;
                center_soi = celestia;
            }
            DebugTools.DrawDirection(T, a_mass * 4e3f, Color.red, baseLen: 0f);
            a += a_mass;
        }

        assigned.SOICenter = center_soi;
        assigned.Frame.ReferenceTo(center_soi.Frame);

        assigned.CurrentOrbitalState.UpdateState(assigned);

        assigned.Acceleration = a;

        DebugTools.DrawVelocity(assigned);
        DebugTools.DrawDirection(T, a * 4e3f, Color.green, baseLen: 0f);
        assigned.Rigid.AddForce(a, ForceMode.Acceleration);
        assigned.AccelerationMagnitude = a.magnitude;
    }
}
