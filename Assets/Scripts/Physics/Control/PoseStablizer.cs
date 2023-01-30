using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseStablizer : PoseControllerBase
{
    private PIDController stabler;
    public bool _enable;
    private bool _inhbit;

    public bool Enable {
        get {
            return _enable;
        }
        set {
            _enable = value;
            if (value) {
                this.stabler.Reset();
            }
        }
    }

    public bool Inhibit {
        get {
            return _inhbit;
        }
        set {
            _inhbit = value;
            if (!value) {
                this.stabler.Reset();
            }
        }
    }

    public PoseStablizer(SpacecraftBase craft) : base(craft)
    {
        this._enable = true;
        this._inhbit = false;
        stabler = new PIDController(0.01f);
        stabler.GoalState = Vector3.zero;
    }

    public void Update(Rigidbody body, float force = 0.005f) {
        if (_enable && !_inhbit) {
            float sz = body.angularVelocity.sqrMagnitude;
            if (sz > 0.00001f) {
                Vector3 w = this.stabler.Update(body.angularVelocity.normalized);
                Vector3 T = WithTorqueKinematics(w);
                body.AddTorque(T, ForceMode.VelocityChange);
            }
            else if (sz != 0f) {
                body.angularVelocity = Vector3.zero;
                this.stabler.Reset();
            }
        }
    }

    public void SwitchEnable() {
        if (this.Enable) {
            this.Enable = false;
        }
        else {
            this.Enable = true;
        }
    }
}
