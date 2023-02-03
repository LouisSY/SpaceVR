using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitSolverManager : MonoBehaviour
{
    public const string TAG = "_solver_mgr";
    public enum OrbitalSolvers
    {
        PatchedConic,
        NBody
    }

    public OrbitalSolvers Solver;

    public OrbitalSolverBase GetSolverInstance(CelestialBodyBase body) {
        switch (Solver)
        {
            case OrbitalSolvers.PatchedConic:
                return new PatchedConicSolver(body);
            case OrbitalSolvers.NBody:
                return new NBodySolver(body);
            default:
                return null;
        }
    }

    public static OrbitalSolverBase GetFallbackSolver(CelestialBodyBase body) {
        return new PatchedConicSolver(body);
    }

    void Awake() {
        this.tag = TAG;
        
    }
}
