using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranslationController : PoseControllerBase
{
    PIDController pid;
    bool inhibit = false;
    Vector3 currentVel;
    public TranslationController(SpacecraftBase craft) : base(craft)
    {
        pid = new PIDController(0.01f);
        pid.GoalState = Vector3.zero;
        currentVel = Vector3.zero;
    }

    private void applyDirectional(Vector3 d) {
        d.Scale(craft.Kinematics.RCSThrust);
        Vector3 a = craft.ToAcceleration(d);
        currentVel += a * Time.fixedDeltaTime;
        craft.Body.AddRelativeForce(a, ForceMode.VelocityChange);
    }

    public void ExertDirectional(Vector3 d) {
        applyDirectional(d);
        inhibit = true;
    }

    public void MoveUpward() {
        ExertDirectional(Vector3.up);
    }

    public void MoveDownward() {
        ExertDirectional(Vector3.down);
    }

    public void MoveLeft() {
        ExertDirectional(Vector3.left);
    }

    public void MoveRight() {
        ExertDirectional(Vector3.right);
    }

    public void MoveForward() {
        ExertDirectional(Vector3.forward);
    }

    public void MoveBackward() {
        ExertDirectional(Vector3.back);
    }

    public void Update() {
        if (inhibit) {
            pid.Reset();
            inhibit = false;
            return;
        }

        if (currentVel.magnitude < 0.0001f) {
            currentVel = Vector3.zero;
            return;
        }

        Vector3 u = pid.Update(currentVel);
        applyDirectional(u.normalized);
    }
}
