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
    public float TrueAnomaly;

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

    [ReadOnly]
    public GameObject MainInfluencer;

    public float AngularVelocity { get; private set; }

    // Transformation between Unity and Geocentric coordinate systems.
    // N.B. This is a orthonormal matrix
    // e.g., G2U: Geocentric -> Unity
    public static readonly Matrix4x4 U2G = new Matrix4x4(
        new Vector4( 0, 1, 0, 0),
        new Vector4( 0, 0, 1, 0),
        new Vector4(-1, 0, 0, 0),
        new Vector4( 0, 0, 0, 1)
    );

    public static readonly Matrix4x4 G2U = new Matrix4x4(
        new Vector4( 0, 0,-1, 0),
        new Vector4( 1, 0, 0, 0),
        new Vector4( 0, 1, 0, 0),
        new Vector4( 0, 0, 0, 1)
    );

    public void UpdateState(CelestialBodyBase self) {
        CelestialBody soi = self.SOICenter;
        if (soi == null) {
            return;
        } 

        RelativeVelocity = self.Frame.State.Velocity;
        RelativeSpeed = RelativeVelocity.magnitude;

        /* Orbital elements from state vector - ref: Curtis, H. Section 4.4 */

        MainInfluencer = soi.gameObject;
        LocalPosition = self.transform.position - soi.transform.position;

        if (RelativeVelocity.magnitude < 1e-4f || float.IsNaN(RelativeSpeed)) {
            return;
        }
        
        float mu = soi.Mu;
        Altitude = LocalPosition.magnitude;
        Vector3 v = U2G.MultiplyPoint3x4(RelativeVelocity);
        Vector3 r = U2G.MultiplyPoint3x4(LocalPosition);
        float v_radial = Vector3.Dot(r, v) / r.magnitude;
        Vector3 h = Vector3.Cross(r, v);
        Vector3 N = Vector3.Cross(Vector3.forward, h);
        float h_mag = h.magnitude;
        
        Vector3 e = (v.sqrMagnitude * r / mu) - r.normalized - Vector3.Dot(r, v) * v / mu;
        Eccentricity = e.magnitude;
        PerigeeDirection = G2U.MultiplyPoint3x4(e).normalized;
        if (PerigeeDirection.magnitude <= float.Epsilon) {
            PerigeeDirection = Vector3.right;
        }

        SpecificAngularMomentum = h_mag;
        PeriapsisAltitude = h.sqrMagnitude / (soi.Mu * (1f + Eccentricity));
        if (Eccentricity >= 1) {
            Period = float.PositiveInfinity;
            SemimajorAxisLength = float.PositiveInfinity;
            AngularVelocity = 0f;
            ApoapsisAltitude = float.PositiveInfinity;
            Inclination = 90f - Mathf.Acos(Vector3.Dot(v, Vector3.forward)) * Mathf.Rad2Deg;
            Ascension = 0f;
            PerigeeArg = 0f;
            return;
        }

        NodeLine = G2U.MultiplyPoint3x4(N).normalized;
        Inclination = Mathf.Acos(h.z / h_mag) * Mathf.Rad2Deg;
        Ascension = Mathf.Acos(N.x/N.magnitude) * Mathf.Rad2Deg;
        if (N.y < 0) {
            Ascension = 360f - Ascension;
        }

        float ne = Vector3.Dot(N, e);
        PerigeeArg = Mathf.Acos(ne / (N.magnitude * Eccentricity)) * Mathf.Rad2Deg;

        if (e.z < 0) {
            PerigeeArg = 360f - PerigeeArg;
        }

        TrueAnomaly = Mathf.Acos(Vector3.Dot(e.normalized, r.normalized)) * Mathf.Rad2Deg;
        if (v_radial < 0) {
            TrueAnomaly = 360f - TrueAnomaly;
        }

        ApoapsisAltitude = h.sqrMagnitude / (soi.Mu * (1f - Eccentricity));
        float a = 0.5f * (ApoapsisAltitude + PeriapsisAltitude);
        SemimajorAxisLength = a;
        Period = 2f * Mathf.PI / Mathf.Sqrt(soi.Mu) * Mathf.Sqrt(a * a * a);
        AngularVelocity = 360f * Mathf.Deg2Rad / Period;
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

    public Vector3 GetOrbitalAltitude(CelestialBody body, float anomaly) {
        float h = SpecificAngularMomentum;
        float e = Eccentricity;
        float r = h * h / (body.SOICenter.Mu * (1f + e * Mathf.Cos(anomaly * Mathf.Deg2Rad)));
        Quaternion rot = Quaternion.FromToRotation(Vector3.right, PerigeeDirection.normalized);
        return rot * new Vector3(
            r * Mathf.Cos(anomaly),
            0f,
            r * Mathf.Sin(anomaly)
        );
    }

    public Quaternion Qxx {
        get {
            return   Quaternion.Euler(0, 0, PerigeeArg)
                   * Quaternion.Euler(Inclination, 0, 0)
                   * Quaternion.Euler(0, 0, Ascension);
        }
    }
}
