using System;
using Runtime.Game.Interfaces;
using UnityEngine;

namespace Runtime.Game
{
    public class HandCollectable : MonoBehaviour
    {
        [SerializeField] private Camera _mainCamera;
        private readonly RaycastHit[] _buffer = new RaycastHit[10];

        private void Start()
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main;
        }

        private void FixedUpdate()
        {
            if (_mainCamera == null)
                return;

            var screenPoint = _mainCamera.WorldToScreenPoint(transform.position);
            var ray = _mainCamera.ScreenPointToRay(screenPoint, Camera.MonoOrStereoscopicEye.Mono);
            var collected = Physics.RaycastNonAlloc(ray, _buffer, float.MaxValue);

            if (collected == 0)
                return;

            for (int i = 0; i < collected; i++)
            {
                if (_buffer[i].collider.TryGetComponent<ICollectableItem>(out var comp))
                    comp.Collect();
            }
        }
    }
}