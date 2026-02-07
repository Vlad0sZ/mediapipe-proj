using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Runtime.Menu
{
    public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
        IPointerUpHandler
    {
        [SerializeField] private float scaleIn;
        [SerializeField] private float scaleOut;
        [SerializeField] private float duration;
        private Tween _dt;
        private RectTransform _rect;

        private void Awake() =>
            _rect = this.GetComponent<RectTransform>();

        public void OnPointerEnter(PointerEventData eventData) =>
            SetScale(scaleIn);

        public void OnPointerExit(PointerEventData eventData) =>
            SetScale(1f);

        public void OnPointerDown(PointerEventData eventData) =>
            SetScale(scaleOut);

        public void OnPointerUp(PointerEventData eventData) =>
            SetScale(1f);

        private void SetScale(float scale)
        {
            _dt?.Kill();
            _dt = _rect.DOScale(Vector3.one * scale, duration);
        }
    }
}