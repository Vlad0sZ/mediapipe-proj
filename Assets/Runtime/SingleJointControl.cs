using System;
using System.Collections.Generic;
using SensorPack.KinectCore.Runtime;
using UnityEngine;

namespace Runtime
{
    public sealed class SingleJointControl : JointControl
    {
        [System.Serializable]
        public class BoneTargetPair
        {
            public KinectInterop.JointType rootJoint;
            public KinectInterop.JointType tipJoint;

            [Header("Bones")] public Transform rootBone;
            public Transform tipBone;

            [Header("IK")] public Transform target;


            public float BoneLength { get; private set; }

            public void Initialize() =>
                BoneLength = Vector3.Distance(rootBone.position, tipBone.position);
        }

        [Header("Configuration")] public List<BoneTargetPair> boneConfigs = new();
        [Header("Settings")] public float smooth;

        [SerializeField] private float size;
        
        private Vector3 _latestPos;

        public override void Initialize(KinectManager km)
        {
            base.Initialize(km);
            CalculateAllBoneLengths();
        }


        private void CalculateAllBoneLengths()
        {
            foreach (var config in boneConfigs)
                config.Initialize();
        }

        public override void UpdateBones()
        {
            foreach (var config in boneConfigs)
            {
                UpdateBoneTarget(config);
            }
        }

        private void UpdateBoneTarget(BoneTargetPair config)
        {
            if (config.target == null)
                return;

            bool baseTracked = IsJointTracked(config.rootJoint, out Vector3 baseKinectPos);
            bool tipTracked = IsJointTracked(config.tipJoint, out Vector3 tipKinectPos);

            if (!baseTracked || !tipTracked)
                return;

            Vector3 basePos = config.rootBone != null ? config.rootBone.position : baseKinectPos;

            Vector3 humanDir = tipKinectPos - baseKinectPos;
            float humanLen = humanDir.magnitude;
            float boneLength = config.BoneLength;
            // Масштабируем под длину кости персонажа
            if (humanLen > 0.01f && boneLength > 0f)
            {
                Vector3 scaledDir = (humanDir / humanLen) * boneLength;
                Vector3 targetPos = basePos + scaledDir;
                _latestPos = targetPos;

                config.target.position = Vector3.Lerp(
                    config.target.position,
                    targetPos,
                    smooth * Time.deltaTime);
            }
        }

        private void OnDrawGizmos()
        {
            if (_latestPos == Vector3.zero)
                return;
            
            
            Gizmos.color = Color.yellow;

            Gizmos.DrawSphere(_latestPos, size);
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(_latestPos, $"j: {boneConfigs[0].tipJoint}");
            #endif
        }

        private bool IsJointTracked(KinectInterop.JointType jointType, out Vector3 position)
        {
            if (!this.TryGetJointPositionIfTracked(jointType, false, out position))
                return false;

            var worldPos = this.GetJointPosition(jointType, true);
            position.y = 1f - position.y;
            position.z = 1f - worldPos.z;
            return true;
        }
    }
}