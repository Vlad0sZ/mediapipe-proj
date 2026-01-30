using SensorPack.KinectCore.Runtime;
using UnityEngine;

namespace Runtime
{
    public class AvatarMovementView : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private Transform characterRoot; // Объект персонажа (Body или Root)

        [Header("Movement Settings")] [SerializeField]
        private int playerIndex = 0;

        [SerializeField] private float smoothFactor = 5f;

        [Header("Room Bounds")] [SerializeField]
        private float leftBoundX = -5f; // Мировая X координата для значения 0

        [SerializeField] private float rightBoundX = 5f; // Мировая X координата для значения 1

        private void Update()
        {
            var km = KinectManager.Instance;
            if (km == null || !km.IsInitialized())
                return;

            if (!km.IsUserDetected(playerIndex))
                return;

            var userId = km.GetUserIdByIndex(playerIndex);
            Vector3 userPos = km.GetJointKinectPosition(userId, (int) KinectInterop.JointType.SpineBase);

            if (characterRoot == null)
                return;

            float clampedX = Mathf.Clamp01(userPos.x);

            float targetX = Mathf.Lerp(leftBoundX, rightBoundX, clampedX);

            Vector3 currentLocalPos = characterRoot.localPosition;
            Vector3 destinationLocal = new Vector3(targetX, currentLocalPos.y, currentLocalPos.z);
            if (smoothFactor > 0)
            {
                characterRoot.localPosition =
                    Vector3.Lerp(currentLocalPos, destinationLocal, smoothFactor * Time.deltaTime);
            }
            else
            {
                characterRoot.localPosition = destinationLocal;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (characterRoot == null || characterRoot.parent == null) return;

            Gizmos.color = Color.green;
            // Рисуем линию в локальном пространстве родителя
            Vector3 localLeft = new Vector3(leftBoundX, 0, 0);
            Vector3 localRight = new Vector3(rightBoundX, 0, 0);

            // Переводим локальные точки родителя в мировые для отрисовки
            var parent = characterRoot.parent;
            Vector3 worldLeft = parent.TransformPoint(localLeft);
            Vector3 worldRight = parent.TransformPoint(localRight);

            Gizmos.DrawLine(worldLeft, worldRight);
        }
    }
}