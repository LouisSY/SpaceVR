using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyProject.Common {
    public class OrbitalTrajectory {
        public Vector3 Ap { get; set; }
        public Vector3 Pe { get; set; }
        public List<Vector3> Path { get; private set; }

        public bool StableOrbit;

        private float ap_dist = float.MinValue, pe_dist = float.MaxValue;

        public OrbitalTrajectory()
        {
            this.Ap = Vector3.zero;
            this.Pe = Vector3.zero;
            this.Path = new List<Vector3>();
            this.StableOrbit = false;
        }

        public void Reset() {
             this.Ap = Vector3.zero;
            this.Pe = Vector3.zero;
            this.Path.Clear();
            this.StableOrbit = false;
        }

        public void AddPathNode(Vector3 node, float dist_to_influencer) {
            Path.Add(node);
            if (ap_dist < dist_to_influencer) {
                Ap = node;
                ap_dist = dist_to_influencer;
            }

            if (pe_dist > dist_to_influencer) {
                Pe = node;
                pe_dist = dist_to_influencer;
            }
        }
    }
}
