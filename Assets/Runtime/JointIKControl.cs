using SensorPack.KinectCore.Runtime;
using UnityEngine;

namespace Runtime
{
    public sealed class JointIKControl : JointControl
    {
        [Header("Joints")] public KinectInterop.JointType targetType;
        public KinectInterop.JointType hintType;
        public KinectInterop.JointType rootType;

        [Header("Bones")] public Transform boneRoot;
        public Transform boneMid;
        public Transform boneTarget;

        [Header("IK Target")] public Transform target;
        public Transform hint;

        [Header("Settings")] public Vector3 offset;
        public float smoothFactor;

        public float _upperArmLength;
        public float _armLength;


        public override void Initialize(KinectManager km)
        {
            base.Initialize(km);

            if (boneRoot != null && boneMid != null)
                _upperArmLength = Vector3.Distance(boneRoot.position, boneMid.position);

            if (boneRoot != null && boneTarget != null)
                _armLength = Vector3.Distance(boneRoot.position, boneTarget.position);

            if (target == null)
                target = transform;
        }

        public override void UpdateBones()
        {
            bool isTrackedTarget = IsJointTracked(targetType, out var targetPos);
            bool isTrackedBase = IsJointTracked(rootType, out var basePos);
            bool isTrackedHint = IsJointTracked(hintType, out var hintPos);

            bool isTracked = isTrackedTarget && isTrackedHint;

            if (!isTracked)
                return;

            if (_armLength > 0f)
            {
                var humanDir = targetPos - basePos;
                var humanLen = humanDir.magnitude;
                if (humanLen > Mathf.Epsilon)
                {
                    var dirNorm = humanDir / humanLen;
                    var scaledDir = dirNorm * _armLength + offset;

                    var desiredTargetPos = boneRoot ? boneRoot.position + scaledDir : scaledDir;
                    target.position = Vector3.Lerp(target.position, desiredTargetPos, smoothFactor * Time.deltaTime);
                }
            }


            if (_upperArmLength > 0f)
            {
                var humanDir = hintPos - basePos;
                var humanLen = humanDir.magnitude;
                if (humanLen > Mathf.Epsilon)
                {
                    var dirNorm = humanDir / humanLen;
                    var scaledDir = dirNorm * _upperArmLength + offset;

                    var desiredTargetPos = boneRoot ? boneRoot.position + scaledDir : scaledDir;
                    hint.position = Vector3.Lerp(hint.position, desiredTargetPos, smoothFactor * Time.deltaTime);
                }
            }
        }


        private bool IsJointTracked(KinectInterop.JointType joint, out Vector3 pos)
        {
            if (!this.TryGetJointPositionIfTracked(joint, false, out pos))
                return false;

            var worldPos = this.GetJointPosition(joint, true);
            pos.y = 1f - pos.y;
            pos.z = 1f - worldPos.z;
            return true;
        }
    }
}