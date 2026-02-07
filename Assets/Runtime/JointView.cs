using System;
using SensorPack.KinectCore.Runtime;
using UnityEngine;

namespace Runtime
{
    /*
     * Раскидать точки на плечи
     * Добавить этот View на руки
     * Посмотреть с вращением рук (upperarm вращается от локтя до руки)
     * Рука вращается от большого пальца
     *
     */
    public sealed class JointView : MonoBehaviour
    {
        [SerializeField] private Transform boneTarget;
        [SerializeField] private Transform target;

        [SerializeField] private KinectInterop.JointType rootType;
        [SerializeField] private KinectInterop.JointType targetType;

        [SerializeField] private int playerIndex;
        [SerializeField] private float smoothFactor;
        [SerializeField] private bool kinectSpace;

        private float _boneLength;
        private bool _lastStateVisible;

        private void Start()
        {
            if (boneTarget != null && boneTarget.parent != null)
                _boneLength = Vector3.Distance(boneTarget.position, boneTarget.parent.position);
        }
        
        private void Update()
        {
            var km = KinectManager.Instance;
            if (km == null || km.IsInitialized() == false)
                return;

            if (km.IsUserDetected(playerIndex) == false)
                return;

            var userId = km.GetUserIdByIndex(playerIndex);

            bool isRootTracked = TryGetJoint(km, userId, rootType, kinectSpace, out var rootPos);
            bool isTargetTracked = TryGetJoint(km, userId, targetType, kinectSpace, out var targetPos);

            _lastStateVisible = isRootTracked && isTargetTracked;
            if (!isRootTracked || !isTargetTracked)
                return;

            if (_boneLength <= 0)
                return;


            var distance = targetPos - rootPos;

            if (distance.sqrMagnitude <= Mathf.Epsilon)
                return;

            var dirNorm = distance.normalized;
            Transform referenceSpace = (target.parent != null) ? target.parent : transform;
            Vector3 dir = referenceSpace.TransformDirection(dirNorm);
            var worldPos = boneTarget.parent.position + (dir * _boneLength);
            if (smoothFactor > 0)
                target.position = Vector3.Lerp(target.position, worldPos, smoothFactor * Time.deltaTime);
            else
                target.position = worldPos;
        }


        private static bool TryGetJoint(KinectManager kinectManager, long userId, KinectInterop.JointType type,
            bool kinectSpace, out Vector3 position)
        {
            var jIndex = (int) type;
            position = Vector3.zero;
            if (kinectManager.IsJointTracked(userId, jIndex) == false)
                return false;

            if (kinectSpace)
                position = kinectManager.GetJointKinectPosition(userId, jIndex);
            else
                position = kinectManager.GetJointPosition(userId, jIndex);


            position.z = -position.z;
            return true;
        }
    }
}