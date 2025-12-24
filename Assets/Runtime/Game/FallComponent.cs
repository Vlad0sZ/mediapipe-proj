using System;
using Runtime.Game.Interfaces;
using Runtime.Game.ScriptableData;
using UnityEngine;

namespace Runtime.Game
{
    [RequireComponent(typeof(Rigidbody))]
    public class FallComponent : MonoBehaviour, IFallComponentSetup
    {
        public Vector2 gravityMinMax;
        public Vector2 rotationSpeedMinMax;

        private Rigidbody _rigidbody;
        private Vector3 _rotationAxis;
        private float _gravity;
        private float _rotationSpeed;
        private bool _isGrounded;

        private void Awake() => _rigidbody = GetComponent<Rigidbody>();

        private void OnEnable() =>
            SetupParameters();

        private void OnDisable() =>
            ReleasePhysics();


        public void Setup(GameSettings.ObjectsSettings payload)
        {
            gravityMinMax = payload.minMaxFallSpeed;
            rotationSpeedMinMax = payload.minMaxRotationSpeed;
        }

        private void FixedUpdate()
        {
            if (!_isGrounded)
                _rigidbody.AddForce(Vector3.down * _gravity, ForceMode.Acceleration);
        }

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


            _rigidbody.useGravity = false;
            _rigidbody.angularDamping = 0.1f;
            _rigidbody.angularVelocity = _rotationAxis * _rotationSpeed;
            _rigidbody.AddForce(Vector3.down * 0.5f, ForceMode.VelocityChange);
            _isGrounded = false;
        }

        private void OnCollisionEnter(Collision other)
        {
            _isGrounded = true;
            _rigidbody.angularDamping = 10f;

            var vel = _rigidbody.linearVelocity;
            vel.y = Mathf.Min(vel.y, 0f);
            _rigidbody.linearVelocity = vel;
        }
    }
}