using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyProject.Common {
    public class InertialFrame
    {
        private InertialFrame referencedFrame;
        private BodyState localState = new BodyState();
        private BodyState globalState = new BodyState();

        public BodyState LocalState { get => localState; }
        public BodyState GlobalState { get => globalState; }

        public CelestialBody Center { get; private set; }

        public InertialFrame()
        {
            
        }

        public void AssignCenter(CelestialBody celestia) {
            this.Center = celestia;
            SyncState();
        }

        public void ReferenceTo(InertialFrame frame) {
            this.referencedFrame = frame;
            ResetLocal();
        }

        public void SyncState() {
            globalState.Position = this.Center.Rigid.position;
            globalState.Velocity = this.Center.Rigid.velocity;
            if (this.referencedFrame != null) {
                ResetLocal();
            }
        }

        public void ApplyAcceleration(Vector3 acceleration, float deltaT) {
            globalState.Velocity += acceleration * deltaT;
            globalState.Position += globalState.Velocity * deltaT;
            
            UpdateLocal();
        }

        public void UpdateLocal() {
            if (referencedFrame == null) {
                return;
            }
            Vector3 rvel = this.globalState.Velocity - referencedFrame.globalState.Velocity;

            Matrix4x4 TlocalInv = Matrix4x4.Translate(referencedFrame.globalState.Position).inverse;
            Quaternion RlocalInv = Quaternion.Inverse(referencedFrame.Center.transform.rotation);
            localState.Velocity = rvel;
            localState.Position = TlocalInv.MultiplyPoint(RlocalInv * this.globalState.Position);
        }

        public void ResetLocal() {
            Transform T_frame = referencedFrame.Center.transform;
            localState.Velocity = Vector3.zero;
            localState.Position = T_frame.transform.InverseTransformPoint(this.globalState.Position);
        }

        public Vector3 GetRelativeVelocity(InertialFrame otherframe) {
            return Center.Rigid.velocity - otherframe.Center.Rigid.velocity;
        }
    }
}