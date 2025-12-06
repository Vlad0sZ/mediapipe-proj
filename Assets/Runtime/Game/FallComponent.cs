using System;
using UnityEngine;

namespace Runtime.Game
{
    [RequireComponent(typeof(Rigidbody))]
    public class FallComponent : MonoBehaviour
    {
        public Vector2 gravityMinMax;
        public Vector2 rotationSpeedMinMax;

        private Rigidbody _rigidbody;
        private Vector3 _rotationAxis;
        private float _gravity;
        private float _rotationSpeed;

        private void Awake() => _rigidbody = GetComponent<Rigidbody>();

        private void OnEnable()
        {
            ReleasePhysics();
            SetupParameters();
        }

        private void OnDisable() =>
            ReleasePhysics();

        private void FixedUpdate() =>
            _rigidbody.AddForce(Vector3.down * _gravity, ForceMode.Acceleration);

        private void ReleasePhysics()
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;

            if (_rigidbody.IsSleeping())
                _rigidbody.WakeUp();
        }

        private void SetupParameters()
        {
            _gravity = gravityMinMax.AsMinMaxNext();
            _rotationSpeed = rotationSpeedMinMax.AsMinMaxNext();
            _rotationAxis = UnityEngine.Random.onUnitSphere;

            _rigidbody.angularDamping = 0.1f;
            _rigidbody.angularVelocity = _rotationAxis * _rotationSpeed;
        }
    }
}