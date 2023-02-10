using System.Collections;
using System.Collections.Generic;
using MyProject.Common;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public abstract class SpacecraftBase : MonoBehaviour
{
    protected Keyboard keybord;

    [Header("Local Orbit Estimation")]
    public float LocalTimeStep = 0.4f;
    public int LocalMaxIteration = 1000;

    [Header("Global Orbit Estimation")]
    public float GlobalTimeStep = 0.8f;
    public int GlobalMaxIteration = 7000;

    [Header("Craft Control")]
    public CraftControl Kinematics;

    protected Rigidbody body;
    protected PoseStablizer poseStablizer;
    protected PoseAdjuster poseAdjuster;
    protected TranslationController transController;
    protected CelestialBody craft;

    protected LineRenderer line;

    public float Mass { get => this.craft.Mass; }
    public Rigidbody Body { get => this.body; }

    public SpacecraftBase()
    {
        poseStablizer = new PoseStablizer(this);
        poseAdjuster = new PoseAdjuster(this);
        transController = new TranslationController(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        keybord = Keyboard.current;
        body = GetComponent<Rigidbody>();
        craft = GetComponent<CelestialBody>();
        line = GameObject.FindGameObjectWithTag("trajectory").GetComponent<LineRenderer>();

        InputSystem.onEvent += onInputEvent;
    }

    protected abstract void onInputEvent(InputEventPtr ptr, InputDevice dev);

    protected Vector3 AttitudePrograde() {
        return craft.Frame.State.Velocity.normalized;
    }

    protected Vector3 AttitudeRetrograde() {
        var q = transform.rotation;
        return -(craft.Frame.State.Velocity.normalized);
    }

    protected Vector3 AttitudeNormal() {
        var relvel = craft.CurrentOrbitalState.RelativeVelocity;
        Vector3 normal = Vector3.Cross(relvel.normalized, this.craft.Acceleration.normalized);
        return normal.normalized;
    }
    protected Vector3 AttitudeAntinormal() {
        var relvel = craft.CurrentOrbitalState.RelativeVelocity;
        Vector3 antinorm = -Vector3.Cross(relvel.normalized, this.craft.Acceleration.normalized);
        return antinorm.normalized;
    }

    protected Vector3 AttitudeRadial() {
        Vector3 radial = this.craft.Acceleration;
        return radial.normalized;
    }

    protected Vector3 AttitudeAntiradial() {
        Vector3 antiradial = -this.craft.Acceleration;
        return antiradial.normalized;
    }

    protected void DrawTrajectory(bool use_local) {
        OrbitalTrajectory info;
        if (use_local) {
            info = this.craft.PredictTrajectory(LocalMaxIteration, LocalTimeStep, true);
        }
        else {
            info = this.craft.PredictTrajectory(GlobalMaxIteration, GlobalTimeStep, false);
        }   
        line.useWorldSpace = true;
        line.loop = info.StableOrbit;
        line.transform.position = info.Path[0];
        line.positionCount = info.Path.Count;
        line.SetPositions(info.Path.ToArray());
    }

    float time = 0;
    bool drawn= false;

    void FixedUpdate() {
        HandleManeuver();
        if (poseAdjuster.IsGoalReached) {
          poseStablizer.Update(body, 0.1f);
        }
        poseAdjuster.Update(body, .01f);
        transController.Update();

        time += Time.fixedDeltaTime;
        if (time > .05f){
            time = 0;
            DrawTrajectory(true);
        }
    }

    protected abstract void HandleManeuver();

    public Vector3 ToAcceleration(Vector3 F) {
        return F / PhysicsConstant.AbsoluteMass(craft.Mass);
    }

    public void AddRelativeAcceleration(Vector3 a) {
        a = transform.rotation * a;
        Debug.Log(a);
        craft.Solver.AddGlobalAcceleration(a);
    }
}
