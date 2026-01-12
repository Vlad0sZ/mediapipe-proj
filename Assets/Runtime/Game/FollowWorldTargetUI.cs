using UnityEngine;

namespace Runtime.Game
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class FollowWorldTargetUI : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;

        private RectTransform _rectTransform;
        private Transform _target;
        private Vector3 _worldOffset;
        private bool _hasTarget;

        private Camera _uiCamera;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();

            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();

            if (canvas == null)
            {
                Debug.LogError("FollowWorldTargetUI: Canvas is not assigned and not found in parents.");
                enabled = false;
                return;
            }

            // Для Overlay камера не нужна, для остальных берём ту, что указана у Canvas.
            _uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera;
        }

        /// <summary>
        /// Задать цель и смещение в мировых координатах.
        /// </summary>
        public void AttachTo(Transform target, Vector3 worldOffset)
        {
            _target = target;
            _worldOffset = worldOffset;
            _hasTarget = _target != null;
        }

        /// <summary>
        /// Отвязать UI от цели.
        /// </summary>
        public void Detach()
        {
            _hasTarget = false;
            _target = null;
        }

        private void LateUpdate()
        {
            if (!_hasTarget || _target == null)
                return;

            switch (canvas.renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                case RenderMode.ScreenSpaceCamera:
                    UpdateScreenSpacePosition();
                    break;
                case RenderMode.WorldSpace:
                    UpdateWorldSpacePosition();
                    break;
            }
        }

        private void UpdateScreenSpacePosition()
        {
            if (_uiCamera == null)
                _uiCamera = Camera.main;

            if (_uiCamera == null)
                return;

            var worldPos = _target.position + _worldOffset;
            var screenPos = RectTransformUtility.WorldToScreenPoint(_uiCamera, worldPos);
            _rectTransform.anchoredPosition = screenPos;
        }

        private void UpdateWorldSpacePosition()
        {
            // Для World Space Canvas достаточно выставить позицию напрямую
            var worldPos = _target.position + _worldOffset;
            _rectTransform.position = worldPos;
        }
    }
}