using SensorPack.KinectCore.Runtime;
using UnityEngine;

namespace Runtime
{
    public abstract class JointControl : MonoBehaviour
    {
        [SerializeField] private int playerIndex;

        private KinectManager _kinectManager;
        protected virtual int PlayerIndex => playerIndex;

        public virtual void Initialize(KinectManager km) => _kinectManager = km;

        public abstract void UpdateBones();


        protected bool TryGetJointPositionIfTracked(KinectInterop.JointType jointType, bool kinectSpace,
            out Vector3 position)
        {
            position = default;
            if (_kinectManager == null)
                return false;


            if (!_kinectManager.IsUserDetected(PlayerIndex))
                return false;

            var userId = _kinectManager.GetUserIdByIndex(PlayerIndex);
            var jointIndex = (int) jointType;

            if (!_kinectManager.IsJointTracked(userId, jointIndex))
                return false;

            position = kinectSpace
                ? _kinectManager.GetJointKinectPosition(userId, jointIndex)
                : _kinectManager.GetJointPosition(userId, jointIndex);

            return true;
        }

        protected Vector3 GetJointPosition(KinectInterop.JointType jointType, bool kinectSpace)
        {
            var userId = _kinectManager.GetUserIdByIndex(PlayerIndex);
            var jointIndex = (int) jointType;

            return kinectSpace
                ? _kinectManager.GetJointKinectPosition(userId, jointIndex)
                : _kinectManager.GetJointPosition(userId, jointIndex);
        }
    }
}