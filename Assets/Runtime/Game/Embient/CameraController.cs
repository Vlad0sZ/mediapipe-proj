using Unity.Cinemachine;
using UnityEngine;

namespace Runtime.Game.Embient
{
    public sealed class CameraController : MonoBehaviour, ICameraController
    {
        [SerializeField] private CinemachineCamera[] cameras;

        public void LiveMainCamera() => ActivateCamera(0);

        public void LiveLevelCamera(int level) => ActivateCamera(level + 1);

        private void ActivateCamera(int index)
        {
            UnityEngine.Debug.Log($"set camera at index {index} is active");
            if (cameras.Length == 0)
                return;

            for (int i = 0; i < cameras.Length; i++)
                cameras[i].enabled = index == i;
        }
    }
}