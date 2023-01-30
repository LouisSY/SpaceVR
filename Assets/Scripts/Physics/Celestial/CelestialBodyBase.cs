using System.Collections;
using System.Collections.Generic;
using MyProject.Common;
using UnityEngine;

public class CelestialBodyBase : MassObject
{
    public Vector3 Acceleration { get; protected set; }

    public CelestialBody SOICenter { get; protected set; }

    public bool EnableSimulation;

    public InertialFrame Frame { get; protected set; }

    [Header("Info")]
    public OrbitalState CurrentOrbitalState;
}
