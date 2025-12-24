using Runtime.Game.Interfaces;
using Runtime.Game.ScriptableData;
using UnityEngine;

namespace Runtime.Game
{
    public sealed class FallComponentNew : MonoBehaviour, IFallComponentSetup
    {
        [Header("Fall Settings")] [SerializeField]
        private float fallSpeed = 5f;

        [SerializeField] private float rotationSpeed = 180f;
        [SerializeField] private Vector3 rotationAxis = Vector3.up;

        [Header("Limits")] [SerializeField] private float disableY = -10f;

        private void OnEnable()
        {
            // Optional reset logic can be added here if needed
        }

        public void Setup(GameSettings.ObjectsSettings payload)
        {
            fallSpeed = payload.minMaxFallSpeed.AsMinMaxNext();
            rotationSpeed = payload.minMaxRotationSpeed.AsMinMaxNext();
        }

        private void Update()
        {
            MoveDown();
            Rotate();
            CheckDisableCondition();
        }

        public void SetFallSpeed(float value)
        {
            fallSpeed = value;
        }

        public void SetRotationSpeed(float value)
        {
            rotationSpeed = value;
        }

        public void SetRotationAxis(Vector3 axis)
        {
            rotationAxis = axis.normalized;
        }

        private void MoveDown()
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        }

        private void Rotate()
        {
            transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.Self);
        }

        private void CheckDisableCondition()
        {
            if (transform.position.y <= disableY)
            {
                gameObject.SetActive(false);
            }
        }
    }
}