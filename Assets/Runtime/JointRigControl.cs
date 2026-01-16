using SensorPack.KinectCore.Runtime;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Runtime
{
    public sealed class JointRigControl : JointControl
    {
        public KinectInterop.JointType targetJoint;
        public Rig targetRig;
        public float smooth;

        public override void UpdateBones()
        {
            if (targetRig == null)
                return;

            bool isTracked = this.TryGetJointPositionIfTracked(targetJoint, false, out _);
            float weight = isTracked ? 1f : 0f;

            if (smooth > 0)
                targetRig.weight = Mathf.Lerp(targetRig.weight, weight, smooth * Time.deltaTime);
            else
                targetRig.weight = weight;
        }
    }
}