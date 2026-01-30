using System;
using Runtime.Game.Interfaces;
using UnityEngine;

namespace Runtime.Game
{
    public class HandCollectable : MonoBehaviour
    {
        [SerializeField] private float castRadius = 1.0f;
        [SerializeField] private float castDistance = 1.0f;

        private Transform _transform;
        private readonly Collider[] _buffer = new Collider[10];

        private void Start() =>
            _transform = transform;

        private void FixedUpdate()
        {
            var origin = _transform.position;
            var forward = Vector3.forward;

            var forwardPoint = origin + forward * castDistance;
            var backwardPoint = origin - forward * castDistance;

            int hits = Physics.OverlapCapsuleNonAlloc(backwardPoint, forwardPoint, castRadius, _buffer);
            for (int i = 0; i < hits; i++)
            {
                if (_buffer[i].TryGetComponent<ICollectableItem>(out var comp))
                    comp.Collect();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            var tr = transform;
            var forward = Vector3.forward;
            var origin = tr.position;

            Vector3 p1 = origin + forward * castDistance;
            Vector3 p2 = origin - forward * castDistance;

            // Рисуем линию между центрами и сферы на концах
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawWireSphere(p1, castRadius);
            Gizmos.DrawWireSphere(p2, castRadius);
        }
    }
}