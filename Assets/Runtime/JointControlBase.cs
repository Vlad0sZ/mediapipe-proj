using System;
using System.Collections;
using SensorPack.KinectCore.Runtime;
using UnityEngine;

namespace Runtime
{
    public sealed class JointControlBase : MonoBehaviour
    {
        public JointControl[] jointControls;

        private KinectManager _kinectManager;
        private bool _initialized;


        private IEnumerator Start()
        {
            _kinectManager = KinectManager.Instance;

            if (_kinectManager == null)
            {
                UnityEngine.Debug.LogError("KM is null.");
                enabled = false;
            }

            yield return new WaitWhile(() => !_kinectManager.IsInitialized());
            ForEach(c => c.Initialize(_kinectManager));
            _initialized = true;
        }

        private void Update()
        {
            if (!_initialized || !enabled)
                return;

            ForEach(c => c.UpdateBones());
        }

        private void ForEach(Action<JointControl> control)
        {
            foreach (var jointControl in jointControls)
                control(jointControl);
        }
    }
}