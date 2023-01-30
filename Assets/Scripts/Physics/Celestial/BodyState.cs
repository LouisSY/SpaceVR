using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyProject.Common {
    public class BodyState {
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }

        public BodyState()
        {
            
        }

        public BodyState(Vector3 pos, Vector3 vel)
        {
            this.Position = pos;
            this.Velocity = vel;
        }
    }
}