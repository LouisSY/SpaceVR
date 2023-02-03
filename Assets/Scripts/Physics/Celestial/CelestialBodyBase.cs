using System.Collections;
using System.Collections.Generic;
using MyProject.Common;
using UnityEngine;

public class CelestialBodyBase : MassObject
{
    public IList<CelestialBody> Celestias { get; private set; }

    [ReadOnly]
    public Vector3 Acceleration;

    [ReadOnly]
    public float AccelerationMagnitude;

    public CelestialBody SOICenter { get; set; }

    public bool EnableSimulation;

    public InertialFrame Frame { get; protected set; }

    [Header("Info")]
    public OrbitalState CurrentOrbitalState;

    public OrbitalSolverBase Solver { get; set; }

    public bool Anchored = true;
    public bool IsRoot = false;

    public float SOIRadius = 0f;

    public CelestialBodyBase()
    {
        Celestias = new List<CelestialBody>();
    }

    protected void RegisterBodies() {
        GameObject[] gos = GameObject.FindGameObjectsWithTag(MassObject.TAG);
        foreach (var go in gos)
        {
            CelestialBody ob;
            if (go.TryGetComponent<CelestialBody>(out ob)) {
                if (!ob.EnableSimulation) {
                    continue;
                }
                if (go.name == this.name) {
                    continue;
                }
                Celestias.Add(ob);
            }
        }
    }
    
}
