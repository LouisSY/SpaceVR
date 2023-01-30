// #define TIMESTEPPING

using System;
using System.Collections;
using System.Collections.Generic;
using MyProject.Common;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CelestialBody : CelestialBodyBase
{
    private IList<CelestialBody> celestias;

    [Tooltip("Use simplified solver rather than the default N-body solver")]
    public bool SimplifiedModel = true;
    
    private Vector3 OrbitalNormal = Vector3.up;

    [Header("Orbiting")]
    public bool OrbitingParent = false;
    public float DeclinationDegree = 0f;

    [Range(0f, 1f)]
    public float Eccentricity = 0f;

    [Header("Root")]

    [Tooltip("Specify whether the root celestial body should experience the perturbation")]
    public bool Anchored = true;

    private bool __start_orbit;

    private bool isRoot;

    public CelestialBody() : base()
    {
        celestias = new List<CelestialBody>();
        Frame = new InertialFrame();
    }

    void Start() {
        isRoot = transform.root.name == this.name;
        Debug.Log(string.Format("{0}:{1}", name, isRoot));
        __start_orbit = true;
        Frame.AssignCenter(this);
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
                celestias.Add(ob);
            }
        }
    }

    public void StartOrbiting() {
        if (this.transform.root.name != this.name) {
            return;
        }
        this.do_orbit(null, Vector3.zero, Quaternion.identity);
    }

    private IEnumerable<CelestialBody> GetChildrenOrbital() {
        for (int i = 0; i < this.transform.childCount; i++)
        {
            var child_go = this.transform.GetChild(i);
            CelestialBody ob;
            if (!child_go.TryGetComponent<CelestialBody>(out ob)) {
                continue;
            }
            
            yield return ob;
        }
    }

    public void do_orbit(CelestialBody soi, Vector3 parent_vel, Quaternion parentR) {
        if (!this.OrbitingParent) {
            foreach (var ob in GetChildrenOrbital())
            {
                ob.do_orbit(this, parent_vel, parentR);
            }
            return;
        }
    
        Vector3 tangent = Quaternion.Euler(DeclinationDegree, 0f, 0f) * Vector3.forward;

        SOICenter = soi;
        OrbitalNormal = (parentR * transform.rotation * tangent).normalized;

        // Estimate velocity direction
        double mu = soi.Mud + this.Mud;
        Vector3 h = OrbitalNormal;
        Vector3 r = transform.position - SOICenter.transform.position;
        double a = r.magnitude * (2d / (1d + (double)Eccentricity));
        double rarp = r.sqrMagnitude * (1d - Eccentricity) / (1d + Eccentricity);
        double hsqr = 2f * mu * (rarp / a);
        Vector3 v = Vector3.Cross(h, r);
        v = (parentR * v).normalized * (float)(Math.Sqrt(hsqr) / (double)r.magnitude);
        v += parent_vel;


        Debug.Log(string.Format("v_init('{0}'): {1}", name, v));
        Frame.Center.Rigid.AddForce(v, ForceMode.VelocityChange);

        foreach (var ob in GetChildrenOrbital())
        {
            ob.do_orbit(this, v, parentR * transform.rotation);
        }
    }

    // Plot the Keplerian orbit.
    // N.B. This method use constant 360 steps to plot any scale invariant orbit, 
    //      this is particularly desirable (computational excellence) for {para|hyper}bola orbit (e >= 1)
    //      However, it suffer from orbital perturbation.
    public OrbitalTrajectory PredictTrajectory(int max_step=100000, float time_step = 1, bool local_frame = false) {
        OrbitalTrajectory info = new OrbitalTrajectory();
        Quaternion rot = Quaternion.FromToRotation(Vector3.right, CurrentOrbitalState.PerigeeDirection.normalized);
        Matrix4x4 T = Matrix4x4.Translate(SOICenter.Rigid.transform.position);
        float theta = 0;
        while (theta < 360f) {
            float thetaR = theta * Mathf.Deg2Rad;
            float h = CurrentOrbitalState.SpecificAngularMomentum;
            float e = CurrentOrbitalState.Eccentricity;
            float r = h * h / (SOICenter.Mu * (1f + e * Mathf.Cos(thetaR)));
            Vector3 pos2d = new Vector3(
                r * Mathf.Cos(thetaR),
                0f,
                r * Mathf.Sin(thetaR)
            );
            info.AddPathNode(T.MultiplyPoint(rot * pos2d), 0f);
            theta += 1f;
        }
        info.StableOrbit = CurrentOrbitalState.Eccentricity < 1f;
        info.Pe = T.MultiplyPoint(rot * CurrentOrbitalState.GetPeriapsisRP(SOICenter));
        info.Ap = T.MultiplyPoint(rot * CurrentOrbitalState.GetApoapsisRP(SOICenter));
        return info;
    }

    float time = 0f;
    public void FixedUpdate() {
        if (!EnableSimulation) {
            return;
        }

        if (__start_orbit) {
            StartOrbiting();
            __start_orbit = false;
            return;
        }

        if (isRoot && Anchored) {
            return;
        }

        Vector3 a = Vector3.zero;
        float magnitude = 0f;
        CelestialBody center_soi = null;
        Matrix4x4 T = Matrix4x4.Translate(transform.position);

        foreach (var celestia in this.celestias)
        {
            var referenced = celestia.Frame.Center;
            var pred = referenced.transform.position + referenced.Rigid.velocity * Time.fixedDeltaTime;
            Vector3 a_mass = MassObject.AccelerationBetween(referenced.Mass, pred, Rigid.position);
            // Debug.Log(string.Format("{0} -> {1}: {2}", name, referenced.name, a_mass.normalized));
            // Debug.DrawLine(transform.position, T.MultiplyPoint(a_mass * 4f), Color.red, Time.fixedDeltaTime, false);
            if (a_mass.magnitude > magnitude) {
                magnitude = a_mass.magnitude;
                center_soi = referenced;
            }
            if (!SimplifiedModel){
                DebugTools.DrawDirection(T, a_mass * 4e3f, Color.red, baseLen: 0f);
                a += a_mass;
            }
        }

        SOICenter = center_soi;

        CurrentOrbitalState.UpdateState(this);

        if (!SimplifiedModel) {
            Acceleration = a;
        }
        else {
            Acceleration = SOICenter.CalculateAcceleration(Frame.Center.Rigid.position);
        }
        DebugTools.DrawVelocity(this);
        DebugTools.DrawDirection(T, Acceleration * 4e3f, Color.green, baseLen: 0f);
        this.Rigid.AddForce(Acceleration, ForceMode.Acceleration);
    }
}
