using UnityEngine;

public class PIDController {
    Vector3 p = Vector3.zero;
    Vector3 i = Vector3.zero;
    Vector3 d = Vector3.zero;
    Vector3 goal = Vector3.zero;
    float t = 0f;

    public Vector3 GoalState {
        get {
            return this.goal;
        }
        set {
            this.goal = value;
            Reset();
        }
    }
    public Vector3 CurrentError { get; private set; }

    private float Kp = 0f;

    public PIDController(float Kp)
    {
        this.Kp = Kp;
    }

    public Vector3 Update(Vector3 state) {
        Vector3 e = this.goal - state;
        this.t += Time.fixedDeltaTime;
        this.i += e;
        Vector3 cmd_vec = this.Kp * (e + this.t * (e - this.d) + 1f / this.t * this.i);
        this.d = e;

        this.CurrentError = e;
        return cmd_vec;
    }

    public void Reset() {
        this.p = Vector3.zero;
        this.i = Vector3.zero;
        this.d = Vector3.zero;
        this.t = 0f;
    }
}