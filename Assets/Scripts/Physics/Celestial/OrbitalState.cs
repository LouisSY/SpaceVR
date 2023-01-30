using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OrbitalState
{
    [ReadOnly]
    [Tooltip("Velocity relative to it's main influencer")]
    public Vector3 RelativeVelocity;

    [ReadOnly]
    [Tooltip("Speed relative to it's main influencer")]
    public float RelativeSpeed;

    [ReadOnly]
    public Vector3 LocalPosition;

    [ReadOnly]
    public float Altitude;


    [ReadOnly]
    public float Eccentricity;

    [ReadOnly]
    [Tooltip("Directional vector point to Perigee (i.e., eccentricity vector)")]
    public Vector3 PerigeeDirection;

    [ReadOnly]
    [Tooltip("Node line point to the ascending node (i.e., eccentricity vector)")]
    public Vector3 NodeLine;

    [ReadOnly]
    [Tooltip("Inclination in degree of orbital plane")]
    public float Inclination;

    [ReadOnly]
    [Tooltip("Sweep angle from x-axis to the ascension node in the direction of satellite's velocity")]
    public float Ascension;

    [ReadOnly]
    [Tooltip("Perigee Argument: the sweep angle from ascension node to perigee in the direction of satellite's velocity")]
    public float PerigeeArg;

    [ReadOnly]
    public float SpecificAngularMomentum;

    [ReadOnly]
    public float SemimajorAxisLength;

    [ReadOnly]
    public float Period;

    [ReadOnly]
    [Tooltip("Periapsis altitude in Megameter")]
    public float PeriapsisAltitude;


    [ReadOnly]
    [Tooltip("Apoapsis altitude in Megameter")]
    public float ApoapsisAltitude;




    // Transformation between Unity and Geocentric coordinate systems.
    // N.B. This is a orthonormal matrix
    private Matrix4x4 GFT = new Matrix4x4(
        new Vector4(1,0,0,0),
        new Vector4(0,0,1,0),
        new Vector4(0,1,0,0),
        new Vector4(0,0,0,1)
    );

    public void UpdateState(CelestialBody self) {
        CelestialBody soi = self.SOICenter;
        if (soi == null) {
            return;
        }

        RelativeVelocity = self.Frame.GetRelativeVelocity(soi.Frame);
        RelativeSpeed = RelativeVelocity.magnitude;

        /* Orbital elements from state vector - ref: Curtis, H. Section 4.4 */

        LocalPosition = self.transform.position - soi.transform.position;
        Altitude = LocalPosition.magnitude;
        Vector3 v = GFT.MultiplyPoint3x4(RelativeVelocity);
        Vector3 r = GFT.MultiplyPoint3x4(LocalPosition);
        float v_radial = Vector3.Dot(r, v) / r.magnitude;
        Vector3 h = Vector3.Cross(r, v);
        Vector3 N = Vector3.Cross(Vector3.forward, h);
        float h_mag = h.magnitude;

        // orbital is co-planar to the reference plane.
        if (N.magnitude <= float.Epsilon) {
            N = Vector3.forward;
        }

        NodeLine = GFT.MultiplyPoint3x4(N);
        SpecificAngularMomentum = h_mag;
        Inclination = Mathf.Acos(h.z / h_mag) * Mathf.Rad2Deg;
        Ascension = Mathf.Acos(N.x/N.magnitude) * Mathf.Rad2Deg;
        if (N.y < 0) {
            Ascension = 360f - Ascension;
        }

        float mu = soi.Mu;
        Vector3 e = (v.sqrMagnitude * r / mu) - r.normalized - Vector3.Dot(r, v) * v / mu;
        Eccentricity = e.magnitude;
        PerigeeDirection = GFT.MultiplyPoint3x4(e).normalized;
        PerigeeArg = Mathf.Acos(Vector3.Dot(N, e) / (N.magnitude * Eccentricity)) * Mathf.Rad2Deg;

        if (PerigeeDirection.z < 0) {
            PerigeeArg = 360f - PerigeeArg;
        }

        PeriapsisAltitude = h.sqrMagnitude / (soi.Mu * (1f + Eccentricity));
        ApoapsisAltitude = h.sqrMagnitude / (soi.Mu * (1f - Eccentricity));
        float a = 0.5f * (ApoapsisAltitude + PeriapsisAltitude);
        SemimajorAxisLength = a;
        Period = 2f * Mathf.PI / Mathf.Sqrt(soi.Mu) * Mathf.Sqrt(a * a * a);
    }


    // Get Periapsis position on reference orbital plane
    public Vector3 GetPeriapsisRP(CelestialBody soi) {
        float h = SpecificAngularMomentum;
        float e = Eccentricity;
        float r = h * h / (soi.Mu * (1f + e));
        Vector3 pos2d = new Vector3(r ,0f,0f);
        return pos2d;
    }

    // Get apoapsis position on reference orbital plane
    public Vector3 GetApoapsisRP(CelestialBody soi) {
        float h = SpecificAngularMomentum;
        float e = Eccentricity;
        float r = h * h / (soi.Mu * (1f - e));
        Vector3 pos2d = new Vector3(-r ,0f,0f);
        return pos2d;
    }
}
