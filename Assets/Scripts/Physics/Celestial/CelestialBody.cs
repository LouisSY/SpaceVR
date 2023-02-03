// #define TIMESTEPPING

using System;
using System.Collections;
using System.Collections.Generic;
using MyProject.Common;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CelestialBody : CelestialBodyBase
{

    [Header("Orbiting")]
    public bool OrbitingParent = false;
    public float Inclination = 0f;

    [Range(0f, 1f)]
    public float Eccentricity = 0f;

    private bool __start_orbit;

    public CelestialBody() : base()
    {
        Frame = new InertialFrame();
    }

    void Start() {
        IsRoot = transform.root.name == this.name;
        Debug.Log(string.Format("{0}:{1}", name, IsRoot));
        __start_orbit = true;
        Frame.AssignCenter(this);
        RegisterBodies();

        var solverObj = GameObject.FindGameObjectWithTag(OrbitSolverManager.TAG);

        if (solverObj == null) {
            Debug.LogError("No solver manager detected. Use fallback solver: PatchedConic");
            Solver = OrbitSolverManager.GetFallbackSolver(this);
        }
        else {
            var solverMgr = solverObj.GetComponent<OrbitSolverManager>();
            Solver = solverMgr.GetSolverInstance(this);
        }
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

    public void StartOrbiting() {
        if (!IsRoot) {
            return;
        }
        this.do_orbit(null, Vector3.zero, Quaternion.identity);
    }

    public void do_orbit(CelestialBody soi, Vector3 parent_vel, Quaternion parentR) {
        if (!this.OrbitingParent) {
            foreach (var ob in GetChildrenOrbital())
            {
                ob.do_orbit(this, parent_vel, parentR);
            }
            return;
        }
    
        Vector3 tangent = Quaternion.Euler(90f + Inclination, 0f, 0f) * Vector3.forward;

        SOICenter = soi;
        if (IsRoot) {
            Frame.ReferenceTo(null);
        }
        else {
            Debug.Log(soi.Frame.Center.name);
            Frame.ReferenceTo(soi.Frame);
        }

        // Estimate velocity direction
        double mu = soi.Mud + this.Mud;
        Vector3 h = (parentR * transform.rotation * tangent).normalized;
        Vector3 r = transform.position - SOICenter.transform.position;
        double e = (double)Eccentricity + 1e-3;
        double a = r.magnitude * (2d / (1d + e));
        double rarp = r.sqrMagnitude * (1d - e) / (1d + e);
        double hsqr = 2f * mu * (rarp / a);
        Vector3 v = Vector3.Cross(h,r);
        v = (parentR * v).normalized * (float)(Math.Sqrt(hsqr) / (double)r.magnitude);

        Debug.Log(string.Format("v_init('{0}'): {1}", name, v));
        Solver.AddGlobalAcceleration(v / Time.fixedDeltaTime);

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
        Quaternion rot = CurrentOrbitalState.Qxx;
        Matrix4x4 T = Matrix4x4.Translate(SOICenter.Rigid.transform.position);
        float theta = 0;
        while (theta < 360f) {
            float thetaR = theta * Mathf.Deg2Rad;
            float h = CurrentOrbitalState.SpecificAngularMomentum;
            float e = CurrentOrbitalState.Eccentricity;
            float r = h * h / (SOICenter.Mu * (1f + e * Mathf.Cos(thetaR)));
            Vector3 pos2d = new Vector3(
                r * Mathf.Cos(thetaR),
                r * Mathf.Sin(thetaR),
                0f
            );
            pos2d = OrbitalState.G2U.MultiplyPoint3x4(rot * pos2d);
            info.AddPathNode(T.MultiplyPoint3x4(pos2d), 0f);
            theta += 1f;
        }
        info.StableOrbit = CurrentOrbitalState.Eccentricity < 1f;
        info.Pe = T.MultiplyPoint(rot * CurrentOrbitalState.GetPeriapsisRP(SOICenter));
        info.Ap = T.MultiplyPoint(rot * CurrentOrbitalState.GetApoapsisRP(SOICenter));
        return info; 
    }

    Vector3 prev_a = Vector3.zero;
    public void FixedUpdate() {
        if (!EnableSimulation) {
            return;
        }

        if (__start_orbit) {
            StartOrbiting();
            __start_orbit = false;
            return;
        }

        Solver.SolveTrajectory();
    }

    void OnDrawGizmos() {
        DebugTools.DrawSOI(this);
    }
}
