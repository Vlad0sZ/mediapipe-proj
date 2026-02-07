using SensorPack.KinectCore.Runtime;
using UnityEngine;

namespace Runtime
{
    public sealed class JointWeight : MonoBehaviour
    {
        [SerializeField] private int playerIndex;
        [SerializeField] private KinectInterop.JointType joint;
        [SerializeField] private bool inverse;

        private IWeightSetter _weightSetter;

        private void Start() =>
            _weightSetter = GetComponent<IWeightSetter>();

        private void Update()
        {
            bool tracked = IsJointTracked();
            _weightSetter.SetWeight(ToWeight(tracked));
        }

        private float ToWeight(bool v) => inverse ^ v ? 1f : 0f;

        private bool IsJointTracked()
        {
            var km = KinectManager.Instance;

            if (km == null || km.IsInitialized() == false)
                return false;

            if (km.IsUserDetected(playerIndex) == false)
                return false;

            var userId = km.GetUserIdByIndex(playerIndex);
            bool isTracked = km.IsJointTracked(userId, (int) joint);
            return isTracked;
        }
    }
}