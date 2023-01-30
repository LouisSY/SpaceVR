using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PoseAdjuster : PoseControllerBase
{
    // Start is called before the first frame update
    private PIDController controller;
    private Vector3 target_direction;
    private Func<Vector3> dyn_target;
    public bool IsGoalReached {get; private set;}
    public bool IsRealtime {get; set;}

    public PoseAdjuster(SpacecraftBase craft) : base(craft)
    {
        this.controller = new PIDController(1f);
        IsGoalReached = true;
        IsRealtime = false;
        this.controller.GoalState = Vector3.zero;
    }

    public void SetTargetPose(Vector3 pose) {
        this.target_direction = pose;
        this.controller.Reset();
        IsGoalReached = false;
        IsRealtime = false;
    }

    public void SetDynamicTargetPose(Func<Vector3> dpose) {
        IsRealtime = true;
        this.dyn_target = dpose;
    }

    public void Update(Rigidbody body, float force = 0.005f) {
        if (IsGoalReached && !IsRealtime) {
            return;
        }
        else if (IsRealtime) {
            this.target_direction = this.dyn_target();
            this.controller.Reset();
        }

        if (this.target_direction.sqrMagnitude <= float.Epsilon) {
            IsGoalReached = true;
            return;
        }

        Vector3 currp = body.transform.TransformDirection(Vector3.forward).normalized;
        Vector3 N = Vector3.Cross(currp, this.target_direction).normalized;
        float theta = Mathf.Acos(Vector3.Dot(currp, this.target_direction));

        Vector3 cmd_vec = this.controller.Update(new Vector3(theta, 0f, 0f));
        Vector3 w = (N * -cmd_vec.x) - body.angularVelocity / Time.fixedDeltaTime;
        Vector3 e = this.controller.CurrentError;
        
        Vector3 T = WithTorqueKinematics(w);

        if (e.magnitude <= 0.001f || float.IsNaN(Vector3.Dot(T,T))) {
            IsGoalReached = true;
            return;
        }

        body.AddTorque(T, ForceMode.VelocityChange);
    }
}
