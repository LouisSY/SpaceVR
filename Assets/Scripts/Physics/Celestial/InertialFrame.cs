using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyProject.Common {
    public class InertialFrame
    {
        private InertialFrame referencedFrame;
        public BodyState State { get; private set; } = new BodyState();

        public CelestialBody Center { get; private set; }

        public InertialFrame Referenced { get => referencedFrame; }

        public InertialFrame()
        {
            referencedFrame = null;
        }

        public void AssignCenter(CelestialBody celestia) {
            this.Center = celestia;
        }

        public void ReferenceTo(InertialFrame frame) {
            if (frame.Center.name != Center.name) {
                this.referencedFrame = frame;
            }
        }

        public void SyncState() {
            State.Velocity = Center.Rigid.velocity - Center.SOICenter.Rigid.velocity;
            State.Position = Center.transform.position;
        }

        public void UpdateState(Vector3 pos, Vector3 vel) {
            if (vel.IsNan()) {
                vel = Vector3.zero;
            }

            State.Velocity = vel;
            State.Position = pos;
        }

        public void ApplyAcceleration(Vector3 acceleration, float deltaT) {
            State.Velocity += acceleration * deltaT;
            State.Position += State.Velocity * deltaT;

            if (State.Velocity.IsNan()) {
                State.Velocity = Vector3.zero;
            }
        }

        public void TransformGalilean(ref InertialFrame result, InertialFrame referenced, float T) {
            result.State.Position = referenced.State.Position + State.Velocity * T; 
            result.State.Velocity = State.Velocity;
        }

        public void TransformGlobal(ref InertialFrame result, float T) {
            result.State.Position = referencedFrame.State.Position + State.Position; 
            result.State.Velocity = referencedFrame.State.Velocity + State.Velocity;
        }

        public Vector3 TransformGlobalVelocity() {
            if (referencedFrame == null) {
                return State.Velocity;
            }
            return referencedFrame.TransformGlobalVelocity() + State.Velocity;
        }

        public Vector3 GetRelativeVelocity(InertialFrame otherframe) {
            return State.Velocity - otherframe.State.Velocity;
        }
    }
}