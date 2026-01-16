using System;
using SensorPack.KinectCore.Runtime;
using UnityEngine;

namespace Runtime
{
    public class AvatarControl : MonoBehaviour
    {
        public Transform headTarget;
        public Transform leftHandTarget;
        public Transform leftHintTarget;

        private KinectManager _kinectManager;

        private void Start()
        {
            _kinectManager = KinectManager.Instance;
        }

        private void Update()
        {
            if (_kinectManager == null || _kinectManager.IsInitialized() == false)
                return;


            var bodyData = _kinectManager.GetUserBodyDataByIndex(0);
            var headPos = bodyData.joint[(int) KinectInterop.JointType.Head].kinectPos;
            headTarget.position = headPos;
        }


        private void TransformIKJoint(Transform target, Transform hint)
        {
        }
    }
}