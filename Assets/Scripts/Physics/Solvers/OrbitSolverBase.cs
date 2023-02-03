using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OrbitalSolverBase
{
    public OrbitSolverManager.OrbitalSolvers SolverID { get; protected set; }
    protected CelestialBodyBase assigned;
    public OrbitalSolverBase(CelestialBodyBase body)
    {
        this.assigned = body;
    }

    public virtual void AddLocalAcceleration(Vector3 acceleration) {
        assigned.Frame.ApplyAcceleration(acceleration, Time.fixedDeltaTime);
    }

    public virtual void AddGlobalAcceleration(Vector3 acceleration) {
        assigned.Frame.ApplyAcceleration(acceleration, Time.fixedDeltaTime);
    }

    public abstract void SolveTrajectory();
}
