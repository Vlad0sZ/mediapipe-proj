using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime
{
    public class SimulateJoints : MonoBehaviour
    {
        public Transform[] joints;


        private void OnDrawGizmos()
        {
            if (joints.Length < 2)
                return;

            var len = joints.Length;
            for (int i = 0; i < len - 1; i++)
            {
                var j = joints[i];
                var n = joints[i + 1];


                Gizmos.DrawLine(j.position, n.position);

                var forward = j.position - n.position;
                var rot = Quaternion.LookRotation(forward, Vector3.up);
            }
        }
    }
}